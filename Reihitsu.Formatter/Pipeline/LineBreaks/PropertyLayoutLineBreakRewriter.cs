using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for property layout: expression-bodied property collapse and
/// auto-property single-line collapse and accessor brace placement
/// </summary>
internal sealed class PropertyLayoutLineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The token gap normalizer
    /// </summary>
    private readonly TokenGapNormalizer _gapNormalizer;

    /// <summary>
    /// The brace placer
    /// </summary>
    private readonly BracePlacer _bracePlacer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="bracePlacer">The brace placer</param>
    public PropertyLayoutLineBreakRewriter(CancellationToken cancellationToken,
                                           TokenGapNormalizer gapNormalizer,
                                           BracePlacer bracePlacer)
    {
        _cancellationToken = cancellationToken;
        _gapNormalizer = gapNormalizer;
        _bracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses a multi-line expression-bodied property to a single line
    /// </summary>
    /// <param name="node">The property declaration with an expression body</param>
    /// <returns>The property declaration collapsed to a single line</returns>
    private static PropertyDeclarationSyntax CollapseExpressionBodiedProperty(PropertyDeclarationSyntax node)
    {
        if (node?.ExpressionBody == null)
        {
            return null;
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

    /// <summary>
    /// Determines whether the given node contains comments or directives
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if comments or directives are present; otherwise, <see langword="false"/></returns>
    private static bool HasCommentsOrDirectives(SyntaxNode node)
    {
        foreach (var trivia in node.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsDirective || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given text span occupies a single line
    /// </summary>
    /// <param name="syntaxTree">Syntax tree</param>
    /// <param name="span">Text span</param>
    /// <returns><see langword="true"/> if the span occupies a single line; otherwise, <see langword="false"/></returns>
    private static bool IsSingleLineSpan(SyntaxTree syntaxTree, TextSpan span)
    {
        var lineSpan = syntaxTree.GetLineSpan(span);

        return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line;
    }

    /// <summary>
    /// Gets the first token of the property signature while skipping property-level attributes
    /// </summary>
    /// <param name="node">The property declaration to inspect</param>
    /// <returns>The first signature token</returns>
    private static SyntaxToken GetSingleLineSignatureStartToken(PropertyDeclarationSyntax node)
    {
        if (node.Modifiers.Count > 0)
        {
            return node.Modifiers[0];
        }

        return node.Type.GetFirstToken();
    }

    /// <summary>
    /// Determines whether the given auto-property can be collapsed to a single line
    /// </summary>
    /// <param name="node">The property declaration to inspect</param>
    /// <returns><see langword="true"/> if the auto-property can be collapsed; otherwise, <see langword="false"/></returns>
    private static bool CanCollapseAutoPropertyToSingleLine(PropertyDeclarationSyntax node)
    {
        if (node?.AccessorList == null || HasCommentsOrDirectives(node.AccessorList))
        {
            return false;
        }

        var tokenBeforeOpenBrace = node.AccessorList.OpenBraceToken.GetPreviousToken();
        var signatureStartToken = GetSingleLineSignatureStartToken(node);

        if (signatureStartToken == default
            || signatureStartToken.IsKind(SyntaxKind.None)
            || tokenBeforeOpenBrace == default
            || tokenBeforeOpenBrace.IsKind(SyntaxKind.None))
        {
            return false;
        }

        if (IsSingleLineSpan(node.SyntaxTree, TextSpan.FromBounds(signatureStartToken.SpanStart, tokenBeforeOpenBrace.Span.End)) == false)
        {
            return false;
        }

        if (node.Initializer != null)
        {
            if (HasCommentsOrDirectives(node.Initializer))
            {
                return false;
            }

            if (IsSingleLineSpan(node.SyntaxTree, node.Initializer.Value.Span) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Collapses a multi-line auto-property accessor list to a single line
    /// </summary>
    /// <param name="node">The property declaration with an auto-property accessor list</param>
    /// <returns>The property declaration with a single-line accessor list</returns>
    private static PropertyDeclarationSyntax CollapseAutoPropertyAccessorList(PropertyDeclarationSyntax node)
    {
        if (node?.AccessorList == null || LineBreakDetection.IsAutoPropertyAccessorList(node.AccessorList) == false)
        {
            return node;
        }

        var updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, node.AccessorList.OpenBraceToken);

        for (var accessorIndex = 0; accessorIndex < updatedNode.AccessorList.Accessors.Count; accessorIndex++)
        {
            var accessor = updatedNode.AccessorList.Accessors[accessorIndex];

            for (var attributeListIndex = 0; attributeListIndex < accessor.AttributeLists.Count; attributeListIndex++)
            {
                updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.AttributeLists[attributeListIndex].OpenBracketToken);
                accessor = updatedNode.AccessorList.Accessors[accessorIndex];
            }

            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.Keyword);
            accessor = updatedNode.AccessorList.Accessors[accessorIndex];

            if (accessor.SemicolonToken.IsMissing == false)
            {
                updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.SemicolonToken);
            }
        }

        updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, updatedNode.AccessorList.CloseBraceToken);

        var accessorList = updatedNode.AccessorList;
        var previousToken = accessorList.OpenBraceToken.GetPreviousToken();
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>
                             {
                                 [accessorList.OpenBraceToken] = accessorList.OpenBraceToken.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                                                                            .WithTrailingTrivia(SyntaxFactory.Space),
                                 [accessorList.CloseBraceToken] = accessorList.CloseBraceToken.WithLeadingTrivia(SyntaxFactory.TriviaList()),
                             };

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            replacementMap[previousToken] = previousToken.WithTrailingTrivia(SyntaxFactory.Space);
        }

        foreach (var accessor in accessorList.Accessors)
        {
            foreach (var attributeList in accessor.AttributeLists)
            {
                replacementMap[attributeList.OpenBracketToken] = attributeList.OpenBracketToken.WithLeadingTrivia(SyntaxFactory.TriviaList());
            }

            var tokenBeforeKeyword = accessor.Keyword.GetPreviousToken();

            if (tokenBeforeKeyword != default && tokenBeforeKeyword.IsKind(SyntaxKind.None) == false)
            {
                replacementMap[tokenBeforeKeyword] = tokenBeforeKeyword.WithTrailingTrivia(SyntaxFactory.Space);
            }

            replacementMap[accessor.Keyword] = accessor.Keyword.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                                               .WithTrailingTrivia(SyntaxFactory.TriviaList());

            if (accessor.SemicolonToken.IsMissing == false)
            {
                replacementMap[accessor.SemicolonToken] = accessor.SemicolonToken.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                                                                 .WithTrailingTrivia(SyntaxFactory.Space);
            }
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
            if (LineBreakDetection.IsAutoPropertyAccessorList(node.AccessorList))
            {
                if (CanCollapseAutoPropertyToSingleLine(node))
                {
                    node = CollapseAutoPropertyAccessorList(node);
                }
                else
                {
                    node = _gapNormalizer.NormalizeGapBeforeToken(node, node.AccessorList.OpenBraceToken, blankLineCount: 0);
                    node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.AccessorList.OpenBraceToken);
                    node = _gapNormalizer.NormalizeGapBeforeToken(node, node.AccessorList.CloseBraceToken, blankLineCount: 0);
                }
            }
            else
            {
                node = _gapNormalizer.NormalizeGapBeforeToken(node, node.AccessorList.OpenBraceToken, blankLineCount: 0);
                node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.AccessorList.OpenBraceToken);
                node = _gapNormalizer.NormalizeGapBeforeToken(node, node.AccessorList.CloseBraceToken, blankLineCount: 0);
            }
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}