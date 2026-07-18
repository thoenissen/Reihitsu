using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #423: <see cref="LineBreakDetection.IsMultiLine"/> measured
/// <c>FullSpan</c>, so a blank line or comment line directly above an otherwise single-line
/// node made it count as multi-line, causing single-line ternaries and lists to explode
/// </summary>
[TestClass]
public class LeadingTriviaMultiLineDetectionTests
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
    /// Asserts that the line-break phase leaves the input unchanged because the construct is
    /// already single-line and must not be exploded due to leading trivia
    /// </summary>
    /// <param name="input">The C# source text</param>
    private void AssertUnchanged(string input)
    {
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        Assert.AreEqual(input, actual, "A single-line construct preceded by trivia containing a line break must not be exploded.");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a single-line ternary preceded by a comment line is not exploded onto separate lines
    /// </summary>
    [TestMethod]
    public void TernaryWithLeadingCommentLineStaysSingleLine()
    {
        const string input = """
                             public class C
                             {
                                 int M(bool cond, int a, int b)
                                 {
                                     var x =
                                         // note
                                         cond ? a : b;

                                     return x;
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a single-line argument list is not forced one-argument-per-line when the
    /// last argument carries a trailing comment that pushes its own trivia onto a new line
    /// </summary>
    [TestMethod]
    public void ArgumentListWithTrailingCommentOnLastArgumentStaysSingleLine()
    {
        const string input = """
                             public class C
                             {
                                 void M()
                                 {
                                     Foo(a, b // note
                                         );
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    #endregion // Tests
}