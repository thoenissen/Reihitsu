using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLineRegionDirectiveRewriter"/> in isolation, without the <c>BlankLineCollapser</c>
/// subphase that normally runs after it in <see cref="BlankLinePhase"/>. Isolation matters here because the
/// collapser would silently absorb a double blank-line insertion, masking a regression where the rewriter
/// mistakes an existing blank line for a missing one (issue #428 review)
/// </summary>
[TestClass]
public class BlankLineRegionDirectiveRewriterTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an already-correct blank line before a <c>#region</c> directive is left as a single blank
    /// line, even though the same leading trivia also requires a blank line to be inserted after the directive.
    /// The "after" insertion replaces the token with a detached copy before the "before" check runs; re-deriving
    /// the previous token from that detached copy must not undercount the existing blank line and double it
    /// (issue #428 review)
    /// </summary>
    [TestMethod]
    public void ExistingBlankLineBeforeRegionIsNotDuplicatedWhenBlankLineAfterIsAlsoInserted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a;

                                 #region R
                                 private int _b;
                                 #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a;

                                    #region R

                                    private int _b;
                                    #endregion
                                }
                                """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Applies <see cref="BlankLineRegionDirectiveRewriter"/> to the given source text
    /// </summary>
    /// <param name="source">The source text to rewrite</param>
    /// <returns>The rewritten source text</returns>
    private static string ApplyRewriter(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var context = new FormattingContext(Environment.NewLine);
        var editor = new BlankLineEditor(context);
        var rewriter = new BlankLineRegionDirectiveRewriter(context, editor, CancellationToken.None);
        var result = rewriter.Visit(tree.GetRoot());

        return result.ToFullString();
    }

    #endregion // Methods
}