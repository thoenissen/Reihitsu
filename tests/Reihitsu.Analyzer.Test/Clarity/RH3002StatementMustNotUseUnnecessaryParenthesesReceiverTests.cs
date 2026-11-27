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
/// Test methods for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/> on nested parentheses and parentheses around the receiver of a member access, an element access, or an invocation
/// </summary>
[TestClass]
public class RH3002StatementMustNotUseUnnecessaryParenthesesReceiverTests : BatchCodeFixTestsBase<RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as an invocation receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInMemberAccessChainKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return ({|#0:(_o?._f)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public string Run()
                                     {
                                         return (_o?._f).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as an invocation receiver with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInMemberAccessChainCrlfKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return ({|#0:(_o?._f)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public string Run()
                                     {
                                         return (_o?._f).ToString();
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as an element access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInElementAccessKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int[] _a;
                                    private Test _o;

                                    public int[] Items => _a;

                                    public int Run()
                                    {
                                        return ({|#0:(_o?.Items)|})[0];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int[] _a;
                                     private Test _o;

                                     public int[] Items => _a;

                                     public int Run()
                                     {
                                         return (_o?.Items)[0];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as a member access assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInMemberAccessAssignmentKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private Test _p;
                                    public int X;

                                    public void Run()
                                    {
                                        ({|#0:(_o?._p)|}).X = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private Test _p;
                                     public int X;

                                     public void Run()
                                     {
                                         (_o?._p).X = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access used as an invocation target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessInInvocationKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private System.Func<int> _d;

                                    public int Run()
                                    {
                                        return ({|#0:(_o?._d)|})();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private System.Func<int> _d;

                                     public int Run()
                                     {
                                         return (_o?._d)();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access await operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessAwaitOperandKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task<int> Run()
                                    {
                                        return await ({|#0:(_o?._t)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task<int> Run()
                                     {
                                         return await (_o?._t);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a binary expression used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-006")]
    public async Task NestedParenthesesAroundBinaryExpressionInMemberAccessChainKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public string Run()
                                    {
                                        return ({|#0:(_a + _b)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public string Run()
                                     {
                                         return (_a + _b).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a null-coalescing await operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundCoalesceAwaitOperandKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Threading.Tasks.Task<int> _t1;
                                    private System.Threading.Tasks.Task<int> _t2;

                                    public async System.Threading.Tasks.Task<int> Run()
                                    {
                                        return await ({|#0:(_t1 ?? _t2)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Threading.Tasks.Task<int> _t1;
                                     private System.Threading.Tasks.Task<int> _t2;

                                     public async System.Threading.Tasks.Task<int> Run()
                                     {
                                         return await (_t1 ?? _t2);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a null-forgiving expression used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundSuppressedExpressionInMemberAccessChainKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public int Run()
                                    {
                                        return ({|#0:(_s!)|}).Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;

                                     public int Run()
                                     {
                                         return (_s!).Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying both redundant inner pairs are reported and fixed when three pairs wrap a conditional access used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TripleParenthesesAroundConditionalAccessInMemberAccessChainKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return ({|#0:({|#1:(_o?._f)|})|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public string Run()
                                     {
                                         return (_o?._f).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying the outermost and the innermost pair are reported when the pair a chain needs sits in the middle of three pairs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundCoalesceInsideAChainKeepTheNecessaryMiddlePair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;
                                    private string _t;

                                    public string Run()
                                    {
                                        return {|#0:(({|#1:(_s ?? _t)|}).Length)|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;
                                     private string _t;

                                     public string Run()
                                     {
                                         return (_s ?? _t).Length.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying an assignment target ending in an await operand is reported together with the redundant inner pair of that operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessAwaitOperandInsideAnAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private Test _o;
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run()
                                    {
                                        {|#0:(_a + await ({|#1:(_o?._t)|}))|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private Test _o;
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task Run()
                                     {
                                         _a + await (_o?._t) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config =>
                            {
                                config.CompilerDiagnostics = CompilerDiagnostics.None;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying the redundant inner pair is fixed when a comment sits inside the necessary outer pair around a conditional access chain receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessWithCommentInsideTheOuterPairAreFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return (/* note */ {|#0:(_o?._f)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public string Run()
                                     {
                                         return (/* note */ _o?._f).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the inner pair is reported and no code fix is offered when a comment sits inside it around a conditional access chain receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundConditionalAccessWithCommentInsideTheInnerPairWithholdTheCodeFix()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public string Run()
                                    {
                                        return ({|#0:(/* note */ _o?._f)|}).ToString();
                                    }
                                }
                                """;

        await Verify(testCode, testCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(testCode.Replace("{|#0:", string.Empty).Replace("|}", string.Empty),
                                                   RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParenthesizedExpressionSyntax>()
                                                               .Last()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying both pairs are reported and fixed in one pass when nested parentheses wrap an identifier used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundIdentifierInMemberAccessChainAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;

                                    public string Run()
                                    {
                                        return {|#0:({|#1:(_a)|})|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;

                                     public string Run()
                                     {
                                         return _a.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying the outer pair is reported when nested parentheses wrap a cast used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-002")]
    [Requirement("RH3002-006")]
    public async Task NestedParenthesesAroundCastInMemberAccessChainReportTheOuterPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;

                                    public string Run()
                                    {
                                        return {|#0:(((object)_a))|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;

                                     public string Run()
                                     {
                                         return ((object)_a).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the outer pair is reported when nested parentheses wrap a switch expression used as a member access receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-002")]
    [Requirement("RH3002-006")]
    public async Task NestedParenthesesAroundSwitchExpressionInMemberAccessChainReportTheOuterPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;

                                    public int Run()
                                    {
                                        return {|#0:((_a switch { _ => "a" }))|}.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;

                                     public int Run()
                                     {
                                         return (_a switch { _ => "a" }).Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying Fix All keeps the necessary pair of one chain and removes both pairs of another chain in the same statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesInSeveralChainsAreFixedTogether()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;
                                    private int _a;

                                    public string Run()
                                    {
                                        return ({|#0:(_o?._f)|}).ToString() + {|#1:({|#2:(_a)|})|}.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;
                                     private int _a;

                                     public string Run()
                                     {
                                         return (_o?._f).ToString() + _a.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifying two pairs of parentheses around an invoked delegate are not reported, because removing one pair leaves a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-004")]
    public async Task ParenthesesAroundDelegateInvokedThroughTwoPairsAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Func<int> _d;

                                    public int Run()
                                    {
                                        return ((_d))();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a field invocation are not reported when the field shares its name with its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundColorColorInvocationAreNotReported()
    {
        const string testCode = """
                                public enum Color
                                {
                                    Red
                                }

                                public class Test
                                {
                                    private System.Func<int, Color> Color;

                                    public Color Run()
                                    {
                                        return ((Color))(0);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a null-forgiving member access receiver are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundSuppressedReceiverAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public int Run()
                                    {
                                        return ((_s))!.Length;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a suppressed identifier are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundSuppressedIdentifierAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public string Run()
                                    {
                                        return ((_s))!;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around an invoked member access of names are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundQualifiedDelegateInvokedThroughTwoPairsAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private System.Func<int> _d;

                                    public int Run()
                                    {
                                        return ((_t._d))();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around the receiver of a with expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundWithExpressionReceiverAreNotReported()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _v;

                                    public Value Run()
                                    {
                                        return ((_v)) with { P = 1 };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around an awaited receiver of a with expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedWithExpressionReceiverAreNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Task<Value> _task;

                                    public async Task<Value> Run()
                                    {
                                        return await ((_task)) with { P = 1 };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a generic name followed by a bracket are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericNameBeforeBracketAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private static T Id<T>(T value) => value;

                                    public void Run()
                                    {
                                        var e = ((Id<int>))[0];
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying two pairs of parentheses around a tuple of names followed by a null-forgiving operator are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTupleOfNamesBeforeSuppressionAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public (int, int) Run()
                                    {
                                        return (((_i, _i)))!;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying only the innermost of three pairs around an invoked delegate is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TripleParenthesesAroundInvokedDelegateKeepTwoPairs()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Func<int> _d;

                                    public int Run()
                                    {
                                        return (({|#0:(_d)|}))();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Func<int> _d;

                                     public int Run()
                                     {
                                         return ((_d))();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying only the innermost of three pairs around a null-forgiving member access receiver is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TripleParenthesesAroundSuppressedReceiverKeepTwoPairs()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public int Run()
                                    {
                                        return (({|#0:(_s)|}))!.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;

                                     public int Run()
                                     {
                                         return ((_s))!.Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the inner pair around a this member access followed by a null-forgiving operator is still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundThisMemberBeforeSuppressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public int Run()
                                    {
                                        return ({|#0:(this._s)|})!.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;

                                     public int Run()
                                     {
                                         return (this._s)!.Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying nested parentheses around an array followed by an element access are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundArrayBeforeBracketAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int[] _arr;

                                    public int Run()
                                    {
                                        return {|#0:({|#1:(_arr)|})|}[0];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int[] _arr;

                                     public int Run()
                                     {
                                         return _arr[0];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying the inner pair before a query keyword is still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesBeforeQueryKeywordAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    private int[] _seq;

                                    public int[] Run()
                                    {
                                        return (from x in ({|#0:(_seq)|}) select x).ToArray();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 public class Test
                                 {
                                     private int[] _seq;

                                     public int[] Run()
                                     {
                                         return (from x in (_seq) select x).ToArray();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the inner pair before a switch expression is still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesBeforeSwitchExpressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public int Run()
                                    {
                                        return ({|#0:(_i)|}) switch { _ => 0 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public int Run()
                                     {
                                         return (_i) switch { _ => 0 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying parentheses inside a parenthesized member access followed by a null-forgiving operator are not reported, because removing them leaves a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InnerParenthesesInsideSuppressedReceiverChainAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private Value _v;
                                    private string _s;
                                    private int _i;
                                    private System.Func<int> _d;

                                    public object Run()
                                    {
                                        return ((_t)._s)!.Length;
                                    }
                                }

                                public struct Value
                                {
                                    public int P;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses inside a parenthesized member access followed by a with expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InnerParenthesesInsideWithExpressionReceiverChainAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private Value _v;
                                    private string _s;
                                    private int _i;
                                    private System.Func<int> _d;

                                    public object Run()
                                    {
                                        return ((_t)._v) with { P = 1 };
                                    }
                                }

                                public struct Value
                                {
                                    public int P;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an element of a parenthesized tuple followed by a null-forgiving operator are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InnerParenthesesInsideSuppressedTupleAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private Value _v;
                                    private string _s;
                                    private int _i;
                                    private System.Func<int> _d;

                                    public object Run()
                                    {
                                        return (((_i), _i))!;
                                    }
                                }

                                public struct Value
                                {
                                    public int P;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying the outer parentheses around an invoked member access are reported while the inner pair is kept until the outer pair is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OuterParenthesesAroundInvokedReceiverChainAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private Value _v;
                                    private string _s;
                                    private int _i;
                                    private System.Func<int> _d;

                                    public object Run()
                                    {
                                        return {|#0:((_t)._d)|}();
                                    }
                                }

                                public struct Value
                                {
                                    public int P;
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private Value _v;
                                     private string _s;
                                     private int _i;
                                     private System.Func<int> _d;

                                     public object Run()
                                     {
                                         return _t._d();
                                     }
                                 }

                                 public struct Value
                                 {
                                     public int P;
                                 }
                                 """;

        // The inner parentheses are reported only once the outer ones are gone, so they are not among the diagnostics of
        // the starting code that one Fix All application corrects (SHARED-FIX-002); the second pass corrects the
        // diagnostic the first one produces
        await Verify(testCode,
                     fixedCode,
                     static config =>
                            {
                                config.NumberOfIncrementalIterations = 2;
                                config.NumberOfFixAllIterations = 2;
                            },
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies parentheses around a name, around a pair that is not reported itself, and around the operand of
    /// <c>await</c> are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-002")]
    public async Task ParenthesesAroundPrimaryReceiversAndAwaitOperandAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task<int> M(string s, Task<int> t)
                                    {
                                        var n = {|RH3002:(s)|}.Length;
                                        var c = {|RH3002:(((object)s))|}.ToString();
                                        return await {|RH3002:(t)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 class C
                                 {
                                     async Task<int> M(string s, Task<int> t)
                                     {
                                         var n = s.Length;
                                         var c = ((object)s).ToString();
                                         return await t;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around a binary, cast, <c>switch</c>, or conditional expression as a receiver are not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-002")]
    public async Task ParenthesesAroundNonPrimaryReceiversAreNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    string M(int a, int b, object o, bool c)
                                    {
                                        var y = (a + b).ToString();
                                        var z = ((int)o).ToString();
                                        var w = (a switch { 1 => "a", _ => "b" }).Length;
                                        return (c ? "a" : "b").ToString();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies parentheses around a lambda, a cast, and a <c>switch</c> expression as variable initializers are not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-003")]
    public async Task ParenthesesAroundLambdaCastAndSwitchInitializersAreNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(int a, object o)
                                    {
                                        Func<int> f = (() => a);
                                        var x = ((int)o);
                                        var y = (a switch { 1 => "a", _ => "b" });
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies neither pair around a delegate invoked through two pairs is reported, because removing the inner
    /// pair would leave a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-004")]
    public async Task PairsAroundDelegateThatWouldLeaveACastAreNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    System.Func<int> _d;

                                    int M()
                                    {
                                        return ((_d))();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that of nested pairs the innermost one stays around a cast or a lambda, and the outermost one stays
    /// around a binary receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-006")]
    public async Task NestedPairsKeepOnePairAroundCastLambdaAndBinaryReceiver()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    string M(int a, int b, object o)
                                    {
                                        var x = {|RH3002:({|RH3002:(((int)o))|})|};
                                        Func<int> f = {|RH3002:((() => a))|};
                                        var s = {|RH3002:(((int)o))|}.ToString();
                                        return ({|RH3002:(a + b)|}).ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     string M(int a, int b, object o)
                                     {
                                         var x = ((int)o);
                                         Func<int> f = (() => a);
                                         var s = ((int)o).ToString();
                                         return (a + b).ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return {|#0:({|#1:(value)|})|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        // Both pairs are rewritten together, building the outer replacement from the already fixed inner pair
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}