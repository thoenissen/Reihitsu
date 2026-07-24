using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3005UseReadableConditionsAnalyzer"/> and <see cref="RH3005UseReadableConditionsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3005UseReadableConditionsAnalyzerTests : AnalyzerTestsBase<RH3005UseReadableConditionsAnalyzer, RH3005UseReadableConditionsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying Yoda conditions with less-than operator are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with greater-than operator are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with equality operator are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with not-equals operator are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with less-than-or-equal operator are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with greater-than-or-equal operator are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with const fields are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with static readonly fields are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying Yoda conditions with default expressions are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions."));
    }

    /// <summary>
    /// Verifying normal (non-Yoda) conditions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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
    /// Verifying comparisons between two constants are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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
    /// Verifying comparisons between two variables are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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
    /// Verifying multiple Yoda conditions in the same method are all reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions.", 2));
    }

    /// <summary>
    /// Verifying that no fix is offered when swapping the operands would require a user-defined operator overload that
    /// does not exist, which would produce non-compiling output
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AsymmetricUserDefinedOperatorWithoutMirrorIsNotFixed()
    {
        const string testCode = """
                                public class MyType
                                {
                                    public static bool operator <(int left, MyType right)
                                    {
                                        return true;
                                    }

                                    public static bool operator >(int left, MyType right)
                                    {
                                        return false;
                                    }
                                }

                                public class Test
                                {
                                    public bool Run(MyType value)
                                    {
                                        return 5 < value;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode, RH3005UseReadableConditionsAnalyzer.DiagnosticId, FirstComparisonOperatorLocation);

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that no fix is offered when the mirrored user-defined operator exists but is not the mirror of the
    /// original, which would silently change behavior
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AsymmetricUserDefinedOperatorWithUnrelatedMirrorIsNotFixed()
    {
        const string testCode = """
                                public class MyType
                                {
                                    public static bool operator <(int left, MyType right)
                                    {
                                        return true;
                                    }

                                    public static bool operator >(int left, MyType right)
                                    {
                                        return false;
                                    }

                                    public static bool operator <(MyType left, int right)
                                    {
                                        return false;
                                    }

                                    public static bool operator >(MyType left, int right)
                                    {
                                        return true;
                                    }
                                }

                                public class Test
                                {
                                    public bool Run(MyType value)
                                    {
                                        return 5 < value;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode, RH3005UseReadableConditionsAnalyzer.DiagnosticId, FirstComparisonOperatorLocation);

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that the fix is still offered for built-in comparison operators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BuiltInComparisonIsStillFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(int count)
                                    {
                                        return 0 < count;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode, RH3005UseReadableConditionsAnalyzer.DiagnosticId, FirstComparisonOperatorLocation);

        Assert.HasCount(1, actions);
    }

    /// <summary>
    /// Verifying that no fix is offered when an operand is dynamic, because the comparison operator is rebound at
    /// runtime and the mirrored operator is not guaranteed to exist or match
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DynamicOperandIsNotFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(dynamic value)
                                    {
                                        return 5 < value;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode, RH3005UseReadableConditionsAnalyzer.DiagnosticId, FirstComparisonOperatorLocation);

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the location of the operator token of the first comparison expression in the syntax tree
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The operator token location</returns>
    private static Location FirstComparisonOperatorLocation(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<BinaryExpressionSyntax>().First().OperatorToken.GetLocation();
    }

    #endregion // Methods
}