using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared fluent-chain analysis logic for formatting rules
/// </summary>
internal static class FluentChainAnalysisHelper
{
    #region Methods

    /// <summary>
    /// Determines whether the given node is an inner member of a larger chain
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns><c>true</c> if the node is part of a larger chain; otherwise <c>false</c></returns>
    internal static bool IsInnerChainMember(SyntaxNode node)
    {
        var current = node.Parent;

        while (current != null)
        {
            switch (current)
            {
                case InvocationExpressionSyntax:
                case ElementAccessExpressionSyntax:
                case PostfixUnaryExpressionSyntax:
                    {
                        current = current.Parent;

                        continue;
                    }
                case MemberAccessExpressionSyntax:
                case ConditionalAccessExpressionSyntax:
                    {
                        return true;
                    }
            }

            break;
        }

        return false;
    }

    /// <summary>
    /// Collects all chain link tokens from the outermost node down to the root expression
    /// </summary>
    /// <param name="node">The outermost node of the chain</param>
    /// <returns>List of chain link tokens in chain order (first link closest to root)</returns>
    internal static List<SyntaxToken> CollectChainLinks(SyntaxNode node)
    {
        var links = new List<SyntaxToken>();

        CollectChainLinksInReverseOrder(node, node.Parent is InvocationExpressionSyntax, links);
        links.Reverse();

        return links;
    }

    /// <summary>
    /// Gets the line number of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Line number</returns>
    internal static int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Determines whether the first invoked chain link is preceded by intermediate member access
    /// </summary>
    /// <param name="token">The chain link token</param>
    /// <returns><c>true</c> if intermediate member access precedes the invocation; otherwise <c>false</c></returns>
    internal static bool HasIntermediateMemberAccess(SyntaxToken token)
    {
        return token.Parent switch
               {
                   MemberAccessExpressionSyntax memberAccess => memberAccess.Expression is MemberAccessExpressionSyntax
                                                                                        or ConditionalAccessExpressionSyntax,
                   ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.Expression is MemberAccessExpressionSyntax
                                                                                                       or ConditionalAccessExpressionSyntax,
                   _ => false,
               };
    }

    /// <summary>
    /// Processes a member access expression within a chain, adding the appropriate token if invoked
    /// </summary>
    /// <param name="memberAccess">The member access expression</param>
    /// <param name="isInvoked">Whether the member access is invoked</param>
    /// <param name="links">The list of chain link tokens to add to</param>
    /// <returns>The next node to process in the chain</returns>
    private static ExpressionSyntax ProcessMemberAccess(MemberAccessExpressionSyntax memberAccess, bool isInvoked, List<SyntaxToken> links)
    {
        if (isInvoked == false)
        {
            return memberAccess.Expression;
        }

        links.Add(FluentChainUtilities.GetInvokedLinkOperator(memberAccess));

        if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
        {
            return postfixUnary.Operand;
        }

        return memberAccess.Expression;
    }

    /// <summary>
    /// Collects chain links from the outermost link toward the root expression
    /// </summary>
    /// <param name="node">The node to collect from</param>
    /// <param name="isInvoked">Whether the node is invoked</param>
    /// <param name="links">The list of chain link tokens to add to</param>
    private static void CollectChainLinksInReverseOrder(SyntaxNode node, bool isInvoked, List<SyntaxToken> links)
    {
        var current = node;

        while (current != null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                current = invocation.Expression;
                isInvoked = true;
            }
            else if (current is MemberAccessExpressionSyntax memberAccess)
            {
                current = ProcessMemberAccess(memberAccess, isInvoked, links);
                isInvoked = false;
            }
            else if (current is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                CollectChainLinksInReverseOrder(conditionalAccess.WhenNotNull, false, links);
                links.Add(conditionalAccess.OperatorToken);
                current = conditionalAccess.Expression;
                isInvoked = false;
            }
            else if (current is ElementAccessExpressionSyntax elementAccess)
            {
                current = elementAccess.Expression;
            }
            else if (current is PostfixUnaryExpressionSyntax postfix)
            {
                current = postfix.Operand;
            }
            else
            {
                break;
            }
        }
    }

    #endregion // Methods
}