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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying two pairs of parentheses around an invoked alias qualified name are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAliasQualifiedDelegateInvokedThroughTwoPairsAreNotReported()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class C
                                    {
                                        public static System.Func<int> D;
                                        public static int A;
                                        public static int Ci;
                                    }
                                }

                                public class Test
                                {
                                    private int B;
                                    private int G;
                                    private int A;

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var x = ((global::N.C.D))();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an alias qualified multiplication at the start of a statement are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAliasQualifiedPointerShapedTargetAtStatementStartAreNotReported()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class C
                                    {
                                        public static System.Func<int> D;
                                        public static int A;
                                        public static int Ci;
                                    }
                                }

                                public class Test
                                {
                                    private int B;
                                    private int G;
                                    private int A;

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        (global::N.C.A * B) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying the closing argument keeps its parentheses when an alias qualified middle argument continues the span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArgumentsJoiningThroughAliasQualifiedMiddleKeepTheClosingPair()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class C
                                    {
                                        public static System.Func<int> D;
                                        public static int A;
                                        public static int Ci;
                                    }
                                }

                                public class Test
                                {
                                    private int B;
                                    private int G;
                                    private int A;

                                    private void M3(bool x, int c, bool y)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M3({|#0:(G < A)|}, global::N.C.Ci, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     public class C
                                     {
                                         public static System.Func<int> D;
                                         public static int A;
                                         public static int Ci;
                                     }
                                 }

                                 public class Test
                                 {
                                     private int B;
                                     private int G;
                                     private int A;

                                     private void M3(bool x, int c, bool y)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M3(G < A, global::N.C.Ci, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the closing argument keeps its parentheses when an index operator follows its greater-than operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundArgumentsJoiningBeforeIndexOperatorKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M({|#0:(G < A)|}, (B > ^1));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private bool z;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     private void M3(bool first, int middle, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(G < A, (B > ^1));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the closing argument keeps its parentheses when the opening argument ends in the false branch of a conditional
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundOpeningArgumentEndingInConditionalKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M({|#0:(z ? x : G < A)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private bool z;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     private void M3(bool first, int middle, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(z ? x : G < A, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the closing argument keeps its parentheses when a middle argument becomes type shaped once its inner parentheses are gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundMiddleArgumentWithParenthesizedReceiverKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M3({|#0:(G < A)|}, {|#1:({|#2:(_t)|}.B)|}, (B > (7)));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private bool z;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     private void M3(bool first, int middle, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M3(G < A, _t.B, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 3));
    }

    /// <summary>
    /// Verifying parentheses around a generic shaped later tuple element are not reported, because removing them would declare a variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericShapedSecondTupleElementAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = (x, (A < B > c));
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a generic shaped first tuple element are not reported, because removing them would declare a variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericShapedFirstTupleElementAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = ((A < B > c), x);
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a multiplication tuple element are still reported and fixed, because a tuple element is never read as a pointer declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundPointerShapedTupleElementAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private bool z;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    private void M3(bool first, int middle, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = (x, {|#0:(A * B)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private bool z;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     private void M3(bool first, int middle, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         var t = (x, A * B);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a generic name with a predefined type argument are still reported and fixed before an assignment operator, because such a type argument list stays one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundDefiniteGenericNameAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    private static T Id<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        {|#0:(Id<int>)|} = 5;

                                        return null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     private static T Id<T>(T value) => value;

                                     public async Task<object> Run()
                                     {
                                         Id<int> = 5;

                                         return null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name followed by an is operator are still reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameBeforeIsOperatorAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    private static T Id<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        return await {|#0:(A<B>)|} is object;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     private int A;
                                     private int B;

                                     private static T Id<T>(T value) => value;

                                     public async Task<object> Run()
                                     {
                                         return await A<B> is object;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a closing tuple element are not reported when removing them would declare a generic variable in the tuple
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingTupleElementAfterBareOpeningElementAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = (x, G < A, (B > c));
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying only the opening tuple element's parentheses are reported when removing both pairs would declare a generic variable in the tuple
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundOpeningTupleElementBeforeClosingElementEndingInIdentifierKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = ({|#0:(G < A)|}, (B > c), x);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool x;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         var t = (G < A, (B > c), x);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a closing argument are not reported when removing them would declare a generic out variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingArgumentAfterOutOpeningArgumentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(out G < A, (B > c));
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a closing argument are not reported when removing them would declare a variable of a generic type pattern
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingArgumentAfterIsOpeningArgumentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(x is G < A, (B > c));
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a closing argument ending in an identifier are still reported and fixed when the opening argument cannot start a declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingArgumentEndingInIdentifierAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(G < A, {|#0:(B > c)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool x;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(G < A, B > c);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a generic name whose predefined type argument is nested are not reported before an assignment operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundGenericNameWithNestedPredefinedTypeArgumentBeforeAssignmentAreNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class H<T>
                                {
                                }

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    private static T Id<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        (Id<H<int>>) = 5;

                                        return null;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name whose predefined type argument is nested are not reported before an arithmetic operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameWithNestedPredefinedTypeArgumentAreNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class H<T>
                                {
                                }

                                public class Test
                                {
                                    private int A;
                                    private int B;

                                    private static T Id<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        return await (A<H<int>>) - 1;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying deeply nested parentheses around tuple elements are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DeeplyNestedParenthesesAroundTupleElementsAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public object Run(int a)
                                    {
                                        return (a, {|#15:((a, {|#14:((a, {|#13:((a, {|#12:((a, {|#11:((a, {|#10:((a, {|#9:((a, {|#8:((a, {|#7:((a, {|#6:((a, {|#5:((a, {|#4:((a, {|#3:((a, {|#2:((a, {|#1:((a, {|#0:((a, a))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public object Run(int a)
                                     {
                                         return (a, (a, (a, (a, (a, (a, (a, (a, (a, (a, (a, (a, (a, (a, (a, (a, (a, a)))))))))))))))));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 16));
    }

    /// <summary>
    /// Verifying parentheses around a closing tuple element are not reported when the opening element's name is qualified by a generic type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingTupleElementAfterGenericQualifiedOpeningElementAreNotReported()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class H<T>
                                    {
                                        public static int G;
                                    }
                                }

                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;
                                    private int y;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = (x, N.H<int>.G < A, (B > c), y);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a closing argument are not reported when removing them would declare a variable of a negated generic type pattern
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingArgumentAfterNegatedPatternOpeningArgumentAreNotReported()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class H<T>
                                    {
                                        public static int G;
                                    }
                                }

                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;
                                    private int y;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(x is not G < A, (B > c));
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a closing argument are still reported and fixed after a generic qualified opening argument that cannot start a declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingArgumentAfterGenericQualifiedOpeningArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class H<T>
                                    {
                                        public static int G;
                                    }
                                }

                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;
                                    private int y;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        M(N.H<int>.G < A, {|#0:(B > c)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     public class H<T>
                                     {
                                         public static int G;
                                     }
                                 }

                                 public class Test
                                 {
                                     private bool x;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;
                                     private int y;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         M(N.H<int>.G < A, B > c);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around tuple elements are still reported and fixed when the identifier after the greater-than operator is followed by an operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTupleElementsWithIdentifierFollowedByOperatorAreReportedAndFixed()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class H<T>
                                    {
                                        public static int G;
                                    }
                                }

                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;
                                    private int y;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = (x, {|#0:(G < A)|}, {|#1:(B > c + 1)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     public class H<T>
                                     {
                                         public static int G;
                                     }
                                 }

                                 public class Test
                                 {
                                     private bool x;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;
                                     private int y;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         var t = (x, G < A, B > c + 1);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around a first and a last tuple element are still reported and fixed, because the first element of a tuple is only read as a declaration when another element follows the declared variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundFirstAndLastTupleElementsAreReportedAndFixed()
    {
        const string testCode = """
                                namespace N
                                {
                                    public class H<T>
                                    {
                                        public static int G;
                                    }
                                }

                                public class Test
                                {
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;
                                    private int y;

                                    private void M(bool first, bool second)
                                    {
                                    }

                                    public void Run()
                                    {
                                        var t = ({|#0:(G < A)|}, {|#1:(B > c)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     public class H<T>
                                     {
                                         public static int G;
                                     }
                                 }

                                 public class Test
                                 {
                                     private bool x;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;
                                     private int y;

                                     private void M(bool first, bool second)
                                     {
                                     }

                                     public void Run()
                                     {
                                         var t = (G < A, B > c);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name with a pointer type argument are not reported before an arithmetic operator, because a pointer to a name does not make the type argument list definite
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameWithPointerTypeArgumentAreNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int B;

                                    private static T Gen<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        return await (Gen<B*>) - 1;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name with a nullable type argument are still reported and fixed before an arithmetic operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameWithNullableTypeArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int B;

                                    private static T Gen<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        return await {|#0:(Gen<B?>)|} - 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     private int B;

                                     private static T Gen<T>(T value) => value;

                                     public async Task<object> Run()
                                     {
                                         return await Gen<B?> - 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around an awaited generic name with an array type argument are still reported and fixed before an arithmetic operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitedGenericNameWithArrayTypeArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class Test
                                {
                                    private int B;

                                    private static T Gen<T>(T value) => value;

                                    public async Task<object> Run()
                                    {
                                        return await {|#0:(Gen<B[]>)|} - 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 public class Test
                                 {
                                     private int B;

                                     private static T Gen<T>(T value) => value;

                                     public async Task<object> Run()
                                     {
                                         return await Gen<B[]> - 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying deeply nested parentheses around relational tuple elements are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DeeplyNestedParenthesesAroundRelationalTupleElementsAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private void M(out bool value)
                                    {
                                        value = false;
                                    }

                                    public object Run(int x, int z, int w)
                                    {
                                        return (x, {|#19:(z < (w, {|#18:(z < (w, {|#17:(z < (w, {|#16:(z < (w, {|#15:(z < (w, {|#14:(z < (w, {|#13:(z < (w, {|#12:(z < (w, {|#11:(z < (w, {|#10:(z < (w, {|#9:(z < (w, {|#8:(z < (w, {|#7:(z < (w, {|#6:(z < (w, {|#5:(z < (w, {|#4:(z < (w, {|#3:(z < (w, {|#2:(z < (w, {|#1:(z < (w, {|#0:(z < (w, x))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private void M(out bool value)
                                     {
                                         value = false;
                                     }

                                     public object Run(int x, int z, int w)
                                     {
                                         return (x, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, x)))))))))))))))))))));
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 20));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying deeply nested parentheses around a relational out argument and its tuple elements are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DeeplyNestedParenthesesAroundRelationalOutArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private void M(out bool value)
                                    {
                                        value = false;
                                    }

                                    public object Run(int x, int z, int w)
                                    {
                                        M(out {|#19:(z < (w, {|#18:(z < (w, {|#17:(z < (w, {|#16:(z < (w, {|#15:(z < (w, {|#14:(z < (w, {|#13:(z < (w, {|#12:(z < (w, {|#11:(z < (w, {|#10:(z < (w, {|#9:(z < (w, {|#8:(z < (w, {|#7:(z < (w, {|#6:(z < (w, {|#5:(z < (w, {|#4:(z < (w, {|#3:(z < (w, {|#2:(z < (w, {|#1:(z < (w, {|#0:(z < (w, x))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|}))|});

                                        return null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private void M(out bool value)
                                     {
                                         value = false;
                                     }

                                     public object Run(int x, int z, int w)
                                     {
                                         M(out z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, z < (w, x)))))))))))))))))))));

                                         return null;
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 20));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around tuple elements are still reported and fixed when the opening element's name is qualified by this, which keeps the angle brackets relational operators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTupleElementsAfterThisQualifiedOpeningElementAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private Test Y() => _t;

                                    public void Run()
                                    {
                                        var t = (x, {|#0:(this.G < A)|}, {|#1:(B > c)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private Test Y() => _t;

                                     public void Run()
                                     {
                                         var t = (x, this.G < A, B > c);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying parentheses around tuple elements are still reported and fixed when the opening element's name is qualified by an invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundTupleElementsAfterInvocationQualifiedOpeningElementAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private Test Y() => _t;

                                    public void Run()
                                    {
                                        var t = (x, {|#0:(Y().G < A)|}, {|#1:(B > c)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private Test Y() => _t;

                                     public void Run()
                                     {
                                         var t = (x, Y().G < A, B > c);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the closing tuple element keeps its parentheses when the opening element's qualifier becomes a name once its own parentheses are gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundClosingTupleElementAfterParenthesizedQualifierOpeningElementKeepTheClosingPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _t;
                                    private bool x;
                                    private int G;
                                    private int A;
                                    private int B;
                                    private int c;

                                    private Test Y() => _t;

                                    public void Run()
                                    {
                                        var t = (x, {|#0:({|#1:(_t)|}.G < A)|}, (B > c));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _t;
                                     private bool x;
                                     private int G;
                                     private int A;
                                     private int B;
                                     private int c;

                                     private Test Y() => _t;

                                     public void Run()
                                     {
                                         var t = (x, _t.G < A, (B > c));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));

        AssertParseIsPreserved(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying a relational tuple element nest forty levels deep is analyzed within a bound that only an analysis deciding every pair more
    /// than once per evaluation exceeds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DeeplyNestedRelationalTupleElementsAreAnalyzedWithoutRepeatingDecisions()
    {
        const string testCode = """
                                public class Test
                                {
                                    private void M(out bool value)
                                    {
                                        value = false;
                                    }

                                    public object Run(int x, int z, int w)
                                    {
                                        return (x, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, x)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
                                    }
                                }
                                """;

        await AssertAnalyzedWithinBoundAsync(testCode, 40);
    }

    /// <summary>
    /// Verifying a relational out argument nest forty levels deep is analyzed within a bound that only an analysis deciding every pair more
    /// than once per evaluation exceeds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DeeplyNestedRelationalOutArgumentIsAnalyzedWithoutRepeatingDecisions()
    {
        const string testCode = """
                                public class Test
                                {
                                    private void M(out bool value)
                                    {
                                        value = false;
                                    }

                                    public object Run(int x, int z, int w)
                                    {
                                        M(out (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, x)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));

                                        return null;
                                    }
                                }
                                """;

        await AssertAnalyzedWithinBoundAsync(testCode, 40);
    }

    /// <summary>
    /// Verifying a relational assignment target nest at the start of a statement forty levels deep is analyzed within a bound that only an analysis deciding every pair more
    /// than once per evaluation exceeds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DeeplyNestedRelationalAssignmentTargetIsAnalyzedWithoutRepeatingDecisions()
    {
        const string testCode = """
                                public class Test
                                {
                                    private void M(out bool value)
                                    {
                                        value = false;
                                    }

                                    public object Run(int x, int z, int w)
                                    {
                                        (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, (z < (w, x)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))) = 1;

                                        return null;
                                    }
                                }
                                """;

        await AssertAnalyzedWithinBoundAsync(testCode, 40);
    }

    /// <summary>
    /// Verifies parentheses around a whole expression are reported and fixed in every position where the expression
    /// stands alone: an expression body of a member and of an accessor, a parameter default value, a variable
    /// initializer, an assignment, an argument, a tuple element, a return and a throw statement, the operand of a
    /// throw expression, and a pair directly inside another pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundAWholeExpressionAreReportedInEveryListedPosition()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    int _f;

                                    int P => {|RH3002:(_f)|};

                                    int Q { get => {|RH3002:(_f)|}; }

                                    int M(int a, string s, int d = {|RH3002:(1)|})
                                    {
                                        int b = {|RH3002:(a)|};
                                        b = {|RH3002:(a + 1)|};
                                        Console.WriteLine({|RH3002:(a)|});
                                        var t = ({|RH3002:(a)|}, 1);
                                        var u = {|RH3002:(s ?? throw {|RH3002:(new InvalidOperationException())|})|};
                                        return {|RH3002:({|RH3002:(b)|})|};
                                    }

                                    void Fail()
                                    {
                                        throw {|RH3002:(new InvalidOperationException())|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Threading.Tasks;

                                 class C
                                 {
                                     int _f;

                                     int P => _f;

                                     int Q { get => _f; }

                                     int M(int a, string s, int d = 1)
                                     {
                                         int b = a;
                                         b = a + 1;
                                         Console.WriteLine(a);
                                         var t = (a, 1);
                                         var u = s ?? throw new InvalidOperationException();
                                         return b;
                                     }

                                     void Fail()
                                     {
                                         throw new InvalidOperationException();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around a whole argument of an object creation are reported over the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundAnArgumentOfAnObjectCreationAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    C(int a)
                                    {
                                    }

                                    static C M(int a)
                                    {
                                        return new C({|RH3002:(a)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     C(int a)
                                     {
                                     }

                                     static C M(int a)
                                     {
                                         return new C(a);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around a whole argument of an element access are reported over the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundAnArgumentOfAnElementAccessAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int[] x, int a)
                                    {
                                        return x[{|RH3002:(a)|}];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int[] x, int a)
                                     {
                                         return x[a];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around the value after <c>=</c> in a field and in a property declaration are reported over
    /// the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundTheValueOfAMemberDeclarationAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int _f = {|RH3002:(1)|};

                                    int P { get; } = {|RH3002:(2)|};
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _f = 1;

                                     int P { get; } = 2;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around the expression body of a local function are reported over the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundTheExpressionBodyOfALocalFunctionAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        int L() => {|RH3002:(a)|};

                                        return L();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int a)
                                     {
                                         int L() => a;

                                         return L();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around the left side of an assignment are reported over the pair and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    [Requirement("RH3002-007")]
    public async Task ParenthesesAroundTheLeftSideOfAnAssignmentAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;

                                    void M()
                                    {
                                        {|RH3002:(_a)|} = 3;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _a;

                                     void M()
                                     {
                                         _a = 3;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a conditional expression as the operand of a throw expression keeps its parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-001")]
    public async Task ConditionalOperandOfThrowExpressionInExpressionBodyIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    int Fail(bool c) => throw (c ? new Exception() : new InvalidOperationException());
                                }
                                """;

        await Verify(testCode);
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
    /// Verifies that of two arguments that would join into a generic invocation, the first pair is reported and fixed
    /// and the second pair stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-004")]
    public async Task ArgumentsThatWouldJoinIntoAGenericNameKeepTheSecondPair()
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
                                        M({|RH3002:(G < A)|}, (B > (7)));
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

                                     void Run()
                                     {
                                         M(G < A, (B > (7)));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies parentheses around a ref conditional in front of an assignment operator stay, while parentheses
    /// around a name in front of an assignment operator are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-005")]
    public async Task OnlyTheAssignmentTargetThatTheOperatorWouldJoinKeepsItsParentheses()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;
                                    int _b;

                                    void M(bool c)
                                    {
                                        (c ? ref _a : ref _b) = 5;
                                        {|RH3002:(_a)|} = 6;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _a;
                                     int _b;

                                     void M(bool c)
                                     {
                                         (c ? ref _a : ref _b) = 5;
                                         _a = 6;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
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

    /// <summary>
    /// Verifies the fix inserts a space where the keyword would otherwise join the content, and keeps the comments
    /// in front of and after the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task FixSeparatesTheKeywordAndKeepsTheCommentsAroundTheParentheses()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return{|RH3002:(a)|};
                                    }

                                    int N(int a)
                                    {
                                        return /* x */ {|RH3002:(a)|} /* y */;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int a)
                                     {
                                         return a;
                                     }

                                     int N(int a)
                                     {
                                         return /* x */ a /* y */;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a comment directly inside the parentheses keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-009")]
    public async Task CommentAfterTheOpeningParenthesisIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(/* x */ a)|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that redundant parentheses in a statement condition, a lambda body, a <c>switch</c> expression
    /// arm, an attribute argument, a binary operand, and around a <c>typeof</c> receiver are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RedundantParenthesesOutsideTheListedPositionsAreNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    [Obsolete(("x"))]
                                    int M(int a, bool c)
                                    {
                                        if ((c)) { }
                                        while ((c)) { }
                                        Func<int, int> f = x => (x);
                                        var n = (typeof(int)).Name;
                                        var s = a switch { _ => (a) };
                                        return a + (a);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around a number, which would read as one identifier with <c>return</c>, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundANumberGetsOneSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run()
                                    {
                                        return{|#0:(42)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run()
                                     {
                                         return 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around an identifier that starts with a Unicode escape, which would read as one identifier with <c>return</c>, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundAUnicodeEscapeIdentifierGetsOneSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int a)
                                    {
                                        return{|#0:(\u0061)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int a)
                                     {
                                         return \u0061;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around a verbatim identifier, which reads as a separate token without a space, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundAVerbatimIdentifierGetsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int a)
                                    {
                                        return{|#0:(@a)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int a)
                                     {
                                         return@a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around a verbatim string, which reads as a separate token without a space, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundAVerbatimStringGetsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return{|#0:(@"s")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return@"s";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying the code fix of parentheses written directly behind <c>return</c> around a string, which reads as a separate token without a space, on the same shape the
    /// formatter's separator behind a created <c>return</c> decides
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-008")]
    public async Task ReturnWithoutSpaceBeforeParenthesesAroundAStringGetsNoSpace()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return{|#0:("s")|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return"s";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifies that a Fix All in the project, started from a document without a hiding carrier, removes the parentheses in that
    /// document and leaves a second document that holds one as it is, with its diagnostic still reported: in a build with
    /// <c>X</c>, the reported line is the content of a verbatim string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task FixAllInTheProjectLeavesADocumentWithAHidingCarrierUnchanged()
    {
        const string source = """
                              public class Test
                              {
                                  public int Run(int value)
                                  {
                                      return {|RH3002:(value)|};
                                  }
                              }
                              """;
        const string fixedSource = """
                                   public class Test
                                   {
                                       public int Run(int value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", CarrierDocumentSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", CarrierDocumentSource));

                         // The diagnostic in the carrier document stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                         test.NumberOfFixAllInDocumentIterations = 1;
                         test.NumberOfFixAllInProjectIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies that a Fix All in the solution, started from a document without a hiding carrier, removes the parentheses in that
    /// document and leaves a document of another project that holds a hiding carrier as it is, with its diagnostic still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task FixAllInTheSolutionLeavesADocumentWithAHidingCarrierInAnotherProjectUnchanged()
    {
        const string source = """
                              public class Test
                              {
                                  public int Run(int value)
                                  {
                                      return {|RH3002:(value)|};
                                  }
                              }
                              """;
        const string fixedSource = """
                                   public class Test
                                   {
                                       public int Run(int value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Other"].Sources.Add(("/Other/Test1.cs", CarrierDocumentSource));
                         test.FixedState.AdditionalProjects["Other"].Sources.Add(("/Other/Test1.cs", CarrierDocumentSource));

                         // The diagnostic in the carrier document stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                         test.NumberOfFixAllInDocumentIterations = 1;
                         test.NumberOfFixAllInProjectIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies that unnecessary parentheses in an <c>#if</c> condition in front of the closing brace of a class with
    /// a primary constructor are reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-007")]
    public async Task ParenthesesInIfConditionInFrontOfClosingBraceOfClassWithPrimaryConstructorAreReportedOnce()
    {
        const string testData = """
                                public class Service(int value)
                                {
                                    public int Value => value;
                                #if ({|#0:(A)|})
                                #endif
                                }
                                """;

        await Verify(testData, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifies that unnecessary parentheses in an <c>#if</c> condition in front of a class with a primary constructor
    /// are reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-007")]
    public async Task ParenthesesInIfConditionInFrontOfClassWithPrimaryConstructorAreReportedOnce()
    {
        const string testData = """
                                #if ({|#0:(A)|})
                                #endif
                                public class Service(int value)
                                {
                                    public int Value => value;
                                }
                                """;

        await Verify(testData, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a returned value whose excluded code stands behind <c>#if DEBUG</c> inside the
    /// parentheses are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyConditionalDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(a
                                #if DEBUG
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a returned value whose excluded code stands behind <c>#if DEBUG</c> inside the
    /// parentheses are reported, and no code fix is offered, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyConditionalDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFixWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(a
                                #if DEBUG
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, "\r\n"));
    }

    /// <summary>
    /// Verifying parentheses around a returned value whose guarded code is active, because the <c>#if</c> condition
    /// holds, are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyConditionalDirectiveWithActiveCodeInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(a
                                #if !NEVER
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a returned value whose excluded code stands behind <c>#if false</c> inside the
    /// parentheses are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyIfFalseDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return {|RH3002:(a
                                #if false
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a returned value split into an excluded <c>#if</c> branch and an active
    /// <c>#else</c> branch are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyElseDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        return {|RH3002:(
                                #if X
                                            a + b
                                #else
                                            a
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a returned value split into <c>#if</c>, <c>#elif</c>, and <c>#else</c> branches are
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyElifDirectiveInsideReturnedParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        return {|RH3002:(
                                #if X
                                            a + b
                                #elif Y
                                            a - b
                                #else
                                            a
                                #endif
                                            )|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an argument whose excluded code stands behind <c>#if</c> inside the parentheses are
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyConditionalDirectiveInsideArgumentParenthesesIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        N({|RH3002:(a
                                #if X
                                            + 1
                                #endif
                                            )|});
                                    }

                                    void N(int value)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an invocation receiver are reported, without a code fix, when the build leaves only a
    /// name between them because the <c>#if</c> code that would make them necessary is excluded
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyReceiverParenthesesAroundExcludedCodeAreReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    string M(int a)
                                    {
                                        return {|RH3002:(a
                                #if X
                                            + 1
                                #endif
                                            )|}.ToString();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying parentheses around an invocation receiver are not reported when the build defines the symbol whose
    /// <c>#if</c> code makes them necessary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyReceiverParenthesesAroundActiveBinaryCodeAreNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    string M(int a)
                                    {
                                        return (a
                                #if X
                                            + 1
                                #endif
                                            ).ToString();
                                    }
                                }
                                """;

        await Verify(testCode,
                     static test => test.SolutionTransforms.Add(static (solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                                    ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("X"))
                                                                                                    : solution));
    }

    /// <summary>
    /// Verifying parentheses around an invocation receiver whose active <c>#else</c> branch holds only a name are
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-010")]
    public async Task VerifyReceiverParenthesesAroundElseBranchNameAreReportedWithoutCodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        return {|RH3002:(
                                #if X
                                            a + b
                                #else
                                            a
                                #endif
                                            )|}.ToString();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifying Fix All removes a removable pair and leaves the reported pair with a conditional directive inside it
    /// unchanged in the same document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-009")]
    [Requirement("RH3002-010")]
    public async Task VerifyFixAllLeavesParenthesesWithConditionalDirectiveReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        var first = {|RH3002:(a)|};

                                        return {|RH3002:(first
                                #if DEBUG
                                            + 1
                                #endif
                                            )|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int a)
                                     {
                                         var first = a;

                                         return {|RH3002:(first
                                 #if DEBUG
                                             + 1
                                 #endif
                                             )|};
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostic whose fix is withheld stays and is marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                            });
    }

    #endregion // Tests
}