using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/>
/// </summary>
public partial class RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests
{
    #region Tests

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

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, testCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 3));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses at the start of a statement are not reported when their removal would declare a pointer local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedTargetAtStatementStartAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        (A * B) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a multiplication assignment target are still reported and fixed when they do not start the statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedTargetOfChainedAssignmentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;
                                    private int _i;

                                    public void Run()
                                    {
                                        _i = {|#0:(A * B)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;
                                     private int _i;

                                     public void Run()
                                     {
                                         _i = A * B = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses at the start of a statement are still reported and fixed when the inner expression cannot start a declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundMultiplicationWithLiteralAtStatementStartAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;

                                    public void Run()
                                    {
                                        {|#0:(A * 5)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;

                                     public void Run()
                                     {
                                         A * 5 = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a with expression at the start of a statement are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundWithExpressionTargetAtStatementStartAreNotReported()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _a;

                                    public void Run()
                                    {
                                        (_a with { }) = _a;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a with expression on an invocation at the start of a statement are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundInvocationWithExpressionTargetAtStatementStartAreReportedAndFixed()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _a;

                                    private Value M() => _a;

                                    public void Run()
                                    {
                                        {|#0:(M() with { })|} = _a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public struct Value
                                 {
                                     public int P;
                                 }

                                 public class Test
                                 {
                                     private Value _a;

                                     private Value M() => _a;

                                     public void Run()
                                     {
                                         M() with { } = _a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses at the start of a statement are not reported when their removal would declare a generic local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericShapedTargetAtStatementStartAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;
                                    private int c;

                                    public void Run()
                                    {
                                        (A < B > c) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a relational expression at the start of a statement are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRelationalTargetAtStatementStartAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        {|#0:(A < B)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     public void Run()
                                     {
                                         A < B = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a receiver at the start of a statement are not reported when their removal would declare a pointer local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundReceiverOfMemberAccessChainAtStatementStartAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test A;
                                    private int B;
                                    private int c;

                                    public void Run()
                                    {
                                        (A).B * c = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the first initializer of a for statement are not reported when their removal would declare a pointer local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedFirstForInitializerAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        for ((A * B) = 5; ;)
                                        {
                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a later initializer of a for statement are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedSecondForInitializerAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;
                                    private int _i;

                                    public void Run()
                                    {
                                        for (_i = 0, {|#0:(A * B)|} = 5; ;)
                                        {
                                            break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;
                                     private int _i;

                                     public void Run()
                                     {
                                         for (_i = 0, A * B = 5; ;)
                                         {
                                             break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around the resource of a using statement are not reported when their removal would declare a pointer local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedUsingResourceAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        using ((A * B) = 5)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an out argument are not reported when their removal would declare a pointer variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedOutArgumentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    private void O(out int value) => value = 0;

                                    public void Run()
                                    {
                                        O(out (A * B));
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an identifier out argument are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundIdentifierOutArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    private void O(out int value) => value = 0;

                                    public void Run()
                                    {
                                        O(out {|#0:(_i)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     private void O(out int value) => value = 0;

                                     public void Run()
                                     {
                                         O(out _i);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the inner of two pairs around a multiplication at the start of a statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundPointerShapedTargetAtStatementStartKeepOnePair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        ({|#0:(A * B)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     public void Run()
                                     {
                                         (A * B) = 5;
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the inner of two pairs around a with expression at the start of a statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundWithExpressionTargetAtStatementStartKeepOnePair()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _a;

                                    public void Run()
                                    {
                                        ({|#0:(_a with { })|}) = _a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public struct Value
                                 {
                                     public int P;
                                 }

                                 public class Test
                                 {
                                     private Value _a;

                                     public void Run()
                                     {
                                         (_a with { }) = _a;
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around the target of an initializer element are not reported when their removal would assign a member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundObjectInitializerElementTargetAreNotReported()
    {
        const string testCode = """
                                public class Bag : System.Collections.IEnumerable
                                {
                                    public int Value { get; set; }

                                    public void Add(int item)
                                    {
                                    }

                                    public System.Collections.IEnumerator GetEnumerator() => null;
                                }

                                public class Test
                                {
                                    public Bag Run()
                                    {
                                        return new Bag { (Value) = 1 };
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the target of a collection initializer element are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCollectionInitializerElementTargetAreNotReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                public class Test
                                {
                                    private int _i;

                                    public List<int> Run()
                                    {
                                        return new List<int> { (_i) = 1 };
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the target of an anonymous object member are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAnonymousObjectMemberTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public object Run()
                                    {
                                        return new { (_i) = 1 };
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the target of a with initializer element are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundWithInitializerElementTargetAreNotReported()
    {
        const string testCode = """
                                public struct Value
                                {
                                    public int P;
                                }

                                public class Test
                                {
                                    private Value _a;
                                    private int P;

                                    public Value Run()
                                    {
                                        return _a with { (P) = 1 };
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around the target of an array initializer element are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArrayInitializerElementTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public int[] Run()
                                    {
                                        int[] values = { {|#0:(_i)|} = 1 };

                                        return values;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public int[] Run()
                                     {
                                         int[] values = { _i = 1 };

                                         return values;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a member access target of a collection initializer element are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundMemberAccessCollectionInitializerElementTargetAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                public class Test
                                {
                                    private Test _t;
                                    private int _i;

                                    public List<int> Run()
                                    {
                                        return new List<int> { {|#0:(_t._i)|} = 1 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 public class Test
                                 {
                                     private Test _t;
                                     private int _i;

                                     public List<int> Run()
                                     {
                                         return new List<int> { _t._i = 1 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around the target of a compound assignment initializer element are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCompoundAssignedInitializerElementTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Bag : System.Collections.IEnumerable
                                {
                                    public int Value { get; set; }

                                    public void Add(int item)
                                    {
                                    }

                                    public System.Collections.IEnumerator GetEnumerator() => null;
                                }

                                public class Test
                                {
                                    public Bag Run()
                                    {
                                        return new Bag { {|#0:(Value)|} += 1 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Bag : System.Collections.IEnumerable
                                 {
                                     public int Value { get; set; }

                                     public void Add(int item)
                                     {
                                     }

                                     public System.Collections.IEnumerator GetEnumerator() => null;
                                 }

                                 public class Test
                                 {
                                     public Bag Run()
                                     {
                                         return new Bag { Value += 1 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying only the inner of two pairs around the target of a collection initializer element is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundCollectionInitializerElementTargetKeepOnePair()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                public class Test
                                {
                                    private int _i;

                                    public List<int> Run()
                                    {
                                        return new List<int> { ({|#0:(_i)|}) = 1 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 public class Test
                                 {
                                     private int _i;

                                     public List<int> Run()
                                     {
                                         return new List<int> { (_i) = 1 };
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a generic name followed by an assignment operator are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericNameAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        (A<B>) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a generic name inside a chained assignment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericNameTargetOfChainedAssignmentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;
                                    private int _i;

                                    public void Run()
                                    {
                                        _i = (A<B>) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name followed by an arithmetic operator are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameBeforeArithmeticOperatorAreNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public async Task<int> Run()
                                    {
                                        return await (A<B>) + 1;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name followed by a logical operator are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameBeforeLogicalOperatorAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public async Task<bool> Run()
                                    {
                                        return await {|#0:(A<B>)|} && true;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     public async Task<bool> Run()
                                     {
                                         return await A<B> && true;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a generic method group followed by a semicolon are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericMethodGroupBeforeSemicolonAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private static T Id<T>(T value) => value;

                                    public System.Func<int, int> Run()
                                    {
                                        return {|#0:(Id<int>)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private static T Id<T>(T value) => value;

                                     public System.Func<int, int> Run()
                                     {
                                         return Id<int>;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the inner of two pairs around a generic name followed by an assignment operator is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundGenericNameAssignmentTargetKeepOnePair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    public void Run()
                                    {
                                        ({|#0:(A<B>)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     public void Run()
                                     {
                                         (A<B>) = 5;
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the opening argument's parentheses are reported when removing both pairs would join the arguments into a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-004")]
    public async Task ParenthesesAroundArgumentsJoiningIntoGenericInvocationKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M({|#0:(G < A)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(G < A, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a closing argument are not reported when the opening argument has no parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingArgumentAfterBareOpeningArgumentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(G < A, (B > (7)));
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an opening argument are not reported when the closing argument has no parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundOpeningArgumentBeforeBareClosingArgumentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M((G < A), B > (7));
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around both arguments are still reported and fixed when no parenthesis follows the greater-than operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArgumentsWithoutTypeArgumentFollowerAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M({|#0:(G < A)|}, {|#1:(B > 7)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(G < A, B > 7);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying parentheses around named arguments are still reported and fixed, because the name keeps the arguments apart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundNamedArgumentsJoiningIntoGenericInvocationAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(x: {|#0:(G < A)|}, y: {|#1:(B > (7))|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(x: G < A, y: B > (7));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying the opening and the type shaped middle arguments are reported while the closing argument keeps its parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTypeShapedMiddleArgumentKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M3({|#0:(G < A)|}, {|#1:(C)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M3(G < A, C, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying every argument is reported and fixed when a literal middle argument keeps the arguments apart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundLiteralMiddleArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M3({|#0:(G < A)|}, {|#1:(1)|}, {|#2:(B > (7))|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M3(G < A, 1, B > (7));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 3));
    }

    /// <summary>
    /// Verifying only the opening tuple element's parentheses are reported when removing both pairs would join the elements into a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTupleElementsJoiningIntoGenericInvocationKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int C;

                                    private void M(bool x, bool y)
                                    {
                                    }

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = ({|#0:(G < A)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int C;

                                     private void M(bool x, bool y)
                                     {
                                     }

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         var t = (G < A, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying no space is added after an interpolated string whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundInterpolatedStringAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:($"x{_i}")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying no space is added after an interpolated string whose parentheses are removed with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundInterpolatedStringCrlfAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:($"x{_i}")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying no space is added after a verbatim interpolated string whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundVerbatimInterpolatedStringAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:($@"x{_i}")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $@"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying no space is added after a raw interpolated string whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRawInterpolatedStringAddNoSpace()
    {
        const string testCode = """"
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:($"""x{_i}""")|};
                                    }
                                }
                                """";

        const string fixedCode = """"
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $"""x{_i}""";
                                     }
                                 }
                                 """";

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying no space is added after an interpolated string argument whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundInterpolatedStringArgumentAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = string.Concat({|#0:($"x{_i}")|}, "y");
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = string.Concat($"x{_i}", "y");
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying no space is added after an interpolated string member access receiver whose inner parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundInterpolatedStringReceiverAddNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var n = ({|#0:($"x{_i}")|}).Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var n = ($"x{_i}").Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying nested parentheses around an interpolated string are removed together without adding a space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundInterpolatedStringAreFixedWithoutSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public void Run()
                                    {
                                        var s = {|#0:({|#1:($"x{_i}")|})|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public void Run()
                                     {
                                         var s = $"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying no space is added after a multi-line raw interpolated string whose parentheses are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundMultiLineRawInterpolatedStringAddNoSpace()
    {
        const string testCode = """"
                                public class Test
                                {
                                    private int _i;

                                    public string Run()
                                    {
                                        return {|#0:($"""
                                               x{_i}
                                               """)|};
                                    }
                                }
                                """";

        const string fixedCode = """"
                                 public class Test
                                 {
                                     private int _i;

                                     public string Run()
                                     {
                                         return $"""
                                                x{_i}
                                                """;
                                     }
                                 }
                                 """";

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying no space is added on either side when parentheses around an interpolated string directly follow a return keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReturnWithoutSpaceBeforeInterpolatedStringParenthesesAddsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _i;

                                    public string Run()
                                    {
                                        return{|#0:($"x{_i}")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _i;

                                     public string Run()
                                     {
                                         return$"x{_i}";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a receiver at the start of a statement are still reported and fixed when the remaining tokens cannot start a declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundReceiverOfMemberAssignmentAtStatementStartAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private int _i;

                                    public void Run()
                                    {
                                        {|#0:(_t)|}._i = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private int _i;

                                     public void Run()
                                     {
                                         _t._i = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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

    #endregion // Tests
}