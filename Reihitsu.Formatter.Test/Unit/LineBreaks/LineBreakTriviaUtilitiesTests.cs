using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for the trivia-edit helpers in <see cref="LineBreakTriviaUtilities"/>
/// </summary>
[TestClass]
public class LineBreakTriviaUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an end-of-line trivia is prepended to a token's leading trivia
    /// </summary>
    [TestMethod]
    public void PrependEndOfLineAddsLeadingEndOfLine()
    {
        // Arrange
        var token = SyntaxFactory.Identifier("x");

        // Act
        var result = LineBreakTriviaUtilities.PrependEndOfLine(token, "\n");

        // Assert
        Assert.IsTrue(result.LeadingTrivia.Count > 0 && result.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia), "An end-of-line trivia should be prepended.");
        Assert.AreEqual("\n", result.LeadingTrivia[0].ToString(), "The prepended end-of-line should use the requested sequence.");
    }

    /// <summary>
    /// Verifies that an end-of-line trivia is appended to a trivia list
    /// </summary>
    [TestMethod]
    public void AppendEndOfLineAddsTrailingEndOfLine()
    {
        // Arrange
        var list = SyntaxFactory.TriviaList(SyntaxFactory.Space);

        // Act
        var result = LineBreakTriviaUtilities.AppendEndOfLine(list, "\n");

        // Assert
        Assert.AreEqual(2, result.Count, "The trivia list should grow by one entry.");
        Assert.IsTrue(result[result.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia), "The appended trivia should be an end-of-line.");
    }

    /// <summary>
    /// Verifies that only trailing whitespace is removed while the space preceding a kept comment is preserved
    /// </summary>
    [TestMethod]
    public void StripTrailingWhitespaceRemovesTrailingWhitespaceAndKeepsCommentSpacing()
    {
        // Arrange
        var list = SyntaxFactory.TriviaList(SyntaxFactory.Space,
                                            SyntaxFactory.Comment("/* c */"),
                                            SyntaxFactory.Space);

        // Act
        var result = LineBreakTriviaUtilities.StripTrailingWhitespace(list);

        // Assert
        Assert.AreEqual(2, result.Count, "Only the trailing whitespace should be removed.");
        Assert.IsTrue(result[0].IsKind(SyntaxKind.WhitespaceTrivia), "The whitespace before the comment should be preserved.");
        Assert.IsTrue(result[1].IsKind(SyntaxKind.MultiLineCommentTrivia), "Comment trivia should be preserved.");
    }

    /// <summary>
    /// Verifies that a token preceded by a line break is collapsed onto the previous line
    /// </summary>
    [TestMethod]
    public void CollapseTokenToSameLineRemovesLineBreakBeforeToken()
    {
        // Arrange
        var block = (BlockSyntax)SyntaxFactory.ParseStatement("{\n}");

        // Act
        var result = LineBreakTriviaUtilities.CollapseTokenToSameLine(block, block.CloseBraceToken);

        // Assert
        Assert.DoesNotContain("\n", result.ToFullString(), "The line break before the collapsed token should be removed.");
    }

    /// <summary>
    /// Verifies that a token preceded by a line break is not collapsed when the previous token
    /// carries a trailing single-line comment, because the join would absorb the token into the comment
    /// </summary>
    [TestMethod]
    public void CollapseTokenToSameLineSkipsWhenPreviousTokenHasTrailingComment()
    {
        // Arrange
        var block = (BlockSyntax)SyntaxFactory.ParseStatement("{ // note\n}");

        // Act
        var result = LineBreakTriviaUtilities.CollapseTokenToSameLine(block, block.CloseBraceToken);

        // Assert
        Assert.AreEqual(block.ToFullString(), result.ToFullString(), "The collapse must be skipped so the comment is preserved.");
        Assert.Contains("\n", result.ToFullString(), "The line break after the comment must be preserved.");
    }

    /// <summary>
    /// Verifies that a token is moved onto a new line
    /// </summary>
    [TestMethod]
    public void MoveTokenToNewLineAddsLineBreakBeforeToken()
    {
        // Arrange
        var block = (BlockSyntax)SyntaxFactory.ParseStatement("{ }");

        // Act
        var result = LineBreakTriviaUtilities.MoveTokenToNewLine(block, block.CloseBraceToken, "\n");

        // Assert
        Assert.Contains("\n", result.ToFullString(), "A line break should be inserted before the moved token.");
    }

    #endregion // Methods
}