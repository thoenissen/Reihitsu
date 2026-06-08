using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Walks method-chain and conditional-access expressions to collect their dot operator tokens.
/// The line-break side collects only the dots of invoked chain links, while the alignment side
/// collects every member-access, conditional-access, and postfix operator token
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
            case InvocationExpressionSyntax invocation when invocation.Expression is MemberAccessExpressionSyntax memberAccess:
                {
                    if (memberAccess.Expression is InvocationExpressionSyntax innerInvocation)
                    {
                        CollectInvokedLinkDots(innerInvocation, dots);
                    }
                    else if (memberAccess.Expression is ConditionalAccessExpressionSyntax innerConditional)
                    {
                        CollectInvokedLinkDots(innerConditional, dots);
                    }

                    dots.Add(memberAccess.OperatorToken);
                }
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    if (conditionalAccess.Expression is InvocationExpressionSyntax innerInvocation)
                    {
                        CollectInvokedLinkDots(innerInvocation, dots);
                    }
                    else if (conditionalAccess.Expression is ConditionalAccessExpressionSyntax innerConditional)
                    {
                        CollectInvokedLinkDots(innerConditional, dots);
                    }

                    dots.Add(conditionalAccess.OperatorToken);
                    CollectWhenNotNullLinkDots(conditionalAccess.WhenNotNull, dots);
                }
                break;
        }
    }

    /// <summary>
    /// Collects invoked chain-link dot tokens from the <c>WhenNotNull</c> part of a conditional access expression
    /// </summary>
    /// <param name="node">The WhenNotNull expression to walk</param>
    /// <param name="dots">The list to accumulate dot tokens into</param>
    private static void CollectWhenNotNullLinkDots(SyntaxNode node,
                                                   List<SyntaxToken> dots)
    {
        if (node is InvocationExpressionSyntax invocation
            && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            CollectWhenNotNullLinkDots(memberAccess.Expression, dots);
            dots.Add(memberAccess.OperatorToken);
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
                    CollectAlignmentDots(invocation.Expression, dots);
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

    #endregion // Methods
}