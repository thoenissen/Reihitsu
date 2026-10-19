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
/// Test methods for <see cref="RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzerTests : BatchCodeFixTestsBase<RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer, RH5415EmptyRecordsShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty record is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5415-001")]
    public async Task VerifyEmptyRecordIsDetectedAndFixed()
    {
        const string testData = """
                                internal record {|#0:Example|}
                                {
                                }
                                """;
        const string fixedData = """
                                 internal record Example;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5415MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon record declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-002")]
    [Requirement("RH5415-001")]
    public async Task VerifySemicolonRecordIsNotFlagged()
    {
        const string testData = """
                                internal record Example;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a comment between the record header and the open brace is reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task VerifyLeadingBraceCommentEmptyRecordIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal record {|#0:Example|}
                                // why this type is empty
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal record Example
                                   // why this type is empty
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5415MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<RecordDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies the specification example of the rule: an empty record and an empty positional
    /// <see langword="record"/> <see langword="class"/> are reported and fixed, while an empty record struct is left
    /// to RH5416
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5415-001")]
    public async Task VerifySpecificationExampleOfEmptyRecordsIsReportedAndFixed()
    {
        const string testData = """
                                record {|#0:A|}
                                {
                                }

                                record class {|#1:B|}(int Value)
                                {
                                }

                                record struct C
                                {
                                }
                                """;
        const string fixedData = """
                                 record A;

                                 record class B(int Value);

                                 record struct C
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5415MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that under C# 8 the record of the specification example is not reported. Below C# 9 the compiler does
    /// not parse the source as a record declaration at all, so no record exists that the rule could report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5415-001")]
    public async Task VerifyEmptyRecordIsNotReportedUnderCSharp8()
    {
        const string testData = """
                                record A
                                {
                                }
                                """;

        var root = SyntaxFactory.ParseCompilationUnit(testData, options: new CSharpParseOptions(LanguageVersion.CSharp8));

        Assert.IsEmpty(root.DescendantNodes().OfType<RecordDeclarationSyntax>(), "Under C# 8 the source is not parsed as a record declaration.");

        await Verify(testData,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp8)),
                     DiagnosticResult.CompilerError("CS0246").WithSpan(1, 1, 1, 7).WithArguments("record"),
                     DiagnosticResult.CompilerError("CS0548").WithSpan(1, 8, 1, 9).WithArguments("<invalid-global-code>.A"),
                     DiagnosticResult.CompilerError("CS9348").WithSpan(1, 8, 1, 9));
    }

    /// <summary>
    /// Verifies that an empty record is reported under C# 9, the first version with records
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5415-001")]
    public async Task VerifyEmptyRecordIsReportedUnderCSharp9()
    {
        const string testData = """
                                record {|#0:A|}
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp9)),
                     Diagnostics(RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5415MessageFormat));
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
                                internal record {|#0:First|}
                                {
                                }
                                internal record {|#1:Second|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal record First;
                                 internal record Second;
                                 """;

        // The two empty records are adjacent with no blank line between them, so the first fix's collapsed
        // replacement span directly abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5415MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}