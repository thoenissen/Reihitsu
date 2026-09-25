using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

/// <summary>
/// Lays out the accessor list of a property or an indexer: a simple auto-accessor list collapses to a single
/// line, a single-line declaration whose auto accessors carry attributes stays as written, and every other
/// accessor list is laid out over several lines with each accessor starting its own line
/// </summary>
internal sealed class AccessorListLayout
{
    #region Fields

    /// <summary>
    /// The brace placer
    /// </summary>
    private readonly BracePlacer _bracePlacer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bracePlacer">The brace placer</param>
    public AccessorListLayout(BracePlacer bracePlacer)
    {
        _bracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Lays out the accessor list of the given property or indexer declaration
    /// </summary>
    /// <typeparam name="TNode">The declaration type</typeparam>
    /// <param name="node">The property or indexer declaration</param>
    /// <returns>The declaration with its accessor list laid out</returns>
    /// <remarks>
    /// An attribute on an accessor makes the auto-accessor list no longer simple: RH5530/RH5531 own that
    /// accessor's attribute layout instead, so the list is never collapsed. It stays as written while the
    /// declaration occupies a single line — the same condition under which <c>AttributeTargetFormattingRewriter</c>
    /// keeps the accessor attributes inline — and is laid out like every other list otherwise, so the attribute
    /// placement that follows never meets a list that is neither single-line nor laid out.
    /// </remarks>
    public TNode Apply<TNode>(TNode node)
        where TNode : BasePropertyDeclarationSyntax
    {
        var accessorList = node.AccessorList;

        if (accessorList == null || accessorList.OpenBraceToken.IsMissing)
        {
            return node;
        }

        if (LineBreakDetection.ShouldNormalizeAccessorListBraces(accessorList) == false)
        {
            if (HasAttributedAccessor(accessorList))
            {
                if (SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists(node))
                {
                    return node;
                }
            }
            else if (CanCollapseAutoAccessorListToSingleLine(node))
            {
                return CollapseAutoAccessorList(node);
            }
        }

        node = _bracePlacer.NormalizeOwnedBraces(node,
                                                 static declaration => declaration.AccessorList.OpenBraceToken,
                                                 static declaration => declaration.AccessorList.CloseBraceToken);

        return _bracePlacer.EnsureAccessorsStartOnSeparateLines(node, static declaration => declaration.AccessorList);
    }

    /// <summary>
    /// Gets the first token of the declaration signature while skipping declaration-level attributes
    /// </summary>
    /// <param name="node">The property or indexer declaration to inspect</param>
    /// <returns>The first signature token</returns>
    private static SyntaxToken GetSingleLineSignatureStartToken(BasePropertyDeclarationSyntax node)
    {
        if (node.Modifiers.Count > 0)
        {
            return node.Modifiers[0];
        }

        return node.Type.GetFirstToken();
    }

    /// <summary>
    /// Determines whether the given auto-accessor list can be collapsed to a single line
    /// </summary>
    /// <param name="node">The property or indexer declaration to inspect</param>
    /// <returns><see langword="true"/> if the auto-accessor list can be collapsed; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The initializer of a property is a sibling that follows the accessor list and is laid out by other subphases.
    /// Collapsing the accessor list does not touch it, so a multi-line initializer must not prevent the
    /// simple auto-property accessor list from staying single-line.
    /// The comment and directive guard is interior-scoped for the same reason: the collapse rewrites only
    /// the closing brace's leading trivia, so a comment trailing that brace sits outside the accessor list
    /// and is never crossed. Guarding the accessor list's full span instead would count that trailing
    /// comment and force an already-correct single-line declaration apart.
    /// Trivia the collapse would cross is still guarded: inside the accessor list by the check below, and
    /// in the gap between the signature and the opening brace by <see cref="LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia"/>.
    /// The signature itself — for an indexer including its parameter list — must already occupy a single line
    /// </remarks>
    private static bool CanCollapseAutoAccessorListToSingleLine(BasePropertyDeclarationSyntax node)
    {
        if (node?.AccessorList == null || SyntaxNodeUtilities.InteriorContainsCommentOrDirective(node.AccessorList))
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

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(tokenBeforeOpenBrace, node.AccessorList.OpenBraceToken))
        {
            return false;
        }

        if (SyntaxNodeUtilities.IsSingleLineSpan(node.SyntaxTree, TextSpan.FromBounds(signatureStartToken.SpanStart, tokenBeforeOpenBrace.Span.End)) == false)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether any accessor in the accessor list carries its own attribute list. Mirrors
    /// <c>RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.HasAttributedAccessor</c>: such a declaration is no
    /// longer simple, and its accessor attribute layout belongs to RH5530/RH5531 instead
    /// </summary>
    /// <param name="accessorList">The accessor list to inspect</param>
    /// <returns><see langword="true"/> if at least one accessor carries an attribute list; otherwise, <see langword="false"/></returns>
    private static bool HasAttributedAccessor(AccessorListSyntax accessorList)
    {
        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.AttributeLists.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Collapses a multi-line auto-accessor list to a single line
    /// </summary>
    /// <typeparam name="TNode">The declaration type</typeparam>
    /// <param name="node">The property or indexer declaration with an auto-accessor list</param>
    /// <returns>The declaration with a single-line accessor list</returns>
    private static TNode CollapseAutoAccessorList<TNode>(TNode node)
        where TNode : BasePropertyDeclarationSyntax
    {
        if (node?.AccessorList == null || LineBreakDetection.IsAutoPropertyAccessorList(node.AccessorList) == false)
        {
            return node;
        }

        var updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, node.AccessorList.OpenBraceToken);

        updatedNode = CollapseAccessorTokensToSingleLine(updatedNode);
        updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, updatedNode.AccessorList.CloseBraceToken);

        var replacementMap = BuildAccessorTriviaReplacementMap(updatedNode.AccessorList);

        return updatedNode.ReplaceTokens(replacementMap.Keys, (original, _) => replacementMap[original]);
    }

