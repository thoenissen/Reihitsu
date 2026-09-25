using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies line-break rules for property layout: expression-bodied property collapse and the
/// accessor-list layout shared with indexers (<see cref="AccessorListLayout"/>)
/// </summary>
internal sealed class PropertyLayoutLineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The accessor-list layout
    /// </summary>
    private readonly AccessorListLayout _accessorListLayout;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="accessorListLayout">The accessor-list layout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public PropertyLayoutLineBreakRewriter(AccessorListLayout accessorListLayout,
                                           CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _accessorListLayout = accessorListLayout;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses a multi-line expression-bodied property to a single line
    /// </summary>
    /// <param name="node">The property declaration with an expression body</param>
    /// <returns>The property declaration collapsed to a single line, or unchanged when it has no expression body</returns>
    private static PropertyDeclarationSyntax CollapseExpressionBodiedProperty(PropertyDeclarationSyntax node)
    {
        if (node?.ExpressionBody == null)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(node.ExpressionBody.ArrowToken.GetPreviousToken(), node.ExpressionBody.ArrowToken)
            || LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(node.ExpressionBody.ArrowToken, node.ExpressionBody.Expression.GetFirstToken()))
        {
            return node;
        }

        var updatedNode = node;
        var arrowToken = updatedNode.ExpressionBody.ArrowToken;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(arrowToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(arrowToken.GetPreviousToken()))
        {
            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, arrowToken);
            arrowToken = updatedNode.ExpressionBody.ArrowToken;
        }

        if (arrowToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            updatedNode = updatedNode.ReplaceToken(arrowToken, arrowToken.WithLeadingTrivia(arrowToken.LeadingTrivia.Add(SyntaxFactory.Space)));
        }

        var firstExpressionToken = updatedNode.ExpressionBody.Expression.GetFirstToken();

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstExpressionToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(firstExpressionToken.GetPreviousToken()))
        {
            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, firstExpressionToken);
        }

        arrowToken = updatedNode.ExpressionBody.ArrowToken;
        firstExpressionToken = updatedNode.ExpressionBody.Expression.GetFirstToken();

        var previousToken = arrowToken.GetPreviousToken();
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>
                             {
                                 [arrowToken] = arrowToken.WithLeadingTrivia(SyntaxFactory.Space)
                                                          .WithTrailingTrivia(SyntaxFactory.Space),
                                 [firstExpressionToken] = firstExpressionToken.WithLeadingTrivia(SyntaxFactory.TriviaList()),
                             };

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            replacementMap[previousToken] = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        }

        return updatedNode.ReplaceTokens(replacementMap.Keys, (original, _) => replacementMap[original]);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);

        if (node == null)
        {
            return null;
        }

        if (node.ExpressionBody != null)
        {
            node = CollapseExpressionBodiedProperty(node);
        }

        if (node.AccessorList != null)
        {
            node = _accessorListLayout.Apply(node);
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}