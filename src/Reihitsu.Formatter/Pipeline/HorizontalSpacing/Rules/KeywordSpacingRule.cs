using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Handles spacing after keywords that should be followed by a single space before their parenthesis
/// or expression (such as <c>if</c>, <c>for</c>, <c>return</c>, <c>new</c>). It also covers the
/// exceptions where these keywords stay compact: the <c>return</c> and <c>throw</c> keywords sit
/// directly before a semicolon, and target-typed <c>new()</c>, constructor constraint <c>new()</c>,
/// and <c>new[]</c> keep their delimiters adjacent. A keyword used as an attribute target is spaced by
/// <see cref="AttributeTargetColonSpacingRule"/>, which decides first. It returns <see langword="null"/>
/// for any other token, leaving the pair to the rules after it
/// </summary>
internal sealed class KeywordSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines spacing for keyword tokens
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token</param>
    /// <returns>The required space count, or <see langword="null"/> if no keyword-specific rule applies</returns>
    private static int? GetKeywordSpacing(SyntaxToken current, SyntaxToken next)
    {
        if (IsSpacedKeyword(current) == false)
        {
            // No specific rule — return null so only collapse of multiple spaces is applied.
            return null;
        }

        // Special case: return and throw statements have no space before the semicolon.
        if ((current.IsKind(SyntaxKind.ReturnKeyword) || current.IsKind(SyntaxKind.ThrowKeyword))
            && next.IsKind(SyntaxKind.SemicolonToken))
        {
            return 0;
        }

        if (current.IsKind(SyntaxKind.NewKeyword))
        {
            return GetNewKeywordSpacing(current, next);
        }

        return 1;
    }

    /// <summary>
    /// Determines spacing after the <c>new</c> keyword
    /// </summary>
    /// <param name="current">The <c>new</c> keyword token</param>
    /// <param name="next">The next token</param>
    /// <returns>The required number of spaces</returns>
    private static int GetNewKeywordSpacing(SyntaxToken current, SyntaxToken next)
    {
        // target-typed new() and constructor constraint new() keep parentheses adjacent.
        if (next.IsKind(SyntaxKind.OpenParenToken)
            && (current.Parent is ImplicitObjectCreationExpressionSyntax
                || current.Parent is ConstructorConstraintSyntax))
        {
            return 0;
        }

        // new[] keeps brackets adjacent.
        if (next.IsKind(SyntaxKind.OpenBracketToken)
            && current.Parent is ImplicitArrayCreationExpressionSyntax)
        {
            return 0;
        }

        return 1;
    }

    /// <summary>
    /// Determines whether the token is a keyword that should be followed by exactly one space
    /// before its parenthesis or expression
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns><see langword="true"/> if the token is a spaced keyword; otherwise, <see langword="false"/></returns>
    private static bool IsSpacedKeyword(SyntaxToken token)
    {
        switch (token.Kind())
        {
            case SyntaxKind.IfKeyword:
            case SyntaxKind.ForKeyword:
            case SyntaxKind.ForEachKeyword:
            case SyntaxKind.WhileKeyword:
            case SyntaxKind.SwitchKeyword:
            case SyntaxKind.CatchKeyword:
            case SyntaxKind.UsingKeyword:
            case SyntaxKind.LockKeyword:
            case SyntaxKind.FixedKeyword:
            case SyntaxKind.ReturnKeyword:
            case SyntaxKind.ThrowKeyword:
            case SyntaxKind.NewKeyword:
            case SyntaxKind.OperatorKeyword:
                return true;

            default:
                return false;
        }
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        return GetKeywordSpacing(left, right);
    }

    #endregion // ISpacingRule
}