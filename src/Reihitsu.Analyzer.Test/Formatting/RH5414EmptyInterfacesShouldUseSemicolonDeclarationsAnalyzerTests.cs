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
/// Test methods for <see cref="RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzerTests : BatchCodeFixTestsBase<RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer, RH5414EmptyInterfacesShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty interface is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5414-001")]
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
    [Requirement("SHARED-EMPTY-002")]
    [Requirement("RH5414-001")]
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
    [Requirement("RH5414-001")]
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
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
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

    /// <summary>
    /// Verifies the specification example of the rule: an empty interface and an empty interface that inherits
    /// another interface are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5414-001")]
    public async Task VerifySpecificationExampleOfEmptyInterfacesIsReportedAndFixed()
    {
        const string testData = """
                                interface {|#0:I|}
                                {
                                }

                                interface {|#1:J|} : I
                                {
                                }
                                """;
        const string fixedData = """
                                 interface I;

                                 interface J : I;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5414MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that under C# 11 neither interface of the specification example is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5414-001")]
    public async Task VerifySpecificationExampleIsNotReportedUnderCSharp11()
    {
        const string testData = """
                                interface I
                                {
                                }

                                interface J : I
                                {
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies that an empty interface is reported under C# 12, the first version with semicolon interface declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5414-001")]
    public async Task VerifyEmptyInterfaceIsReportedUnderCSharp12()
    {
        const string testData = """
                                interface {|#0:I|}
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp12)),
                     Diagnostics(RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5414MessageFormat));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Applies the given C# language version to the test project parse options
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <param name="languageVersion">Language version</param>
    /// <returns>Solution</returns>
    private static Solution ApplyLanguageVersion(Solution solution, ProjectId projectId, LanguageVersion languageVersion)
    {
        var project = solution.GetProject(projectId);

        if (project?.ParseOptions is CSharpParseOptions parseOptions)
        {
            solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(languageVersion));
        }

        return solution;
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal interface {|#0:IFirst|}
                                {
                                }
                                internal interface {|#1:ISecond|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal interface IFirst;
                                 internal interface ISecond;
                                 """;

        // The two empty interfaces are adjacent with no blank line between them, so the first fix's collapsed
        // replacement span directly abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5414MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}