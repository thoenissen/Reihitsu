using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies line-break rules for property layout: the arrow placement of expression-bodied properties and
/// indexers, and the property accessor-list layout shared with indexers (<see cref="AccessorListLayout"/>)
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
    /// Moves the arrow of an expression-bodied property or indexer and the first token of its expression onto the
    /// line of the token that precedes the arrow — the property name or the indexer's closing bracket. The layout is
    /// kept when either join would cross a comment, a directive, or disabled text
    /// </summary>
    /// <typeparam name="TNode">The declaration type</typeparam>
    /// <param name="node">The property or indexer declaration</param>
    /// <param name="getExpressionBody">Gets the declaration's expression body</param>
    /// <returns>The declaration with the arrow on the signature line, or unchanged when it has no expression body or the join is unsafe</returns>
    private static TNode CollapseExpressionBody<TNode>(TNode node, Func<TNode, ArrowExpressionClauseSyntax> getExpressionBody)
        where TNode : BasePropertyDeclarationSyntax
    {
        var expressionBody = getExpressionBody(node);

        if (expressionBody == null)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(expressionBody.ArrowToken.GetPreviousToken(), expressionBody.ArrowToken)
            || LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(expressionBody.ArrowToken, expressionBody.Expression.GetFirstToken()))
        {
            return node;
        }

        var updatedNode = node;
        var arrowToken = getExpressionBody(updatedNode).ArrowToken;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(arrowToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(arrowToken.GetPreviousToken()))
        {
            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, arrowToken);
            arrowToken = getExpressionBody(updatedNode).ArrowToken;
        }

        if (arrowToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            updatedNode = updatedNode.ReplaceToken(arrowToken, arrowToken.WithLeadingTrivia(arrowToken.LeadingTrivia.Add(SyntaxFactory.Space)));
        }

        var firstExpressionToken = getExpressionBody(updatedNode).Expression.GetFirstToken();

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstExpressionToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(firstExpressionToken.GetPreviousToken()))
        {
            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, firstExpressionToken);
        }

        arrowToken = getExpressionBody(updatedNode).ArrowToken;
        firstExpressionToken = getExpressionBody(updatedNode).Expression.GetFirstToken();

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
            node = CollapseExpressionBody(node, static declaration => declaration.ExpressionBody);
        }

        if (node.AccessorList != null)
        {
            node = _accessorListLayout.Apply(node);
        }

        return node;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Only the expression-bodied form is handled here; the accessor list of an indexer is laid out by
    /// <see cref="DeclarationBraceLineBreakRewriter"/>
    /// </remarks>
    public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (IndexerDeclarationSyntax)base.VisitIndexerDeclaration(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        return CollapseExpressionBody(node, static declaration => declaration.ExpressionBody);
    }

    #endregion // CSharpSyntaxVisitor
}