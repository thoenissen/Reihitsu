using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="TokenLocator"/>
/// </summary>
[TestClass]
public class TokenLocatorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a token owned by a node is reported as contained
    /// </summary>
    [TestMethod]
    public void ContainsTokenReturnsTrueForOwnedToken()
    {
        // Arrange
        var declaration = ParseClassDeclaration("class C { }");

        // Act
        var result = TokenLocator.ContainsToken(declaration, declaration.Identifier);

        // Assert
        Assert.IsTrue(result, "A token owned by the node should be reported as contained.");
    }

    /// <summary>
    /// Verifies that a default token is not reported as contained
    /// </summary>
    [TestMethod]
    public void ContainsTokenReturnsFalseForDefaultToken()
    {
        // Arrange
        var declaration = ParseClassDeclaration("class C { }");

        // Act
        var result = TokenLocator.ContainsToken(declaration, default);

        // Assert
        Assert.IsFalse(result, "A default token should not be reported as contained.");
    }

    /// <summary>
    /// Verifies that the previous token is resolved within a node
    /// </summary>
    [TestMethod]
    public void TryGetPreviousTokenResolvesPredecessor()
    {
        // Arrange
        var statement = SyntaxFactory.ParseStatement("int x;");
        var identifier = statement.DescendantTokens().First(token => token.IsKind(SyntaxKind.IdentifierToken) && token.Text == "x");

        // Act
        var found = TokenLocator.TryGetPreviousToken(statement, identifier, out var previousToken);

        // Assert
        Assert.IsTrue(found, "A predecessor token should be found.");
        Assert.AreEqual("int", previousToken.ToString(), "The predecessor of the identifier should be the type keyword.");
    }

    /// <summary>
    /// Verifies that the refreshed token shares the original span start
    /// </summary>
    [TestMethod]
    public void GetCurrentTokenReturnsEquivalentToken()
    {
        // Arrange
        var declaration = ParseClassDeclaration("class C { }");

        // Act
        var current = TokenLocator.GetCurrentToken(declaration, declaration.Identifier);

        // Assert
        Assert.AreEqual(declaration.Identifier.SpanStart, current.SpanStart, "The refreshed token should share the original span start.");
        Assert.AreEqual(declaration.Identifier.RawKind, current.RawKind, "The refreshed token should share the original kind.");
    }

    /// <summary>
    /// Parses the first class declaration found in the given source text
    /// </summary>
    /// <param name="source">The C# source text</param>
    /// <returns>The first class declaration</returns>
    private ClassDeclarationSyntax ParseClassDeclaration(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);

        return tree.GetRoot(TestContext.CancellationToken)
                   .DescendantNodes()
                   .OfType<ClassDeclarationSyntax>()
                   .First();
    }

    #endregion // Methods
}