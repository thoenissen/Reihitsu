using Microsoft.CodeAnalysis;
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
    /// and the following member-access dot form one <c>!.</c> link even when trivia separates them, so the
    /// <c>!</c> is used as the alignment token and the formatter can keep both tokens together
    /// </summary>
    /// <param name="memberAccess">The invoked member access</param>
    /// <returns>The operator token that represents the invoked link</returns>
    public static SyntaxToken GetInvokedLinkOperator(MemberAccessExpressionSyntax memberAccess)
    {
        if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
        {
            return postfixUnary.OperatorToken;
        }

        return memberAccess.OperatorToken;
    }

    #endregion // Methods
}