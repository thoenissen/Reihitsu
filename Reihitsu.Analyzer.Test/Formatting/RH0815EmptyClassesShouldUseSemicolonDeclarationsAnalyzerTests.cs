using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests : AnalyzerTestsBase<RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer, RH0815EmptyClassesShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty class is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyClassIsDetectedAndFixed()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH0815MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon class declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySemicolonClassIsNotFlagged()
    {
        const string testData = """
                                internal class Example;
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
                                internal class Example
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

    /// <summary>
    /// Verifying that commented empty classes are reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedEmptyClassIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                    // Comment
                                }
                                """;
        const string codeFixData = """
                                   internal class Example
                                   {
                                       // Comment
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH0815MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ClassDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}