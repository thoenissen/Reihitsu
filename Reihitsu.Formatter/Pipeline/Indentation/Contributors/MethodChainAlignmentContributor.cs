using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns the continuation lines of member-access chains, based on the shared <see cref="FluentChain"/> model. Every
/// link operator that starts a line (<c>.</c>, <c>?.</c>, <c>!.</c> or <c>!?.</c>) is aligned to the chain's anchor;
/// the rest of an operator that blocking trivia keeps on the next line goes under the operator's first token; an
/// attached part that blocking trivia keeps on its own line goes to the anchor, or to the root when it precedes the
/// first link
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

        return GetAdjustedColumn(anchorDot, model);
    }

    /// <summary>
    /// Gets the column a token has after formatting. A token on the closing-delimiter line of a multi-line raw string
    /// literal is not on a line the layout model indents: the raw-string alignment that runs after indentation moves that
    /// line until the closing delimiter is under the opening quotes, so the token's column is predicted with the same shift
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="model">The layout model</param>
    /// <returns>The column of the token after formatting</returns>
    private static int GetAdjustedColumn(SyntaxToken token, LayoutModel model)
    {
        var previousToken = token.GetPreviousToken();

        while (previousToken.IsKind(SyntaxKind.None) == false
               && LayoutComputer.GetLine(previousToken) == LayoutComputer.GetLine(token)
               && IsMultiLineRawStringEnd(previousToken) == false)
        {
            previousToken = previousToken.GetPreviousToken();
        }

        if (IsMultiLineRawStringEnd(previousToken) == false
            || previousToken.GetLocation().GetLineSpan().EndLinePosition.Line != LayoutComputer.GetLine(token))
        {
            return LayoutComputer.GetAdjustedColumn(token, model);
        }

        int openingColumn;
        int closingColumn;

        if (previousToken.Parent is InterpolatedStringExpressionSyntax interpolatedString)
        {
            var startToken = interpolatedString.StringStartToken;

            openingColumn = LayoutComputer.GetAdjustedColumn(startToken, model) + RawStringLiteralUtilities.GetQuoteOffset(startToken.Text);
            closingColumn = GetLeadingSpaceCountOfLastLine(previousToken.Text);
        }
        else
        {
            openingColumn = LayoutComputer.GetAdjustedColumn(previousToken, model);
            closingColumn = GetLeadingSpaceCountOfLastLine(previousToken.Text);
        }

        return LayoutComputer.GetColumn(token) + (openingColumn - closingColumn);
    }

    /// <summary>
    /// Determines whether a token ends a multi-line raw string literal, interpolated or not
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token ends a multi-line raw string literal</returns>
    private static bool IsMultiLineRawStringEnd(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
               || token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken)
               || (token.IsKind(SyntaxKind.InterpolatedRawStringEndToken)
                   && token.Parent is InterpolatedStringExpressionSyntax interpolatedString
                   && interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken));
    }

    /// <summary>
    /// Gets the number of spaces in front of the closing delimiter on the last line of a raw string token's text
    /// </summary>
    /// <param name="tokenText">The token text</param>
    /// <returns>The number of leading spaces of the last line</returns>
    private static int GetLeadingSpaceCountOfLastLine(string tokenText)
    {
        var lastLine = tokenText.Substring(tokenText.LastIndexOf('\n') + 1);

        return lastLine.Length - lastLine.TrimStart(' ').Length;
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

    /// <summary>
    /// Aligns every token after the first one in a token run (the rest of an operator, or the rest of an attached part)
    /// under the run's first token when blocking trivia keeps it on its own line
    /// </summary>
    /// <param name="tokens">The token run in source order</param>
    /// <param name="model">The layout model</param>
    private static void AlignRestOfRun(IReadOnlyList<SyntaxToken> tokens, LayoutModel model)
    {
        for (var tokenIndex = 1; tokenIndex < tokens.Count; tokenIndex++)
        {
            if (LayoutComputer.IsFirstOnLine(tokens[tokenIndex]))
            {
                LayoutComputer.SetIfFirstOnLine(tokens[tokenIndex], GetAdjustedColumn(tokens[0], model), "MethodChainOperator", model);
            }
        }
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        var chain = FluentChain.Create(node, false);

        if (chain == null)
        {
            return;
        }

        var rootColumn = LayoutComputer.GetAdjustedColumn(node.GetFirstToken(), model);

        if (chain.Links.Count == 0)
        {
            foreach (var attachedPart in chain.AttachedParts)
            {
                AlignRestOfRun(attachedPart.Tokens, model);
            }

            return;
        }

        var anchorColumn = GetAnchorColumn(chain, model, rootColumn);
        var firstOperatorStart = chain.FirstLink.OperatorToken.SpanStart;

        foreach (var link in chain.Links)
        {
            LayoutComputer.SetIfFirstOnLine(link.OperatorToken, anchorColumn, "MethodChain", model);

            AlignRestOfRun(link.OperatorTokens, model);
        }

        foreach (var attachedPart in chain.AttachedParts)
        {
            var column = attachedPart.FirstToken.SpanStart < firstOperatorStart
                             ? rootColumn
                             : anchorColumn;

            LayoutComputer.SetIfFirstOnLine(attachedPart.FirstToken, column, "MethodChainAttachedPart", model);

            AlignRestOfRun(attachedPart.Tokens, model);
        }
    }

    #endregion // ILayoutContributor
}