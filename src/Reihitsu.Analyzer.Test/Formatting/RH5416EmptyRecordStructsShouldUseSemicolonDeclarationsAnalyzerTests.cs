using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzerTests : BatchCodeFixTestsBase<RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer, RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty record struct is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5416-001")]
    public async Task VerifyEmptyRecordStructIsDetectedAndFixed()
    {
        const string testData = """
                                internal record struct {|#0:Example|}
                                {
                                }
                                """;
        const string fixedData = """
                                 internal record struct Example;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon record struct declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-002")]
    [Requirement("RH5416-001")]
    public async Task VerifySemicolonRecordStructIsNotFlagged()
    {
        const string testData = """
                                internal record struct Example;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a record struct whose body holds only a conditional directive: it is reported without offering an
    /// unsafe code fix. This test pins today's behavior; the decided behavior is stated by SHARED-DIAG-008, under which a
    /// conditional directive in the body the rule judges keeps the record struct from being reported, and its change
    /// belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal record struct {|#0:Example|}
                                {
                                #if DEBUG
                                #endif
                                }
                                """;
        const string codeFixData = """
                                   internal record struct Example
                                   {
                                   #if DEBUG
                                   #endif
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<RecordDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment between the record struct header and the open brace is reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task VerifyLeadingBraceCommentEmptyRecordStructIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal record struct {|#0:Example|}
                                // why this type is empty
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal record struct Example
                                   // why this type is empty
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<RecordDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies the specification example of the rule: an empty record struct and an empty positional
    /// <see langword="readonly"/> record struct are reported and fixed, while an empty record class is left to RH5415
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5416-001")]
    public async Task VerifySpecificationExampleOfEmptyRecordStructsIsReportedAndFixed()
    {
        const string testData = """
                                record struct {|#0:A|}
                                {
                                }

                                readonly record struct {|#1:B|}(int Value)
                                {
                                }

                                record C
                                {
                                }
                                """;
        const string fixedData = """
                                 record struct A;

                                 readonly record struct B(int Value);

                                 record C
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that under C# 9 the record struct of the specification example is not reported. The source is still
    /// parsed as a record struct declaration, so the language-version check is what keeps it from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5416-001")]
    public async Task VerifyEmptyRecordStructIsNotReportedUnderCSharp9()
    {
        const string testData = """
                                record struct A
                                {
                                }
                                """;

        var root = SyntaxFactory.ParseCompilationUnit(testData, options: new CSharpParseOptions(LanguageVersion.CSharp9));

        Assert.IsNotEmpty(root.DescendantNodes().Where(node => node.IsKind(SyntaxKind.RecordStructDeclaration)), "The source must be parsed as a record struct declaration under C# 9.");

        await Verify(testData,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp9)),
                     DiagnosticResult.CompilerError("CS8773").WithSpan(1, 8, 1, 14).WithArguments("record structs", "10.0"));
    }

    /// <summary>
    /// Verifies that an empty record struct is reported under C# 10, the first version with record structs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5416-001")]
    public async Task VerifyEmptyRecordStructIsReportedUnderCSharp10()
    {
        const string testData = """
                                record struct {|#0:A|}
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp10)),
                     Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat));
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
                                internal record struct {|#0:First|}
                                {
                                }
                                internal record struct {|#1:Second|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal record struct First;
                                 internal record struct Second;
                                 """;

        // The two empty record structs are adjacent with no blank line between them, so the first fix's collapsed
        // replacement span directly abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}