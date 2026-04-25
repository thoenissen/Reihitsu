using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns dots in method chains so that continuation dots align to the first dot's column.
/// Conditional access operators (<c>?.</c>) are treated as chain links.
/// </summary>
internal sealed class MethodChainAlignmentContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Determines the anchor column for a method chain. If the first dot in the chain is part of
    /// a qualified name prefix (e.g., <c>System.Linq.Enumerable</c>), the anchor is the first
    /// invocation dot that remains on the root line. Otherwise, the anchor is the first dot.
    /// </summary>
    /// <param name="dots">The collected chain dots.</param>
    /// <param name="model">The layout model.</param>
    /// <returns>The column to which continuation-line dots should be aligned.</returns>
    private static int FindChainAnchorColumn(List<SyntaxToken> dots, LayoutModel model)
    {
        for (var dotIndex = 0; dotIndex < dots.Count; dotIndex++)
        {
            var dot = dots[dotIndex];

            if (LayoutComputer.IsFirstOnLine(dot))
            {
                break;
            }

            var isInvocationDot = dot.Parent is MemberAccessExpressionSyntax memberAccess
                                  && memberAccess.Parent is InvocationExpressionSyntax;

            if (isInvocationDot)
            {
                return GetChainAnchorColumn(dot, model);
            }
        }

        return GetChainAnchorColumn(dots[0], model);
    }

    /// <summary>
    /// Computes the alignment column for the first dot in a chain. When the first dot shares
    /// a line with a closing brace of an initializer expression, the initializer contributor
    /// may not have adjusted that line yet (due to pre-order traversal). In that case, the
    /// column is computed directly from the creation expression's <c>new</c> keyword position.
    /// </summary>
    /// <param name="firstDot">The first dot token in the chain.</param>
    /// <param name="model">The layout model.</param>
    /// <returns>The adjusted column for the chain anchor.</returns>
    private static int GetChainAnchorColumn(SyntaxToken firstDot, LayoutModel model)
    {
        var prevToken = firstDot.GetPreviousToken();

        if (prevToken.IsKind(SyntaxKind.CloseBraceToken)
            && prevToken.Parent is InitializerExpressionSyntax initExpr)
        {
            var newKeyword = GetCreationNewKeyword(initExpr.Parent);

            if (newKeyword != default)
            {
                var dotOffset = LayoutComputer.GetColumn(firstDot) - LayoutComputer.GetColumn(prevToken);
                var newColumn = LayoutComputer.GetAdjustedColumn(newKeyword, model);

                return newColumn + dotOffset;
            }
        }

        return LayoutComputer.GetAdjustedColumn(firstDot, model);
    }

    /// <summary>
    /// Returns the <c>new</c> keyword token from a creation expression, or <see langword="default"/>
    /// if the node is not a recognized creation expression.
    /// </summary>
    /// <param name="node">The potential creation expression node.</param>
    /// <returns>The <c>new</c> keyword token, or <see langword="default"/>.</returns>
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
    /// Recursively collects dot operator tokens from a chain expression.
    /// For conditional access, the <c>?</c> token from the <see cref="ConditionalAccessExpressionSyntax"/>
    /// is collected instead of the <c>.</c> from the <see cref="MemberBindingExpressionSyntax"/>,
    /// because the <c>?</c> is the first token on a continuation line.
    /// </summary>
    /// <param name="expr">The expression to walk.</param>
    /// <param name="dots">The list to accumulate dot tokens into.</param>
    private static void CollectChainDots(ExpressionSyntax expr, List<SyntaxToken> dots)
    {
        switch (expr)
        {
            case InvocationExpressionSyntax invocation:
                {
                    CollectChainDots(invocation.Expression, dots);
                }
                break;

            case MemberAccessExpressionSyntax memberAccess:
                {
                    CollectChainDots(memberAccess.Expression, dots);
                    dots.Add(memberAccess.OperatorToken);
                }
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    CollectChainDots(conditionalAccess.Expression, dots);
                    dots.Add(conditionalAccess.OperatorToken);
                    CollectChainDots(conditionalAccess.WhenNotNull, dots);
                }
                break;

            case PostfixUnaryExpressionSyntax postfixUnary:
                {
                    CollectChainDots(postfixUnary.Operand, dots);
                    dots.Add(postfixUnary.OperatorToken);
                }
                break;
        }
    }

    /// <summary>
    /// Determines whether a syntax node is nested inside a <see cref="ConditionalAccessExpressionSyntax"/>.
    /// Walks up the parent chain until a statement or member declaration is found.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns><see langword="true"/> if the node is inside a conditional access expression; otherwise, <see langword="false"/>.</returns>
    private static bool IsInsideConditionalAccess(SyntaxNode node)
    {
        var current = node.Parent;

        while (current != null)
        {
            if (current is ConditionalAccessExpressionSyntax)
            {
                return true;
            }

            if (current is StatementSyntax || current is MemberDeclarationSyntax)
            {
                return false;
            }

            current = current.Parent;
        }

        return false;
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        var dots = CreateDotsForNode(node);

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
    /// Creates chain dot tokens for supported node types.
    /// </summary>
    /// <param name="node">The syntax node to inspect.</param>
    /// <returns>The collected dot tokens; empty when the node is not handled.</returns>
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

                    CollectChainDots(conditionalAccess, dots);

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

                    CollectChainDots(chainRoot, dots);

                    return dots;
                }

            default:
                {
                    return [];
                }
        }
    }

    /// <summary>
    /// Determines whether an invocation should be skipped for chain alignment.
    /// </summary>
    /// <param name="invocation">The invocation node to evaluate.</param>
    /// <returns><see langword="true"/> if the invocation should be skipped; otherwise, <see langword="false"/>.</returns>
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

        return IsInsideConditionalAccess(invocation);
    }

    /// <summary>
    /// Gets the outer chain root for an invocation, including trailing member-access properties.
    /// </summary>
    /// <param name="invocation">The invocation expression.</param>
    /// <returns>The chain root expression.</returns>
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