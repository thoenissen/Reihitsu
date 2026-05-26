using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied methods to block body.
/// Value-returning methods wrap the expression in a <see cref="ReturnStatementSyntax"/>.
/// Void and non-generic async task methods wrap it in an <see cref="ExpressionStatementSyntax"/>
/// </summary>
internal sealed class ExpressionBodiedMethodTransform : CSharpSyntaxRewriter
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
    public ExpressionBodiedMethodTransform(CancellationToken cancellationToken)
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

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        var expression = node.ExpressionBody.Expression;
        var useExpressionStatement = ExpressionBodiedTransformUtilities.UsesExpressionStatement(node.ReturnType, node.Modifiers);

        StatementSyntax statement;

        if (useExpressionStatement)
        {
            statement = SyntaxFactory.ExpressionStatement(expression);
        }
        else
        {
            statement = SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                                                      expression,
                                                      SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        var closeBraceTrivia = node.SemicolonToken.TrailingTrivia;

        var block = SyntaxFactory.Block(SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                                        SyntaxFactory.SingletonList(statement),
                                        SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrivia));

        var closeParen = node.ParameterList.CloseParenToken;
        var cleanTrailing = StripTrailingWhitespace(closeParen.TrailingTrivia);

        return node.WithParameterList(node.ParameterList.WithCloseParenToken(closeParen.WithTrailingTrivia(cleanTrailing)))
                   .WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxVisitor
}