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
    /// Verifies that the code fix uses the attribute list's own source line indentation, rather than its leading
    /// trivia, when another declaration precedes the list on that line. The list's leading trivia is empty in
    /// this shape, because the intervening whitespace belongs to the preceding token's trailing trivia instead
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUsesLineIndentationWhenListIsPrecededByOtherSourceOnSameLine()
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
    /// Verifies that the code fix uses the attribute list's own source line indentation rather than a preceding
    /// directive's indentation. A directive in the leading trivia swallows the preceding end-of-line, so scanning
    /// the leading trivia for the last end-of-line lands on trivia index 0 and returns the directive's line
    /// instead of the attribute list's own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUsesListsOwnLineIndentationRatherThanPrecedingDirectiveIndentation()
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
    /// Verifies that the code fix copies the attribute list's line indentation verbatim rather than canonicalizing
    /// it, when the list already starts its own line at a non-canonical depth
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixPreservesNonCanonicalIndentation()
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
    /// Verifies that the code fix copies a tab-indented line's leading trivia verbatim, rather than expanding it
    /// to spaces, when another declaration precedes the attribute list on that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixPreservesTabIndentationWhenPrecededByOtherSourceOnSameLine()
    {
        const string testData = "internal class Example\n{\n\tprivate int _pad; {|#0:[First]|} internal Example() { }\n}\nsealed class FirstAttribute : System.Attribute\n{\n}\n";
        const string fixedData = "internal class Example\n{\n\tprivate int _pad; [First]\n\tinternal Example() { }\n}\nsealed class FirstAttribute : System.Attribute\n{\n}\n";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5511MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix uses the indentation of the line on which a multi-line attribute list starts,
    /// rather than the line of its closing bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUsesStartLineIndentationForMultiLineAttributeList()
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
    /// Verifies that the code fix uses the declaration's own indentation, rather than the physical line's literal
    /// leading whitespace, when the attribute list's line is itself the continuation of a multi-line comment that
    /// started on an earlier line. That whitespace is the comment's own internal alignment, not code indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUsesDeclarationIndentationWhenLineBeginsInsideMultiLineComment()
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