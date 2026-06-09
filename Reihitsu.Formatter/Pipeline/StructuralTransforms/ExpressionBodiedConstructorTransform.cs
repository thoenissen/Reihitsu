using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Enumerations;
using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied constructors to block body.
/// The expression always becomes an <see cref="ExpressionStatementSyntax"/>.
/// Constructor initializers (<c>: this()</c>, <c>: base()</c>) are preserved
/// </summary>
internal sealed class ExpressionBodiedConstructorTransform : CSharpSyntaxRewriter
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
    public ExpressionBodiedConstructorTransform(ExpressionBodyToBlockConverter converter, CancellationToken cancellationToken)
    {
        _converter = converter;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        var expression = node.ExpressionBody.Expression;

        var block = _converter.CreateBlock(expression,
                                           ExpressionBodyStatementForm.ExpressionStatement,
                                           default,
                                           node.SemicolonToken.TrailingTrivia);

        // Strip trailing whitespace from parameter list close paren
        var paramCloseParen = node.ParameterList.CloseParenToken;
        var paramCleanTrailing = LineBreakTriviaUtilities.StripTrailingWhitespace(paramCloseParen.TrailingTrivia);
        node = node.WithParameterList(node.ParameterList.WithCloseParenToken(paramCloseParen.WithTrailingTrivia(paramCleanTrailing)));

        if (node.Initializer != null)
        {
            var initCloseParen = node.Initializer.ArgumentList.CloseParenToken;
            var initCleanTrailing = LineBreakTriviaUtilities.StripTrailingWhitespace(initCloseParen.TrailingTrivia);

            var newInitializer = node.Initializer.WithArgumentList(node.Initializer.ArgumentList.WithCloseParenToken(initCloseParen.WithTrailingTrivia(initCleanTrailing)));

            return node.WithInitializer(newInitializer)
                       .WithBody(block)
                       .WithExpressionBody(null)
                       .WithSemicolonToken(default);
        }

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxVisitor
}