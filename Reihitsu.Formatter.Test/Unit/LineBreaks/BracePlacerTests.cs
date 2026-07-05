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
    /// Test context for the current test
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
    /// Verifies that a single-line contained block whose open brace is inline with the header is fully
    /// normalized in a single pass, even though the open-brace edit shifts the positions of the first
    /// statement and the close brace (issue #241)
    /// </summary>
    [TestMethod]
    public void NormalizeContainedBlockFullyNormalizesInlineOpenBraceInSinglePass()
    {
        // Arrange — the open brace shares the header line, so normalizing the gap before it shifts every
        //           following token; the first-content and close-brace steps must still apply
        var bracePlacer = CreateBracePlacer();

        var first = (WhileStatementSyntax)SyntaxFactory.ParseStatement("while (a) { B(); }");
        var firstPass = bracePlacer.NormalizeContainedBlock(first, (BlockSyntax)first.Statement).ToFullString();

        // Act — a second pass must find nothing left to normalize
        var second = (WhileStatementSyntax)SyntaxFactory.ParseStatement(firstPass);
        var secondPass = bracePlacer.NormalizeContainedBlock(second, (BlockSyntax)second.Statement).ToFullString();

        // Assert
        Assert.AreEqual(firstPass, secondPass, "A single NormalizeContainedBlock pass must fully normalize the block.");
        Assert.DoesNotContain("{ B();", firstPass, "The first statement should be moved onto its own line in a single pass.");
        Assert.DoesNotContain("B(); }", firstPass, "The close brace should be moved onto its own line in a single pass.");
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