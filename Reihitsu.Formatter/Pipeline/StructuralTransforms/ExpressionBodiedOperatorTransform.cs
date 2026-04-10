using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied operators (<c>operator +</c>, etc.) to block body
/// with a <see cref="ReturnStatementSyntax"/>.
/// </summary>
internal sealed class ExpressionBodiedOperatorTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    ///     The formatting context.
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    ///     The cancellation token.
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ExpressionBodiedOperatorTransform(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        var expression = node.ExpressionBody.Expression;

        var statement = SyntaxFactory.ReturnStatement(
            SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
            expression,
            SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var openBraceTrivia = node.ExpressionBody.ArrowToken.LeadingTrivia;
        var closeBraceTrivia = node.SemicolonToken.TrailingTrivia;

        var block = SyntaxFactory.Block(
            SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(openBraceTrivia),
            SyntaxFactory.SingletonList<StatementSyntax>(statement),
            SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrivia));

        return node
            .WithBody(block)
            .WithExpressionBody(null)
            .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxRewriter
}