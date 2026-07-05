using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Enumerations;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied local functions to block body.
/// Value-returning functions wrap the expression in a <see cref="ReturnStatementSyntax"/>.
/// Void and non-generic async task functions wrap it in an <see cref="ExpressionStatementSyntax"/>
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

    #region CSharpSyntaxVisitor

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
        var statementForm = ExpressionBodiedTransformUtilities.UsesExpressionStatement(node.ReturnType, node.Modifiers)
                                ? ExpressionBodyStatementForm.ExpressionStatement
                                : ExpressionBodyStatementForm.ReturnStatement;

        var block = ExpressionBodyToBlockConverter.CreateBlock(expression,
                                                               statementForm,
                                                               node.ExpressionBody.ArrowToken.LeadingTrivia,
                                                               node.SemicolonToken.TrailingTrivia);

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxVisitor
}