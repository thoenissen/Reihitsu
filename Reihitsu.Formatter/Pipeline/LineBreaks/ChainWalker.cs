using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Centralizes traversal of method-chain and conditional-access expressions. It collects chain
/// tokens (invoked-link dots for the line-break side, every member-access/conditional-access/postfix
/// operator for the alignment side, and the full spine for chain rejoining) and answers chain-shape
/// queries such as outermost-node detection, spine invocation count, multi-line argument detection,
/// and intermediate member access
/// </summary>
internal static class ChainWalker
{
    #region Methods

    /// <summary>
    /// Collects the dot tokens of invoked chain links from a method chain or conditional access chain.
    /// Only invoked member accesses count as chain links.
    /// For conditional access, the <c>?</c> operator token is collected (not the binding dot)
    /// </summary>
    /// <param name="node">The chain node to walk</param>
    /// <param name="dots">The list to accumulate dot tokens into</param>
    public static void CollectInvokedLinkDots(SyntaxNode node,
                                              List<SyntaxToken> dots)
    {
        switch (node)
        {
            case InvocationExpressionSyntax invocation:
                {
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
                        {
                            CollectInvokedLinkDots(postfixUnary.Operand, dots);
                        }
                        else
                        {
                            CollectInvokedLinkDots(memberAccess.Expression, dots);
                        }

                        dots.Add(FluentChainUtilities.GetInvokedLinkOperator(memberAccess));
                    }
                }
                break;

            case MemberAccessExpressionSyntax memberAccess:
                {
                    CollectInvokedLinkDots(memberAccess.Expression, dots);
                }
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    CollectInvokedLinkDots(conditionalAccess.Expression, dots);

                    dots.Add(conditionalAccess.OperatorToken);
                    CollectInvokedLinkDots(conditionalAccess.WhenNotNull, dots);
                }
                break;

            case ElementAccessExpressionSyntax elementAccess:
                {
                    CollectInvokedLinkDots(elementAccess.Expression, dots);
                }
                break;

