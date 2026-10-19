using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer, RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                [System.Obsolete]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-001")]
    [Requirement("RH6013-002")]
    [Requirement("RH6013-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                [{|#0: |}System.Obsolete]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6013MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace after an opening attribute bracket is reported once per bracket on every attribute
    /// target, as spaces, a tab, or a run at the end of the line, and that a bracket followed by a line break is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-001")]
    [Requirement("RH6013-002")]
    public async Task VerifyWhitespaceAfterOpeningAttributeBracketIsReportedOnEveryAttributeTarget()
    {
        const string testData = """
                                using System;
                                using System.Runtime.InteropServices;

                                [{|RH6013: |}assembly: CLSCompliant(true)]

                                [{|RH6013: |}Obsolete]
                                class C
                                {
                                    [{|RH6013:→|}Obsolete]
                                    [{|RH6013: |}return: MarshalAs(UnmanagedType.I4)]
                                    int M([{|RH6013: |}In] int a)
                                    {
                                        Func<int> f = [{|RH6013: |}Obsolete] () => 1;

                                        return a;
                                    }

                                    [{|RH6013:··|}
                                        Obsolete]
                                    void N()
                                    {
                                    }

                                    [
                                        Obsolete]
                                    void O()
                                    {
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyWhitespaceAfterOpeningAttributeBracketIsReportedOnEveryAttributeTarget"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-001")]
    [Requirement("RH6013-002")]
    public async Task VerifyWhitespaceAfterOpeningAttributeBracketIsReportedOnEveryAttributeTargetCarriageReturnLineFeed()
    {
        const string testData = """
                                using System;
                                using System.Runtime.InteropServices;

                                [{|RH6013: |}assembly: CLSCompliant(true)]

                                [{|RH6013: |}Obsolete]
                                class C
                                {
                                    [{|RH6013:→|}Obsolete]
                                    [{|RH6013: |}return: MarshalAs(UnmanagedType.I4)]
                                    int M([{|RH6013: |}In] int a)
                                    {
                                        Func<int> f = [{|RH6013: |}Obsolete] () => 1;

                                        return a;
                                    }

                                    [{|RH6013:··|}
                                        Obsolete]
                                    void N()
                                    {
                                    }

                                    [
                                        Obsolete]
                                    void O()
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that the fix deletes spaces, a tab, and a run at the end of the line after an opening attribute bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterOpeningAttributeBracket()
    {
        const string testData = """
                                using System;

                                [{|RH6013: |}Obsolete]
                                class C
                                {
                                    [{|RH6013:→|}Obsolete]
                                    void M()
                                    {
                                    }

                                    [{|RH6013:··|}
                                        Obsolete]
                                    void N()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 [Obsolete]
                                 class C
                                 {
                                     [Obsolete]
                                     void M()
                                     {
                                     }

                                     [
                                         Obsolete]
                                     void N()
                                     {
                                     }
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), fixedData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixDeletesWhitespaceAfterOpeningAttributeBracket"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterOpeningAttributeBracketCarriageReturnLineFeed()
    {
        const string testData = """
                                using System;

                                [{|RH6013: |}Obsolete]
                                class C
                                {
                                    [{|RH6013:→|}Obsolete]
                                    void M()
                                    {
                                    }

                                    [{|RH6013:··|}
                                        Obsolete]
                                    void N()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 [Obsolete]
                                 class C
                                 {
                                     [Obsolete]
                                     void M()
                                     {
                                     }

                                     [
                                         Obsolete]
                                     void N()
                                     {
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(fixedData));
    }

    /// <summary>
    /// Verifies that a comment on the next line of the gap after an opening attribute bracket keeps the diagnostic
    /// and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-001")]
    [Requirement("RH6013-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    [{|RH6013:··|}
                                        // why
                                        Obsolete]
                                    void M()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-001")]
    [Requirement("RH6013-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLineCarriageReturnLineFeed()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    [{|RH6013:··|}
                                        // why
                                        Obsolete]
                                    void M()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that a directive on the next line of the gap after an opening attribute bracket keeps the diagnostic
    /// and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6013-001")]
    [Requirement("RH6013-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    [{|RH6013:··|}
                                #if DEBUG
                                #endif
                                        Obsolete]
                                    void M()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                [{|#0: |}System.Obsolete]
                                [{|#1:	|}System.CLSCompliant(true)]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 [System.CLSCompliant(true)]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        // Two attribute lists sit on adjacent lines with no blank line between them; each fix only removes its
        // own trailing whitespace run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6013MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}