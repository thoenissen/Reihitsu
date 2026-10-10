using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

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
    /// Verifies that a zero-width omitted type argument (the placeholder in an unbound generic such as
    /// <c>Dictionary&lt;,&gt;</c>) is still resolved as the predecessor, matching the full
    /// <c>DescendantTokens(descendIntoTrivia: true)</c> walk this method replaces — a plain
    /// <see cref="SyntaxToken.GetPreviousToken(bool, bool, bool, bool)"/> call skips a zero-width token by default
    /// </summary>
    [TestMethod]
    public void TryGetPreviousTokenResolvesZeroWidthOmittedTypeArgument()
    {
        // Arrange
        var statement = SyntaxFactory.ParseStatement("var type = typeof(System.Collections.Generic.Dictionary<,>);");
        var closeAngle = statement.DescendantTokens().First(token => token.IsKind(SyntaxKind.GreaterThanToken));

        // Act
        var found = TokenLocator.TryGetPreviousToken(statement, closeAngle, out var previousToken);

        // Assert
        Assert.IsTrue(found, "The zero-width omitted type argument should be found as the predecessor.");
        Assert.AreEqual(SyntaxKind.OmittedTypeArgumentToken, previousToken.Kind(), "The predecessor should be the omitted type argument's own token, not a token further back.");
    }

    /// <summary>
    /// Verifies that the predecessor of a token following a <c>#region</c> directive is the directive's own
    /// end-of-directive token — the same trivia-embedded token a full node scan would have yielded
    /// </summary>
    [TestMethod]
    public void TryGetPreviousTokenResolvesEndOfRegionDirectiveToken()
    {
        // Arrange
        const string source = """
                              class C
                              {
                              #region Fields
                                  private int _value;
                              #endregion
                              }
                              """;
        var root = ParseCompilationUnit(source);
        var fieldToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.PrivateKeyword));

        // Act
        var found = TokenLocator.TryGetPreviousToken(root, fieldToken, out var previousToken);

        // Assert
        Assert.IsTrue(found, "The predecessor across the region directive should be found.");
        Assert.AreEqual(SyntaxKind.EndOfDirectiveToken, previousToken.Kind(), "The predecessor should be the region directive's own end-of-directive token.");
    }

    /// <summary>
    /// Verifies that the predecessor of a token following a <c>#pragma</c> directive is the directive's own
    /// end-of-directive token
    /// </summary>
    [TestMethod]
    public void TryGetPreviousTokenResolvesEndOfPragmaDirectiveToken()
    {
        // Arrange
        const string source = """
                              class C
                              {
                              #pragma warning disable CS0219
                                  private int _value;
                              #pragma warning restore CS0219
                              }
                              """;
        var root = ParseCompilationUnit(source);
        var fieldToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.PrivateKeyword));

        // Act
        var found = TokenLocator.TryGetPreviousToken(root, fieldToken, out var previousToken);

        // Assert
        Assert.IsTrue(found, "The predecessor across the pragma directive should be found.");
        Assert.AreEqual(SyntaxKind.EndOfDirectiveToken, previousToken.Kind(), "The predecessor should be the pragma directive's own end-of-directive token.");
    }

    /// <summary>
    /// Verifies that the predecessor of a token following a disabled <c>#if</c> region is the closest directive's
    /// own end-of-directive token (<c>#endif</c>) rather than a token from the earlier, farther <c>#if</c>
    /// </summary>
    [TestMethod]
    public void TryGetPreviousTokenResolvesEndOfDirectiveTokenAcrossDisabledText()
    {
        // Arrange
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  private int _value;
                              #endif
                              }
                              """;
        var root = ParseCompilationUnit(source);
        var closeBrace = root.DescendantTokens().Last(token => token.IsKind(SyntaxKind.CloseBraceToken));

        // Act
        var found = TokenLocator.TryGetPreviousToken(root, closeBrace, out var previousToken);

        // Assert
        Assert.IsTrue(found, "The predecessor across the disabled region should be found.");
        Assert.AreEqual(SyntaxKind.EndOfDirectiveToken, previousToken.Kind(), "The predecessor should be the #endif directive's own end-of-directive token.");

        var directive = (EndIfDirectiveTriviaSyntax)previousToken.Parent;

        Assert.AreEqual(SyntaxKind.EndIfDirectiveTrivia, directive.Kind(), "The resolved directive should be the closer #endif, not the farther #if.");
    }

    /// <summary>
    /// Verifies that the predecessor of a token following a documentation comment is the comment's own
    /// end-of-documentation-comment token
    /// </summary>
    [TestMethod]
    public void TryGetPreviousTokenResolvesEndOfDocumentationCommentToken()
    {
        // Arrange
        const string source = """
                              class C
                              {
                                  /// <summary>
                                  /// Summary
                                  /// </summary>
                                  private int _value;
                              }
                              """;
        var root = ParseCompilationUnit(source);
        var fieldToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.PrivateKeyword));

        // Act
        var found = TokenLocator.TryGetPreviousToken(root, fieldToken, out var previousToken);

        // Assert
        Assert.IsTrue(found, "The predecessor across the documentation comment should be found.");
        Assert.AreEqual(SyntaxKind.EndOfDocumentationCommentToken, previousToken.Kind(), "The predecessor should be the documentation comment's own end-of-documentation-comment token.");
    }

    /// <summary>
    /// Verifies that a plain gap with no structured trivia still resolves to the ordinary previous token —
    /// the boundary that must stay unaffected by the structured-trivia lookup
    /// </summary>
    [TestMethod]
    public void TryGetPreviousTokenResolvesOrdinaryTokenWhenGapHasNoStructuredTrivia()
    {
        // Arrange
        const string source = """
                              class C
                              {
                                  private int _value;
                              }
                              """;
        var root = ParseCompilationUnit(source);
        var fieldToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.PrivateKeyword));

        // Act
        var found = TokenLocator.TryGetPreviousToken(root, fieldToken, out var previousToken);

        // Assert
        Assert.IsTrue(found, "The ordinary predecessor should be found.");
        Assert.AreEqual(SyntaxKind.OpenBraceToken, previousToken.Kind(), "The predecessor should be the class's own open brace, with no structured trivia involved.");
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
    /// span start coincides after a tree mutation shifted positions. Refreshing a token captured
    /// before an edit must never return an unrelated token that merely shares the original kind and
    /// span start
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

    /// <summary>
    /// Parses the given source text and returns its compilation unit root
    /// </summary>
    /// <param name="source">The C# source text</param>
    /// <returns>The compilation unit root</returns>
    private CompilationUnitSyntax ParseCompilationUnit(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);

        return (CompilationUnitSyntax)tree.GetRoot(TestContext.CancellationToken);
    }

    #endregion // Methods
}