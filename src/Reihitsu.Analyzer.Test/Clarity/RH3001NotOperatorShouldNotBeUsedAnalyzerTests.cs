using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3001NotOperatorShouldNotBeUsedAnalyzer"/> and <see cref="RH3001NotOperatorShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3001NotOperatorShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3001NotOperatorShouldNotBeUsedAnalyzer, RH3001NotOperatorShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying not operator on literal is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-001")]
    [Requirement("RH3001-003")]
    [Requirement("RH3001-004")]
    public async Task NotOperatorOnLiteral()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetBool()
                                    {
                                        return {|#0:!|}false;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool GetBool()
                                     {
                                         return false == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on field is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-001")]
    [Requirement("RH3001-003")]
    [Requirement("RH3001-004")]
    public async Task NotOperatorOnField()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return {|#0:!|}_field;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on property is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-001")]
    [Requirement("RH3001-003")]
    [Requirement("RH3001-004")]
    public async Task NotOperatorOnProperty()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Property { get; set; }

                                    public bool GetProperty()
                                    {
                                        return {|#0:!|}Property;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Property { get; set; }

                                     public bool GetProperty()
                                     {
                                         return Property == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on method call is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-001")]
    [Requirement("RH3001-003")]
    [Requirement("RH3001-004")]
    public async Task NotOperatorOnMethodCall()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetBool()
                                    {
                                        return true;
                                    }

                                    public bool GetMethod()
                                    {
                                        return {|#0:!|}GetBool();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool GetBool()
                                     {
                                         return true;
                                     }

                                     public bool GetMethod()
                                     {
                                         return GetBool() == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on a nullable bool operand is not reported, because <c>!x</c> and <c>x == false</c> are not equivalent for <see langword="null"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-002")]
    public async Task NotOperatorOnNullableBoolIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool? GetValue(bool? value)
                                    {
                                        return !value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying not operator on an operand with a user-defined operator is not reported, because the rewrite is not equivalent
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-002")]
    public async Task NotOperatorOnUserDefinedOperatorIsNotReported()
    {
        const string testCode = """
                                public struct Custom
                                {
                                    public static Custom operator !(Custom value)
                                    {
                                        return value;
                                    }
                                }

                                public class Test
                                {
                                    public Custom GetValue(Custom value)
                                    {
                                        return !value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying leading trivia of the not operator is preserved when the fix is applied
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorTriviaIsPreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return /* comment */ {|#0:!|}_field;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return /* comment */ _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifies a comment between a not operator and its operand survives the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-005")]
    public async Task CommentBetweenNotOperatorAndOperandIsPreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return {|#0:!|} /* Keep. */ _field;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return /* Keep. */ _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifies an end-of-line comment between a not operator and its operand survives the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-005")]
    public async Task EndOfLineCommentBetweenNotOperatorAndOperandIsPreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return {|#0:!|} // Keep.
                                               _field;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return // Keep.
                                                _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifies directives and disabled text nested inside an operand do not suppress a fix for a clean operator gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-006")]
    public async Task NestedOperandDirectivesRemainAvailableAndArePreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetField()
                                    {
                                        return {|#0:!|}(
                                #if FEATURE
                                            true /* enabled branch */
                                #else
                                            false /* disabled branch */
                                #endif
                                        );
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool GetField()
                                     {
                                         return (
                                 #if FEATURE
                                             true /* enabled branch */
                                 #else
                                             false /* disabled branch */
                                 #endif
                                         ) == false;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test => test.SolutionTransforms.Add(static (solution, projectId) =>
                                                                       {
                                                                           var project = solution.GetProject(projectId);

                                                                           return project?.ParseOptions is Microsoft.CodeAnalysis.CSharp.CSharpParseOptions parseOptions
                                                                                      ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("FEATURE"))
                                                                                      : solution;
                                                                       }),
                     Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifies no fix is offered when conditional-compilation trivia separates the not operator and operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-006")]
    public async Task FixIsNotOfferedAcrossDirectiveAndDisabledText()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetField()
                                    {
                                        return !
                                #if FEATURE
                                               true
                                #else
                                               false
                                #endif
                                               ;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>().Single().OperatorToken.GetLocation(),
                                                   "FEATURE");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies no fix is offered when skipped syntax separates the not operator and operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-006")]
    public async Task FixIsNotOfferedAcrossSkippedSyntax()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetField(bool value)
                                    {
                                        return ! @ value;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>().Single().OperatorToken.GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies a not operator in a condition and in front of an invocation is reported on the operator and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-001")]
    [Requirement("RH3001-003")]
    public async Task NotOperatorInConditionAndBeforeInvocationIsReportedOnTheOperator()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, string s)
                                    {
                                        if ({|RH3001:!|}a)
                                        {
                                        }

                                        return {|RH3001:!|}s.Contains("a");
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, string s)
                                     {
                                         if (a == false)
                                         {
                                         }

                                         return s.Contains("a") == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a nullable bool operand and an operand with its own not operator are not reported in one document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-002")]
    public async Task NullableAndUserDefinedNotOperandsAreNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public static S operator !(S s) => s;
                                }

                                class C
                                {
                                    bool? M(bool? a)
                                    {
                                        return !a;
                                    }

                                    S N(S s)
                                    {
                                        return !s;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix parenthesizes the comparison as the right operand of <c>==</c>, adds no parentheses as the left
    /// operand of <c>&amp;&amp;</c>, and keeps the parentheses of an already parenthesized operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixParenthesizesTheComparisonWhereItWouldBindDifferently()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var x = {|RH3001:!|}a && b;
                                        var y = b == {|RH3001:!|}a;
                                        return {|RH3001:!|}(a && b);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b)
                                     {
                                         var x = a == false && b;
                                         var y = b == (a == false);
                                         return (a && b) == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a block comment and an end-of-line comment after the operator move in front of the operand without
    /// the whitespace that separated them from the operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-005")]
    public async Task CommentsAfterTheOperatorMoveInFrontOfTheOperand()
    {
        const string testCode = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        var x = {|RH3001:!|} /* first */ a;
                                        return {|RH3001:!|} // second
                                            b;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M(bool a, bool b)
                                     {
                                         var x = /* first */ a == false;
                                         return // second
                                             b == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a directive between the operator and its operand keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-006")]
    public async Task DirectiveAfterTheOperatorIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    bool M()
                                    {
                                        return {|RH3001:!|}
                                #if DEBUG
                                            true
                                #else
                                            false
                                #endif
                                            ;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies directives inside the parentheses of the operand do not withhold the fix and stay where they are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-006")]
    public async Task DirectivesInsideTheParenthesizedOperandDoNotWithholdTheFix()
    {
        const string testCode = """
                                class C
                                {
                                    bool M()
                                    {
                                        return {|RH3001:!|}(
                                #if DEBUG
                                            true
                                #else
                                            false
                                #endif
                                        );
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M()
                                     {
                                         return (
                                 #if DEBUG
                                             true
                                 #else
                                             false
                                 #endif
                                         ) == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation as the right operand of a user-defined <c>&gt;</c> in an argument is reported without a code
    /// fix, because the parentheses the fix writes would, with the <c>&lt;</c> of the previous argument, read as a
    /// generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-007")]
    public async Task NegationAfterGreaterThanInAnArgumentThatWouldReadAsAGenericNameIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class T
                                {
                                    public static bool operator >(T a, bool b) => b;

                                    public static bool operator <(T a, bool b) => b;
                                }

                                class C
                                {
                                    int G;
                                    int A;
                                    T B;

                                    void M(bool x, bool y)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M(G < A, B > {|RH3001:!|}x);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation as the right operand of a user-defined <c>&gt;</c> in a comparison chain is reported without
    /// a code fix, because the parentheses the fix writes would, with the <c>&lt;</c> in front, read as a generic
    /// invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-007")]
    public async Task NegationAfterGreaterThanInAComparisonChainThatWouldReadAsAGenericNameIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class U
                                {
                                    public static bool operator >(U a, bool b) => b;

                                    public static bool operator <(U a, bool b) => b;
                                }

                                class T
                                {
                                    public static U operator <(T a, int b) => null;

                                    public static U operator >(T a, int b) => null;
                                }

                                class C
                                {
                                    T G;
                                    int A;

                                    bool Run(bool x)
                                    {
                                        return G < A > {|RH3001:!|}x;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation that starts the right operand of a built-in <c>&gt;</c> in an argument, as the governing
    /// expression of a <see langword="switch"/> expression, is reported without a code fix, because the parentheses the
    /// fix writes would, with the <c>&lt;</c> of the previous argument, read as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-007")]
    public async Task NegationStartingASwitchAfterGreaterThanInAnArgumentThatWouldReadAsAGenericNameIsReportedWithoutACodeFix()
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

                                    void Run(bool x)
                                    {
                                        M(G < A, B > {|RH3001:!|}x switch { true => 1, false => 0 });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation as the right operand of a user-defined <c>&gt;&gt;</c> in an argument is reported without a
    /// code fix, because the parentheses the fix writes would, with the two <c>&lt;</c> in front, read as a generic
    /// invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-007")]
    public async Task NegationAfterShiftRightInAnArgumentThatWouldReadAsAGenericNameIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class V
                                {
                                    public static bool operator <(V a, W b) => true;

                                    public static bool operator >(V a, W b) => true;
                                }

                                class W
                                {
                                }

                                class T
                                {
                                    public static W operator >>(T a, bool b) => new W();
                                }

                                class C
                                {
                                    int G;
                                    int A;
                                    V H;
                                    T B;

                                    void M(bool p, bool q)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M(G < A, H < B >> {|RH3001:!|}x);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the fix of a negation as the right operand of a <c>&gt;&gt;&gt;=</c> compound assignment with a
    /// user-defined <c>&gt;&gt;&gt;</c> writes the comparison without parentheses, because it binds as the negation did
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixOfNegationAfterUnsignedRightShiftAssignmentAddsNoParentheses()
    {
        const string testCode = """
                                class T
                                {
                                    public static T operator >>>(T a, bool b) => a;
                                }

                                class C
                                {
                                    T B;

                                    void Run(bool x)
                                    {
                                        B >>>= {|RH3001:!|}x;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class T
                                 {
                                     public static T operator >>>(T a, bool b) => a;
                                 }

                                 class C
                                 {
                                     T B;

                                     void Run(bool x)
                                     {
                                         B >>>= x == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix of a negation as the right operand of a <c>??=</c> compound assignment writes the comparison
    /// without parentheses, because it binds as the negation did
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixOfNegationAfterNullCoalescingAssignmentAddsNoParentheses()
    {
        const string testCode = """
                                class C
                                {
                                    bool? B;

                                    void Run(bool x)
                                    {
                                        B ??= {|RH3001:!|}x;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool? B;

                                     void Run(bool x)
                                     {
                                         B ??= x == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specified example: the fix writes no parentheses behind <c>??=</c>, in an <c>orderby</c> key with a
    /// direction, and in a <c>select</c> clause
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixWritesNoParenthesesWhereTheComparisonBindsAsTheNegationDid()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    bool? _b;

                                    void Run(bool x, bool[] v)
                                    {
                                        _b ??= {|RH3001:!|}x;
                                        var q = from w in v
                                                orderby {|RH3001:!|}w descending
                                                select {|RH3001:!|}w;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 class C
                                 {
                                     bool? _b;

                                     void Run(bool x, bool[] v)
                                     {
                                         _b ??= x == false;
                                         var q = from w in v
                                                 orderby w == false descending
                                                 select w == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix writes no parentheses in the element and in the key of a <c>group</c> clause
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixWritesNoParenthesesInTheElementAndTheKeyOfAGroupClause()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var g = from a in v
                                                group {|RH3001:!|}a by a;
                                        var h = from b in v
                                                group b by {|RH3001:!|}b;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 class C
                                 {
                                     void Run(bool[] v)
                                     {
                                         var g = from a in v
                                                 group a == false by a;
                                         var h = from b in v
                                                 group b by b == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix writes no parentheses in a named attribute argument, written with <c>=</c> or with <c>:</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixWritesNoParenthesesInANamedAttributeArgument()
    {
        const string testCode = """
                                using System;

                                class AAttribute : Attribute
                                {
                                    public AAttribute()
                                    {
                                    }

                                    public AAttribute(bool b)
                                    {
                                    }

                                    public bool P { get; set; }
                                }

                                [A(P = {|RH3001:!|}true)]
                                class X
                                {
                                }

                                [A(b: {|RH3001:!|}true)]
                                class Y
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class AAttribute : Attribute
                                 {
                                     public AAttribute()
                                     {
                                     }

                                     public AAttribute(bool b)
                                     {
                                     }

                                     public bool P { get; set; }
                                 }

                                 [A(P = true == false)]
                                 class X
                                 {
                                 }

                                 [A(b: true == false)]
                                 class Y
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix writes no parentheses behind <c>goto case</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixWritesNoParenthesesBehindGotoCase()
    {
        const string testCode = """
                                class C
                                {
                                    void Run(bool x)
                                    {
                                        switch (x)
                                        {
                                            case true:
                                                goto case {|RH3001:!|}true;
                                            case false:
                                                break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void Run(bool x)
                                     {
                                         switch (x)
                                         {
                                             case true:
                                                 goto case true == false;
                                             case false:
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specified example: a negation that is a whole argument, tuple element, object creation argument,
    /// element access argument, attribute argument, or collection-expression element is fixed without parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationsThatAreWholeArgumentsAttributeArgumentsOrElementsAreFixed()
    {
        const string testCode = """
                                using System;

                                class AAttribute : Attribute
                                {
                                    public AAttribute(bool b)
                                    {
                                    }
                                }

                                class C
                                {
                                    public C(bool b)
                                    {
                                    }

                                    bool this[bool b] => b;

                                    void M(bool b)
                                    {
                                    }

                                    [A({|RH3001:!|}true)]
                                    void Run(bool x, bool y)
                                    {
                                        M({|RH3001:!|}x);
                                        var t = ({|RH3001:!|}x, y);
                                        var c = new C({|RH3001:!|}y);
                                        var i = this[{|RH3001:!|}y];
                                        bool[] a = [{|RH3001:!|}x, y];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class AAttribute : Attribute
                                 {
                                     public AAttribute(bool b)
                                     {
                                     }
                                 }

                                 class C
                                 {
                                     public C(bool b)
                                     {
                                     }

                                     bool this[bool b] => b;

                                     void M(bool b)
                                     {
                                     }

                                     [A(true == false)]
                                     void Run(bool x, bool y)
                                     {
                                         M(x == false);
                                         var t = (x == false, y);
                                         var c = new C(y == false);
                                         var i = this[y == false];
                                         bool[] a = [x == false, y];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole argument of an invocation is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeArgumentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool x)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M({|RH3001:!|}x);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool x)
                                     {
                                     }

                                     void Run(bool x)
                                     {
                                         M(x == false);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the first of several whole arguments of an invocation is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeArgumentNextToAnotherArgumentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void N(bool x, bool y)
                                    {
                                    }

                                    void Run(bool x, bool y)
                                    {
                                        N({|RH3001:!|}x, y);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void N(bool x, bool y)
                                     {
                                     }

                                     void Run(bool x, bool y)
                                     {
                                         N(x == false, y);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is a whole named argument stays fixed without parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeNamedArgumentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M(b: {|RH3001:!|}x);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool b)
                                     {
                                     }

                                     void Run(bool x)
                                     {
                                         M(b: x == false);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is a whole tuple element is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeTupleElementIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void Run(bool x, bool y)
                                    {
                                        var t = ({|RH3001:!|}x, y);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void Run(bool x, bool y)
                                     {
                                         var t = (x == false, y);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole argument of an object creation is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeObjectCreationArgumentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    C(bool b)
                                    {
                                    }

                                    static C Run(bool y)
                                    {
                                        return new C({|RH3001:!|}y);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     C(bool b)
                                     {
                                     }

                                     static C Run(bool y)
                                     {
                                         return new C(y == false);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole argument of an object creation written without its type is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeImplicitObjectCreationArgumentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    C(bool b)
                                    {
                                    }

                                    static C Run(bool y)
                                    {
                                        C k = new({|RH3001:!|}y);

                                        return k;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     C(bool b)
                                     {
                                     }

                                     static C Run(bool y)
                                     {
                                         C k = new(y == false);

                                         return k;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole argument of an element access is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeElementAccessArgumentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    int this[bool b] => 0;

                                    int Run(bool y)
                                    {
                                        return this[{|RH3001:!|}y];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int this[bool b] => 0;

                                     int Run(bool y)
                                     {
                                         return this[y == false];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole argument of a conditional element access is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeConditionalElementAccessArgumentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    int this[bool b] => 0;

                                    static int? Run(C o, bool y)
                                    {
                                        return o?[{|RH3001:!|}y];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int this[bool b] => 0;

                                     static int? Run(C o, bool y)
                                     {
                                         return o?[y == false];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole index of an object initializer is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeIndexInAnObjectInitializerIsFixed()
    {
        const string testCode = """
                                class E
                                {
                                    public bool this[bool b]
                                    {
                                        get
                                        {
                                            return b;
                                        }
                                        set
                                        {
                                        }
                                    }
                                }

                                class C
                                {
                                    static E Run(bool x)
                                    {
                                        return new E { [{|RH3001:!|}x] = true };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class E
                                 {
                                     public bool this[bool b]
                                     {
                                         get
                                         {
                                             return b;
                                         }
                                         set
                                         {
                                         }
                                     }
                                 }

                                 class C
                                 {
                                     static E Run(bool x)
                                     {
                                         return new E { [x == false] = true };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole argument of a constructor initializer is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeConstructorInitializerArgumentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    public C(bool b)
                                    {
                                    }

                                    public C(bool b, int i)
                                        : this({|RH3001:!|}b)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     public C(bool b)
                                     {
                                     }

                                     public C(bool b, int i)
                                         : this(b == false)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole argument of a primary constructor's base type is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholePrimaryConstructorBaseArgumentIsFixed()
    {
        const string testCode = """
                                class Base
                                {
                                    public Base(bool b)
                                    {
                                    }
                                }

                                class D(bool x) : Base({|RH3001:!|}x)
                                {
                                }
                                """;

        const string fixedCode = """
                                 class Base
                                 {
                                     public Base(bool b)
                                     {
                                     }
                                 }

                                 class D(bool x) : Base(x == false)
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is a whole positional attribute argument is fixed, and the result compiles as a
    /// constant attribute argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeAttributeArgumentIsFixed()
    {
        const string testCode = """
                                class A : System.Attribute
                                {
                                    public A(bool b)
                                    {
                                    }
                                }

                                [A({|RH3001:!|}true)]
                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 class A : System.Attribute
                                 {
                                     public A(bool b)
                                     {
                                     }
                                 }

                                 [A(true == false)]
                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is a whole element of a collection expression is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeCollectionExpressionElementIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void Run(bool x, bool y)
                                    {
                                        bool[] a = [{|RH3001:!|}x, y];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void Run(bool x, bool y)
                                     {
                                         bool[] a = [x == false, y];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specified example: a negation that is the whole key of an <c>orderby</c> clause without a direction
    /// is fixed without parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsAWholeOrderByKeyWithoutADirectionIsFixed()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var q = from a in v
                                                orderby {|RH3001:!|}a
                                                select a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 class C
                                 {
                                     void Run(bool[] v)
                                     {
                                         var q = from a in v
                                                 orderby a == false
                                                 select a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is a whole argument and is itself part of a comparison gets the comparison in
    /// parentheses, because there it binds differently from the negation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task NegationAsTheLeftOperandOfIsInAnArgumentIsParenthesized()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M({|RH3001:!|}x is bool);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool b)
                                     {
                                     }

                                     void Run(bool x)
                                     {
                                         M((x == false) is bool);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a comment between the operator and the operand of a whole argument moves in front of the operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    [Requirement("RH3001-005")]
    public async Task CommentAfterTheOperatorOfAWholeArgumentMovesInFrontOfTheOperand()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M({|RH3001:!|} /* c */ x);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool b)
                                     {
                                     }

                                     void Run(bool x)
                                     {
                                         M(/* c */ x == false);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a directive between the operator and the operand of a whole argument withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-006")]
    public async Task DirectiveAfterTheOperatorOfAWholeArgumentIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M({|RH3001:!|}
                                #if DEBUG
                                          x);
                                #else
                                          x);
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the fix of a whole argument led by a comment on the line after <c>(</c> keeps every line break as it
    /// was, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH3001-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixOfACommentedWholeArgumentOnTheNextLineKeepsTheLineBreaks(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M(
                                            /* x */ {|RH3001:!|}x);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool b)
                                     {
                                     }

                                     void Run(bool x)
                                     {
                                         M(
                                             /* x */ x == false);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies Fix All fixes two negations that are whole arguments of one invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixAllFixesTwoWholeArgumentsOfOneInvocation()
    {
        const string testCode = """
                                class C
                                {
                                    void N(bool x, bool y)
                                    {
                                    }

                                    void Run(bool x, bool y)
                                    {
                                        N({|RH3001:!|}x, {|RH3001:!|}y);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void N(bool x, bool y)
                                     {
                                     }

                                     void Run(bool x, bool y)
                                     {
                                         N(x == false, y == false);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix of a negation that is a whole argument and holds a parenthesized negation fixes both, the outer
    /// one as a whole argument and the inner one inside its parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-004")]
    public async Task FixOfANestedNegationInAWholeArgumentFixesBoth()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M({|RH3001:!|}({|RH3001:!|}x));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool b)
                                     {
                                     }

                                     void Run(bool x)
                                     {
                                         M((x == false) == false);
                                     }
                                 }
                                 """;

        // The inner operator sits inside the operand the outer rewrite replaces, so the two text changes overlap
        // and the batch cannot apply them in one pass
        await Verify(testCode, fixedCode, static test => test.NumberOfFixAllIterations = 2);
    }

    /// <summary>
    /// Verifies the value of a <c>case</c> label without a <c>when</c> clause, which is an expression and not a pattern,
    /// keeps its fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationAsTheValueOfACaseLabelWithoutAWhenClauseIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    int Run(bool x)
                                    {
                                        switch (x)
                                        {
                                            case {|RH3001:!|}true:
                                                return 1;
                                            default:
                                                return 2;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int Run(bool x)
                                     {
                                         switch (x)
                                         {
                                             case true == false:
                                                 return 1;
                                             default:
                                                 return 2;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the constant pattern of a <c>case</c> label with a <c>when</c> clause is reported and
    /// no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationAsTheConstantPatternOfACaseLabelWithAWhenClauseIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int Run(bool x, bool y)
                                    {
                                        switch (y)
                                        {
                                            case {|RH3001:!|}true when x:
                                                return 1;
                                            default:
                                                return 2;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specified example: a negation that is the constant pattern of an <c>is</c> expression or of a
    /// switch-expression arm is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationsThatAreConstantPatternsAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int Run(bool x)
                                    {
                                        var r = x is {|RH3001:!|}true;
                                        return x switch
                                        {
                                            {|RH3001:!|}true => 1,
                                            _ => 2,
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specified example: a negation in the expression behind a switch-expression arm's <c>=&gt;</c> is not
    /// a pattern and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationBehindTheArrowOfASwitchExpressionArmIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    bool Run(bool x, bool y)
                                    {
                                        return x switch
                                        {
                                            true => {|RH3001:!|}y,
                                            _ => y,
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool Run(bool x, bool y)
                                     {
                                         return x switch
                                         {
                                             true => y == false,
                                             _ => y,
                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation that is the constant pattern of a property subpattern is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationAsTheConstantPatternOfAPropertySubpatternIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    public bool P { get; set; }

                                    static bool Run(object o)
                                    {
                                        return o is C { P: {|RH3001:!|}true };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation that is the constant pattern of a positional subpattern is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationAsTheConstantPatternOfAPositionalSubpatternIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    static bool Run((bool, int) t)
                                    {
                                        return t is ({|RH3001:!|}true, _);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation that is the constant pattern of a list pattern element is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationAsTheConstantPatternOfAListPatternElementIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    static bool Run(bool[] a)
                                    {
                                        return a is [{|RH3001:!|}true];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation that is the constant pattern behind <c>not</c> or in front of <c>or</c> is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationAsTheConstantPatternOfALogicalPatternIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    static bool Run(bool x)
                                    {
                                        return x is not {|RH3001:!|}true || x is {|RH3001:!|}true or false;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole constant pattern of an <c>is</c> expression is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-011")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3001-008")]
    public async Task NegationAsAWholeIsPatternIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    bool Run(bool x)
                                    {
                                        return x is {|RH3001:!|}true;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation that is the whole constant pattern of a <c>switch</c> expression arm is reported and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-008")]
    public async Task NegationAsAWholeSwitchArmPatternIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int Run(bool x)
                                    {
                                        return x switch
                                               {
                                                   {|RH3001:!|}true => 1,
                                                   _ => 2
                                               };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation that starts the right operand of a built-in <c>&gt;&gt;&gt;</c> in an argument is reported
    /// without a code fix, because the parentheses the fix writes would, with the three <c>&lt;</c> in front, read as
    /// a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-007")]
    public async Task NegationAfterUnsignedShiftRightInAnArgumentThatWouldReadAsAGenericNameIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int G;
                                    int A;
                                    int H;
                                    int J;
                                    int B;

                                    void M(bool x, bool y, bool z)
                                    {
                                    }

                                    void Run(bool x)
                                    {
                                        M(G < A, H < A, J < B >>> {|RH3001:!|}x switch { true => 1, false => 0 });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation of a parenthesized operand after a <c>&gt;</c> in an argument is reported without a code
    /// fix, because the parentheses the fix writes around the comparison would, with the <c>&lt;</c> of the previous
    /// argument, read as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-007")]
    public async Task NegationOfAParenthesizedOperandThatWouldReadAsAGenericNameIsReportedWithoutACodeFix()
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

                                    void Run(bool x)
                                    {
                                        M(G < A, B > {|RH3001:!|}(x) switch { true => 1, false => 0 });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation after a <c>&gt;</c> in an argument is still fixed where no <c>&lt;</c> in front of it could
    /// make the code the fix writes read as a generic name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-007")]
    public async Task NegationAfterGreaterThanWithoutALessThanInFrontIsFixed()
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

                                    void Run(bool x)
                                    {
                                        M(G > A, B > {|RH3001:!|}x switch { true => 1, false => 0 });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int G;
                                     int A;
                                     int B;

                                     void M(bool x, bool y)
                                     {
                                     }

                                     void Run(bool x)
                                     {
                                         M(G > A, B > (x == false) switch { true => 1, false => 0 });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies Fix All leaves a negation whose fix would read as a generic name as written and reported, and fixes
    /// the other negations of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-007")]
    public async Task FixAllLeavesANegationThatWouldReadAsAGenericNameReported()
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

                                    bool Run(bool x)
                                    {
                                        M(G < A, B > {|RH3001:!|}x switch { true => 1, false => 0 });

                                        return {|RH3001:!|}x && {|RH3001:!|}x;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int G;
                                     int A;
                                     int B;

                                     void M(bool x, bool y)
                                     {
                                     }

                                     bool Run(bool x)
                                     {
                                         M(G < A, B > {|RH3001:!|}x switch { true => 1, false => 0 });

                                         return x == false && x == false;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostics whose fix is withheld stay and are marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies the specified example: negations written directly against a keyword in front of the <c>!</c> or behind
    /// the operand are reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationsTouchingAWordAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    bool M(bool a, bool b, bool[] v)
                                    {
                                        var q = from w in v
                                                where {|RH3001:!|}(w)select w;
                                        var r = from w in v
                                                where{|RH3001:!|}w
                                                select{|RH3001:!|}w;

                                        if (b) return{|RH3001:!|}a;
                                        return a;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specified example: with a space or a comment between the negation and the word, the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationsSeparatedFromAWordBySpaceOrCommentAreFixed()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    bool M(bool a, bool b, bool[] v)
                                    {
                                        var q = from w in v
                                                where {|RH3001:!|}(w) select w;

                                        if (b) return {|RH3001:!|}a;
                                        if (a) return/* c */{|RH3001:!|}b;
                                        return a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 class C
                                 {
                                     bool M(bool a, bool b, bool[] v)
                                     {
                                         var q = from w in v
                                                 where (w) == false select w;

                                         if (b) return a == false;
                                         if (a) return/* c */b == false;
                                         return a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation written directly behind <c>return</c> is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindReturnIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    bool Run(bool a, bool b)
                                    {
                                        if (b) return{|RH3001:!|}a;
                                        return a;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation written directly behind <c>yield return</c> is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindYieldReturnIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    IEnumerable<bool> Run(bool a)
                                    {
                                        yield return{|RH3001:!|}a;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation written directly behind <c>case</c> in a case label without a <c>when</c> clause is reported
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindCaseIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int Run(bool x)
                                    {
                                        switch (x)
                                        {
                                            case{|RH3001:!|}true:
                                                return 1;
                                            default:
                                                return 2;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation written directly behind <c>goto case</c> is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindGotoCaseIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int Run(bool x)
                                    {
                                        switch (x)
                                        {
                                            case true:
                                                goto case{|RH3001:!|}true;
                                            case false:
                                                return 1;
                                        }

                                        return 2;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation written directly behind the <c>when</c> of a switch-expression arm is reported and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindWhenOfASwitchExpressionArmIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int Run(bool x, bool y)
                                    {
                                        return x switch
                                        {
                                            _ when{|RH3001:!|}y => 1,
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation behind the <c>when</c> of a switch-expression arm, separated from it by a space, keeps its fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationBehindWhenOfASwitchExpressionArmAndASpaceIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    int Run(bool x, bool y)
                                    {
                                        return x switch
                                        {
                                            _ when {|RH3001:!|}y => 1,
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int Run(bool x, bool y)
                                     {
                                         return x switch
                                         {
                                             _ when y == false => 1,
                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation written directly behind <c>where</c> is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindWhereIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var q = from w in v
                                                where{|RH3001:!|}w
                                                select w;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation written directly behind <c>select</c> is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindSelectIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var q = from w in v
                                                select{|RH3001:!|}w;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation written directly behind <c>orderby</c> is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindOrderbyIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var q = from w in v
                                                orderby{|RH3001:!|}w
                                                select w;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies negations written directly behind <c>group</c> and <c>by</c> are reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationsDirectlyBehindGroupAndByAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var q = from w in v
                                                group{|RH3001:!|}w by{|RH3001:!|}w;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies negations written directly behind <c>on</c> and <c>equals</c> of a join clause are reported and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationsDirectlyBehindOnAndEqualsAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var q = from x in v
                                                join y in v on{|RH3001:!|}x equals{|RH3001:!|}y
                                                select x;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation whose last token is written directly against <c>select</c> behind it is reported and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationWhoseOperandTouchesSelectBehindItIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var q = from w in v
                                                where {|RH3001:!|}(w)select w;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation whose last token is written directly against the keyword <c>is</c> behind it, outside a
    /// query, is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationWhoseOperandTouchesIsBehindItIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    bool Run(bool a)
                                    {
                                        return {|RH3001:!|}(a)is true;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a negation whose last token is separated from <c>select</c> behind it by a comment keeps its fix, and
    /// the comment stays where it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationWhoseOperandIsSeparatedFromSelectByACommentIsFixed()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void Run(bool[] v)
                                    {
                                        var q = from w in v
                                                where {|RH3001:!|}(w)/* c */select w;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 class C
                                 {
                                     void Run(bool[] v)
                                     {
                                         var q = from w in v
                                                 where (w) == false/* c */select w;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation behind <c>return</c> and a space keeps its fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationBehindReturnAndASpaceIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    bool Run(bool a, bool b)
                                    {
                                        if (b) return {|RH3001:!|}a;
                                        return a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool Run(bool a, bool b)
                                     {
                                         if (b) return a == false;
                                         return a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation separated from <c>return</c> by a comment and no whitespace keeps its fix, and the comment
    /// stays where it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationSeparatedFromReturnByACommentIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    bool Run(bool a, bool b)
                                    {
                                        if (b) return/* c */{|RH3001:!|}a;
                                        return a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool Run(bool a, bool b)
                                     {
                                         if (b) return/* c */a == false;
                                         return a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a negation on the line below <c>return</c> keeps its fix, and the line break stays, under both line
    /// endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH3001-009")]
    public async Task NegationOnTheLineBelowReturnIsFixed(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool Run(bool a)
                                    {
                                        return
                                            {|RH3001:!|}a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool Run(bool a)
                                     {
                                         return
                                             a == false;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies a negation behind punctuation with no whitespace between them keeps its fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task NegationDirectlyBehindAnOpeningParenthesisIsFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                    }

                                    void Run(bool a)
                                    {
                                        if({|RH3001:!|}a)
                                        {
                                            M({|RH3001:!|}a);
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool b)
                                     {
                                     }

                                     void Run(bool a)
                                     {
                                         if(a == false)
                                         {
                                             M(a == false);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies Fix All leaves a negation that touches a word as written and reported, and fixes the other negations of
    /// the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3001-009")]
    public async Task FixAllLeavesANegationTouchingAWordReported()
    {
        const string testCode = """
                                class C
                                {
                                    bool Run(bool a, bool b)
                                    {
                                        if (b) return{|RH3001:!|}a;
                                        if (a) return {|RH3001:!|}b;
                                        return {|RH3001:!|}a && b;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool Run(bool a, bool b)
                                     {
                                         if (b) return{|RH3001:!|}a;
                                         if (a) return b == false;
                                         return a == false && b;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(bool first, bool second)
                                    {
                                        return {|#0:!|}(first && {|#1:!|}second);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Run(bool first, bool second)
                                     {
                                         return (first && second == false) == false;
                                     }
                                 }
                                 """;

        // The inner operator sits inside the operand the outer rewrite replaces, so the two text changes overlap
        // and the batch cannot apply them in one pass
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}