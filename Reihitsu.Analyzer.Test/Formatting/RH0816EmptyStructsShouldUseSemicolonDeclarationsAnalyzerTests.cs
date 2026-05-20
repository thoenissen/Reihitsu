using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzerTests : AnalyzerTestsBase<RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer, RH0816EmptyStructsShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty struct is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyStructIsDetectedAndFixed()
    {
        const string testData = """
                                internal struct {|#0:Example|}
                                {
                                }
                                """;
        const string fixedData = """
                                 internal struct Example;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH0816MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon struct declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySemicolonStructIsNotFlagged()
    {
        const string testData = """
                                internal struct Example;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that unsupported language versions are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUnsupportedLanguageVersionIsNotFlagged()
    {
        const string testData = """
                                internal struct Example
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add(ApplyCSharp11ToTestProject));

        static Microsoft.CodeAnalysis.Solution ApplyCSharp11ToTestProject(Microsoft.CodeAnalysis.Solution solution, Microsoft.CodeAnalysis.ProjectId projectId)
        {
            var project = solution.GetProject(projectId);

            if (project?.ParseOptions is CSharpParseOptions parseOptions)
            {
                solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion.CSharp11));
            }

            return solution;
        }
    }

    #endregion // Tests
}