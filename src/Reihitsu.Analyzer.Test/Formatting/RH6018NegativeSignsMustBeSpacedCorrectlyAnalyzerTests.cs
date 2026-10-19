using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6018NegativeSignsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer, RH6018NegativeSignsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-001")]
    [Requirement("RH6018-003")]
    [Requirement("RH6018-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = -{|#0: |}1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = -1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6018MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when the space separates two minus signs, because removing it would glue them into a pre-decrement operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-002")]
    public async Task VerifyNoDiagnosticWhenOperandStartsWithMinusSign()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var y = - -x;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when the space separates a minus sign from a pre-decrement operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-002")]
    public async Task VerifyNoDiagnosticWhenOperandStartsWithPreDecrement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var y = - --x;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that spaces, a tab, or a run at the end of the line after a unary minus are reported once per sign,
    /// also before a parenthesized operand, after a binary operator, and inside an interpolation hole, and that a sign
    /// followed by a line break is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-001")]
    [Requirement("RH6018-003")]
    public async Task VerifyWhitespaceAfterUnaryMinusIsReported()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = -{|RH6018: |}1;
                                        var y = -{|RH6018:→|}a;
                                        var u = -{|RH6018: |}(-a);
                                        var t = a - -{|RH6018: |}1;
                                        var s = $"{-{|RH6018: |}a}";
                                        var r = -{|RH6018:···|}
                                            a;
                                        var q = -
                                            a;
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyWhitespaceAfterUnaryMinusIsReported"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-001")]
    [Requirement("RH6018-003")]
    public async Task VerifyWhitespaceAfterUnaryMinusIsReportedCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = -{|RH6018: |}1;
                                        var y = -{|RH6018:→|}a;
                                        var u = -{|RH6018: |}(-a);
                                        var t = a - -{|RH6018: |}1;
                                        var s = $"{-{|RH6018: |}a}";
                                        var r = -{|RH6018:···|}
                                            a;
                                        var q = -
                                            a;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that the whitespace after a unary minus whose operand starts with <c>-</c> or <c>--</c> is not reported, however
    /// much whitespace there is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-002")]
    public async Task VerifyWhitespaceThatKeepsTwoMinusSignsApartIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = - -a;
                                        var y = -  -a;
                                        var z = - --a;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix deletes spaces, a tab, and a run at the end of the line after a unary minus
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterUnaryMinus()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = -{|RH6018: |}1;
                                        var y = -{|RH6018:→|}a;
                                        var u = -{|RH6018: |}(-a);
                                        var r = -{|RH6018:···|}
                                            a;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         var x = -1;
                                         var y = -a;
                                         var u = -(-a);
                                         var r = -
                                             a;
                                     }
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), fixedData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixDeletesWhitespaceAfterUnaryMinus"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterUnaryMinusCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = -{|RH6018: |}1;
                                        var y = -{|RH6018:→|}a;
                                        var u = -{|RH6018: |}(-a);
                                        var r = -{|RH6018:···|}
                                            a;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         var x = -1;
                                         var y = -a;
                                         var u = -(-a);
                                         var r = -
                                             a;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(fixedData));
    }

    /// <summary>
    /// Verifies that a comment on the next line of the gap after a unary minus keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-001")]
    [Requirement("RH6018-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = -{|RH6018:·|}
                                            // the operand
                                            a;
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
    [Requirement("RH6018-001")]
    [Requirement("RH6018-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLineCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = -{|RH6018:·|}
                                            // the operand
                                            a;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that a directive on the next line of the gap after a unary minus keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6018-001")]
    [Requirement("RH6018-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = -{|RH6018:·|}
                                #if DEBUG
                                #endif
                                            a;
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
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = -{|#0: |}1;
                                        int second = -{|#1: |}2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = -1;
                                         int second = -2;
                                     }
                                 }
                                 """;

        // Two statements on adjacent lines each carry their own unwanted whitespace run after the negative
        // sign; the fixes only remove their own run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6018MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}