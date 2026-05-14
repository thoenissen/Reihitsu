using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for <see cref="TokenGapUtilities"/>
/// </summary>
[TestClass]
public class TokenGapUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a blank line before a leading multi-line block comment counts
    /// as a single blank line between neighboring statements
    /// </summary>
    [TestMethod]
    public void CountBlankLinesBetweenReturnsOneForLeadingMultiLineBlockCommentBeforeLocalDeclaration()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var url = "";

                                     /* Explanation
                                        of the branch */
                                     var client = new object();
                                 }
                             }
                             """;

        var (previousToken, currentToken) = GetSiblingStatementBoundary(input);

        // Act
        var actual = TokenGapUtilities.CountBlankLinesBetween(previousToken, currentToken);

        // Assert
        Assert.AreEqual(1, actual);
    }

    /// <summary>
    /// Verifies that a blank line before a leading multi-line documentation comment
    /// counts as a single blank line between neighboring statements
    /// </summary>
    [TestMethod]
    public void CountBlankLinesBetweenReturnsOneForLeadingMultiLineDocumentationCommentBeforeLocalFunction()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var value = 0;

                                     /**
                                      * Helper.
                                      */
                                     void Local()
                                     {
                                     }
                                 }
                             }
                             """;

        var (previousToken, currentToken) = GetSiblingStatementBoundary(input);

        // Act
        var actual = TokenGapUtilities.CountBlankLinesBetween(previousToken, currentToken);

        // Assert
        Assert.AreEqual(1, actual);
    }

    /// <summary>
    /// Gets the boundary tokens between the first two sibling statements in a method body
    /// </summary>
    /// <param name="input">The source text to parse</param>
    /// <returns>The previous statement's last token and the current statement's first token</returns>
    private (SyntaxToken PreviousToken, SyntaxToken CurrentToken) GetSiblingStatementBoundary(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        return (method.Body!.Statements[0].GetLastToken(), method.Body.Statements[1].GetFirstToken());
    }

    #endregion // Methods
}