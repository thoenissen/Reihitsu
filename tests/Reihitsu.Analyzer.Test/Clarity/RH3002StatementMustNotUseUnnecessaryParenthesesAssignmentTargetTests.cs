using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/> on parentheses around the target of an assignment
/// </summary>
[TestClass]
public class RH3002StatementMustNotUseUnnecessaryParenthesesAssignmentTargetTests : BatchCodeFixTestsBase<RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    #region Methods

    /// <summary>
    /// Assert that the fixed code parses into the same syntax kinds as the tested code once the parenthesized expressions
    /// are left out of both, so that a fix cannot pass by producing the expected text with a different reading
    /// </summary>
    /// <param name="testCode">Tested code, including diagnostic markup</param>
    /// <param name="fixedCode">Fixed code</param>
    private static void AssertParseIsPreserved(string testCode, string fixedCode)
    {
        var source = Regex.Replace(testCode, @"\{\|#\d+:|\|\}", string.Empty, RegexOptions.None, Regex.InfiniteMatchTimeout);

        Assert.AreSequenceEqual(GetSyntaxKindsWithoutParentheses(source), GetSyntaxKindsWithoutParentheses(fixedCode));
    }

    /// <summary>
    /// Get the kinds of all syntax nodes of the code except the parenthesized expressions
    /// </summary>
    /// <param name="code">Code</param>
    /// <returns>The syntax kinds in document order</returns>
    private static List<SyntaxKind> GetSyntaxKindsWithoutParentheses(string code)
    {
        return CSharpSyntaxTree.ParseText(code)
                               .GetRoot()
                               .DescendantNodesAndSelf()
                               .Where(static node => node is not ParenthesizedExpressionSyntax)
                               .Select(static node => node.Kind())
                               .ToList();
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifying parentheses around a ref conditional simple assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-005")]
    public async Task ParenthesesAroundRefConditionalAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional compound assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalCompoundAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) += 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional coalesce assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalCoalesceAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int? _a;
                                    private int? _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) ??= 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional simple assignment target are not reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalAssignmentTargetAreNotReportedWithCrLf()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifying parentheses around a ref assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        ref int r = ref _a;

                                        (r = ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional target of a chained assignment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRefConditionalTargetOfChainedAssignmentAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private int _c;

                                    public void Run(bool condition)
                                    {
                                        _c = (condition ? ref _a : ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a ref conditional assignment target containing a comment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCommentedRefConditionalAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (/* note */ condition ? ref _a : ref _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying parentheses around a non-ref conditional assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (condition ? _a : _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a value assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundValueAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        (_a = _b) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a query expression assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundQueryAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    private int[] _xs;

                                    public void Run(bool condition)
                                    {
                                        (from x in _xs select x) = null;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a conditional access assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAccessAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a conditional element access compound assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAccessElementAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int[] _arr;

                                    public void Run(bool condition)
                                    {
                                        (_arr?[0]) += 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a prefix unary assignment target whose operand is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundNegatedConditionalAccessAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (-_o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a binary assignment target whose right operand is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundBinaryAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a + _o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a coalesce assignment target whose right operand is a query expression are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundBinaryAssignmentTargetEndingInQueryAreNotReported()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    private System.Collections.Generic.IEnumerable<int> _xs;

                                    public void Run(bool condition)
                                    {
                                        (_xs ?? from x in _xs select x) = null;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a binary assignment target whose right operand is a cast of a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCastAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a + (int)_o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around an await assignment target whose operand is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundAwaitAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run(bool condition)
                                    {
                                        (await _o?._t) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a coalesce assignment target whose right operand throws a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundThrowAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        (_n ?? throw _o?._e) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a range assignment target whose right operand is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRangeAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a.._o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an identifier assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-005")]
    public async Task UnnecessaryParenthesesAroundAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a ref returning invocation assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRefReturningInvocationAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    private ref int GetRef() => ref _a;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(GetRef())|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     private ref int GetRef() => ref _a;

                                     public void Run(bool condition)
                                     {
                                         GetRef() = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a member access of a parenthesized conditional access assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundMemberAccessOfConditionalAccessAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:((_o?._o)._f)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         (_o?._o)._f = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying nested unnecessary parentheses around an identifier assignment target are both reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedUnnecessaryParenthesesAroundAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:({|#1:(_a)|})|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a prefix unary assignment target whose operand is parenthesized are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundNegatedParenthesizedConditionalAccessAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(-(_o?._f))|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         -(_o?._f) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a binary assignment target whose left operand is a conditional access are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundBinaryAssignmentTargetStartingWithConditionalAccessAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_o?._f + _a)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         _o?._f + _a = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a ref conditional assignment value are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRefConditionalAssignmentValueAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private int _c;

                                    public void Run(bool condition)
                                    {
                                        _c = {|#0:(condition ? ref _a : ref _b)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;
                                     private int _c;

                                     public void Run(bool condition)
                                     {
                                         _c = condition ? ref _a : ref _b;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the assignment value parentheses are reported when the target is a parenthesized ref conditional
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OnlyTheAssignmentValueParenthesesAreReportedNextToARefConditionalTarget()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private int _c;

                                    public void Run(bool condition)
                                    {
                                        (condition ? ref _a : ref _b) = {|#0:(condition ? _c : 5)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;
                                     private int _c;

                                     public void Run(bool condition)
                                     {
                                         (condition ? ref _a : ref _b) = condition ? _c : 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a ref conditional assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3002-006")]
    public async Task NestedParenthesesAroundRefConditionalAssignmentTargetKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        ({|#0:(condition ? ref _a : ref _b)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         (condition ? ref _a : ref _b) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying both redundant inner pairs are reported and fixed when three pairs wrap a ref conditional assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TripleParenthesesAroundRefConditionalAssignmentTargetKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        ({|#0:({|#1:(condition ? ref _a : ref _b)|})|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         (condition ? ref _a : ref _b) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a conditional access operand of a throw expression stay reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundConditionalAccessInThrowExpressionAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private System.Exception _e;
                                    private object _x;

                                    public object Get() => _x ?? throw {|#0:(_o?._e)|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private System.Exception _e;
                                     private object _x;

                                     public object Get() => _x ?? throw _o?._e;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a null-forgiving conditional access assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundSuppressedConditionalAccessAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_o?._f!) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a prefix unary assignment target whose operand is a null-forgiving conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundNegatedSuppressedConditionalAccessAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (-_o?._f!) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a null-forgiving conditional access assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedParenthesesAroundSuppressedConditionalAccessAssignmentTargetKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        ({|#0:(_o?._f!)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         (_o?._f!) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a null-forgiving identifier assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundSuppressedIdentifierAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _s;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_s!)|} = null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _s;

                                     public void Run(bool condition)
                                     {
                                         _s! = null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a null-forgiving assignment target whose operand is parenthesized are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundSuppressedParenthesizedConditionalAccessAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:((_o?._f)!)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         (_o?._f)! = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a relational pattern assignment target whose expression is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundRelationalPatternAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a is > _o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a combined pattern assignment target whose right pattern ends in a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCombinedPatternAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a is > 1 and < _o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying parentheses around a negated constant pattern assignment target whose expression is a conditional access are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundNegatedPatternAssignmentTargetEndingInConditionalAccessAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        (_a is not _o?._f) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a relational pattern assignment target ending in a literal are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRelationalPatternAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a is > 5)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a is > 5 = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an assignment target ending in a parenthesized pattern are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundParenthesizedPatternAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;
                                    private Test _o;
                                    private int _f;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a is (> _o?._f))|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;
                                     private Test _o;
                                     private int _f;

                                     public void Run(bool condition)
                                     {
                                         _a is (> _o?._f) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying parentheses around a conditional access throw operand ending an assignment target are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAccessThrowOperandEndingAnAssignmentTargetAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        _n ?? throw (_o?._e) = 5;
                                    }
                                }
                                """;

        await Verify(testCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying only the throw operand parentheses are reported when an assignment target ends in a parenthesized conditional access throw operand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundCoalesceAssignmentTargetEndingInParenthesizedThrowOperandKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        (_n ?? throw {|#0:(_o?._e)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int? _n;
                                     private System.Exception _e;

                                     public void Run(bool condition)
                                     {
                                         (_n ?? throw _o?._e) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying only the redundant inner pair is reported when nested parentheses wrap a conditional access await operand ending an assignment target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ParenthesesAroundConditionalAccessAwaitOperandEndingAnAssignmentTargetKeepTheNecessaryPair()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private Test _o;
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run(bool condition)
                                    {
                                        _a + await ({|#0:(_o?._t)|}) = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private Test _o;
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task Run(bool condition)
                                     {
                                         _a + await (_o?._t) = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an await assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundAwaitAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run(bool condition)
                                    {
                                        {|#0:(await _t)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task Run(bool condition)
                                     {
                                         await _t = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a binary assignment target whose right operand is a cast are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundBinaryAssignmentTargetEndingInCastAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a + (int)_b)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a + (int)_b = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a coalesce assignment target whose right operand is a throw expression are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundCoalesceAssignmentTargetEndingInThrowAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private Test _o;
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_n ?? throw _e)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private Test _o;
                                     private int? _n;
                                     private System.Exception _e;

                                     public void Run(bool condition)
                                     {
                                         _n ?? throw _e = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around a range assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundRangeAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a.._b)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a.._b = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an assignment target that is a range without a right operand are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundOpenRangeAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _a;
                                    private int _b;

                                    public void Run(bool condition)
                                    {
                                        {|#0:(_a..)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _a;
                                     private int _b;

                                     public void Run(bool condition)
                                     {
                                         _a.. = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an identifier throw operand ending an assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundThrowOperandEndingAnAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int? _n;
                                    private System.Exception _e;

                                    public void Run(bool condition)
                                    {
                                        _n ?? throw {|#0:(_e)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int? _n;
                                     private System.Exception _e;

                                     public void Run(bool condition)
                                     {
                                         _n ?? throw _e = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around an identifier await operand ending an assignment target are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundAwaitOperandEndingAnAssignmentTargetAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private System.Threading.Tasks.Task<int> _t;

                                    public async System.Threading.Tasks.Task Run(bool condition)
                                    {
                                        await {|#0:(_t)|} = 5;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private System.Threading.Tasks.Task<int> _t;

                                     public async System.Threading.Tasks.Task Run(bool condition)
                                     {
                                         await _t = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses"));
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
                                  Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}