    /// <summary>
    /// Collapses each accessor's attribute-list brackets, modifiers, keyword, and semicolon onto the accessor line
    /// </summary>
    /// <typeparam name="TNode">The declaration type</typeparam>
    /// <param name="updatedNode">The declaration whose accessors are collapsed</param>
    /// <returns>The declaration with each accessor collapsed onto a single line</returns>
    private static TNode CollapseAccessorTokensToSingleLine<TNode>(TNode updatedNode)
        where TNode : BasePropertyDeclarationSyntax
    {
        for (var accessorIndex = 0; accessorIndex < updatedNode.AccessorList.Accessors.Count; accessorIndex++)
        {
            var accessor = updatedNode.AccessorList.Accessors[accessorIndex];

            for (var attributeListIndex = 0; attributeListIndex < accessor.AttributeLists.Count; attributeListIndex++)
            {
                updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.AttributeLists[attributeListIndex].OpenBracketToken);
                accessor = updatedNode.AccessorList.Accessors[accessorIndex];
            }

            // Modifiers precede the keyword, so an accessor such as "private set;" carries the line break and the
            // indentation on its modifier rather than on its keyword. Collapsing only the keyword would leave that
            // indentation behind as stray spacing that a second pass has to clean up.
            for (var modifierIndex = 0; modifierIndex < accessor.Modifiers.Count; modifierIndex++)
            {
                updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.Modifiers[modifierIndex]);
                accessor = updatedNode.AccessorList.Accessors[accessorIndex];
            }

            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.Keyword);
            accessor = updatedNode.AccessorList.Accessors[accessorIndex];

            if (accessor.SemicolonToken.IsMissing == false)
            {
                updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.SemicolonToken);
            }
        }

        return updatedNode;
    }

    /// <summary>
    /// Builds the trivia replacement map that normalizes the spacing of a collapsed accessor list
    /// </summary>
    /// <param name="accessorList">The collapsed accessor list</param>
    /// <returns>The token replacement map</returns>
    private static Dictionary<SyntaxToken, SyntaxToken> BuildAccessorTriviaReplacementMap(AccessorListSyntax accessorList)
    {
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
            foreach (var openBracketToken in accessor.AttributeLists.Select(attributeList => attributeList.OpenBracketToken))
            {
                replacementMap[openBracketToken] = openBracketToken.WithLeadingTrivia(SyntaxFactory.TriviaList());
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

        return replacementMap;
    }

    #endregion // Methods
}