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
    /// Test context for the current test
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

    /// <summary>
    /// Verifies that a single-line-mode parameter attribute list followed by a comment line does not
    /// loop forever. The single-line placement wants to collapse the parameter onto the attribute line,
    /// but the comment on the following token forbids the join, so the placement must report no change
    /// and the phase must terminate leaving the shape untouched. The timeout guards the non-termination
    /// regression (the placement loop previously spun forever because it reported a change unconditionally)
    /// </summary>
    [TestMethod]
    [Timeout(30000, CooperativeCancellation = true)]
    public void SingleLineParameterAttributeFollowedByCommentDoesNotLoopForever()
    {
        const string input = """
                             public class C
                             {
                                 void M([NotNull]
                                     // legacy callers pass null
                                     string target)
                                 {
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a single-line-mode parameter attribute list followed by a preprocessor directive does
    /// not loop forever. As with the comment case, the directive forbids the single-line join (removing its
    /// terminating line break would invalidate the directive), so the placement must report no change and
    /// leave the shape untouched. The timeout guards the non-termination regression
    /// </summary>
    [TestMethod]
    [Timeout(30000, CooperativeCancellation = true)]
    public void SingleLineParameterAttributeFollowedByDirectiveDoesNotLoopForever()
    {
        const string input = """
                             public class C
                             {
                                 void M([NotNull]
                             #nullable enable
                                     string target)
                                 {
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    #endregion // Tests
}