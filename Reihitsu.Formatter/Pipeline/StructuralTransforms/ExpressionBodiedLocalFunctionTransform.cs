using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied local functions to block body.
/// Non-void functions wrap the expression in a <see cref="ReturnStatementSyntax"/>.
/// Void functions wrap it in an <see cref="ExpressionStatementSyntax"/>
/// </summary>
internal sealed class ExpressionBodiedLocalFunctionTransform : CSharpSyntaxRewriter
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
    public ExpressionBodiedLocalFunctionTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the given return type represents <see langword="void"/>
    /// </summary>
    /// <param name="returnType">The return type syntax to check</param>
    /// <returns><see langword="true"/> if the return type is <see langword="void"/>; otherwise, <see langword="false"/></returns>
    private static bool IsVoidReturn(TypeSyntax returnType)
    {
        if (returnType is PredefinedTypeSyntax predefined
            && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return true;
        }

        return false;
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node);

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

        var openBraceTrivia = node.ExpressionBody.ArrowToken.LeadingTrivia;
        var closeBraceTrivia = node.SemicolonToken.TrailingTrivia;

        var block = SyntaxFactory.Block(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(openBraceTrivia),
                                        SyntaxFactory.SingletonList(statement),
                                        SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrivia));

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxRewriter
}