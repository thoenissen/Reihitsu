using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Line-join operations must never remove the end-of-line that terminates a single-line
/// comment, which would absorb the joined token into the comment
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

    /// <summary>
    /// Asserts that the formatter refuses the input as unsupported before the line-break phase runs: the input is returned
    /// unchanged with the given reason
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="reason">The reason the formatter has to give</param>
    private void AssertRefusedAsUnsupported(string input, string reason)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(input, result.Output.ToString(), "The formatter must return a refused input unchanged.");
        Assert.AreEqual(reason, result.Unsupported?.Reason);
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a chain dot is not collapsed into a trailing comment on the previous line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void ChainDotIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             var x = Foo() // note
                                 .Bar();
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a comment behind an assignment operator is refused as unsupported before the line-break phase runs, so the
    /// join behind the operator never meets it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void CommentBehindAssignmentOperatorIsRefusedBeforeTheJoin()
    {
        const string input = """
                             x = // note
                                 5;
                             """;

        AssertRefusedAsUnsupported(input, "a comment or directive behind an assignment operator or the = of a declaration");
    }

    /// <summary>
    /// Verifies that an assignment operator is not collapsed up into a trailing comment on the target line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void AssignmentOperatorIsNotCollapsedIntoTrailingComment()
    {
        const string input = """
                             x // note
                                 = 5;
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a comment behind the equals token of a variable declarator is refused as unsupported before the line-break
    /// phase runs, with the reason of a comment inside a variable declaration, so the join behind the equals token never meets it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-020")]
    public void CommentBehindVariableInitializerEqualsIsRefusedBeforeTheJoin()
    {
        const string input = """
                             var value = // note
                                 "test";
                             """;

        AssertRefusedAsUnsupported(input, "a comment between a variable declaration's own tokens");
    }

    /// <summary>
    /// Verifies that the first argument is not collapsed into a trailing comment after the open parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
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
    [Requirement("FMT-BREAK-001")]
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
    /// regression (the placement loop previously spun forever because it reported a change unconditionally).
    /// The timeout is cooperative, so it stops a reintroduced loop only because the placement loop checks the
    /// cancellation token on every pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
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
    /// leave the shape untouched. The timeout guards the non-termination regression. The timeout is cooperative,
    /// so it stops a reintroduced loop only because the placement loop checks the cancellation token on every pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
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