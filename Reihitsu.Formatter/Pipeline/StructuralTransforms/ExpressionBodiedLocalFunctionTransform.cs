using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    public ExpressionBodiedLocalFunctionTransform(ExpressionBodyToBlockConverter converter, CancellationToken cancellationToken)
    {
        _converter = converter;
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

        var block = _converter.CreateBlock(expression,
                                           statementForm,
                                           node.ExpressionBody.ArrowToken.LeadingTrivia,
                                           node.SemicolonToken.TrailingTrivia);

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxVisitor
}