using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5511ConstructorAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzerTests : BatchCodeFixTestsBase<RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer, RH5511ConstructorAttributesMustFollowPlacementRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First]|} internal Example() { }
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
                                     internal Example() { }
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
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [First]
                                    internal Example() { }
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
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First /* keep */]|} internal Example() { }
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
                                       [First /* keep */] internal Example() { }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the code fix indents the member at the attribute list's syntactic nesting depth when another
    /// declaration precedes the list on the same physical line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsAtNestingDepthWhenPrecededByOtherSourceOnSameLine()
    {
        const string testData = """
                                internal class Example
                                {
                                    private int _pad; {|#0:[First]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private int _pad; [First]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents the member at the attribute list's syntactic nesting depth rather than
    /// at a preceding directive's own indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsAtNestingDepthRatherThanPrecedingDirectiveIndentation()
    {
        const string testData = """
                                internal class Example
                                {
                                        #pragma warning disable 1591
                                    {|#0:[First]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                         #pragma warning disable 1591
                                     [First]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents the member at the attribute list's canonical nesting depth rather than
    /// matching whatever non-canonical depth the list itself already happens to sit at
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUsesCanonicalIndentationRegardlessOfTheAttributeListsOwnDepth()
    {
        const string testData = """
                                internal class Example
                                {
                                        {|#0:[First]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                         [First]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix always emits spaces for the computed indentation, even when another declaration
    /// on the same line is itself tab-indented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUsesSpacesRegardlessOfSurroundingTabIndentation()
    {
        const string testData = "internal class Example\n{\n\tprivate int _pad; {|#0:[First]|} internal Example() { }\n}\nsealed class FirstAttribute : System.Attribute\n{\n}\n";
        const string fixedData = "internal class Example\n{\n\tprivate int _pad; [First]\n    internal Example() { }\n}\nsealed class FirstAttribute : System.Attribute\n{\n}\n";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents the member at the attribute list's syntactic nesting depth when the
    /// attribute list itself spans multiple lines, using the depth rather than the column of either line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsAtNestingDepthForMultiLineAttributeList()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First(
                                        1)]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                    internal FirstAttribute(int value) { }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [First(
                                         1)]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                     internal FirstAttribute(int value) { }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents the member correctly when the attribute list's line is itself the
    /// continuation of a multi-line comment that started on an earlier line. Nesting depth does not read that
    /// line's text at all, so the comment's own internal alignment cannot influence the result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsCorrectlyWhenLineBeginsInsideMultiLineComment()
    {
        const string testData = """
                                internal class Example
                                {
                                    /* note
                                           continued */ {|#0:[First]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /* note
                                            continued */ [First]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents the member correctly when the attribute list's line is itself the
    /// continuation of a multi-line verbatim string literal that started on an earlier line. Unlike a comment,
    /// this continuation is part of a token's own span rather than trivia, but nesting depth is unaffected either way
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsCorrectlyWhenLineBeginsInsideMultiLineVerbatimString()
    {
        const string testData = """
                                internal class Example
                                {
                                    private const string Text = @"a
                                          b"; {|#0:[First]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private const string Text = @"a
                                           b"; [First]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents the member correctly when another declaration precedes a multi-line
    /// comment on the comment's own starting line, leaving the attribute list's leading trivia empty
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsCorrectlyWhenTokenPrecedesMultiLineCommentOnSameLine()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal int Field; /* note
                                       continued */ {|#0:[First]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Field; /* note
                                        continued */ [First]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents the member correctly when a second attribute list starts on the
    /// continuation line of a first, multi-line attribute list. The second list's own first token begins its
    /// physical line, but that line is a syntactic continuation of the multi-line list, not the declaration's own
    /// indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsCorrectlyWhenSecondAttributeListStartsOnMultiLineListsContinuationLine()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First(
                                        1)]|} {|#1:[Second]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                    internal FirstAttribute(int value) { }
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [First(
                                         1)]
                                     [Second]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                     internal FirstAttribute(int value) { }
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix indents the member correctly when the attribute list follows a multi-line
    /// parameter list that ends on the attribute's own physical line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsCorrectlyWhenAttributeListFollowsMultiLineParameterList()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M(int a,
                                                    int b) { } {|#0:[First]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void M(int a,
                                                     int b) { } [First]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents the member correctly when the attribute list follows a multi-line field
    /// initializer that ends on the attribute's own physical line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIndentsCorrectlyWhenAttributeListFollowsMultiLineInitializer()
    {
        const string testData = """
                                internal class Example
                                {
                                    private int Pad =
                                        1 + 2; {|#0:[First]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private int Pad =
                                         1 + 2; [First]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    {|#0:[First]|} {|#1:[Second]|} internal Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     [First]
                                     [Second]
                                     internal Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        // The two attribute lists already share one line: under WellKnownFixAllProviders.BatchFixer both fixes
        // are computed against the same original document. The second list's own leading trivia is empty there
        // (the intervening whitespace belongs to the first list's close bracket's trailing trivia instead),
        // which used to make its derived indentation zero-width
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}