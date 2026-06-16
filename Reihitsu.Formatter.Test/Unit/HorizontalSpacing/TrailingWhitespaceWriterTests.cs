using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.HorizontalSpacing;

namespace Reihitsu.Formatter.Test.Unit.HorizontalSpacing;

/// <summary>
/// Tests for <see cref="TrailingWhitespaceWriter"/>, the trivia-mechanics collaborator of the
/// horizontal spacing phase
/// </summary>
[TestClass]
public class TrailingWhitespaceWriterTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that requesting a single space on a token with no trailing trivia adds one space
    /// </summary>
    [TestMethod]
    public void SetTrailingWhitespaceAddsSpaceWhenNoTrailingTrivia()
    {
        var token = ParseFirstIdentifier("int x;");
        var writer = new TrailingWhitespaceWriter();

        var result = writer.SetTrailingWhitespace(token, 1);

        Assert.AreEqual(" ", result.TrailingTrivia.ToFullString());
    }

    /// <summary>
    /// Verifies that requesting zero spaces removes existing whitespace-only trailing trivia
    /// </summary>
    [TestMethod]
    public void SetTrailingWhitespaceRemovesWhitespaceWhenZeroRequested()
    {
        var token = ParseFirstIdentifier("int   x;");
        var writer = new TrailingWhitespaceWriter();

        var result = writer.SetTrailingWhitespace(token, 0);

        Assert.AreEqual(string.Empty, result.TrailingTrivia.ToFullString());
    }

    /// <summary>
    /// Verifies that multiple trailing spaces are collapsed to the requested single space
    /// </summary>
    [TestMethod]
    public void SetTrailingWhitespaceCollapsesMultipleSpacesToOne()
    {
        var token = ParseFirstIdentifier("int     x;");
        var writer = new TrailingWhitespaceWriter();

        var result = writer.SetTrailingWhitespace(token, 1);

        Assert.AreEqual(" ", result.TrailingTrivia.ToFullString());
    }

    /// <summary>
    /// Verifies that non-whitespace trailing trivia such as an inline comment is preserved while
    /// the trailing space after it is set to the requested count
    /// </summary>
    [TestMethod]
    public void SetTrailingWhitespacePreservesInlineComment()
    {
        var token = ParseFirstIdentifier("int /* note */ x;");
        var writer = new TrailingWhitespaceWriter();

        var result = writer.SetTrailingWhitespace(token, 1);

        Assert.AreEqual(" /* note */ ", result.TrailingTrivia.ToFullString());
    }

    /// <summary>
    /// Verifies that collapsing does not add or remove spacing when a single space is already present
    /// </summary>
    [TestMethod]
    public void CollapseMultipleTrailingSpacesLeavesSingleSpaceUnchanged()
    {
        var token = ParseFirstIdentifier("int x;");
        var writer = new TrailingWhitespaceWriter();

        var result = writer.CollapseMultipleTrailingSpaces(token);

        Assert.AreEqual(" ", result.TrailingTrivia.ToFullString());
    }

    /// <summary>
    /// Verifies that collapsing reduces multiple consecutive spaces to a single space
    /// </summary>
    [TestMethod]
    public void CollapseMultipleTrailingSpacesReducesMultipleSpaces()
    {
        var token = ParseFirstIdentifier("int      x;");
        var writer = new TrailingWhitespaceWriter();

        var result = writer.CollapseMultipleTrailingSpaces(token);

        Assert.AreEqual(" ", result.TrailingTrivia.ToFullString());
    }

    /// <summary>
    /// Parses the given code and returns the first identifier-or-keyword token, which carries the
    /// trailing trivia under test
    /// </summary>
    /// <param name="code">The C# code to parse</param>
    /// <returns>The first token of the parsed code</returns>
    private SyntaxToken ParseFirstIdentifier(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.CancellationToken);

        return tree.GetRoot(TestContext.CancellationToken).DescendantTokens().First();
    }

    #endregion // Methods
}