using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Verifiers;

/// <summary>
/// Test methods for the warning-wave check on the code-fix paths of <see cref="AnalyzerTestsBase{TAnalyzer, TCodeFix}"/>:
/// <c>Verify</c>, which runs <see cref="CSharpCodeFixVerifierTest{TAnalyzer, TCodeFix}"/>, and the paths that apply the
/// fix to an ad-hoc document and return its text. Every path fails a test when the fix writes a warning-wave diagnostic
/// the source does not hold. The tests use <see cref="ParenthesisRemovalProbeCodeFixProvider"/>, which removes
/// parentheses without checking whether they were needed
/// </summary>
[TestClass]
public class CodeFixWarningWaveCheckBaseTests : BatchCodeFixTestsBase<ParenthesisRemovalProbeAnalyzer, ParenthesisRemovalProbeCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Source whose only parenthesized expression is a query expression standing as the operand of <c>throw</c>; removing
    /// the parentheses writes CS8848, a wave 5 warning
    /// </summary>
    private const string ParenthesizedQueryInThrow = """
                                                     using System;

                                                     internal class QueryableException : Exception
                                                     {
                                                         public QueryableException Select(Func<QueryableException, QueryableException> selector) => this;
                                                     }

                                                     internal class Test
                                                     {
                                                         private readonly QueryableException _exception = new QueryableException();

                                                         public int X => throw (from exception in _exception select exception);
                                                     }
                                                     """;

    /// <summary>
    /// <see cref="ParenthesizedQueryInThrow"/> with the parentheses removed
    /// </summary>
    private const string UnparenthesizedQueryInThrow = """
                                                       using System;

                                                       internal class QueryableException : Exception
                                                       {
                                                           public QueryableException Select(Func<QueryableException, QueryableException> selector) => this;
                                                       }

                                                       internal class Test
                                                       {
                                                           private readonly QueryableException _exception = new QueryableException();

                                                           public int X => throw from exception in _exception select exception;
                                                       }
                                                       """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Asserts that the action fails with a message that names CS8848
    /// </summary>
    /// <param name="action">Action</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task AssertFailsWithCS8848(Func<Task> action)
    {
        var exception = await Assert.ThrowsAsync<Exception>(action);

        Assert.Contains("CS8848", exception.Message);
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that <c>Verify</c> fails when the fixed state holds CS8848 and the source state does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFailsWhenTheFixWritesAWarningWaveDiagnostic()
    {
        var source = ParenthesizedQueryInThrow.Replace("throw (from exception in _exception select exception)", "throw {|#0:(from exception in _exception select exception)|}");

        await AssertFailsWithCS8848(() => Verify(source, UnparenthesizedQueryInThrow, Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, AnalyzerResources.RH3002MessageFormat)));
    }

    /// <summary>
    /// Verifies that <c>ApplyCodeFixAsync</c> fails when the text the fix returns holds CS8848 and the source does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ApplyCodeFixFailsWhenTheFixWritesAWarningWaveDiagnostic()
    {
        await AssertFailsWithCS8848(() => ApplyCodeFixAsync(ParenthesizedQueryInThrow));
    }

    /// <summary>
    /// Verifies that <c>TryApplyCodeFixAtAsync</c> fails when the text the fix returns holds CS8848 and the source does
    /// not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TryApplyCodeFixAtFailsWhenTheFixWritesAWarningWaveDiagnostic()
    {
        await AssertFailsWithCS8848(() => TryApplyCodeFixAtAsync(ParenthesizedQueryInThrow,
                                                                 RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId,
                                                                 root => root.DescendantNodes().First(node => node.IsKind(SyntaxKind.ParenthesizedExpression)).GetLocation()));
    }

    /// <summary>
    /// Verifies that <c>VerifyCodeFixIsWithheldOrProducesOneOf</c> fails when the accepted text the fix returns holds
    /// CS8848 and the source does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixIsWithheldOrProducesOneOfFailsWhenTheFixWritesAWarningWaveDiagnostic()
    {
        var source = ParenthesizedQueryInThrow.Replace("throw (from exception in _exception select exception)", "throw {|RH3002:(from exception in _exception select exception)|}");

        await AssertFailsWithCS8848(() => VerifyCodeFixIsWithheldOrProducesOneOf(source, false, UnparenthesizedQueryInThrow));
    }

    /// <summary>
    /// Verifies that <c>ApplyCodeFixAsync</c> returns the fixed text when the source already holds CS8848 and the fix
    /// leaves it in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ApplyCodeFixPassesWhenTheSourceAlreadyHoldsTheWarningWaveDiagnostic()
    {
        var source = UnparenthesizedQueryInThrow.Replace("public int X", "public int Y => (1);\n\n    public int X");
        var fixedSource = UnparenthesizedQueryInThrow.Replace("public int X", "public int Y => 1;\n\n    public int X");

        Assert.AreEqual(fixedSource, await ApplyCodeFixAsync(source));
    }

    /// <summary>
    /// Verifies that <c>ApplyCodeFixAsync</c> returns the fixed text when the fix writes an ordinary warning of warning
    /// level 1 to 4 (CS0184: the string concatenation is never an <see cref="int"/>)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ApplyCodeFixPassesWhenTheFixWritesAnOrdinaryWarning()
    {
        const string source = """
                              internal class Test
                              {
                                  public object Get(object value) => "a" + (value is int);
                              }
                              """;
        const string fixedSource = """
                                   internal class Test
                                   {
                                       public object Get(object value) => "a" + value is int;
                                   }
                                   """;

        Assert.AreEqual(fixedSource, await ApplyCodeFixAsync(source));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              internal class Test
                              {
                                  public int X => {|#0:(1)|};

                                  public int Y => {|#1:(2)|};
                              }
                              """;
        const string fixedSource = """
                                   internal class Test
                                   {
                                       public int X => 1;

                                       public int Y => 2;
                                   }
                                   """;

        return new FixAllScenario(source,
                                  fixedSource,
                                  Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, AnalyzerResources.RH3002MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}