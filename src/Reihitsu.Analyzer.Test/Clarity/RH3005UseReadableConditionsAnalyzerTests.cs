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
public class RH3005UseReadableConditionsAnalyzerTests : BatchCodeFixTestsBase<RH3005UseReadableConditionsAnalyzer, RH3005UseReadableConditionsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying Yoda conditions with less-than operator are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
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
    [Requirement("RH3005-002")]
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
    [Requirement("RH3005-002")]
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
    [Requirement("RH3005-002")]
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
    /// Verifying that no fix is offered when swapping the operands would require a user-defined operator overload that
    /// does not exist, which would produce non-compiling output
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-005")]
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
    [Requirement("RH3005-005")]
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
    [Requirement("RH3005-004")]
    [Requirement("RH3005-005")]
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
    [Requirement("RH3005-005")]
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

    /// <summary>
    /// Verifying that no fix is offered for a user-defined equality operator, because its commutativity cannot be
    /// verified statically and swapping the operands could silently change behavior
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-005")]
    public async Task UserDefinedEqualityOperatorIsNotFixed()
    {
        const string testCode = """
                                public struct MyType
                                {
                                    public static bool operator ==(MyType left, MyType right)
                                    {
                                        return true;
                                    }

                                    public static bool operator !=(MyType left, MyType right)
                                    {
                                        return false;
                                    }
                                }

                                public class Test
                                {
                                    public bool Run(MyType value)
                                    {
                                        return default(MyType) == value;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode, RH3005UseReadableConditionsAnalyzer.DiagnosticId, FirstComparisonOperatorLocation);

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a documentation comment inside the condition keeps the fix from being offered. The swap
    /// reparses both operands without their trivia, so offering the fix would delete the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-006")]
    public async Task DocumentationCommentedYodaConditionIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   public class Test
                                   {
                                       public bool Run(int count)
                                       {
                                           return 0 < /** The counter. */ count;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3005UseReadableConditionsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<BinaryExpressionSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a documentation comment on the member itself does not block the fix. It sits in the leading
    /// trivia of the member, outside the comparison, so the swap cannot touch it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DocumentedMemberWithYodaConditionIsFixedAndKeepsItsComment()
    {
        const string codeFixData = """
                                   public class Test
                                   {
                                       /// <summary>Checks the counter.</summary>
                                       public bool Run(int count)
                                       {
                                           return 0 < count;
                                       }
                                   }
                                   """;

        var fixedCode = await ApplyCodeFixAsync(codeFixData);

        Assert.Contains("/// <summary>Checks the counter.</summary>", fixedCode);
        Assert.Contains("return count > 0;", fixedCode);
    }

    /// <summary>
    /// Verifying that a comment written before the comparison does not withhold the code fix. The swap transplants
    /// the comparison's own outer trivia onto the replacement, so that comment is never crossed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-006")]
    public async Task VerifyCommentBeforeTheComparisonIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   public class Test
                                   {
                                       public bool Run(int count)
                                       {
                                           return
                                               /* note */ 0 < count;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3005UseReadableConditionsAnalyzer.DiagnosticId,
                                                   FirstComparisonOperatorLocation);

        Assert.IsNotEmpty(actions);

        var fixedCode = await ApplyCodeFixAsync(codeFixData);

        Assert.Contains("/* note */ count > 0;", fixedCode);
    }

    /// <summary>
    /// Verifying that a comment written inside the comparison still withholds the code fix, because the swap would
    /// relocate it relative to the operands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-006")]
    public async Task VerifyCommentInsideTheComparisonWithholdsTheCodeFix()
    {
        const string codeFixData = """
                                   public class Test
                                   {
                                       public bool Run(int count)
                                       {
                                           return 0 < /* note */ count;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3005UseReadableConditionsAnalyzer.DiagnosticId,
                                                   FirstComparisonOperatorLocation);

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies comparisons with a literal, a <c>const</c> field, a <c>static readonly</c> field, <c>null</c>, and a
    /// <c>default</c> expression on the left are reported on the operator and fixed with the mirrored operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-001")]
    [Requirement("RH3005-003")]
    [Requirement("RH3005-004")]
    public async Task ConstantLikeLeftOperandsAreReportedAndSwappedWithMirroredOperator()
    {
        const string testCode = """
                                class C
                                {
                                    const int Max = 10;
                                    static readonly int Min = 0;

                                    void M(int a, string s)
                                    {
                                        if (0 {|RH3005:<|} a) { }
                                        if (Max {|RH3005:==|} a) { }
                                        if (Min {|RH3005:>=|} a) { }
                                        if (null {|RH3005:==|} s) { }
                                        if (default(int) {|RH3005:!=|} a) { }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     const int Max = 10;
                                     static readonly int Min = 0;

                                     void M(int a, string s)
                                     {
                                         if (a > 0) { }
                                         if (a == Max) { }
                                         if (a <= Min) { }
                                         if (s == null) { }
                                         if (a != default(int)) { }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies comparisons with a value on the left, or with two constant-like operands, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-002")]
    public async Task ComparisonsWithoutAConstantOnlyOnTheLeftAreNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    const int Max = 10;

                                    void M(int a, int b)
                                    {
                                        if (a < 0) { }
                                        if (a < b) { }
                                        if (Max > 0) { }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a comparison bound to a user-defined operator and a comparison with a <c>dynamic</c> operand are
    /// reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-005")]
    public async Task UserDefinedAndDynamicComparisonsAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                class V
                                {
                                    public static bool operator ==(int a, V b) => true;
                                    public static bool operator !=(int a, V b) => false;
                                    public override bool Equals(object o) => false;
                                    public override int GetHashCode() => 0;
                                }

                                class C
                                {
                                    void M(V v, dynamic d)
                                    {
                                        if (1 {|RH3005:==|} v) { }
                                        if (0 {|RH3005:<|} d) { }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a built-in comparison whose swapped form would bind to a user-defined operator is reported without a
    /// code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-005")]
    public async Task BuiltInComparisonWhoseMirrorBindsToAUserDefinedOperatorIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class V
                                {
                                    public static implicit operator int(V v) => 0;
                                    public static bool operator <(V a, int b) => true;
                                    public static bool operator >(V a, int b) => true;
                                }

                                class C
                                {
                                    bool M(V v)
                                    {
                                        return 1 {|RH3005:<|} v;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a comment between the left operand and the operator keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-006")]
    public async Task CommentBeforeTheOperatorIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (0 /* x */ {|RH3005:<|} a) { }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a comment in front of the comparison stays in front of the swapped comparison
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3005-004")]
    [Requirement("RH3005-006")]
    public async Task CommentInFrontOfTheComparisonStaysInFront()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(int a)
                                    {
                                        return /* x */ 0 {|RH3005:<|} a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(int a)
                                     {
                                         return /* x */ a > 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Characterizes that the fix of a comparison whose left operand is itself a comparison writes the swapped
    /// operands without parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfComparisonWithComparisonOperandWritesTheSwapWithoutParentheses()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        if (1 == 1 == b) { }
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(bool b)
                                        {
                                            if (b == 1 == 1) { }
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a constant-left comparison in an argument writes a new <c>&lt;</c> that, with the
    /// <c>&gt;</c> of the next argument, reads as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfConstantLeftArgumentBeforeAGreaterThanArgumentWritesAGenericReading()
    {
        const string testCode = """
                                class C
                                {
                                    const int Max = 1;
                                    int G;
                                    int B;

                                    void M(bool x, bool y)
                                    {
                                    }

                                    void Run()
                                    {
                                        M(Max > G, B > (7));
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        const int Max = 1;
                                        int G;
                                        int B;

                                        void M(bool x, bool y)
                                        {
                                        }

                                        void Run()
                                        {
                                            M(G < Max, B > (7));
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a comparison with a <c>static readonly</c> left operand in an array initializer
    /// writes a new <c>&lt;</c> that, with the <c>&gt;</c> of the next element, reads as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfStaticReadonlyLeftElementBeforeAGreaterThanElementWritesAGenericReading()
    {
        const string testCode = """
                                class C
                                {
                                    static readonly int Min = 1;
                                    int G;
                                    int B;

                                    void Run()
                                    {
                                        var a = new[] { Min > G, B > (7) };
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        static readonly int Min = 1;
                                        int G;
                                        int B;

                                        void Run()
                                        {
                                            var a = new[] { G < Min, B > (7) };
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a literal-left comparison in an argument writes a new <c>&gt;</c> that, with the
    /// <c>&lt;</c> of the previous argument, reads as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfLiteralLeftArgumentAfterALessThanArgumentWritesAGenericReading()
    {
        const string testCode = """
                                class C
                                {
                                    int G;
                                    int A;
                                    int B;

                                    void M(bool x, bool y)
                                    {
                                    }

                                    void Run()
                                    {
                                        M(G < A, (7) < B);
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        int G;
                                        int A;
                                        int B;

                                        void M(bool x, bool y)
                                        {
                                        }

                                        void Run()
                                        {
                                            M(G < A, B > (7));
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
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

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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

        // Both operands of the same AND expression are swapped, so the two rewrites replace siblings of one parent node
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3005UseReadableConditionsAnalyzer.DiagnosticId, "Use readable conditions.", 2));
    }

    #endregion // BatchCodeFixTestsBase
}