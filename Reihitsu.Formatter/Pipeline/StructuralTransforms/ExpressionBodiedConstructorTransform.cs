using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied constructors to block body.
/// The expression always becomes an <see cref="ExpressionStatementSyntax"/>.
/// Constructor initializers (<c>: this()</c>, <c>: base()</c>) are preserved
/// </summary>
internal sealed class ExpressionBodiedConstructorTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public ExpressionBodiedConstructorTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Removes trailing whitespace trivia from the given trivia list
    /// </summary>
    /// <param name="trivia">The trivia list to clean</param>
    /// <returns>The trivia list without trailing whitespace</returns>
    private static SyntaxTriviaList StripTrailingWhitespace(SyntaxTriviaList trivia)
    {
        return SyntaxFactory.TriviaList(trivia.Where(static entry => entry.IsKind(SyntaxKind.WhitespaceTrivia) == false));
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        var expression = node.ExpressionBody.Expression;
        var statement = SyntaxFactory.ExpressionStatement(expression);

        var closeBraceTrivia = node.SemicolonToken.TrailingTrivia;

        var block = SyntaxFactory.Block(SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                                        SyntaxFactory.SingletonList<StatementSyntax>(statement),
                                        SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrivia));

        // Strip trailing whitespace from parameter list close paren
        var paramCloseParen = node.ParameterList.CloseParenToken;
        var paramCleanTrailing = StripTrailingWhitespace(paramCloseParen.TrailingTrivia);
        node = node.WithParameterList(node.ParameterList.WithCloseParenToken(paramCloseParen.WithTrailingTrivia(paramCleanTrailing)));

        if (node.Initializer != null)
        {
            var initCloseParen = node.Initializer.ArgumentList.CloseParenToken;
            var initCleanTrailing = StripTrailingWhitespace(initCloseParen.TrailingTrivia);

            var newInitializer = node.Initializer.WithArgumentList(node.Initializer.ArgumentList.WithCloseParenToken(initCloseParen.WithTrailingTrivia(initCleanTrailing)));

            return node.WithInitializer(newInitializer)
                       .WithBody(block)
                       .WithExpressionBody(null)
                       .WithSemicolonToken(default);
        }

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxRewriter
}