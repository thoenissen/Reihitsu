using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5516PropertyAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer, RH5516PropertyAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5516-001")]
    [Requirement("RH5516-002")]
    [Requirement("RH5516-003")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First, Second]|}
                                    internal int Value { get; set; }
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
                                     [First]
                                     [Second]
                                     internal int Value { get; set; }
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
                     Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5516-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [First]
                                    [Second]
                                    internal int Value { get; set; }
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
    [Requirement("RH5516-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First, /* keep */ Second]|}
                                    internal int Value { get; set; }
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
                                       [First, /* keep */ Second]
                                       internal int Value { get; set; }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute list with several attributes on a positional record parameter is left to the parameter rules
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5516-001")]
    public async Task VerifyNoDiagnosticForPropertyTargetedAttributeListOnRecordParameter()
    {
        const string testData = """
                                internal sealed record Example([property: First, Second] int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute list with several attributes on a type parameter is left to the type-parameter rules
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPropertyTargetedAttributeListOnTypeParameter()
    {
        const string testData = """
                                internal class Example<[property: First, Second] T>
                                {
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
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a merged property-specifier list on an accessor of a single-line property is not reported, because the formatter keeps the list inline and merged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMergedPropertySpecifierListOnSingleLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { [property: First, Second] get; set; }
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
    /// Verifies that the multi-attribute lists of a property, an indexer, and a <c>property:</c> list are reported, while the <c>property:</c> list of a record parameter is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5516-001")]
    [Requirement("RH5516-002")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForMultiAttributeListsOfPropertiesAndIndexers()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    int P { get; set; }

                                    {|#1:[A, B]|}
                                    int this[int i] => i;

                                    {|#2:[property: A, B]|}
                                    int Q { get; set; }
                                }

                                record R([property: A, B] int Id);
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the code fix replaces the property attribute list by one list per attribute, each on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5516-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachPropertyAttributeAListOfItsOwn()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    int P { get; set; }
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
                                     [A]
                                     [B]
                                     int P { get; set; }
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
                     Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the property attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5516-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachPropertyAttributeAListOfItsOwnWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    int P { get; set; }
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
                                     [A]
                                     [B]
                                     int P { get; set; }
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
                     Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    {|#0:[First, Second]|} {|#1:[Third, Fourth]|}
                                    internal int Value { get; set; }
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
                                     [First]
                                     [Second]
                                     [Third]
                                     [Fourth]
                                     internal int Value { get; set; }
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

        // Two multi-attribute property lists sit back to back on the same declaration: each fix computes its
        // split against the same original document and both target the same property attribute-list run, so the
        // batch fixer discards one of the two conflicting changes in its first pass and needs a second pass to
        // correct the diagnostic that survives
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}