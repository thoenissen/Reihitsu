using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzerTests : AnalyzerTestsBase<RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer, RH5414EmptyInterfacesShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty interface is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyInterfaceIsDetectedAndFixed()
    {
        const string testData = """
                                internal interface {|#0:IExample|}
                                {
                                }
                                """;
        const string fixedData = """
                                 internal interface IExample;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5414MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon interface declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySemicolonInterfaceIsNotFlagged()
    {
        const string testData = """
                                internal interface IExample;
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
                                internal interface IExample
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
    /// Verifying that a comment between the interface header and the open brace is reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLeadingBraceCommentEmptyInterfaceIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal interface {|#0:IExample|}
                                // why this type is empty
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal interface IExample
                                   // why this type is empty
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5414MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<InterfaceDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}