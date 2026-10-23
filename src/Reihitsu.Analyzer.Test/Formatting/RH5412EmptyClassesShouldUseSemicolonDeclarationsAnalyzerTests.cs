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
/// Test methods for <see cref="RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests : BatchCodeFixTestsBase<RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer, RH5412EmptyClassesShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty class is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("RH5412-001")]
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
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon class declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-002")]
    [Requirement("RH5412-001")]
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
    [Requirement("RH5412-001")]
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
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
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
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ClassDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment between the class header and the open brace is reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task VerifyLeadingBraceCommentEmptyClassIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                // why this type is empty
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal class Example
                                   // why this type is empty
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ClassDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a comment trailing the class header line is reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task VerifyTrailingHeaderCommentEmptyClassIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal class {|#0:Example|} // why this type is empty
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal class Example // why this type is empty
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ClassDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that the fix keeps the blank line between a file-scoped namespace and the converted class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    public async Task VerifyBlankLineAboveFixedClassInFileScopedNamespaceIsKept()
    {
        const string testData = """
                                namespace Sample;

                                internal class {|#0:Example|}
                                {
                                }
                                """;
        const string fixedData = """
                                 namespace Sample;

                                 internal class Example;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifying that the fix inserts the blank line above a comment that directly follows a file-scoped namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted()
    {
        const string testData = """
                                namespace Sample;
                                // Describes the class
                                internal class {|#0:Example|}
                                {
                                }
                                """;
        const string fixedData = """
                                 namespace Sample;

                                 // Describes the class
                                 internal class Example;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of empty bodies: a multi-line and a one-line empty body, a nested empty
    /// class, and a body that holds only a comment are reported on the name, while the class holding the nested
    /// class is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("RH5412-001")]
    public async Task VerifySpecificationExampleOfEmptyBodiesIsReported()
    {
        const string testData = """
                                class {|#0:A|}
                                {
                                }

                                class {|#1:B|} { }

                                class C
                                {
                                    class {|#2:D|}
                                    {
                                    }
                                }

                                class {|#3:E|}
                                {
                                    // reserved
                                }
                                """;

        await Verify(testData, Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that the fix converts an empty class nested in another class and leaves the outer class's braces in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    public async Task VerifyNestedEmptyClassIsReportedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    class {|#0:D|}
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     class D;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of declarations that are not reported: a class with a member and a
    /// semicolon declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-002")]
    public async Task VerifySpecificationExampleOfClassWithMemberAndSemicolonDeclarationIsNotReported()
    {
        const string testData = """
                                class A
                                {
                                    int _x;
                                }

                                class B;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of the fix: the braces are replaced with a semicolon, and a comment behind
    /// the closing brace follows the semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    public async Task VerifySpecificationExampleOfSemicolonDeclarationFix()
    {
        const string testData = """
                                class {|#0:A|}
                                {
                                }

                                class {|#1:B|} { } // note
                                """;
        const string fixedData = """
                                 class A;

                                 class B; // note
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix keeps the CRLF line breaks of the document when it writes the semicolon declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    public async Task VerifySemicolonDeclarationFixWithCarriageReturnLineFeeds()
    {
        const string testData = """
                                class A
                                {
                                }

                                class B
                                {
                                    int _x;
                                }

                                """;
        const string fixedData = """
                                 class A;

                                 class B
                                 {
                                     int _x;
                                 }

                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), actual);
    }

    /// <summary>
    /// Verifies that a comment behind the closing brace of a multi-line empty class follows the semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    public async Task VerifyCommentBehindClosingBraceOfMultiLineClassFollowsSemicolon()
    {
        const string testData = """
                                class {|#0:A|}
                                {
                                } // note
                                """;
        const string fixedData = """
                                 class A; // note
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside the declaration's header does not withhold the fix, since the formatter keeps
    /// the braces only for a comment after the header, around the opening brace, or before the closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task VerifyCommentInsideHeaderDoesNotWithholdFix()
    {
        const string testData = """
                                class /* c */ {|#0:A|}
                                {
                                }
                                """;
        const string fixedData = """
                                 class /* c */ A;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a withheld fix: a comment after the header, a comment on its own line in
    /// front of the opening brace, and a comment inside the body are reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task VerifySpecificationExampleOfWithheldFix()
    {
        const string testData = """
                                class {|#0:A|} // note
                                {
                                }

                                class {|#1:B|}
                                // note
                                {
                                }

                                class {|#2:C|}
                                {
                                    // reserved
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData,
                                       test => test.ExpectedDiagnostics.AddRange(Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat, 3)));
    }

    /// <summary>
    /// Verifies that a comment behind the opening brace is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task VerifyCommentBehindOpeningBraceIsReportedWithoutCodeFix()
    {
        const string testData = """
                                class {|#0:A|}
                                { // note
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData,
                                       test => test.ExpectedDiagnostics.AddRange(Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat)));
    }

    /// <summary>
    /// Verifies that modifiers, a primary constructor, and a base list stay in the header written by the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    public async Task VerifyEmptyPartialClassWithPrimaryConstructorAndBaseListIsFixed()
    {
        const string testData = """
                                using System;

                                internal sealed partial class {|#0:Example|}(int value) : EventArgs
                                {
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal sealed partial class Example(int value) : EventArgs;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of the rule: an empty class and an empty static class are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5412-001")]
    public async Task VerifySpecificationExampleOfEmptyClassesIsReportedAndFixed()
    {
        const string testData = """
                                class {|#0:A|}
                                {
                                }

                                static class {|#1:B|}
                                {
                                }
                                """;
        const string fixedData = """
                                 class A;

                                 static class B;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that under C# 11 neither class of the specification example is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5412-001")]
    [Requirement("SHARED-DIAG-011")]
    public async Task VerifySpecificationExampleIsNotReportedUnderCSharp11()
    {
        const string testData = """
                                class A
                                {
                                }

                                static class B
                                {
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies that an empty class is reported under C# 12, the first version with semicolon class declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5412-001")]
    [Requirement("SHARED-DIAG-011")]
    public async Task VerifyEmptyClassIsReportedUnderCSharp12()
    {
        const string testData = """
                                class {|#0:A|}
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp12)),
                     Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifies a <c>#region</c>/<c>#endregion</c> pair in the body does not keep an empty class from being reported, and
    /// that no code fix is offered, since the fix would rewrite the body that holds the directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("RH5412-001")]
    public async Task VerifyRegionDirectivesInBodyAreReportedWithoutCodeFix()
    {
        const string testData = """
                                class {|#0:A|}
                                {
                                    #region R
                                    #endregion
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData,
                                       test => test.ExpectedDiagnostics.AddRange(Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat)));
    }

    /// <summary>
    /// Verifies a <c>#pragma</c> directive in the body does not keep an empty class from being reported, and that no code
    /// fix is offered, since the fix would rewrite the body that holds the directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("RH5412-001")]
    public async Task VerifyPragmaDirectiveInBodyIsReportedWithoutCodeFix()
    {
        const string testData = """
                                class {|#0:A|}
                                {
                                #pragma warning disable CS0169
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData,
                                       test => test.ExpectedDiagnostics.AddRange(Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat)));
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
                                internal class {|#0:First|}
                                {
                                }
                                internal class {|#1:Second|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal class First;
                                 internal class Second;
                                 """;

        // The two empty classes are adjacent with no blank line between them, so the first fix's collapsed
        // replacement span directly abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}