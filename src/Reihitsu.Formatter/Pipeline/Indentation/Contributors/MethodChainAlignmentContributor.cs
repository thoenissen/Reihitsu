using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns the continuation lines of member-access chains, based on the shared <see cref="FluentChain"/> model. Every
/// link operator that starts a line (<c>.</c>, <c>?.</c>, <c>!.</c> or <c>!?.</c>) is aligned to the chain's anchor.
/// No later token of a link operator starts a line: the line-break phase joins every gap inside one (FMT-BREAK-019),
/// and an input that holds a comment, a directive, or disabled text there is refused (FMT-UNSUP-015). An
/// attached part never starts a line: the line-break phase joins every gap in front of and inside one (FMT-BREAK-039,
/// FMT-BREAK-040), and an input that holds a comment, a directive, or disabled text there is refused (FMT-UNSUP-014).
/// Only the outermost node of a chain is aligned. The chain rule decides which node that is, and for a node
/// whose decision reads above a node-level formatting root it takes the root's
/// <see cref="FormattingContext.RootChainBoundary"/>, as the line-break phase does
/// </summary>
internal sealed class MethodChainAlignmentContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Determines the anchor column of a chain from its <see cref="FluentChain.GetAnchorLink"/>, or the root column when
    /// the chain aligns to its root
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <param name="model">The layout model</param>
    /// <param name="rootColumn">The column of the chain's root</param>
    /// <returns>The anchor column</returns>
    private static int GetAnchorColumn(FluentChain chain, LayoutModel model, int rootColumn)
    {
        var anchorLink = chain.GetAnchorLink();

        return anchorLink == null
                   ? rootColumn
                   : GetChainAnchorColumn(anchorLink.OperatorToken, chain.FirstLink.OperatorToken, model);
    }

    /// <summary>
    /// Computes the alignment column for the chain anchor. When the chain's first collected dot
    /// directly follows a closing brace of an initializer expression, the initializer contributor may
    /// not have adjusted that brace's line yet (due to pre-order traversal) — regardless of whether
    /// the first dot itself was wrapped onto its own line. In that case, and only when the anchor is
    /// still on the first dot's own line, the column is computed directly from the creation
    /// expression's <c>new</c> keyword position, preserving the anchor's original source offset from
    /// the closing brace — even when the anchor is a later link separated from the first dot by a
    /// non-link prefix dot. An anchor that wraps onto a later line than the first dot has no such
    /// fixed offset from the brace, so it falls back to the ordinary adjusted-column lookup, which
    /// resolves the anchor's own line through the layout model instead of mixing columns from
    /// unrelated lines.
    /// Rebasing onto the <c>new</c> keyword is only valid when the closing brace itself ends up at
    /// that keyword's column, which happens exactly when the brace starts its own line — the same
    /// condition <see cref="ObjectInitializerContributor"/> uses before it moves the brace there. A
    /// brace that shares its line with earlier content (<c>new[] { 1, 2, 3 }.Select(…)</c>, or a
    /// multi-line initializer closed by <c>2 }</c>) keeps its own line's indentation, so the rebase
    /// would shift the anchor by the initializer's printed width; such a chain falls back to the
    /// ordinary lookup instead.
    /// </summary>
    /// <param name="anchorDot">The chain-link token chosen as the alignment anchor</param>
    /// <param name="firstDot">The chain's first collected dot, used to detect an initializer-rooted chain</param>
    /// <param name="model">The layout model</param>
    /// <returns>The adjusted column for the chain anchor</returns>
    private static int GetChainAnchorColumn(SyntaxToken anchorDot, SyntaxToken firstDot, LayoutModel model)
    {
        var prevToken = firstDot.GetPreviousToken();

        if (prevToken.IsKind(SyntaxKind.CloseBraceToken)
            && prevToken.Parent is InitializerExpressionSyntax initExpr
            && LayoutComputer.IsFirstOnLine(prevToken)
            && LayoutComputer.GetLine(anchorDot) == LayoutComputer.GetLine(firstDot))
        {
            var newKeyword = GetCreationNewKeyword(initExpr.Parent);

            if (newKeyword != default)
            {
                var dotOffset = LayoutComputer.GetColumn(anchorDot) - LayoutComputer.GetColumn(prevToken);
                var newColumn = LayoutComputer.GetAdjustedColumn(newKeyword, model);

                return newColumn + dotOffset;
            }
        }

        return LayoutComputer.GetAdjustedColumn(anchorDot, model);
    }

    /// <summary>
    /// Returns the <c>new</c> keyword token from a creation expression, or <see langword="default"/>
    /// if the node is not a recognized creation expression
    /// </summary>
    /// <param name="node">The potential creation expression node</param>
    /// <returns>The <c>new</c> keyword token, or <see langword="default"/></returns>
    private static SyntaxToken GetCreationNewKeyword(SyntaxNode node)
    {
        switch (node)
        {
            case ObjectCreationExpressionSyntax objCreation:
                return objCreation.NewKeyword;

            case ArrayCreationExpressionSyntax arrayCreation:
                return arrayCreation.NewKeyword;

            case ImplicitArrayCreationExpressionSyntax implicitArray:
                return implicitArray.NewKeyword;

            case ImplicitObjectCreationExpressionSyntax implicitObj:
                return implicitObj.NewKeyword;

            default:
                return default;
        }
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        var chain = FluentChain.Create(node, false, context.RootChainBoundary);

        if (chain == null
            || chain.Links.Count == 0)
        {
            return;
        }

        var rootColumn = LayoutComputer.GetAdjustedColumn(node.GetFirstToken(), model);
        var anchorColumn = GetAnchorColumn(chain, model, rootColumn);

        foreach (var link in chain.Links)
        {
            LayoutComputer.SetIfFirstOnLine(link.OperatorToken, anchorColumn, "MethodChain", model);
        }
    }

    #endregion // ILayoutContributor
}