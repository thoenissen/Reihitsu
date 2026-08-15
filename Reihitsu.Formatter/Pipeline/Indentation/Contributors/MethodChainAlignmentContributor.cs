using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns dots in method chains so that continuation dots align to the first chain link's column.
/// Conditional access operators (<c>?.</c>) and null-forgiving operators introducing an invoked
/// link (<c>!.</c>) are treated as chain links, matching the RH5201 analyzer's definition
/// </summary>
internal sealed class MethodChainAlignmentContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Determines the anchor column for a method chain. The anchor is the first collected dot that
    /// is itself an invoked chain link — a plain dot on an invoked member access, a conditional-access
    /// operator, or a null-forgiving operator introducing an invoked link. If no such link precedes
    /// the first wrapped dot, the anchor falls back to the first collected dot
    /// </summary>
    /// <param name="dots">The collected chain dots</param>
    /// <param name="model">The layout model</param>
    /// <returns>The column to which continuation-line dots should be aligned</returns>
    private static int FindChainAnchorColumn(List<SyntaxToken> dots, LayoutModel model)
    {
        foreach (var dot in dots)
        {
            if (LayoutComputer.IsFirstOnLine(dot))
            {
                break;
            }

            if (ChainWalker.IsInvokedLinkDot(dot))
            {
                return GetChainAnchorColumn(dot, dots[0], model);
            }
        }

        return GetChainAnchorColumn(dots[0], dots[0], model);
    }

    /// <summary>
    /// Computes the alignment column for the chain anchor. When the chain's first collected dot
    /// shares a line with a closing brace of an initializer expression, the initializer contributor
    /// may not have adjusted that line yet (due to pre-order traversal). In that case, and only when
    /// the anchor itself is still on that same source line, the column is computed directly from the
    /// creation expression's <c>new</c> keyword position, preserving the anchor's original source
    /// offset from the closing brace — even when the anchor is a later link separated from the brace
    /// by a non-link prefix dot. An anchor that wraps onto a later line has no such fixed offset from
    /// the brace, so it falls back to the ordinary adjusted-column lookup, which resolves the anchor's
    /// own line through the layout model instead of mixing columns from two different lines
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
            && LayoutComputer.GetLine(anchorDot) == LayoutComputer.GetLine(prevToken))
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

    /// <summary>
    /// Computes the continuation column for a chain that a preceding comment keeps wrapped. Such a
    /// chain has no first dot to align against, so the continuation dots line up with the chain root
    /// token itself. Measuring from the root rather than from the continuation line's block
    /// indentation keeps the chain under its root even when the root sits far into the line, for
    /// example inside an argument or a lambda body
    /// </summary>
    /// <param name="node">The chain node being laid out</param>
    /// <param name="model">The layout model</param>
    /// <returns>The column for the chain's continuation lines</returns>
    private static int GetCommentExemptContinuationColumn(SyntaxNode node, LayoutModel model)
    {
        return LayoutComputer.GetAdjustedColumn(node.GetFirstToken(), model);
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        var dots = CreateDotsForNode(node);

        if (dots.Count == 0)
        {
            return;
        }

        if (ShouldKeepFirstWrappedCallOnContinuationLine(dots[0]))
        {
            var continuationColumn = GetCommentExemptContinuationColumn(node, model);

            foreach (var dot in dots)
            {
                LayoutComputer.SetIfFirstOnLine(dot, continuationColumn, "MethodChainCommentExempt", model);
            }

            return;
        }

        if (dots.Count < 2)
        {
            return;
        }

        var firstDotColumn = FindChainAnchorColumn(dots, model);

        for (var dotIndex = 1; dotIndex < dots.Count; dotIndex++)
        {
            LayoutComputer.SetIfFirstOnLine(dots[dotIndex], firstDotColumn, "MethodChain", model);
        }
    }

    /// <summary>
    /// Creates chain dot tokens for supported node types
    /// </summary>
    /// <param name="node">The syntax node to inspect</param>
    /// <returns>The collected dot tokens; empty when the node is not handled</returns>
    private static List<SyntaxToken> CreateDotsForNode(SyntaxNode node)
    {
        switch (node)
        {
            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    if (conditionalAccess.Parent is ConditionalAccessExpressionSyntax)
                    {
                        return [];
                    }

                    List<SyntaxToken> dots = [];

                    ChainWalker.CollectAlignmentDots(conditionalAccess, dots);

                    return dots;
                }

            case InvocationExpressionSyntax invocation:
                {
                    if (ShouldSkipInvocation(invocation))
                    {
                        return [];
                    }

                    var chainRoot = GetChainRoot(invocation);
                    List<SyntaxToken> dots = [];

                    ChainWalker.CollectAlignmentDots(chainRoot, dots);

                    return dots;
                }

            default:
                {
                    return [];
                }
        }
    }

    /// <summary>
    /// Determines whether an invocation should be skipped for chain alignment
    /// </summary>
    /// <param name="invocation">The invocation node to evaluate</param>
    /// <returns><see langword="true"/> if the invocation should be skipped; otherwise, <see langword="false"/></returns>
    private static bool ShouldSkipInvocation(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax
            && invocation.Expression is not MemberBindingExpressionSyntax)
        {
            return true;
        }

        // Skip if this invocation is inside a chain that has an outer invocation
        var ancestor = invocation.Parent;

        while (ancestor is MemberAccessExpressionSyntax)
        {
            ancestor = ancestor.Parent;
        }

        if (ancestor is InvocationExpressionSyntax)
        {
            return true;
        }

        return ChainWalker.IsInsideConditionalAccess(invocation);
    }

    /// <summary>
    /// Determines whether an entire chain should be skipped. The first dot stays on its continuation
    /// line whenever the line-break phase refused to join it onto the root line, which happens for
    /// every kind of unjoinable trivia — a comment, a preprocessor directive, or disabled text. This
    /// mirrors <see cref="LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia"/> so the alignment
    /// phase stays in lock-step with the refusal instead of recognizing comments only (issue #489)
    /// </summary>
    /// <param name="firstDot">The first chain link token</param>
    /// <returns><see langword="true"/> if the chain should be skipped; otherwise, <see langword="false"/></returns>
    private static bool ShouldKeepFirstWrappedCallOnContinuationLine(SyntaxToken firstDot)
    {
        if (LayoutComputer.IsFirstOnLine(firstDot) == false)
        {
            return false;
        }

        var previousToken = firstDot.GetPreviousToken();

        if (previousToken == default
            || previousToken.IsKind(SyntaxKind.None))
        {
            return SyntaxTriviaUtilities.ContainsUnjoinableTrivia(firstDot.LeadingTrivia);
        }

        return LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, firstDot);
    }

    /// <summary>
    /// Gets the outer chain root for an invocation, including trailing member-access properties
    /// </summary>
    /// <param name="invocation">The invocation expression</param>
    /// <returns>The chain root expression</returns>
    private static ExpressionSyntax GetChainRoot(InvocationExpressionSyntax invocation)
    {
        // Walk up to include trailing member accesses after the last invocation
        // (e.g., .GetLineSpan().StartLinePosition where .StartLinePosition is a property)
        ExpressionSyntax chainRoot = invocation;

        while (chainRoot.Parent is MemberAccessExpressionSyntax trailingAccess
               && trailingAccess.Parent is not InvocationExpressionSyntax)
        {
            chainRoot = trailingAccess;
        }

        return chainRoot;
    }

    #endregion // ILayoutContributor
}