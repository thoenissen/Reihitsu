using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="TokenGapNormalizer"/>
/// </summary>
[TestClass]
public class TokenGapNormalizerTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

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