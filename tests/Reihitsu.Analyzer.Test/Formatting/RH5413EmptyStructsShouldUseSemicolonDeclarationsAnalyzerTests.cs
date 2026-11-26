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
/// Test methods for <see cref="RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzerTests : BatchCodeFixTestsBase<RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer, RH5413EmptyStructsShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty struct is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5413-001")]
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
                     Diagnostics(RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon struct declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-002")]
    [Requirement("RH5413-001")]
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
    [Requirement("RH5413-001")]
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

    /// <summary>
    /// Verifying that a comment between the struct header and the open brace is reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task VerifyLeadingBraceCommentEmptyStructIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal struct {|#0:Example|}
                                // why this type is empty
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal struct Example
                                   // why this type is empty
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<StructDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies the specification example of the rule: an empty struct and an empty <see langword="readonly"/>
    /// <see langword="ref"/> struct are reported and fixed, while an empty record struct is left to RH5416
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5413-001")]
    public async Task VerifySpecificationExampleOfEmptyStructsIsReportedAndFixed()
    {
        const string testData = """
                                struct {|#0:A|}
                                {
                                }

                                readonly ref struct {|#1:B|}
                                {
                                }

                                record struct C
                                {
                                }
                                """;
        const string fixedData = """
                                 struct A;

                                 readonly ref struct B;

                                 record struct C
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that under C# 11 no struct of the specification example is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5413-001")]
    public async Task VerifySpecificationExampleIsNotReportedUnderCSharp11()
    {
        const string testData = """
                                struct A
                                {
                                }

                                readonly ref struct B
                                {
                                }

                                record struct C
                                {
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies that an empty struct is reported under C# 12, the first version with semicolon struct declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5413-001")]
    public async Task VerifyEmptyStructIsReportedUnderCSharp12()
    {
        const string testData = """
                                struct {|#0:A|}
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp12)),
                     Diagnostics(RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that an empty struct the fix gives a semicolon body, and which shares its line with a field in front of it,
    /// starts on the next line in the column of that line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAStructThatSharesItsLineWithAFieldInFrontALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _x; public struct {|RH5413:A|} { }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int _x;
                                     public struct A;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that an empty struct the fix gives a semicolon body, and which shares its line with a struct behind it,
    /// keeps its own line, and that the struct behind it starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAStructThatSharesItsLineWithAStructBehindItALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                namespace N
                                {
                                    public struct {|RH5413:A|} { } public struct B
                                    {
                                        public int X;
                                    }
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     public struct A;
                                     public struct B
                                     {
                                         public int X;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that an empty union body is not reported as an empty struct, although the compiler emits a union as a
    /// struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyUnionIsNotReportedAsEmptyStruct()
    {
        const string testCode = """
                                union U(int, string)
                                {
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a multi-line and a one-line empty struct followed by a semicolon are reported, and that the fix writes
    /// one semicolon behind each header
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-005")]
    [Requirement("RH5413-001")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task VerifyEmptyStructFollowedByASemicolonIsReportedAndFixed(string endOfLine)
    {
        const string testData = """
                                struct {|#0:B|}
                                {
                                };

                                struct {|#1:C|} { };
                                """;
        const string fixedData = """
                                 struct B;

                                 struct C;
                                 """;

        await Verify(ConvertEndOfLine(testData, endOfLine),
                     ConvertEndOfLine(fixedData, endOfLine),
                     Diagnostics(RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, MessageFormat, 2));
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
                                internal struct {|#0:First|}
                                {
                                }
                                internal struct {|#1:Second|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal struct First;
                                 internal struct Second;
                                 """;

        // The two empty structs are adjacent with no blank line between them, so the first fix's collapsed
        // replacement span directly abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}