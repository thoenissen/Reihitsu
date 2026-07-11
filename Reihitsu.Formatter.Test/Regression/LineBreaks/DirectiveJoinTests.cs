using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #406: line-join operations must never remove the end-of-line that
/// terminates a preprocessor directive, which would re-emit the directive mid-line (CS1040).
/// These mirror <see cref="CommentJoinTests"/> for the directive family that previously passed the
/// comment-only join guard
/// </summary>
[TestClass]
public class DirectiveJoinTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Executes the <see cref="LineBreakPhase"/> on the given input, parsing with <c>DEBUG</c>
    /// defined so the conditional branch stays active and the directive trivia sits in the join gap
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var parseOptions = new CSharpParseOptions(preprocessorSymbols: new[] { "DEBUG" });
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = new LineBreakPhase().Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Asserts that the line-break phase leaves the input unchanged because a preprocessor directive
    /// in the join gap forbids the collapse
    /// </summary>
    /// <param name="input">The C# source text</param>
    private void AssertUnchanged(string input)
    {
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        Assert.AreEqual(input, actual, "The join must be skipped so the directive is preserved on its own line.");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a binary operator right operand is not collapsed across a directive
    /// </summary>
    [TestMethod]
    public void BinaryOperatorIsNotCollapsedAcrossDirective()
    {
        const string input = """
                             var x = a +
                             #if DEBUG
                                 b;
                             #endif
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that the ternary when-true branch is not collapsed across a directive after the question token
    /// </summary>
    [TestMethod]
    public void TernaryQuestionIsNotCollapsedAcrossDirective()
    {
        const string input = """
                             var x = cond ?
                             #if DEBUG
                                 a
                             #endif
                                 : b;
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that the ternary when-false branch is not collapsed across a directive after the colon token
    /// </summary>
    [TestMethod]
    public void TernaryColonIsNotCollapsedAcrossDirective()
    {
        const string input = """
                             var x = cond
                                 ? a :
                             #if DEBUG
                                 b;
                             #endif
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that an assignment value is not collapsed across a directive after the operator
    /// </summary>
    [TestMethod]
    public void AssignmentValueIsNotCollapsedAcrossDirective()
    {
        const string input = """
                             x =
                             #if DEBUG
                                 5;
                             #endif
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that an assignment operator is not collapsed up across a directive on the target line
    /// </summary>
    [TestMethod]
    public void AssignmentOperatorIsNotCollapsedAcrossDirective()
    {
        const string input = """
                             x
                             #if DEBUG
                                 = 5;
                             #endif
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a variable initializer is not collapsed across a directive after the equals token
    /// </summary>
    [TestMethod]
    public void VariableInitializerIsNotCollapsedAcrossDirective()
    {
        const string input = """
                             var value =
                             #if DEBUG
                                 "test";
                             #endif
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a method parameter-list opener is not collapsed onto the declaration line across a directive
    /// </summary>
    [TestMethod]
    public void ParameterListOpenerIsNotCollapsedAcrossDirective()
    {
        const string input = """
                             class C
                             {
                                 void M
                             #if DEBUG
                                     (int a)
                             #endif
                                 {
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that an expression-bodied property is not collapsed across a directive
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedPropertyIsNotCollapsedAcrossDirective()
    {
        const string input = """
                             class C
                             {
                                 int X =>
                             #if DEBUG
                                     1;
                             #endif
                             }
                             """;

        AssertUnchanged(input);
    }

    #endregion // Tests
}