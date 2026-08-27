using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer"/> and <see cref="RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzerTests : AnalyzerTestsBase<RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer, RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(
                                        int first,
                                        int second)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                                {|#0:,|} int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Reproduction test for issue #724: verifies that the code fix aligns the continuation parameter under the
    /// first parameter (right after the opening parenthesis) when the original leading comma was indented one
    /// column past that column, i.e. the issue's literal minimal reproducible example
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssue724OneColumnOffsetIsFixedCorrectly()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                               {|#0:,|}int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Reproduction test for issue #724: verifies that the code fix aligns the continuation parameter under the
    /// first parameter when the original leading comma sat only 4 spaces in, closer to the block's base
    /// indentation, i.e. the issue's third reported example
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssue724BaseIndentationOffsetIsFixedCorrectly()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                {|#0:,|}int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that a leading comma indented past the alignment column is pulled left to it, not merely relieved of
    /// its one-column shortfall (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommaRightOfAlignedColumnIsPulledLeft()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                                        {|#0:,|}int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that when the first parameter already sits on its own line at the same column the leading comma
    /// used, the code fix leaves that (already correct) alignment untouched — the anchor is the first parameter's
    /// own column, not one column past the opening parenthesis (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationStaysAlignedWhenFirstParameterIsOnItsOwnLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(
                                        int first
                                        {|#0:,|}int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(
                                         int first,
                                         int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that when the first parameter sits on its own line, a leading comma closer to the block's base
    /// indentation aligns under the first parameter's own column rather than the opening parenthesis (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyHangingContinuationAlignsWithFirstParameterNotOpeningParenthesis()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(
                                        int first
                                {|#0:,|}int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(
                                         int first,
                                         int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that a multi-space run after the comma is fully collapsed rather than only the first character
    /// (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleSpacesAfterCommaAreCollapsed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                                {|#0:,|}  int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab following the comma is collapsed like any other whitespace, not just the single space
    /// the previous implementation special-cased (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabAfterCommaIsCollapsed()
    {
        const string testData = "internal class TestClass\r\n{\r\n    void Method(int first\r\n                {|#0:,|}\tint second)\r\n    {\r\n    }\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n    void Method(int first,\r\n                int second)\r\n    {\r\n    }\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment immediately following the comma is preserved verbatim and lands at the alignment
    /// column, rather than being hoisted with the comma (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentFollowingCommaIsPreservedAndAligned()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                               {|#0:,|}/* note */ int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 /* note */ int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that several leading commas in the same parameter list all converge to the same alignment column
    /// (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySeveralLeadingCommasInOneListAreAllAligned()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                               {|#0:,|}int second
                                               {|#1:,|}int third)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that Fix All aligns every violating continuation line in one document in a single batch application
    /// (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixAllAlignsEveryContinuationInOneDocument()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First(int first
                                               {|#0:,|}int second)
                                    {
                                    }

                                    void Second(int first
                                                {|#1:,|}int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void First(int first,
                                                int second)
                                     {
                                     }

                                     void Second(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the alignment applies to a constructor's parameter list, not only ordinary methods
    /// (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationAlignsForConstructor()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass(int first
                                             {|#0:,|}int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass(int first,
                                               int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that the alignment applies to a local function's parameter list (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationAlignsForLocalFunction()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        void Local(int first
                                                  {|#0:,|}int second)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         void Local(int first,
                                                    int second)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that the alignment applies to a parenthesized lambda's parameter list (issue #724)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationAlignsForLambdaParameterList()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Action<int, int> action = (int first
                                                                   {|#0:,|}int second) => { };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         Action<int, int> action = (int first,
                                                                    int second) => { };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported and no fix is offered for the conditional-parameter shape from
    /// issue #409, where the comma sits between an <c>#if</c>/<c>#endif</c> pair guarding the next parameter: the
    /// formatter refuses to hoist the comma across the directive boundary, so the analyzer must not flag it (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAndNoCodeFixWhenDirectivesSeparateConditionalParameter()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                #if FEATURE
                                                , int second
                                #endif
                                                )
                                    {
                                    }
                                }
                                """;
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       void Method(int first
                                   #if FEATURE
                                                   , int second
                                   #endif
                                                   )
                                       {
                                       }
                                   }
                                   """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyPreprocessorSymbolToTestProject(solution, projectId, "FEATURE")));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId,
                                                   root => GetFirstSeparatorLocation(root),
                                                   "FEATURE");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported and no fix is offered when the token gap contains a comment, because
    /// the formatter refuses to hoist the comma across that comment (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAndNoCodeFixWhenCommentIsPresent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                                // comment
                                                , int second)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId,
                                                   root => GetFirstSeparatorLocation(root));

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the location of the first separator of the first parameter list of the first method declaration
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The location of the first parameter separator</returns>
    private static Location GetFirstSeparatorLocation(SyntaxNode root)
    {
        var parameterList = root.DescendantNodes()
                                .OfType<MethodDeclarationSyntax>()
                                .First()
                                .ParameterList;

        return parameterList.Parameters.GetSeparator(0).GetLocation();
    }

    /// <summary>
    /// Defines the given preprocessor symbol on the test project's parse options
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <param name="symbol">Preprocessor symbol</param>
    /// <returns>The updated solution</returns>
    private static Solution ApplyPreprocessorSymbolToTestProject(Solution solution, ProjectId projectId, string symbol)
    {
        var project = solution.GetProject(projectId);

        if (project?.ParseOptions is CSharpParseOptions parseOptions)
        {
            solution = solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols(symbol));
        }

        return solution;
    }

    #endregion // Methods
}