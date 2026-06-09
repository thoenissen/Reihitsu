using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Requires a single space before braces (outside string interpolation) and around the colon of a
/// base list or constructor initializer, for example <c>class C : Base</c> and <c>: this()</c>
/// </summary>
internal sealed class SingleSpaceSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (RequiresSingleSpace(left, right))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule

    #region Methods

    /// <summary>
    /// Determines whether spacing must be normalized to a single space between the two tokens
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token</param>
    /// <returns><see langword="true"/> if the tokens must be separated by a single space; otherwise, <see langword="false"/></returns>
    private static bool RequiresSingleSpace(SyntaxToken current, SyntaxToken next)
    {
        return RequiresSpaceBeforeBrace(next)
               || RequiresSpaceBeforeColon(current, next);
    }

    /// <summary>
    /// Determines whether a single space is required before the specified brace token
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a single space is required before the brace token; otherwise, <see langword="false"/></returns>
    private static bool RequiresSpaceBeforeBrace(SyntaxToken token)
    {
        if ((token.IsKind(SyntaxKind.OpenBraceToken) || token.IsKind(SyntaxKind.CloseBraceToken)) == false)
        {
            return false;
        }

        return token.Parent is not InterpolationSyntax;
    }

    /// <summary>
    /// Determines whether a single space is required around the specified colon token
    /// </summary>
    /// <param name="current">The current token</param>
    /// <param name="next">The next token</param>
    /// <returns><see langword="true"/> if the colon should be surrounded by single spaces; otherwise, <see langword="false"/></returns>
    private static bool RequiresSpaceBeforeColon(SyntaxToken current, SyntaxToken next)
    {
        return IsSpacedColonToken(current) || IsSpacedColonToken(next);
    }

    /// <summary>
    /// Determines whether the token is a colon that should be surrounded by spaces
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a spaced colon; otherwise, <see langword="false"/></returns>
    private static bool IsSpacedColonToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.ColonToken)
               && (token.Parent is BaseListSyntax || token.Parent is ConstructorInitializerSyntax);
    }

    #endregion // Methods
}