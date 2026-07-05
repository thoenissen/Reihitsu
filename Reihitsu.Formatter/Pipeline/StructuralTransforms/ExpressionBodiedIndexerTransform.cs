using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied indexers to block body with a <see cref="ReturnStatementSyntax"/>
/// </summary>
internal sealed class ExpressionBodiedIndexerTransform : CSharpSyntaxRewriter
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
    public ExpressionBodiedIndexerTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

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

        var accessorList = ExpressionBodyToBlockConverter.CreateGetAccessorList(expression,
                                                                                node.ExpressionBody.ArrowToken.LeadingTrivia,
                                                                                node.SemicolonToken.TrailingTrivia);

        return node.WithAccessorList(accessorList)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxVisitor
}