            case PostfixUnaryExpressionSyntax postfixUnary:
                {
                    CollectInvokedLinkDots(postfixUnary.Operand, dots);
                }
                break;
        }
    }

    /// <summary>
    /// Recursively collects all dot operator tokens from a chain expression for alignment.
    /// For conditional access, the <c>?</c> token from the <see cref="ConditionalAccessExpressionSyntax"/>
    /// is collected instead of the <c>.</c> from the <see cref="MemberBindingExpressionSyntax"/>,
    /// because the <c>?</c> is the first token on a continuation line
    /// </summary>
    /// <param name="expr">The expression to walk</param>
    /// <param name="dots">The list to accumulate dot tokens into</param>
    public static void CollectAlignmentDots(ExpressionSyntax expr,
                                            List<SyntaxToken> dots)
    {
        switch (expr)
        {
            case InvocationExpressionSyntax invocation:
                {
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                        && memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
                    {
                        CollectAlignmentDots(postfixUnary.Operand, dots);
                        dots.Add(FluentChainUtilities.GetInvokedLinkOperator(memberAccess));
                    }
                    else
                    {
                        CollectAlignmentDots(invocation.Expression, dots);
                    }
                }
                break;

            case MemberAccessExpressionSyntax memberAccess:
                {
                    CollectAlignmentDots(memberAccess.Expression, dots);
                    dots.Add(memberAccess.OperatorToken);
                }
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    CollectAlignmentDots(conditionalAccess.Expression, dots);
                    dots.Add(conditionalAccess.OperatorToken);
                    CollectAlignmentDots(conditionalAccess.WhenNotNull, dots);
                }
                break;

            case PostfixUnaryExpressionSyntax postfixUnary:
                {
                    CollectAlignmentDots(postfixUnary.Operand, dots);
                    dots.Add(postfixUnary.OperatorToken);
                }
                break;
        }
    }

    /// <summary>
    /// Determines whether an invocation expression is the outermost node in a method chain.
    /// An invocation is outermost if it is not an inner link of a larger chain
    /// and not nested inside a conditional access expression
    /// </summary>
    /// <param name="node">The invocation expression to check</param>
    /// <returns><see langword="true"/> if the invocation is the outermost chain node; otherwise, <see langword="false"/></returns>
    public static bool IsOutermostChainInvocation(InvocationExpressionSyntax node)
    {
        if (node.Expression is not MemberAccessExpressionSyntax
            && node.Expression is not MemberBindingExpressionSyntax)
        {
            return false;
        }

        if (node.Parent is MemberAccessExpressionSyntax parentAccess
            && parentAccess.Parent is InvocationExpressionSyntax)
        {
            return false;
        }

        return IsInsideConditionalAccess(node) == false;
    }

    /// <summary>
    /// Determines whether an access chain node is the outermost node of its chain, i.e. its parent
    /// is not another access/invocation node that would treat it as an inner link
    /// </summary>
    /// <param name="node">The chain node to check</param>
    /// <returns><see langword="true"/> if the node is the outermost chain node; otherwise, <see langword="false"/></returns>
    public static bool IsOutermostChainNode(SyntaxNode node)
    {
        return node.Parent is not MemberAccessExpressionSyntax
               && node.Parent is not MemberBindingExpressionSyntax
               && node.Parent is not InvocationExpressionSyntax
               && node.Parent is not ConditionalAccessExpressionSyntax
               && node.Parent is not ElementAccessExpressionSyntax
               && node.Parent is not PostfixUnaryExpressionSyntax;
    }

    /// <summary>
    /// Determines whether a syntax node is nested inside a <see cref="ConditionalAccessExpressionSyntax"/>
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns><see langword="true"/> if the node is inside a conditional access expression; otherwise, <see langword="false"/></returns>
    public static bool IsInsideConditionalAccess(SyntaxNode node)
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

    /// <summary>
    /// Determines whether an expression contains an invocation expression
    /// </summary>
    /// <param name="expression">The expression to inspect</param>
    /// <returns><see langword="true"/> if the expression contains an invocation; otherwise, <see langword="false"/></returns>
    public static bool ContainsInvocation(ExpressionSyntax expression)
    {
        if (expression is InvocationExpressionSyntax)
        {
            return true;
        }

        foreach (var child in expression.ChildNodes())
        {
            if (child is ExpressionSyntax childExpression && ContainsInvocation(childExpression))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a single chain dot token has an intermediate member access between
    /// the dot and the chain root
    /// </summary>
    /// <param name="dotToken">The dot token from a member access expression</param>
    /// <returns><see langword="true"/> if there is an intermediate member access; otherwise, <see langword="false"/></returns>
    public static bool DotHasIntermediateMemberAccess(SyntaxToken dotToken)
    {
        if (dotToken.Parent is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Expression is MemberAccessExpressionSyntax
                   || memberAccess.Expression is ConditionalAccessExpressionSyntax;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the chain contains a member access whose own expression is another member
    /// or conditional access. Such fluent chains (for example <c>x.Prop.Select(...)</c>) are kept
    /// wrapped and aligned rather than rejoined
    /// </summary>
    /// <param name="expression">The chain expression to inspect</param>
    /// <returns><see langword="true"/> if the chain has an intermediate member access; otherwise, <see langword="false"/></returns>
    public static bool ChainHasIntermediateMemberAccess(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                return memberAccess.Expression is MemberAccessExpressionSyntax
                       || memberAccess.Expression is ConditionalAccessExpressionSyntax
                       || ChainHasIntermediateMemberAccess(memberAccess.Expression);

            case ConditionalAccessExpressionSyntax conditionalAccess:
                return ChainHasIntermediateMemberAccess(conditionalAccess.Expression)
                       || ChainHasIntermediateMemberAccess(conditionalAccess.WhenNotNull);

            case InvocationExpressionSyntax invocation:
                return ChainHasIntermediateMemberAccess(invocation.Expression);

            case ElementAccessExpressionSyntax elementAccess:
                return ChainHasIntermediateMemberAccess(elementAccess.Expression);

            case PostfixUnaryExpressionSyntax postfixUnary:
                return ChainHasIntermediateMemberAccess(postfixUnary.Operand);

            default:
                return false;
        }
    }

    /// <summary>
    /// Counts the number of invocations along the spine of an access chain, ignoring invocations
    /// that appear inside argument lists. A chain with at most one invocation is short enough to be
    /// rejoined onto a single line rather than kept wrapped
    /// </summary>
    /// <param name="expression">The chain expression to inspect</param>
    /// <returns>The number of invocations on the chain spine</returns>
    public static int CountSpineInvocations(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case InvocationExpressionSyntax invocation:
                return 1 + CountSpineInvocations(invocation.Expression);

            case MemberAccessExpressionSyntax memberAccess:
                return CountSpineInvocations(memberAccess.Expression);

            case ConditionalAccessExpressionSyntax conditionalAccess:
                return CountSpineInvocations(conditionalAccess.Expression) + CountSpineInvocations(conditionalAccess.WhenNotNull);

            case ElementAccessExpressionSyntax elementAccess:
                return CountSpineInvocations(elementAccess.Expression);

            case PostfixUnaryExpressionSyntax postfixUnary:
                return CountSpineInvocations(postfixUnary.Operand);

            default:
                return 0;
        }
    }

    /// <summary>
    /// Determines whether any invocation on the chain spine has a multi-line argument list. Such a
    /// chain is intentionally wrapped and must keep its alignment rather than being rejoined
    /// </summary>
    /// <param name="expression">The chain expression to inspect</param>
    /// <returns><see langword="true"/> if a spine invocation wraps its arguments; otherwise, <see langword="false"/></returns>
    public static bool HasMultiLineArgumentList(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case InvocationExpressionSyntax invocation:
                return LineBreakDetection.IsMultiLine(invocation.ArgumentList)
                       || HasMultiLineArgumentList(invocation.Expression);

            case MemberAccessExpressionSyntax memberAccess:
                return HasMultiLineArgumentList(memberAccess.Expression);

            case ConditionalAccessExpressionSyntax conditionalAccess:
                return HasMultiLineArgumentList(conditionalAccess.Expression)
                       || HasMultiLineArgumentList(conditionalAccess.WhenNotNull);

            case ElementAccessExpressionSyntax elementAccess:
                return HasMultiLineArgumentList(elementAccess.Expression);

            case PostfixUnaryExpressionSyntax postfixUnary:
                return HasMultiLineArgumentList(postfixUnary.Operand);

            default:
                return false;
        }
    }

    /// <summary>
    /// Collects the spine tokens of an access chain, separating the operator tokens (dots, the
    /// conditional-access <c>?</c>, the null-forgiving <c>!</c>) from the member-name and root tokens.
    /// Tokens inside argument lists are intentionally left untouched
    /// </summary>
    /// <param name="expression">The chain expression to walk</param>
    /// <param name="operatorTokens">The list that receives operator tokens</param>
    /// <param name="otherTokens">The list that receives member-name and root tokens</param>
    public static void CollectSpineTokens(ExpressionSyntax expression,
                                          List<SyntaxToken> operatorTokens,
                                          List<SyntaxToken> otherTokens)
    {
        switch (expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                {
                    CollectSpineTokens(memberAccess.Expression, operatorTokens, otherTokens);
                    operatorTokens.Add(memberAccess.OperatorToken);
                    otherTokens.Add(memberAccess.Name.GetFirstToken());
                }
                break;

            case MemberBindingExpressionSyntax memberBinding:
                {
                    operatorTokens.Add(memberBinding.OperatorToken);
                    otherTokens.Add(memberBinding.Name.GetFirstToken());
                }
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    CollectSpineTokens(conditionalAccess.Expression, operatorTokens, otherTokens);
                    operatorTokens.Add(conditionalAccess.OperatorToken);
                    CollectSpineTokens(conditionalAccess.WhenNotNull, operatorTokens, otherTokens);
                }
                break;

            case InvocationExpressionSyntax invocation:
                {
                    CollectSpineTokens(invocation.Expression, operatorTokens, otherTokens);
                }
                break;

            case ElementAccessExpressionSyntax elementAccess:
                {
                    CollectSpineTokens(elementAccess.Expression, operatorTokens, otherTokens);
                }
                break;

            case PostfixUnaryExpressionSyntax postfixUnary:
                {
                    CollectSpineTokens(postfixUnary.Operand, operatorTokens, otherTokens);
                    operatorTokens.Add(postfixUnary.OperatorToken);
                }
                break;

            default:
                {
                    otherTokens.Add(expression.GetLastToken());
                }
                break;
        }
    }

    #endregion // Methods
}