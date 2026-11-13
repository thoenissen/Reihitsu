using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5518FieldAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5518FieldAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer, RH5518FieldAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5518-001")]
    [Requirement("RH5518-002")]
    [Requirement("RH5518-003")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First, Second]|}
                                    internal int value;
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
                                     internal int value;
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
                     Diagnostics(RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5518MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5518-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [First]
                                    [Second]
                                    internal int value;
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
    [Requirement("RH5518-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First, /* keep */ Second]|}
                                    internal int value;
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
                                       internal int value;
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5518MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a <c>field:</c> attribute list with several attributes on a positional record parameter is left to the parameter rules
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5518-001")]
    public async Task VerifyNoDiagnosticForFieldTargetedAttributeListOnRecordParameter()
    {
        const string testData = """
                                internal sealed record Example([field: First, Second] int Id);
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
    /// Verifies that a merged field-specifier list on an accessor of a single-line property is not reported, because the formatter keeps the list inline and merged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMergedFieldSpecifierListOnSingleLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { [field: First, Second] get; set; }
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
    /// Verifies the spec example: a merged field-specifier list on the line of the accessor list's opening brace is still reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5518-004")]
    [Requirement("SHARED-ATTR-022")]
    public async Task VerifyDiagnosticWithoutCodeFixForMergedFieldSpecifierListOnSingleLinePropertyAccessorWithExpressionBody()
    {
        const string testData = """
                                class C
                                {
                                    public int Value { {|#0:[field: First, Second]|} get => 1; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5518MessageFormat));

        var actions = await GetCodeFixActionsAsync(RemoveMarkup(testData),
                                                   RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First(list => list.Attributes.Count > 1)
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the multi-attribute lists of a field, a <c>field:</c> list on a property, and an enum member are reported, while the <c>field:</c> list of a record parameter is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5518-001")]
    [Requirement("RH5518-002")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForMultiAttributeListsOfFieldsAndEnumMembers()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    int _f;

                                    {|#1:[field: A, B]|}
                                    int P { get; set; }
                                }

                                enum E
                                {
                                    {|#2:[A, B]|}
                                    X
                                }

                                record R([field: A, B] int Id);
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5518MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the code fix replaces the field attribute list by one list per attribute, each on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5518-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachFieldAttributeAListOfItsOwn()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    int _f;
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
                                     int _f;
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
                     Diagnostics(RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5518MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the field attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5518-003")]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-FIX-008")]
    public async Task VerifyCodeFixGivesEachFieldAttributeAListOfItsOwnWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    int _f;
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
                                     int _f;
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
                     Diagnostics(RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5518MessageFormat));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(RemoveMarkup(testData))), "Every line break the fix inserts must be CRLF.");
    }

    /// <summary>
    /// Verifies the spec example: a merged field-specifier list below an accessor list whose opening brace stands on a line of its own is split, and the accessor stays behind the last list, under LF and CRLF line endings, line endings included
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyCodeFixSplitsAFieldSpecifierListBelowAnAccessorListBraceOnItsOwnLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    public int Value
                                    {
                                        {|RH5518:[field: First, Second]|} get => 1;
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
                                 class C
                                 {
                                     public int Value
                                     {
                                         [field: First]
                                         [field: Second] get => 1;
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a merged field-specifier list on the line below an accessor list whose opening brace stands behind the property name is split, and the rest of the line stays behind the last list, under LF and CRLF line endings, line endings included
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyCodeFixSplitsAFieldSpecifierListBelowAnAccessorListBraceBehindThePropertyName(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    public int Other {
                                        {|RH5518:[field: First, Second]|} get => 1; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     public int Other {
                                         [field: First]
                                         [field: Second] get => 1; }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
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
                                    internal int value;
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
                                     [Second] [Third]
                                     [Fourth]
                                     internal int value;
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

        // Two multi-attribute field lists sit back to back on one line of the same declaration: each split changes only
        // its own list, so the second list's first attribute stays on the line on which the first list's last attribute
        // now stands. Whether the batch fixer needs one pass or two for the two changes is not what this scenario
        // asserts
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5518MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = -2);
    }

    #endregion // BatchCodeFixTestsBase
}