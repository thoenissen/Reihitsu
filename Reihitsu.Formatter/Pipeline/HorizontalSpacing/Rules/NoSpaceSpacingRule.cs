using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Removes spacing between tokens that must stay adjacent, such as inside parentheses and brackets,
/// before commas and semicolons, around generic angle brackets, member-access symbols (the dot, the
/// conditional-access question mark and the pointer arrow), nullable question marks, and tight unary
/// operators
/// </summary>
internal sealed class NoSpaceSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether spacing must be removed between the two tokens
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token</param>
    /// <returns><see langword="true"/> if no separating space is allowed; otherwise, <see langword="false"/></returns>
    private static bool HasNoSpaceAfter(SyntaxToken current, SyntaxToken next)
    {
        if (UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(current, next))
        {
            return false;
        }

        return current.IsKind(SyntaxKind.OpenParenToken)
               || current.IsKind(SyntaxKind.OpenBracketToken)
               || next.IsKind(SyntaxKind.CloseParenToken)
               || next.IsKind(SyntaxKind.CloseBracketToken)
               || next.IsKind(SyntaxKind.CommaToken)
               || next.IsKind(SyntaxKind.SemicolonToken)
               || IsGenericOpeningAngleBracket(current)
               || IsGenericOpeningAngleBracket(next)
               || IsGenericClosingAngleBracket(next)
               || IsNullableTypeQuestionToken(next)
               || IsMemberAccessToken(current)
               || IsMemberAccessToken(next)
               || IsUnarySignToken(current)
               || IsPrefixTightUnaryOperatorToken(current)
               || IsPostfixTightUnaryOperatorToken(next);
    }

    /// <summary>
    /// Determines whether the token is a generic opening angle bracket
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token opens a generic argument or parameter list; otherwise, <see langword="false"/></returns>
    private static bool IsGenericOpeningAngleBracket(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.LessThanToken)
               && (token.Parent is TypeArgumentListSyntax || token.Parent is TypeParameterListSyntax);
    }

    /// <summary>
    /// Determines whether the token is a generic closing angle bracket
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token closes a generic argument or parameter list; otherwise, <see langword="false"/></returns>
    private static bool IsGenericClosingAngleBracket(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.GreaterThanToken)
               && (token.Parent is TypeArgumentListSyntax || token.Parent is TypeParameterListSyntax);
    }

    /// <summary>
    /// Determines whether the token is the nullable type question mark
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is part of a nullable type; otherwise, <see langword="false"/></returns>
    private static bool IsNullableTypeQuestionToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.QuestionToken) && token.Parent is NullableTypeSyntax;
    }

    /// <summary>
    /// Determines whether the token is a member-access symbol: the dot of a plain, bound or qualified
    /// access, the question mark that opens a conditional access, or the arrow of a pointer access.
    /// The chain rewriter already closes the gap that follows a conditional access or an arrow, so
    /// leaving them out here tightened one side only and produced <c>x ?.Foo</c> and <c>p -&gt;V</c>
    /// (issue #434)
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a member-access symbol; otherwise, <see langword="false"/></returns>
    private static bool IsMemberAccessToken(SyntaxToken token)
    {
        return IsMemberAccessDotToken(token)
               || IsConditionalAccessToken(token)
               || IsPointerMemberAccessToken(token);
    }

    /// <summary>
    /// Determines whether the token is a member-access dot token
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a member-access dot; otherwise, <see langword="false"/></returns>
    private static bool IsMemberAccessDotToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.DotToken)
               && (token.Parent is MemberAccessExpressionSyntax
                   || token.Parent is MemberBindingExpressionSyntax
                   || token.Parent is QualifiedNameSyntax
                   || token.Parent is AliasQualifiedNameSyntax);
    }

    /// <summary>
    /// Determines whether the token is the question mark that opens a conditional access. The parent
    /// check is what separates it from the question mark of a conditional expression and of a nullable
    /// type, both of which keep their spacing
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token opens a conditional access; otherwise, <see langword="false"/></returns>
    private static bool IsConditionalAccessToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.QuestionToken)
               && token.Parent is ConditionalAccessExpressionSyntax;
    }

    /// <summary>
    /// Determines whether the token is the arrow of a pointer member access
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a pointer member access arrow; otherwise, <see langword="false"/></returns>
    private static bool IsPointerMemberAccessToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.MinusGreaterThanToken)
               && token.Parent is MemberAccessExpressionSyntax;
    }

    /// <summary>
    /// Determines whether the token is a unary plus or minus sign
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a unary sign; otherwise, <see langword="false"/></returns>
    private static bool IsUnarySignToken(SyntaxToken token)
    {
        return (token.IsKind(SyntaxKind.PlusToken) || token.IsKind(SyntaxKind.MinusToken))
               && token.Parent is PrefixUnaryExpressionSyntax;
    }

    /// <summary>
    /// Determines whether the token is a prefix unary operator that must stay adjacent to its operand
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token belongs to a prefix tight unary operator; otherwise, <see langword="false"/></returns>
    private static bool IsPrefixTightUnaryOperatorToken(SyntaxToken token)
    {
        return token.Parent is PrefixUnaryExpressionSyntax;
    }

    /// <summary>
    /// Determines whether the token is a postfix unary operator that must stay adjacent to its operand
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token belongs to a postfix tight unary operator; otherwise, <see langword="false"/></returns>
    private static bool IsPostfixTightUnaryOperatorToken(SyntaxToken token)
    {
        return token.Parent is PostfixUnaryExpressionSyntax;
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (HasNoSpaceAfter(left, right))
        {
            return 0;
        }

        return null;
    }

    #endregion // ISpacingRule
}