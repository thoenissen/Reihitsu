using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    /// Characterizes that the fix of a negation as the right operand of a user-defined <c>&gt;</c> in an argument
    /// writes parentheses that, with the <c>&lt;</c> of the previous argument, read as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNegationAfterGreaterThanInAnArgumentWritesAGenericReading()
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
                                        M(G < A, B > !x);
                                    }
                                }
                                """;

        const string expectedCode = """
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
                                            M(G < A, B > (x == false));
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a negation as the right operand of a user-defined <c>&gt;</c> in a comparison
    /// chain writes parentheses that, with the <c>&lt;</c> in front, read as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNegationAfterGreaterThanInAComparisonChainWritesAGenericReading()
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
                                        return G < A > !x;
                                    }
                                }
                                """;

        const string expectedCode = """
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
                                            return G < A > (x == false);
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a negation that starts the right operand of a built-in <c>&gt;</c> in an argument,
    /// as the governing expression of a <see langword="switch"/> expression, writes parentheses that, with the
    /// <c>&lt;</c> of the previous argument, read as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNegationStartingASwitchAfterGreaterThanInAnArgumentWritesAGenericReading()
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
                                        M(G < A, B > !x switch { true => 1, false => 0 });
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

                                        void Run(bool x)
                                        {
                                            M(G < A, B > (x == false) switch { true => 1, false => 0 });
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a negation as the right operand of a user-defined <c>&gt;&gt;</c> in an argument
    /// writes parentheses that, with the two <c>&lt;</c> in front, read as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNegationAfterShiftRightInAnArgumentWritesAGenericReading()
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
                                        M(G < A, H < B >> !x);
                                    }
                                }
                                """;

        const string expectedCode = """
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
                                            M(G < A, H < B >> (x == false));
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a negation as the right operand of a <c>&gt;&gt;&gt;=</c> compound assignment with
    /// a user-defined <c>&gt;&gt;&gt;</c> puts the comparison in parentheses, although the assignment binds more weakly
    /// than the comparison
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNegationAfterUnsignedRightShiftAssignmentAddsParentheses()
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
                                        B >>>= !x;
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class T
                                    {
                                        public static T operator >>>(T a, bool b) => a;
                                    }

                                    class C
                                    {
                                        T B;

                                        void Run(bool x)
                                        {
                                            B >>>= (x == false);
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a negation as the right operand of a <c>??=</c> compound assignment puts the
    /// comparison in parentheses, although the assignment binds more weakly than the comparison
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNegationAfterNullCoalescingAssignmentAddsParentheses()
    {
        const string testCode = """
                                class C
                                {
                                    bool? B;

                                    void Run(bool x)
                                    {
                                        B ??= !x;
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        bool? B;

                                        void Run(bool x)
                                        {
                                            B ??= (x == false);
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that a negation that is the whole expression of an argument is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NegationAsAWholeArgumentIsReportedWithoutACodeFix()
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

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that a negation that is the whole expression of a tuple element is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NegationAsAWholeTupleElementIsReportedWithoutACodeFix()
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

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that a negation that is the whole expression of an object creation argument is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NegationAsAWholeObjectCreationArgumentIsReportedWithoutACodeFix()
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

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that a negation that is the whole expression of an element access argument is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NegationAsAWholeElementAccessArgumentIsReportedWithoutACodeFix()
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

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that a negation that is the whole expression of an attribute argument is reported and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NegationAsAWholeAttributeArgumentIsReportedWithoutACodeFix()
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

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that a negation that is a whole element of a collection expression is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NegationAsAWholeCollectionExpressionElementIsReportedWithoutACodeFix()
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

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that a negation that is the whole constant pattern of an <c>is</c> expression is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Characterizes that a negation that is the whole constant pattern of a <c>switch</c> expression arm is reported
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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