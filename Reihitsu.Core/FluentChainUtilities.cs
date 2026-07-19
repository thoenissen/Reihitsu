using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for fluent-chain operator selection
/// </summary>
public static class FluentChainUtilities
{
    #region Methods

    /// <summary>
    /// Gets the operator token that represents an invoked member-access link. A null-forgiving operator
    /// and the following member-access dot form one link when they share a line, so the <c>!</c> is used
    /// as the alignment token. When the dot starts a later line, the dot represents that continuation
    /// and must be aligned independently
    /// </summary>
    /// <param name="memberAccess">The invoked member access</param>
    /// <returns>The operator token that represents the invoked link</returns>
    public static SyntaxToken GetInvokedLinkOperator(MemberAccessExpressionSyntax memberAccess)
    {
        if (memberAccess.Expression is not PostfixUnaryExpressionSyntax postfixUnary)
        {
            return memberAccess.OperatorToken;
        }

        return AreOnSameLine(postfixUnary.OperatorToken, memberAccess.OperatorToken)
                   ? postfixUnary.OperatorToken
                   : memberAccess.OperatorToken;
    }

    /// <summary>
    /// Determines whether two tokens start on the same physical source line
    /// </summary>
    /// <param name="firstToken">The first token</param>
    /// <param name="secondToken">The second token</param>
    /// <returns><see langword="true"/> if both tokens start on the same line; otherwise, <see langword="false"/></returns>
    private static bool AreOnSameLine(SyntaxToken firstToken, SyntaxToken secondToken)
    {
        if (firstToken.SyntaxTree != null && firstToken.SyntaxTree == secondToken.SyntaxTree)
        {
            return firstToken.GetLocation().GetLineSpan().StartLinePosition.Line
                   == secondToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        }

        return firstToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) == false
               && secondToken.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) == false;
    }

    #endregion // Methods
}