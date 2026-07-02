using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="TokenGapNormalizer"/>
/// </summary>
[TestClass]
public class TokenGapNormalizerTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a leading gap is reduced to a single line break when no blank lines are requested
    /// </summary>
    [TestMethod]
    public void NormalizeLeadingGapCollapsesToSingleLineBreak()
    {
        // Arrange
        var endOfLine = SyntaxFactory.EndOfLine("\n");
        var token = SyntaxFactory.Identifier(SyntaxFactory.TriviaList(endOfLine, endOfLine, endOfLine), "x", SyntaxFactory.TriviaList());
        var normalizer = new TokenGapNormalizer("\n");

        // Act
        var result = normalizer.NormalizeLeadingGap(token, blankLineCount: 0);

        // Assert
        Assert.ContainsSingle(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia), result.LeadingTrivia, "Without blank lines the leading gap should hold a single line break.");
    }

    /// <summary>
    /// Verifies that a requested blank line is preserved as two line breaks
    /// </summary>
    [TestMethod]
    public void NormalizeLeadingGapKeepsRequestedBlankLine()
    {
        // Arrange
        var endOfLine = SyntaxFactory.EndOfLine("\n");
        var token = SyntaxFactory.Identifier(SyntaxFactory.TriviaList(endOfLine, endOfLine, endOfLine), "x", SyntaxFactory.TriviaList());
        var normalizer = new TokenGapNormalizer("\n");

        // Act
        var result = normalizer.NormalizeLeadingGap(token, blankLineCount: 1);

        // Assert
        Assert.AreEqual(2, result.LeadingTrivia.Count(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)), "One blank line should be represented by two line breaks.");
    }

    /// <summary>
    /// Verifies that a missing line break before a token is inserted
    /// </summary>
    [TestMethod]
    public void NormalizeGapBeforeTokenInsertsMissingLineBreak()
    {
        // Arrange
        var block = (BlockSyntax)SyntaxFactory.ParseStatement("{ A();B(); }");
        var secondStatementToken = block.DescendantTokens()
                                        .First(token => token.IsKind(SyntaxKind.IdentifierToken) && token.Text == "B");
        var normalizer = new TokenGapNormalizer("\n");

        // Act
        var result = normalizer.NormalizeGapBeforeToken(block, secondStatementToken, blankLineCount: 0);

        // Assert
        Assert.Contains("\n", result.ToFullString(), "A line break should be inserted before the second statement.");
    }

    #endregion // Methods
}