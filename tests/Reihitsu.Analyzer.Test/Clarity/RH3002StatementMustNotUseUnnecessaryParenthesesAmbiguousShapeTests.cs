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
/// Test methods for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/> on parentheses whose removal would let the parser read pointer-shaped, generic-shaped, or tuple-shaped code differently
/// </summary>
[TestClass]
public class RH3002StatementMustNotUseUnnecessaryParenthesesAmbiguousShapeTests : BatchCodeFixTestsBase<RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

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

    /// <summary>
    /// Assert that analyzing the code reports the expected number of diagnostics before a generous time bound. The bound
    /// is two orders of magnitude above the analysis time of the expected behavior and only guards against an analysis
    /// whose time grows exponentially with the nesting depth, which would not finish at all.
    /// </summary>
    /// <param name="code">Code</param>
    /// <param name="expectedCount">Expected number of diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertAnalyzedWithinBoundAsync(string code, int expectedCount)
    {
        var cancellationToken = TestContext.CancellationToken;
        var analysis = Task.Run(async () =>
                                {
                                    var compilation = CSharpCompilation.Create("Test", [CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken)]);
                                    var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation, [new RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer()], (AnalyzerOptions)null);
                                    var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken)
                                                                                    .ConfigureAwait(false);

                                    return diagnostics.Length;
                                },
                                cancellationToken);
        var completed = await Task.WhenAny(analysis, Task.Delay(TimeSpan.FromSeconds(30), cancellationToken)).ConfigureAwait(false);

        Assert.AreSame(analysis, completed, "The analysis did not finish within the bound.");
        Assert.AreEqual(expectedCount, await analysis.ConfigureAwait(false));
    }

    #endregion // Methods

    #region Tests

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 3));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 3));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 16));
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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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

        await Verify(testCode, fixedCode, static config => config.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat));

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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 20));

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
                     Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 20));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));

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

        await Verify(testCode, fixedCode, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));

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

        await Verify(testCode, fixedCode, static config => config.NumberOfFixAllIterations = 1, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, MessageFormat, 2));

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