using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Enumerations;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied finalizers to block body with an <see cref="ExpressionStatementSyntax"/>
/// </summary>
internal sealed class ExpressionBodiedFinalizerTransform : CSharpSyntaxRewriter
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
    public ExpressionBodiedFinalizerTransform(ExpressionBodyToBlockConverter converter, CancellationToken cancellationToken)
    {
        _converter = converter;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (DestructorDeclarationSyntax)base.VisitDestructorDeclaration(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        var expression = node.ExpressionBody.Expression;

        var block = _converter.CreateBlock(expression,
                                           ExpressionBodyStatementForm.ExpressionStatement,
                                           node.ExpressionBody.ArrowToken.LeadingTrivia,
                                           node.SemicolonToken.TrailingTrivia);

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxVisitor
}