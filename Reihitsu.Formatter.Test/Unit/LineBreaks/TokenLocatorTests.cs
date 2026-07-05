using Microsoft.CodeAnalysis;
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
    /// Test context for the current test
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
    /// Verifies that a stale token is not silently resolved to a different same-kind token whose
    /// span start coincides after a tree mutation shifted positions. This reproduces the resolution
    /// hazard behind issue #306 / #329: refreshing a token captured before an edit must never return
    /// an unrelated token that merely shares the original kind and span start
    /// </summary>
    [TestMethod]
    public void GetCurrentTokenDoesNotResolveStaleTokenToDifferentSameKindToken()
    {
        // Arrange
        var declaration = ParseClassDeclaration("class C { int A { get; set; } }");
        var classOpenBrace = declaration.OpenBraceToken;
        var propertyOpenBrace = declaration.DescendantTokens()
                                           .First(token => token.IsKind(SyntaxKind.OpenBraceToken) && token != classOpenBrace);

        // Prepend leading trivia so the class open brace shifts onto the stale span start of the
        // property open brace, which is the offset collision that previously matched the wrong brace
        var padding = new string(' ', propertyOpenBrace.SpanStart - classOpenBrace.SpanStart);
        var shifted = declaration.WithKeyword(declaration.Keyword.WithLeadingTrivia(SyntaxFactory.Whitespace(padding)));

        Assert.AreEqual(propertyOpenBrace.SpanStart, shifted.OpenBraceToken.SpanStart, "The padding must align the class open brace with the stale property brace span start.");

        // Act
        var result = TokenLocator.GetCurrentToken(shifted, propertyOpenBrace);

        // Assert
        Assert.IsFalse(result == shifted.OpenBraceToken, "The stale property brace must not be resolved to the unrelated class brace that now shares its span start.");
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