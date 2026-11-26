using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer"/> and <see cref="RH5409FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzerTests : BatchCodeFixTestsBase<RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer, RH5409FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that trailing commas on final enum members are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    [Requirement("SHARED-TRAIL-003")]
    [Requirement("RH5409-001")]
    public async Task VerifyTrailingCommaOnFinalEnumMemberIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    First,
                                    Second{|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     First,
                                     Second
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing comma on a single enum member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    [Requirement("SHARED-TRAIL-003")]
    [Requirement("RH5409-001")]
    public async Task VerifyTrailingCommaOnSingleEnumMemberIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    Value{|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the trailing comma is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-003")]
    public async Task VerifyTrailingCommaWithSameLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    Value{|#0:,|} // Comment
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value // Comment
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment placed before a comma-only line is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-003")]
    public async Task VerifyTrailingCommaOnOwnLineAfterCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    Value
                                    // Comment
                                    {|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value
                                     // Comment
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the enum code fix removes only the trailing comma and does not reformat surrounding code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-003")]
    public async Task VerifyTrailingCommaIsRemovedSurgically()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    Value{|#0:,|} // Comment
                                }

                                internal class Example
                                {
                                    private static void Method(){ }
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value // Comment
                                 }

                                 internal class Example
                                 {
                                     private static void Method(){ }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that multiple trailing commas are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    [Requirement("RH5409-001")]
    public async Task VerifyMultipleEnumsWithTrailingCommasAreDetected()
    {
        const string testData = """
                                internal enum FirstEnum
                                {
                                    Value{|#0:,|}
                                }
                                
                                internal enum SecondEnum
                                {
                                    Other{|#1:,|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that enums without trailing commas are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    public async Task VerifyEnumWithoutTrailingCommaIsNotFlagged()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    First,
                                    Second
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that empty enums are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    public async Task VerifyEmptyEnumIsNotFlagged()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the trailing comma is reported without a code fix when a conditional-compilation block carrying a
    /// further member follows the final active member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyEnumWithConditionalBlockAfterFinalMemberIsReportedWithoutFix()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    First{|RH5409:,|}
                                #if SYMBOL
                                    Second,
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that, where the symbol is defined, the comma of the final member inside the conditional block, in front
    /// of its <c>#endif</c>, is reported and removed by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyCommaOfFinalMemberInsideConditionalBlockIsFixedWhereTheSymbolIsDefined()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    First,
                                #if SYMBOL
                                    Second{|RH5409:,|}
                                #endif
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     First,
                                 #if SYMBOL
                                     Second
                                 #endif
                                 }
                                 """;

        await VerifyFixWithPreprocessorSymbols(testData, fixedData, "SYMBOL");
    }

    /// <summary>
    /// Verifies the specification example of a comma after the last element: only the comma after the last member
    /// is reported, while a comma between two members, an enum without a trailing comma, and an empty enum are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-001")]
    [Requirement("RH5409-001")]
    public async Task VerifySpecificationExampleReportsOnlyTheCommaAfterTheLastMember()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                    B{|#0:,|}
                                }

                                enum F
                                {
                                    A,
                                    B
                                }

                                enum G
                                {
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                     B
                                 }

                                 enum F
                                 {
                                     A,
                                     B
                                 }

                                 enum G
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a comma in front of a directive: the comma in front of a conditional
    /// block and the comma in front of a "#region"/"#endregion" pair are both reported; the fix removes only the comma
    /// in front of the region pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifySpecificationExampleReportsCommaInFrontOfRegionAndInFrontOfConditionalBlock()
    {
        const string testData = """
                                enum E
                                {
                                    A{|RH5409:,|}
                                #if DEBUG
                                    B,
                                #endif
                                }

                                enum F
                                {
                                    A,
                                    B{|#0:,|}
                                #region R
                                #endregion
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A{|RH5409:,|}
                                 #if DEBUG
                                     B,
                                 #endif
                                 }

                                 enum F
                                 {
                                     A,
                                     B
                                 #region R
                                 #endregion
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     },
                     Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of the same enum in a build that defines <c>DEBUG</c>: the comma behind the
    /// last member, in front of the <c>#endif</c>, is reported and the code fix removes it, and the comma behind <c>A</c>
    /// is not a trailing comma in that build
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifySpecificationExampleRemovesCommaInFrontOfEndIfWhereTheSymbolIsDefined()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if DEBUG
                                    B{|RH5409:,|}
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if DEBUG
                                     B
                                 #endif
                                 }
                                 """;

        await VerifyFixWithPreprocessorSymbols(testData, fixedData, "DEBUG");
        await VerifyFixWithPreprocessorSymbols(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), "DEBUG");
    }

    /// <summary>
    /// Verifies the specification example of the comma written inside the branch, in front of the member that needs it:
    /// it is not reported in a build with or without the symbol
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifySpecificationExampleCommaInsideTheBranchIsNotReportedInEitherBuild()
    {
        const string testData = """
                                enum E
                                {
                                    A
                                #if DEBUG
                                    , B
                                #endif
                                }
                                """;

        await Verify(testData);
        await VerifyWithPreprocessorSymbols(testData, "DEBUG");
    }

    /// <summary>
    /// Verifies that the comma behind the last member of an active conditional block, in front of its <c>#endif</c>, is
    /// reported and removed by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedAndFixed()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !NEVER
                                    B{|RH5409:,|}
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if !NEVER
                                     B
                                 #endif
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies the specification example of a comma in front of an <c>#endif</c> behind an active <c>#else</c> branch:
    /// the code fix removes it and leaves the comma in the excluded branch as written, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifySpecificationExampleRemovesCommaAtTheEndOfAnActiveElseBranch()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if X
                                    B,
                                #else
                                    C{|RH5409:,|}
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if X
                                     B,
                                 #else
                                     C
                                 #endif
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies the specification example of a comma in front of an <c>#elif</c>: the code fix removes it and leaves the
    /// <c>#elif</c> branch, code the analyzed build excludes, as written, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifySpecificationExampleRemovesCommaInFrontOfElif()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|}
                                #elif Y
                                    C,
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if !X
                                     B
                                 #elif Y
                                     C,
                                 #endif
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a comma in front of an <c>#elif</c> and an <c>#else</c> of one group, with no <c>#if</c> behind it,
    /// is reported and removed by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCommaInFrontOfElifAndElseBranchesWithoutIfIsReportedAndFixed()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|}
                                #elif Y
                                    C,
                                #else
                                    D,
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if !X
                                     B
                                 #elif Y
                                     C,
                                 #else
                                     D,
                                 #endif
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies the change's example of Fix All: the comma in front of an <c>#elif</c> in one enum and the comma in front
    /// of an <c>#endif</c> behind an active <c>#else</c> branch in another are both removed in one pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyFixAllRemovesCommasAtTheEndOfActiveBranches()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|}
                                #elif Y
                                    C,
                                #endif
                                }

                                enum F
                                {
                                    A,
                                #if X
                                    B,
                                #else
                                    C{|RH5409:,|}
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if !X
                                     B
                                 #elif Y
                                     C,
                                 #endif
                                 }

                                 enum F
                                 {
                                     A,
                                 #if X
                                     B,
                                 #else
                                     C
                                 #endif
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that Fix All removes the comma in front of an <c>#endif</c> and leaves the comma in front of an <c>#if</c>
    /// in another enum of the same document reported and in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyFixAllRemovesCommaInFrontOfEndIfAndKeepsCommaInFrontOfIf()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|}
                                #endif
                                }

                                enum F
                                {
                                    A{|RH5409:,|}
                                #if X
                                    B,
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if !X
                                     B
                                 #endif
                                 }

                                 enum F
                                 {
                                     A{|RH5409:,|}
                                 #if X
                                     B,
                                 #endif
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies the specification example of a comma in front of an <c>#endif</c> and a further <c>#if</c>: the comma is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    public async Task VerifySpecificationExampleCommaInFrontOfEndIfAndFurtherIfIsReportedWithoutFix()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|}
                                #endif
                                #if Y
                                    C,
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of an <c>#if</c> in code an <c>#else</c> excludes: the comma in front of the
    /// <c>#else</c> is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    public async Task VerifySpecificationExampleIfInExcludedCodeWithholdsTheFix()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|}
                                #else
                                #if Y
                                    C,
                                #endif
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that an <c>#if</c> in code an <c>#elif</c> excludes withholds the fix for the comma in front of the
    /// <c>#elif</c>, and the comma is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    public async Task VerifyIfInCodeAnElifExcludesWithholdsTheFix()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|}
                                #elif Y
                                #if Z
                                    C,
                                #endif
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comma inside two active conditional blocks, in front of both <c>#endif</c> directives, is reported
    /// and removed by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCommaInFrontOfTwoNestedEndIfDirectivesIsReportedAndFixed()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                #if !Y
                                    B{|RH5409:,|}
                                #endif
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if !X
                                 #if !Y
                                     B
                                 #endif
                                 #endif
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a comma in front of an <c>#endif</c> followed by a region pair is reported and removed by the code fix,
    /// which keeps every directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCommaInFrontOfEndIfAndRegionPairIsReportedAndFixed()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|}
                                #endif
                                #region R
                                #endregion
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if !X
                                     B
                                 #endif
                                 #region R
                                 #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a comma on a line of its own inside an active conditional block, in front of its <c>#endif</c>, is
    /// reported and that the code fix removes the comma-only line and keeps both directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCommaOnOwnLineInFrontOfEndIfIsReportedAndFixed()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                    B
                                #if !X
                                    {|RH5409:,|}
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                     B
                                 #if !X
                                 #endif
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that removing a comma in front of an <c>#endif</c> keeps the comment behind the comma on the line of the
    /// last member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCommaWithCommentInFrontOfEndIfIsReportedAndFixed()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                #if !X
                                    B{|RH5409:,|} // the last
                                #endif
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                 #if !X
                                     B // the last
                                 #endif
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that Fix All removes the trailing commas in front of the closing brace and in front of a region pair and
    /// leaves the comma in front of a conditional block reported and in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-TRAIL-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock()
    {
        const string testData = """
                                enum E
                                {
                                    A{|RH5409:,|}
                                #if DEBUG
                                    B,
                                #endif
                                }

                                enum F
                                {
                                    A,
                                    B{|RH5409:,|}
                                }

                                enum G
                                {
                                    A{|RH5409:,|}
                                #region R
                                #endregion
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A{|RH5409:,|}
                                 #if DEBUG
                                     B,
                                 #endif
                                 }

                                 enum F
                                 {
                                     A,
                                     B
                                 }

                                 enum G
                                 {
                                     A
                                 #region R
                                 #endregion
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that "#pragma" directives between the comma and the closing brace do not keep the comma from being
    /// reported, and that the fix keeps both directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-TRAIL-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyCommaInFrontOfPragmaDirectivesIsReportedAndFixed()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                    B{|#0:,|}
                                #pragma warning disable CS0168
                                #pragma warning restore CS0168
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                     B
                                 #pragma warning disable CS0168
                                 #pragma warning restore CS0168
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a conditional directive pair after the comma withholds the fix, and the comma is still reported,
    /// even when the pair excludes no code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyCommaInFrontOfEmptyConditionalBlockIsReportedWithoutFix()
    {
        const string testData = """
                                enum E
                                {
                                    A{|RH5409:,|}
                                #if SYMBOL
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that "#else" and "#elif" branches after the comma withhold the fix, and the comma is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyCommaInFrontOfElseAndElifBranchesIsReportedWithoutFix()
    {
        const string testData = """
                                enum E
                                {
                                    A{|RH5409:,|}
                                #if SYMBOL
                                    B,
                                #elif OTHER
                                    C,
                                #else
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional block in front of the comma does not keep it from being reported, since the
    /// block does not lie between the comma and the closing brace, and that the fix removes the comma-only line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-TRAIL-003")]
    public async Task VerifyCommaBehindConditionalBlockIsReportedAndFixed()
    {
        const string testData = """
                                enum E
                                {
                                    A
                                #if SYMBOL
                                #endif
                                    {|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A
                                 #if SYMBOL
                                 #endif
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of the fix: a comment behind the comma stays on its line, and a line that
    /// holds nothing but the comma is removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-003")]
    public async Task VerifySpecificationExampleOfCommaRemoval()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                    B{|#0:,|} // last
                                }

                                enum F
                                {
                                    A
                                    // last
                                    {|#1:,|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                     B // last
                                 }

                                 enum F
                                 {
                                     A
                                     // last
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix removes a line that holds nothing but the comma together with its CRLF line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-003")]
    public async Task VerifyCommaOnlyLineIsRemovedWithCarriageReturnLineFeeds()
    {
        const string testData = """
                                enum F
                                {
                                    A
                                    // last
                                    ,
                                }

                                """;
        const string fixedData = """
                                 enum F
                                 {
                                     A
                                     // last
                                 }

                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), actual);
    }

    /// <summary>
    /// Verifies the specification example of enum members with values and attributes, and of an enum written on one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5409-001")]
    public async Task VerifySpecificationExampleOfMembersWithValuesAttributesAndOneLineEnum()
    {
        const string testData = """
                                using System;

                                enum E
                                {
                                    A = 1,
                                    [Obsolete]
                                    B = 2{|#0:,|}
                                }

                                enum F { A{|#1:,|} }
                                """;
        const string fixedData = """
                                 using System;

                                 enum E
                                 {
                                     A = 1,
                                     [Obsolete]
                                     B = 2
                                 }

                                 enum F { A }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a fully inactive <c>#if false</c> block after the comma, whose excluded code lies in the gap the rule
    /// judges, withholds the fix, and the comma is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-TRAIL-002")]
    public async Task VerifyCommaInFrontOfInactiveIfFalseBlockIsReportedWithoutFix()
    {
        const string testData = """
                                enum E
                                {
                                    A{|RH5409:,|}
                                #if false
                                    B,
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that removing a comma behind a space leaves no whitespace at the end of the line, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task FixLeavesNoTrailingWhitespaceWhereTheCommaStoodBehindASpace()
    {
        const string testCode = """
                                enum E
                                {
                                    A,
                                    B {|RH5409:,|}
                                }
                                """;
        const string fixedCode = """
                                 enum E
                                 {
                                     A,
                                     B
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
        AssertFormattingChangesNothing(fixedCode);
    }

    /// <summary>
    /// Verifies that removing a comma that starts the line of the closing brace leaves the brace in the column the
    /// formatter gives it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task FixPutsTheClosingBraceBehindARemovedCommaInItsColumn()
    {
        const string testCode = """
                                enum E
                                {
                                    A,
                                    B
                                    {|RH5409:,|} }
                                """;
        const string fixedCode = """
                                 enum E
                                 {
                                     A,
                                     B
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
        AssertFormattingChangesNothing(fixedCode);
    }

    /// <summary>
    /// Verifies that removing a comma between a space and a trailing line comment leaves the spaces on both sides of it in front of
    /// the comment, as the formatter does, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task FixKeepsTheGapsAroundTheRemovedCommaInFrontOfATrailingLineComment()
    {
        const string testCode = """
                                enum E
                                {
                                    A,
                                    B {|RH5409:,|} // c
                                }
                                """;
        const string fixedCode = """
                                 enum E
                                 {
                                     A,
                                     B  // c
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
        AssertFormattingChangesNothing(fixedCode);
    }

    /// <summary>
    /// Verifies that removing a comma that a trailing line comment follows directly leaves the space in front of the comma in front
    /// of the comment, as the formatter does, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task FixKeepsTheGapInFrontOfATrailingLineCommentDirectlyBehindTheRemovedComma()
    {
        const string testCode = """
                                enum E
                                {
                                    A,
                                    B {|RH5409:,|}// c
                                }
                                """;
        const string fixedCode = """
                                 enum E
                                 {
                                     A,
                                     B // c
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
        AssertFormattingChangesNothing(fixedCode);
    }

    /// <summary>
    /// Verifies that removing a comma between a space and a trailing block comment leaves the spaces on both sides of it in front of
    /// the comment, as the formatter does, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task FixKeepsTheGapsAroundTheRemovedCommaInFrontOfATrailingBlockComment()
    {
        const string testCode = """
                                enum E
                                {
                                    A,
                                    B {|RH5409:,|} /* c */
                                }
                                """;
        const string fixedCode = """
                                 enum E
                                 {
                                     A,
                                     B  /* c */
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
        AssertFormattingChangesNothing(fixedCode);
    }

    /// <summary>
    /// Verifies that removing a comma that ends its line behind a block comment leaves the comment at the end of the line with no
    /// whitespace behind it, as the formatter does, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task FixLeavesNoTrailingWhitespaceWhereTheCommaStoodBehindABlockComment()
    {
        const string testCode = """
                                enum E
                                {
                                    A,
                                    B /* c */ {|RH5409:,|}
                                }
                                """;
        const string fixedCode = """
                                 enum E
                                 {
                                     A,
                                     B /* c */
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testCode, fixedCode);
        AssertFormattingChangesNothing(fixedCode);
    }

    /// <summary>
    /// Verifies the diagnostics marked in the source and the fix result in a build that defines the given preprocessor
    /// symbols
    /// </summary>
    /// <param name="testCode">The source with markup</param>
    /// <param name="fixedCode">The fixed source</param>
    /// <param name="preprocessorSymbols">Preprocessor symbols the build defines</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFixWithPreprocessorSymbols(string testCode, string fixedCode, params string[] preprocessorSymbols)
    {
        await Verify(testCode,
                     fixedCode,
                     test => test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                      ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols(preprocessorSymbols))
                                                                                      : solution));
    }

    /// <summary>
    /// Verifies the diagnostics marked in the source and the fix result under LF and under CRLF
    /// </summary>
    /// <param name="testCode">The source with markup and LF line endings</param>
    /// <param name="fixedCode">The fixed source with LF line endings</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyUnderBothLineEndings(string testCode, string fixedCode)
    {
        await Verify(testCode, fixedCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal enum FirstEnum
                                {
                                    Value{|#0:,|}
                                }

                                internal enum SecondEnum
                                {
                                    Other{|#1:,|}
                                }
                                """;

        const string fixedCode = """
                                 internal enum FirstEnum
                                 {
                                     Value
                                 }

                                 internal enum SecondEnum
                                 {
                                     Other
                                 }
                                 """;

        // The rule reports at most one trailing comma per enum (its final member), so a shared owner between two
        // occurrences is unreachable; two independent enum declarations exercise the batch fixer applying two
        // genuinely separate, surgical comma removals
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}