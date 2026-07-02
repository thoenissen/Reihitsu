using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Enumerations;
using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied methods to block body.
/// Value-returning methods wrap the expression in a <see cref="ReturnStatementSyntax"/>.
/// Void and non-generic async task methods wrap it in an <see cref="ExpressionStatementSyntax"/>
/// </summary>
internal sealed class ExpressionBodiedMethodTransform : CSharpSyntaxRewriter
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
    public ExpressionBodiedMethodTransform(ExpressionBodyToBlockConverter converter, CancellationToken cancellationToken)
    {
        _converter = converter;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

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
        var statementForm = ExpressionBodiedTransformUtilities.UsesExpressionStatement(node.ReturnType, node.Modifiers)
                                ? ExpressionBodyStatementForm.ExpressionStatement
                                : ExpressionBodyStatementForm.ReturnStatement;

        var block = _converter.CreateBlock(expression,
                                           statementForm,
                                           node.ExpressionBody.ArrowToken.LeadingTrivia,
                                           node.SemicolonToken.TrailingTrivia);

        var closeParen = node.ParameterList.CloseParenToken;
        var cleanTrailing = LineBreakTriviaUtilities.StripTrailingWhitespace(closeParen.TrailingTrivia);

        return node.WithParameterList(node.ParameterList.WithCloseParenToken(closeParen.WithTrailingTrivia(cleanTrailing)))
                   .WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxVisitor
}