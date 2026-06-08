using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="BracePlacer"/>
/// </summary>
[TestClass]
public class BracePlacerTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the first token after an open brace is moved onto a new line
    /// </summary>
    [TestMethod]
    public void EnsureFirstContentOnNewLineMovesFirstStatement()
    {
        // Arrange
        var block = (BlockSyntax)SyntaxFactory.ParseStatement("{ A(); }");
        var bracePlacer = CreateBracePlacer();

        // Act
        var result = bracePlacer.EnsureFirstContentOnNewLine(block, block.OpenBraceToken);

        // Assert
        Assert.Contains("\n", result.ToFullString(), "The first statement should be moved onto its own line.");
    }

    /// <summary>
    /// Verifies that a single-line contained block is expanded onto separate lines
    /// </summary>
    [TestMethod]
    public void NormalizeContainedBlockExpandsSingleLineBlock()
    {
        // Arrange
        var whileStatement = (WhileStatementSyntax)SyntaxFactory.ParseStatement("while (a)\n{ B(); }");
        var bracePlacer = CreateBracePlacer();

        // Act
        var result = bracePlacer.NormalizeContainedBlock(whileStatement, (BlockSyntax)whileStatement.Statement);

        // Assert — the body statement and the close brace should each gain their own line
        Assert.IsGreaterThan(CountLineBreaks(whileStatement.ToFullString()),
                             CountLineBreaks(result.ToFullString()),
                             "Normalizing a single-line contained block should introduce additional line breaks.");
    }

    /// <summary>
    /// Creates a brace placer wired with a token gap normalizer for the line-feed end-of-line sequence
    /// </summary>
    /// <returns>The brace placer</returns>
    private static BracePlacer CreateBracePlacer()
    {
        var gapNormalizer = new TokenGapNormalizer("\n");

        return new BracePlacer(gapNormalizer, "\n");
    }

    /// <summary>
    /// Counts the number of line-feed characters in the given text
    /// </summary>
    /// <param name="text">The text to inspect</param>
    /// <returns>The number of line-feed characters</returns>
    private static int CountLineBreaks(string text)
    {
        return text.Count(character => character == '\n');
    }

    #endregion // Methods
}