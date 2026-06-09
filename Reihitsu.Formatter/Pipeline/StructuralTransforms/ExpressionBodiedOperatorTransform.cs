using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Enumerations;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied operators (<c>operator +</c>, etc.) to block body
/// with a <see cref="ReturnStatementSyntax"/>
/// </summary>
internal sealed class ExpressionBodiedOperatorTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// Builds the replacement block body
    /// </summary>
    private readonly ExpressionBodyToBlockConverter _converter;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="converter">Builds the replacement block body</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public ExpressionBodiedOperatorTransform(ExpressionBodyToBlockConverter converter, CancellationToken cancellationToken)
    {
        _converter = converter;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

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

        var block = _converter.CreateBlock(expression,
                                           ExpressionBodyStatementForm.ReturnStatement,
                                           node.ExpressionBody.ArrowToken.LeadingTrivia,
                                           node.SemicolonToken.TrailingTrivia);

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxVisitor
}