using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5528ReturnValueAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer, RH5528ReturnValueAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("RH5528-002")]
    [Requirement("RH5528-003")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[return: First, Second]|}
                                    internal int M() => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [return: First]
                                     [return: Second]
                                     internal int M() => 0;
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [return: First]
                                    [return: Second]
                                    internal int M() => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that commented violations are still reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[return: First, /* keep */ Second]|}
                                    internal int M() => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal class Example
                                   {
                                       [return: First, /* keep */ Second]
                                       internal int M() => 0;
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a merged return-specifier list on a single-line property accessor is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("SHARED-ATTR-014")]
    public async Task VerifyNoDiagnosticForMergedReturnSpecifierListOnSingleLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Other { [return: First, Second] get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a merged return-specifier list on a single-line property accessor is not reported (CRLF)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("SHARED-ATTR-014")]
    public async Task VerifyNoDiagnosticForMergedReturnSpecifierListOnSingleLinePropertyAccessorCrLf()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Other { [return: First, Second] get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that separate return-specifier lists on a single-line property accessor are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSeparateReturnSpecifierListsOnSingleLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Other { [return: First] [return: Second] get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that separate return-specifier lists on a single-line property accessor are not reported (CRLF)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSeparateReturnSpecifierListsOnSingleLinePropertyAccessorCrLf()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Other { [return: First] [return: Second] get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that a merged return-specifier list is still reported when an accessor has an expression body, because the formatter expands the accessor list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMergedReturnSpecifierListOnSingleLinePropertyAccessorWithExpressionBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { {|#0:[return: First, Second]|} get => 1; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Value {
                                         [return: First]
                                         [return: Second]
                                         get => 1; }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat));
    }

    /// <summary>
    /// Verifies that a merged return-specifier list on an accessor of a multi-line property is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("RH5528-003")]
    [Requirement("SHARED-ATTR-014")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticForMergedReturnSpecifierListOnMultiLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value
                                    {
                                        {|#0:[return: First, Second]|}
                                        get;
                                        set;
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Value
                                     {
                                         [return: First]
                                         [return: Second]
                                         get;
                                         set;
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat));
    }

    /// <summary>
    /// Verifies that no code fix is offered for a merged return-specifier list on an accessor of a single-line auto-property, which the formatter keeps inline and merged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoCodeFixForMergedReturnSpecifierListOnSingleLinePropertyAccessor()
    {
        const string codeFixData = """
                                   internal class Example
                                   {
                                       public int Value { [return: First, Second] get; set; }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that multi-attribute <c>return:</c> lists of a method, a multi-line accessor list, a lambda, and a delegate are reported, while the list of an inline auto-property accessor is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("RH5528-002")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-014")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForMultiAttributeReturnValueListsOutsideInlineAutoProperties()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[return: A, B]|}
                                    int M() => 0;

                                    int P { [return: A, B] get; set; }

                                    int Q
                                    {
                                        {|#1:[return: A, B]|}
                                        get;
                                        set;
                                    }

                                    void N()
                                    {
                                        System.Func<int> f = {|#2:[return: A, B]|} () => 0;
                                    }
                                }

                                {|#3:[return: A, B]|}
                                delegate int D();
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that a multi-attribute accessor list is not reported on an inline auto-property, but is reported once an accessor has an expression body or the accessor list spans several lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("SHARED-ATTR-014")]
    public async Task VerifyDiagnosticForMultiAttributeAccessorListUnlessThePropertyIsAnInlineAutoProperty()
    {
        const string testData = """
                                class C
                                {
                                    int P { [return: A, B] get; set; }

                                    int Q { {|#0:[return: A, B]|} get => 1; }

                                    int R
                                    {
                                        {|#1:[return: A, B]|}
                                        get;
                                        set;
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a multi-attribute accessor list of a one-line property is reported when a comment sits between the accessors
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("SHARED-ATTR-014")]
    public async Task VerifyDiagnosticForMultiAttributeAccessorListWhenCommentSitsBetweenTheAccessors()
    {
        const string testData = """
                                class C
                                {
                                    int S { {|#0:[return: A, B]|} get; /* c */ set; }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat));
    }

    /// <summary>
    /// Verifies that a property attribute list on its own line and a comment after the accessor list do not make the accessor list of an inline auto-property reportable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-001")]
    [Requirement("SHARED-ATTR-014")]
    public async Task VerifyNoDiagnosticForInlineAutoPropertyWithOwnAttributeListAndTrailingComment()
    {
        const string testData = """
                                class C
                                {
                                    [D]
                                    int Q { [return: A, B] get; set; } = 1; // c
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix replaces the <c>return:</c> list by one list per attribute, each on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachReturnValueAttributeAListOfItsOwn()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[return: A, B]|}
                                    int M() => 0;
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [return: A]
                                     [return: B]
                                     int M() => 0;
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the <c>return:</c> list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5528-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachReturnValueAttributeAListOfItsOwnWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[return: A, B]|}
                                    int M() => 0;
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [return: A]
                                     [return: B]
                                     int M() => 0;
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    {|#0:[return: First, Second]|} {|#1:[return: Third, Fourth]|}
                                    internal int M() => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                sealed class FourthAttribute : System.Attribute
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     [return: First]
                                     [return: Second]
                                     [return: Third]
                                     [return: Fourth]
                                     internal int M() => 0;
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class ThirdAttribute : System.Attribute
                                 {
                                 }
                                 sealed class FourthAttribute : System.Attribute
                                 {
                                 }
                                 """;

        // Two multi-attribute return-value lists sit back to back on the same declaration: each fix computes
        // its split against the same original document and both target the same return-value attribute-list
        // run, so the batch fixer discards one of the two conflicting changes in its first pass and needs a
        // second pass to correct the diagnostic that survives
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}