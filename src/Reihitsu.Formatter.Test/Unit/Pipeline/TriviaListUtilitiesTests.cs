using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for <see cref="TriviaListUtilities"/>
/// </summary>
[TestClass]
public class TriviaListUtilitiesTests
{
    #region Methods

    /// <summary>
    /// Verifies that only trailing whitespace is removed while the space preceding a kept comment is preserved
    /// </summary>
    [TestMethod]
    public void RemoveTrailingWhitespaceRemovesTrailingWhitespaceAndKeepsCommentSpacing()
    {
        // Arrange
        var list = SyntaxFactory.TriviaList(SyntaxFactory.Space,
                                            SyntaxFactory.Comment("/* c */"),
                                            SyntaxFactory.Space);

        // Act
        var result = TriviaListUtilities.RemoveTrailingWhitespace(list);

        // Assert
        Assert.AreEqual(2, result.Count, "Only the trailing whitespace should be removed.");
        Assert.IsTrue(result[0].IsKind(SyntaxKind.WhitespaceTrivia), "The whitespace before the comment should be preserved.");
        Assert.IsTrue(result[1].IsKind(SyntaxKind.MultiLineCommentTrivia), "Comment trivia should be preserved.");
    }

    #endregion // Methods
}