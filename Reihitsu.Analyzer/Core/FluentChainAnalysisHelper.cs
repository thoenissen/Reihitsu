using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        var current = node;
        var isInvoked = node.Parent is InvocationExpressionSyntax;

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
    /// Determines whether the token has a comment directly above its line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><c>true</c> if a comment is directly above the token; otherwise <c>false</c></returns>
    internal static bool HasCommentDirectlyAbove(SyntaxToken token)
    {
        if (token.LeadingTrivia.Any(IsCommentTrivia) == false)
        {
            return false;
        }

        if (token.SyntaxTree == null)
        {
            return true;
        }

        var line = GetLine(token);

        if (line <= 0)
        {
            return false;
        }

        var previousLine = token.SyntaxTree.GetText().Lines[line - 1].ToString().Trim();

        return previousLine.StartsWith("//", StringComparison.Ordinal)
               || previousLine.StartsWith("/*", StringComparison.Ordinal)
               || previousLine.StartsWith("*", StringComparison.Ordinal)
               || previousLine.EndsWith("*/", StringComparison.Ordinal);
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

        if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
        {
            links.Add(postfixUnary.OperatorToken);

            return postfixUnary.Operand;
        }

        links.Add(memberAccess.OperatorToken);

        return memberAccess.Expression;
    }

    /// <summary>
    /// Determines whether a trivia is a comment
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <returns><c>true</c> if the trivia is a comment; otherwise <c>false</c></returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    #endregion // Methods
}