using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer"/> and <see cref="RH3006ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzerTests : BatchCodeFixTestsBase<RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer, RH3006ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying mixed logical operators with AND on right side are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    [Requirement("RH3006-002")]
    [Requirement("RH3006-003")]
    public async Task MixedLogicalOperatorsWithAndOnRightAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool left, bool middle, bool right)
                                    {
                                        return left || {|#0:middle && right|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(bool left, bool middle, bool right)
                                     {
                                         return left || (middle && right);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }

    /// <summary>
    /// Verifying mixed logical operators with AND on left side are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    [Requirement("RH3006-002")]
    [Requirement("RH3006-003")]
    public async Task MixedLogicalOperatorsWithAndOnLeftAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool left, bool middle, bool right)
                                    {
                                        return {|#0:left && middle|} || right;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(bool left, bool middle, bool right)
                                     {
                                         return (left && middle) || right;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }

    /// <summary>
    /// Verifying nested mixed logical operators are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    [Requirement("RH3006-002")]
    [Requirement("RH3006-003")]
    public async Task NestedMixedLogicalOperatorsAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool a, bool b, bool c, bool d, bool e)
                                    {
                                        return a || ({|#0:b && c|} || {|#1:d && e|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(bool a, bool b, bool c, bool d, bool e)
                                     {
                                         return a || ((b && c) || (d && e));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence.", 2));
    }

    /// <summary>
    /// Verifying AND-only expressions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    public async Task AndOnlyExpressionIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool a, bool b, bool c)
                                    {
                                        return a && b && c;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying OR-only expressions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    public async Task OrOnlyExpressionIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool a, bool b, bool c)
                                    {
                                        return a || b || c;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying already parenthesized mixed expressions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    public async Task ParenthesizedMixedExpressionIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool left, bool middle, bool right)
                                    {
                                        return left || (middle && right);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying mixed or-pattern with and-patterns on right side are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    [Requirement("RH3006-002")]
    [Requirement("RH3006-003")]
    public async Task MixedOrPatternWithAndOnRightIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool CheckValue(object obj)
                                    {
                                        return obj is string or {|#0:int and > 0|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool CheckValue(object obj)
                                     {
                                         return obj is string or (int and > 0);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }

    /// <summary>
    /// Verifying mixed or-pattern with and-patterns on left side are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    [Requirement("RH3006-002")]
    [Requirement("RH3006-003")]
    public async Task MixedOrPatternWithAndOnLeftIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool CheckValue(object obj)
                                    {
                                        return obj is {|#0:int and > 0|} or string;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool CheckValue(object obj)
                                     {
                                         return obj is (int and > 0) or string;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }

    /// <summary>
    /// Verifying mixed or-pattern with and-patterns on both sides are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    [Requirement("RH3006-002")]
    [Requirement("RH3006-003")]
    public async Task MixedOrPatternWithAndOnBothSidesIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool CheckValue(object obj)
                                    {
                                        return obj is {|#0:int and > 0|} or {|#1:long and < 100|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool CheckValue(object obj)
                                     {
                                         return obj is (int and > 0) or (long and < 100);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence.", 2));
    }

    /// <summary>
    /// Verifying and-pattern-only expressions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    public async Task AndPatternOnlyIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool CheckValue(object obj)
                                    {
                                        return obj is int and > 0 and < 100;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying or-pattern-only expressions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    public async Task OrPatternOnlyIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool CheckValue(object obj)
                                    {
                                        return obj is int or long or double;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies <c>&amp;&amp;</c> operands of <c>||</c> and <c>and</c> operands of <c>or</c> are reported over the
    /// whole operand, while parenthesized operands and expressions with only one of the operators are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-001")]
    [Requirement("RH3006-002")]
    public async Task MixedOperandsAreReportedOverTheWholeOperand()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c, bool d, int x)
                                    {
                                        var p = a || {|RH3006:b && c|};
                                        var q = {|RH3006:a && b|} || {|RH3006:c && d|};
                                        var r = x is {|RH3006:> 0 and < 10|} or 20;
                                        var s = a || (b && c);
                                        var t = a && b && c;
                                        return a || b || c;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c, bool d, int x)
                                     {
                                         var p = a || (b && c);
                                         var q = (a && b) || (c && d);
                                         var r = x is (> 0 and < 10) or 20;
                                         var s = a || (b && c);
                                         var t = a && b && c;
                                         return a || b || c;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix puts the operand in parentheses and keeps a comment in front of it outside the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3006-003")]
    public async Task FixKeepsTheCommentInFrontOfTheOperandOutsideTheParentheses()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c, bool d, int x)
                                    {
                                        var p = a || /* x */ {|RH3006:b && c|};
                                        var q = {|RH3006:a && b|} || {|RH3006:c && d|};
                                        return x is {|RH3006:> 0 and < 10|} or 20;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c, bool d, int x)
                                     {
                                         var p = a || /* x */ (b && c);
                                         var q = (a && b) || (c && d);
                                         return x is (> 0 and < 10) or 20;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix of an operand on the line after the operator, behind a block comment, keeps the line break that
    /// ends the operator's line as it was, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3006-003")]
    public async Task FixOfCommentedOperandOnTheNextLineKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        return a ||
                                            /* x */ {|RH3006:b && c|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c)
                                     {
                                         return a ||
                                             /* x */ (b && c);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of an operand on the line after the operator keeps the line break in front of it when no
    /// comment leads it, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixOfOperandOnTheNextLineWithoutACommentKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        return a ||
                                            {|RH3006:b && c|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c)
                                     {
                                         return a ||
                                             (b && c);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of an operand led by a line comment on its own line keeps both line breaks in front of the
    /// operand, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3006-003")]
    public async Task FixOfOperandBehindALineCommentOnItsOwnLineKeepsTheLineBreaks(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        return a ||
                                            // x
                                            {|RH3006:b && c|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c)
                                     {
                                         return a ||
                                             // x
                                             (b && c);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of an operand behind a blank line and a comment writes the line break of the blank line, which
    /// lies in the operand's own leading trivia, as the document's line ending, and keeps the line break in front of it,
    /// under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-FIX-008")]
    [Requirement("RH3006-003")]
    public async Task FixOfOperandBehindABlankLineAndACommentWritesTheDocumentsLineEnding(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        return a ||

                                            /* x */ {|RH3006:b && c|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c)
                                     {
                                         return a ||

                                             /* x */ (b && c);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a commented operand on the next line keeps the line breaks when the semicolon stands on a line
    /// of its own, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixOfCommentedOperandOnTheNextLineKeepsTheLineBreakInFrontOfTheSemicolonLine(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        return a ||
                                            /* x */ {|RH3006:b && c|}
                                            ;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c)
                                     {
                                         return a ||
                                             /* x */ (b && c)
                                             ;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a commented left operand on the line after <c>return</c> keeps the line break in front of it,
    /// under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3006-003")]
    public async Task FixOfCommentedLeftOperandOnTheNextLineKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        return
                                            /* x */ {|RH3006:a && b|} || c;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c)
                                     {
                                         return
                                             /* x */ (a && b) || c;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a commented left operand of a whole argument on the line after <c>(</c> keeps the line break
    /// in front of it, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3006-003")]
    public async Task FixOfCommentedOperandOfAnArgumentOnTheNextLineKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void N(bool v)
                                    {
                                    }

                                    void M(bool a, bool b, bool c)
                                    {
                                        N(
                                            /* x */ {|RH3006:a && b|} || c);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void N(bool v)
                                     {
                                     }

                                     void M(bool a, bool b, bool c)
                                     {
                                         N(
                                             /* x */ (a && b) || c);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a commented pattern operand on the line after <c>or</c> keeps the line break in front of it,
    /// under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3006-003")]
    public async Task FixOfCommentedPatternOperandOnTheNextLineKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int i)
                                    {
                                        return i is 1 or
                                            /* x */ {|RH3006:> 2 and < 5|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(int i)
                                     {
                                         return i is 1 or
                                             /* x */ (> 2 and < 5);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of an operand behind a comment on the operator's line keeps the line break behind the comment,
    /// under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixOfOperandBehindALineCommentOnTheOperatorsLineKeepsTheLineBreak(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        return a || // x
                                            {|RH3006:b && c|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c)
                                     {
                                         return a || // x
                                             (b && c);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies Fix All, and the single fix applied to one operand after the other, over two operands that are each led by
    /// a comment on their own line keep every line break as it was, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3006-003")]
    public async Task FixAllOfTwoCommentedOperandsOnTheirOwnLinesKeepsTheLineBreaks(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c, bool d)
                                    {
                                        return
                                            // first
                                            {|RH3006:a && b|} ||
                                            // second
                                            {|RH3006:c && d|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b, bool c, bool d)
                                     {
                                         return
                                             // first
                                             (a && b) ||
                                             // second
                                             (c && d);
                                     }
                                 }
                                 """;

        var expectedCode = ConvertEndOfLine(fixedCode, endOfLine);

        await Verify(ConvertEndOfLine(testCode, endOfLine), expectedCode);

        var onceFixedCode = await ApplyCodeFixAsync(ConvertEndOfLine(RemoveMarkup(testCode), endOfLine));

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(onceFixedCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool a, bool b, bool c, bool d)
                                    {
                                        return {|#0:a && b|} || {|#1:c && d|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(bool a, bool b, bool c, bool d)
                                     {
                                         return (a && b) || (c && d);
                                     }
                                 }
                                 """;

        // Both operands of the same OR expression are reported, so the two rewrites replace siblings of one parent node
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence.", 2));
    }

    #endregion // BatchCodeFixTestsBase
}