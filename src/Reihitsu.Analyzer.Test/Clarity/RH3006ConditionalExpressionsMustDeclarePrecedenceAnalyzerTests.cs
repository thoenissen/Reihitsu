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
    /// Characterizes the line endings the fix produces in a CRLF document when the operand starts on the line after
    /// the operator, behind a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfOperandOnTheNextLineInCarriageReturnLineFeedDocument()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b, bool c)
                                    {
                                        return a ||
                                            /* x */ b && c;
                                    }
                                }

                                """;

        const string expectedCode = "class C\r\n"
                                    + "{\r\n"
                                    + "    bool M(bool a, bool b, bool c)\r\n"
                                    + "    {\r\n"
                                    + "        return a ||\n"
                                    + "            /* x */ (b && c);\r\n"
                                    + "    }\r\n"
                                    + "}\r\n";

        var fixedCode = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(expectedCode, fixedCode);
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