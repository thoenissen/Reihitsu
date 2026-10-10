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
    /// Gets the outermost node of the chain the given member or conditional access belongs to
    /// </summary>
    /// <param name="node">A member access or conditional access expression of the chain</param>
    /// <returns>The outermost member access or conditional access expression of the chain</returns>
    internal static SyntaxNode GetOutermostChainNode(SyntaxNode node)
    {
        var current = node;

        while (IsInnerChainMember(current))
        {
            var parent = current.Parent;

            while (parent is InvocationExpressionSyntax or ElementAccessExpressionSyntax or PostfixUnaryExpressionSyntax)
            {
                parent = parent.Parent;
            }

            current = parent;
        }

        return current;
    }

    /// <summary>
    /// Determines whether the token is a link of a fluent chain other than its first link. The first link is the one
    /// <see cref="CollectChainLinks"/> returns first; every invoked call, null-forgiving operator, or conditional
    /// access after it is a later link
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token is a later chain link</returns>
    internal static bool IsLaterChainLink(SyntaxToken token)
    {
        SyntaxNode owner = token.Parent switch
                           {
                               MemberAccessExpressionSyntax memberAccess when memberAccess.OperatorToken == token => memberAccess,
                               PostfixUnaryExpressionSyntax postfixUnary when postfixUnary.OperatorToken == token
                                                                              && postfixUnary.Parent is MemberAccessExpressionSyntax memberAccess
                                                                              && memberAccess.Expression == postfixUnary => memberAccess,
                               ConditionalAccessExpressionSyntax conditionalAccess when conditionalAccess.OperatorToken == token => conditionalAccess,
                               _ => null
                           };

        if (owner == null)
        {
            return false;
        }

        return CollectChainLinks(GetOutermostChainNode(owner)).IndexOf(token) >= 1;
    }

    /// <summary>
    /// Determines whether the operator of a chain link starts a line. The line break is looked for behind the token in
    /// front of the operator, so a link directly behind the closing delimiter of a multi-line raw string literal does
    /// not start a line
    /// </summary>
    /// <param name="link">The chain link</param>
    /// <returns><see langword="true"/> if the link's operator starts a line</returns>
    internal static bool StartsLine(FluentChainLink link)
    {
        return SyntaxTokenPositionUtilities.IsFirstOnLine(link.OperatorToken);
    }

    /// <summary>
    /// Determines whether the operator of a chain link holds a line break between its own tokens (<c>?</c> ⏎ <c>.</c>,
    /// <c>!</c> ⏎ <c>.</c>, <c>!</c> ⏎ <c>?.</c>)
    /// </summary>
    /// <param name="link">The chain link</param>
    /// <param name="isJoinableOnly">Whether a line break behind a comment, a preprocessor directive or disabled text is ignored</param>
    /// <returns><see langword="true"/> if the operator holds a line break</returns>
    internal static bool HasInnerLineBreak(FluentChainLink link, bool isJoinableOnly)
    {
        for (var tokenIndex = 1; tokenIndex < link.OperatorTokens.Count; tokenIndex++)
        {
            var previousToken = link.OperatorTokens[tokenIndex - 1];
            var token = link.OperatorTokens[tokenIndex];

            if (SyntaxTokenPositionUtilities.IsFirstOnLine(token)
                && (isJoinableOnly == false
                    || SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, token) == false))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a chain is wrapped: one of its link operators starts a line or holds a line break
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <returns><see langword="true"/> if the chain is wrapped</returns>
    internal static bool IsWrapped(FluentChain chain)
    {
        return chain.Links.Any(static link => StartsLine(link)
                                              || HasInnerLineBreak(link, false));
    }

    /// <summary>
    /// Determines whether the first link of a chain starts a line, so the chain's first member access is not on the
    /// line its root ends on
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <returns><see langword="true"/> if the first link is wrapped</returns>
    internal static bool IsFirstLinkWrapped(FluentChain chain)
    {
        return StartsLine(chain.FirstLink);
    }

    /// <summary>
    /// Determines whether the first link of a chain is kept on its own line by a comment, a preprocessor directive or
    /// disabled text in front of it
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <returns><see langword="true"/> if the first link cannot be joined onto the root</returns>
    internal static bool IsFirstLinkBlocked(FluentChain chain)
    {
        return SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(chain.RootLastToken, chain.FirstLink.OperatorToken);
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