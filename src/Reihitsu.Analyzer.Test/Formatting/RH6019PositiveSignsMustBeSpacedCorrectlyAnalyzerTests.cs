using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6019PositiveSignsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzer, RH6019PositiveSignsMustBeSpacedCorrectlyCodeFixProvider>
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
    [Requirement("RH6019-001")]
    [Requirement("RH6019-003")]
    [Requirement("RH6019-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = +{|#0: |}1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = +1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6019MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when the space separates two plus signs, because removing it would glue them into a pre-increment operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-002")]
    public async Task VerifyNoDiagnosticWhenOperandStartsWithPlusSign()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var y = + +x;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when the space separates a plus sign from a pre-increment operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-002")]
    public async Task VerifyNoDiagnosticWhenOperandStartsWithPreIncrement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var y = + ++x;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that spaces, a tab, or a run at the end of the line after a unary plus are reported once per sign,
    /// also before a parenthesized operand and after a binary operator, and that a sign followed by a line break is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-001")]
    [Requirement("RH6019-003")]
    public async Task VerifyWhitespaceAfterUnaryPlusIsReported()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = +{|RH6019: |}1;
                                        var y = +{|RH6019:→|}a;
                                        var u = +{|RH6019: |}(+a);
                                        var t = a + +{|RH6019: |}1;
                                        var r = +{|RH6019:···|}
                                            a;
                                        var q = +
                                            a;
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyWhitespaceAfterUnaryPlusIsReported"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-001")]
    [Requirement("RH6019-003")]
    public async Task VerifyWhitespaceAfterUnaryPlusIsReportedCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = +{|RH6019: |}1;
                                        var y = +{|RH6019:→|}a;
                                        var u = +{|RH6019: |}(+a);
                                        var t = a + +{|RH6019: |}1;
                                        var r = +{|RH6019:···|}
                                            a;
                                        var q = +
                                            a;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that the whitespace after a unary plus whose operand starts with <c>+</c> or <c>++</c> is not reported, however
    /// much whitespace there is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-002")]
    public async Task VerifyWhitespaceThatKeepsTwoPlusSignsApartIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = + +a;
                                        var y = +  +a;
                                        var z = + ++a;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix deletes spaces, a tab, and a run at the end of the line after a unary plus
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterUnaryPlus()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = +{|RH6019: |}1;
                                        var y = +{|RH6019:→|}a;
                                        var r = +{|RH6019:···|}
                                            a;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         var x = +1;
                                         var y = +a;
                                         var r = +
                                             a;
                                     }
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), fixedData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixDeletesWhitespaceAfterUnaryPlus"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterUnaryPlusCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = +{|RH6019: |}1;
                                        var y = +{|RH6019:→|}a;
                                        var r = +{|RH6019:···|}
                                            a;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         var x = +1;
                                         var y = +a;
                                         var r = +
                                             a;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(fixedData));
    }

    /// <summary>
    /// Verifies that a comment on the next line of the gap after a unary plus keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-001")]
    [Requirement("RH6019-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = +{|RH6019:·|}
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
    [Requirement("RH6019-001")]
    [Requirement("RH6019-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLineCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = +{|RH6019:·|}
                                            // the operand
                                            a;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that a directive on the next line of the gap after a unary plus keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6019-001")]
    [Requirement("RH6019-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = +{|RH6019:·|}
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
                                        int first = +{|#0: |}1;
                                        int second = +{|#1: |}2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = +1;
                                         int second = +2;
                                     }
                                 }
                                 """;

        // Two statements on adjacent lines each carry their own unwanted whitespace run after the positive
        // sign; the fixes only remove their own run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6019MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}