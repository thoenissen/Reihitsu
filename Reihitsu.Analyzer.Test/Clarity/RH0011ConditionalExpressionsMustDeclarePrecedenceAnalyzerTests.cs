using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer"/> and <see cref="RH0011ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzerTests : AnalyzerTestsBase<RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer, RH0011ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifying mixed logical operators with AND on right side are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }

    /// <summary>
    /// Verifying mixed logical operators with AND on left side are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }

    /// <summary>
    /// Verifying mixed logical operators with AND on both sides are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MixedLogicalOperatorsWithAndOnBothSidesAreReportedAndFixed()
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

        await Verify(testCode, fixedCode, Diagnostics(RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence.", 2));
    }

    /// <summary>
    /// Verifying nested mixed logical operators are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence.", 2));
    }

    /// <summary>
    /// Verifying AND-only expressions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }

    /// <summary>
    /// Verifying mixed or-pattern with and-patterns on left side are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence."));
    }

    /// <summary>
    /// Verifying mixed or-pattern with and-patterns on both sides are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0011ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId, "Conditional expressions must declare precedence.", 2));
    }

    /// <summary>
    /// Verifying and-pattern-only expressions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Members
}