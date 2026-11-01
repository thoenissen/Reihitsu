using System;

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
    /// line of the token that precedes the arrow — the property name or the indexer's closing bracket. Each join writes
    /// the minimal separator (FMT-BREAK-031), and a gap that is already on one line keeps its whitespace: the spaces
    /// around the arrow are the horizontal-spacing phase's (FMT-SPACE-017). The layout is kept when either join would
    /// cross a comment, a directive, or disabled text
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
        }

        var firstExpressionToken = getExpressionBody(updatedNode).Expression.GetFirstToken();

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstExpressionToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(firstExpressionToken.GetPreviousToken()))
        {
            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, firstExpressionToken);
        }

        return updatedNode;
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