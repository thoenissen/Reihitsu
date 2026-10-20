using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5306ListPatternsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer, RH5306ListPatternsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a multiline list pattern with multiple inner patterns on one line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-001")]
    [Requirement("RH5306-002")]
    [Requirement("RH5306-003")]
    [Requirement("RH5306-004")]
    public async Task VerifyDiagnosticAndCodeFixForMultilineListPatternWithMultipleInnerPatternsOnOneLine()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is {|#0:[
                                            1, 2,
                                            3
                                                  ]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(int[] values)
                                     {
                                         return values is [
                                                              1,
                                                              2,
                                                              3
                                                          ];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5306MessageFormat));
    }

    /// <summary>
    /// Verifies that a multiline list pattern with misaligned brackets is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-001")]
    [Requirement("RH5306-003")]
    [Requirement("RH5306-004")]
    public async Task VerifyDiagnosticAndCodeFixForMultilineListPatternWithMisalignedBrackets()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is {|#0:[
                                            1,
                                            .. var rest
                                                  ]|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(int[] values)
                                     {
                                         return values is [
                                                              1,
                                                              .. var rest
                                                          ];
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5306MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line list pattern remains valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-001")]
    public async Task VerifyNoDiagnosticForSingleLineListPattern()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is [1, .. var rest];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly formatted multiline list pattern remains valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-001")]
    [Requirement("RH5306-002")]
    public async Task VerifyNoDiagnosticForCorrectlyFormattedMultilineListPattern()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is [
                                                             1,
                                                             .. var rest
                                                         ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documentation comment does not suppress the diagnostic. The list-pattern phase only
    /// inserts line breaks, it never joins, so it reshapes the pattern without losing the comment and a
    /// documented pattern must be reported exactly like an undocumented one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForListPatternWithDocumentationComment()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is {|#0:[/** doc */ 1,
                                            2]|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5306MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix offered for a documented list pattern preserves the documentation comment. The rule
    /// reports on a documentation comment on purpose, so the formatter-backed fix has to keep it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixForListPatternWithDocumentationCommentPreservesTheComment()
    {
        const string codeFixData = """
                                   internal class Example
                                   {
                                       private static bool Method(int[] values)
                                       {
                                           return values is [/** doc */ 1,
                                               2];
                                       }
                                   }
                                   """;

        var fixedCode = await ApplyCodeFixAsync(codeFixData);

        Assert.Contains("/** doc */", fixedCode);
    }

    /// <summary>
    /// Verifies that a wrapped list pattern whose closing bracket is not in the column of its opening bracket is
    /// reported on the whole list pattern, while a list pattern on one line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-001")]
    [Requirement("RH5306-003")]
    public async Task VerifyWrappedBracketsOutOfColumnAreReportedOnListPattern()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int[] v)
                                    {
                                        var a = v is [1, 2];
                                        var b = v is {|RH5306:[
                                                         1
                                               ]|};

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a wrapped list pattern with an inner pattern on the line of a bracket or of an earlier inner
    /// pattern is reported on the whole list pattern, also when a comment trails the token in front of it, while one
    /// inner pattern per line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-001")]
    [Requirement("RH5306-002")]
    [Requirement("RH5306-003")]
    public async Task VerifyWrappedInnerPatternsThatShareALineAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int[] v)
                                    {
                                        var a = v is {|RH5306:[1,
                                                      2]|};
                                        var b = v is {|RH5306:[
                                                         1, .. var rest
                                                     ]|};
                                        var c = v is // note
                                                {|RH5306:[1,
                                                 2]|};
                                        var d = v is [
                                                         1,
                                                         2
                                                     ];

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix puts every inner pattern on its own line one level deeper than the opening bracket and the
    /// closing bracket in the column of the opening bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-004")]
    public async Task VerifyFixLaysOutListPattern()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int[] v)
                                    {
                                        var a = v is {|RH5306:[1, .. var rest
                                                     ]|};

                                        return a;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     bool M(int[] v)
                                     {
                                         var a = v is [
                                                          1,
                                                          .. var rest
                                                      ];

                                         return a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix puts every inner pattern on its own line one level deeper than the opening bracket and the
    /// closing bracket in the column of the opening bracket, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-004")]
    public async Task VerifyFixLaysOutListPatternWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int[] v)
                                    {
                                        var a = v is {|RH5306:[1, .. var rest
                                                     ]|};

                                        return a;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     bool M(int[] v)
                                     {
                                         var a = v is [
                                                          1,
                                                          .. var rest
                                                      ];

                                         return a;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that a comment behind the token in front of the list pattern does not withhold the fix and stays on its
    /// line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-001")]
    [Requirement("RH5306-004")]
    public async Task VerifyFixLaysOutListPatternBehindTrailingCommentOfPrecedingToken()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int[] v)
                                    {
                                        var c = v is // note
                                                {|RH5306:[1,
                                                 2]|};

                                        return c;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     bool M(int[] v)
                                     {
                                         var c = v is // note
                                         [
                                             1,
                                             2
                                         ];

                                         return c;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a comment behind the token in front of the list pattern does not withhold the fix and stays on its
    /// line, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5306-001")]
    [Requirement("RH5306-004")]
    public async Task VerifyFixLaysOutListPatternBehindTrailingCommentOfPrecedingTokenWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int[] v)
                                    {
                                        var c = v is // note
                                                {|RH5306:[1,
                                                 2]|};

                                        return c;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     bool M(int[] v)
                                     {
                                         var c = v is // note
                                         [
                                             1,
                                             2
                                         ];

                                         return c;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Characterizes the current behavior that a line comment inside a wrapped list pattern keeps it from being
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInsideListPatternKeepsItFromBeingReported()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int[] v)
                                    {
                                        var a = v is [1, // note
                                                      2];

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Characterizes the current behavior that a wrapped list pattern whose inner pattern spans several lines is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineInnerPatternKeepsListPatternFromBeingReported()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int[][] v)
                                    {
                                        var a = v is [{ Length: 1,
                                                        Rank: 1 }, { Length: 2 },
                                                      { Length: 3 }];

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    private static bool Method(int[] values1, int[] values2)
                                    {
                                        return values1 is {|#0:[
                                        1,
                                            2
                                        ]|} && values2 is {|#1:[
                                            3,
                                        4
                                        ]|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     private static bool Method(int[] values1, int[] values2)
                                     {
                                         return values1 is [
                                                               1,
                                                               2
                                                           ] && values2 is [
                                                                               3,
                                                                               4
                                                                           ];
                                     }
                                 }
                                 """;

        // The two list patterns sit side by side in one logical expression, so both resolve to the identical fix
        // target: the enclosing return statement. The batch fixer discards the second, overlapping action, and
        // the surviving single fix already re-anchors both list patterns' brackets in one pass. Nesting a list
        // pattern inside another instead would make the outer pattern's own elements multi-line, and
        // CanSafelyFormat requires every immediate element to be single-line, so that shape would silently
        // withhold the outer diagnostic rather than exercise interference
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5306MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}