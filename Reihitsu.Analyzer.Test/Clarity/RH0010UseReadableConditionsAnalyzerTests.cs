using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0010UseReadableConditionsAnalyzer"/> and <see cref="RH0010UseReadableConditionsCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0010UseReadableConditionsAnalyzerTests : AnalyzerTestsBase<RH0010UseReadableConditionsAnalyzer, RH0010UseReadableConditionsCodeFixProvider>
{
    /// <summary>
    /// Verifying Yoda conditions with less-than operator are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithLessThanIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int count)
                                    {
                                        return 0 {|#0:<|} count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(int count)
                                     {
                                         return count > 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with greater-than operator are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithGreaterThanIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int count)
                                    {
                                        return 100 {|#0:>|} count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(int count)
                                     {
                                         return count < 100;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with equality operator are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithEqualsIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(string input)
                                    {
                                        return null {|#0:==|} input;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(string input)
                                     {
                                         return input == null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with not-equals operator are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithNotEqualsIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int value)
                                    {
                                        return 0 {|#0:!=|} value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(int value)
                                     {
                                         return value != 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with less-than-or-equal operator are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithLessThanOrEqualIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int count)
                                    {
                                        return 5 {|#0:<=|} count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(int count)
                                     {
                                         return count >= 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with greater-than-or-equal operator are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithGreaterThanOrEqualIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int count)
                                    {
                                        return 10 {|#0:>=|} count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(int count)
                                     {
                                         return count <= 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with const fields are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithConstFieldIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private const int MaxValue = 100;

                                    public bool Run(int count)
                                    {
                                        return MaxValue {|#0:>|} count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private const int MaxValue = 100;

                                     public bool Run(int count)
                                     {
                                         return count < MaxValue;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with static readonly fields are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithStaticReadonlyFieldIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private static readonly int MinValue = 0;

                                    public bool Run(int count)
                                    {
                                        return MinValue {|#0:<|} count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private static readonly int MinValue = 0;

                                     public bool Run(int count)
                                     {
                                         return count > MinValue;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with default expressions are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task YodaConditionWithDefaultExpressionIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(string input)
                                    {
                                        return default(string) {|#0:==|} input;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(string input)
                                     {
                                         return input == default(string);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying normal (non-Yoda) conditions are not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task NormalConditionIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int count)
                                    {
                                        return count > 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying comparisons between two constants are not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task ConstantToConstantComparisonIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run()
                                    {
                                        return 5 > 3;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying comparisons between two variables are not reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VariableToVariableComparisonIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int left, int right)
                                    {
                                        return left > right;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying multiple Yoda conditions in the same method are all reported.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task MultipleYodaConditionsAreReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int count, string name)
                                    {
                                        return 0 {|#0:<|} count && null {|#1:!=|} name;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(int count, string name)
                                     {
                                         return count > 0 && name != null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0010UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions.", 2));
    }
}