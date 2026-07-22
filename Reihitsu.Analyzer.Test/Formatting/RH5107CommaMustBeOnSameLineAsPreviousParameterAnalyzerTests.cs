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