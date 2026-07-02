using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #226: line-join operations must never remove the end-of-line
/// that terminates a single-line comment, which would absorb the joined token into the comment
/// </summary>
[TestClass]
public class CommentJoinTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Executes the <see cref="LineBreakPhase"/> on the given input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = new LineBreakPhase().Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Asserts that the line-break phase leaves the input unchanged because a comment in the
    /// join gap forbids the collapse
    /// </summary>
    /// <param name="input">The C# source text</param>
    private void AssertUnchanged(string input)
    {
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        Assert.AreEqual(input, actual, "The join must be skipped so the comment is preserved.");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a chain dot is not collapsed into a trailing comment on the previous line
    /// </summary>
    [TestMethod]
    public void ChainDotIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             var x = Foo() // note
                                 .Bar();
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a binary operator is not collapsed into a trailing comment
    /// </summary>
    [TestMethod]
    public void BinaryOperatorIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             var x = a + // note
                                 b;
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that the ternary question token is not collapsed into a trailing comment
    /// </summary>
    [TestMethod]
    public void TernaryQuestionIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             var x = cond ? // note
                                 a
                                 : b;
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that the ternary colon token is not collapsed into a trailing comment
    /// </summary>
    [TestMethod]
    public void TernaryColonIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             var x = cond
                                 ? a : // note
                                 b;
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that an assignment value is not collapsed into a trailing comment after the operator
    /// </summary>
    [TestMethod]
    public void AssignmentValueIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             x = // note
                                 5;
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that an assignment operator is not collapsed up into a trailing comment on the target line
    /// </summary>
    [TestMethod]
    public void AssignmentOperatorIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             x // note
                                 = 5;
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a variable initializer is not collapsed into a trailing comment after the equals token
    /// </summary>
    [TestMethod]
    public void VariableInitializerIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             var value = // note
                                 "test";
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that the first argument is not collapsed into a trailing comment after the open parenthesis
    /// </summary>
    [TestMethod]
    public void FirstArgumentIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             Foo( // note
                                 arg);
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a method parameter-list opener is not collapsed into a trailing comment on the declaration line
    /// </summary>
    [TestMethod]
    public void ParameterListOpenerIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             class C
                             {
                                 void M // note
                                     (int a)
                                 {
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that an expression-bodied property is not collapsed into a trailing comment
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedPropertyIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             class C
                             {
                                 int X => // note
                                     1;
                             }
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that an auto-property accessor list is not collapsed into a trailing comment on the property header
    /// </summary>
    [TestMethod]
    public void AutoPropertyIsNotCollapsedIntoHeaderTrailingComment()
    {
        const string input = """
                             class C
                             {
                                 public int X // note
                                 {
                                     get;
                                     set;
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    #endregion // Tests
}