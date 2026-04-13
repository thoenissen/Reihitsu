using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied methods to block body.
/// Non-void methods wrap the expression in a <see cref="ReturnStatementSyntax"/>.
/// Void methods wrap it in an <see cref="ExpressionStatementSyntax"/>.
/// </summary>
internal sealed class ExpressionBodiedMethodTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    ///     The cancellation token.
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ExpressionBodiedMethodTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the given return type represents <see langword="void"/>.
    /// </summary>
    /// <param name="returnType">The return type syntax to check.</param>
    /// <returns><see langword="true"/> if the return type is <see langword="void"/>; otherwise, <see langword="false"/>.</returns>
    private static bool IsVoidReturn(TypeSyntax returnType)
    {
        if (returnType is PredefinedTypeSyntax predefined
            && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes trailing whitespace trivia from the given trivia list.
    /// </summary>
    /// <param name="trivia">The trivia list to clean.</param>
    /// <returns>The trivia list without trailing whitespace.</returns>
    private static SyntaxTriviaList StripTrailingWhitespace(SyntaxTriviaList trivia)
    {
        var result = new List<SyntaxTrivia>();

        foreach (var t in trivia)
        {
            if (t.IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                result.Add(t);
            }
        }

        return SyntaxFactory.TriviaList(result);
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

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
        var isVoid = IsVoidReturn(node.ReturnType);

        StatementSyntax statement;

        if (isVoid)
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

    #endregion // CSharpSyntaxRewriter
}