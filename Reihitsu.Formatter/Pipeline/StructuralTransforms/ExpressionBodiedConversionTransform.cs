using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied conversion operators (<c>implicit operator</c> / <c>explicit operator</c>)
/// to block body with a <see cref="ReturnStatementSyntax"/>.
/// </summary>
internal sealed class ExpressionBodiedConversionTransform : CSharpSyntaxRewriter
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
    public ExpressionBodiedConversionTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ConversionOperatorDeclarationSyntax)base.VisitConversionOperatorDeclaration(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        var expression = node.ExpressionBody.Expression;

        var statement = SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                                                      expression,
                                                      SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var openBraceTrivia = node.ExpressionBody.ArrowToken.LeadingTrivia;
        var closeBraceTrivia = node.SemicolonToken.TrailingTrivia;

        var block = SyntaxFactory.Block(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(openBraceTrivia),
                                        SyntaxFactory.SingletonList<StatementSyntax>(statement),
                                        SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrivia));

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxRewriter
}