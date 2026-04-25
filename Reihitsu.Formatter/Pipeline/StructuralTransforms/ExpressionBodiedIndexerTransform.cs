using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied indexers to block body with a <see cref="ReturnStatementSyntax"/>.
/// </summary>
internal sealed class ExpressionBodiedIndexerTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token.
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ExpressionBodiedIndexerTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (IndexerDeclarationSyntax)base.VisitIndexerDeclaration(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        var expression = node.ExpressionBody.Expression;

        var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                                                            expression,
                                                            SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                  .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(returnStatement)));

        var openBraceTrivia = node.ExpressionBody.ArrowToken.LeadingTrivia;
        var closeBraceTrivia = node.SemicolonToken.TrailingTrivia;

        var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(openBraceTrivia),
                                                      SyntaxFactory.SingletonList(getter),
                                                      SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrivia));

        return node.WithAccessorList(accessorList)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxRewriter
}