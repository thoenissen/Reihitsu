using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space on each side of the <c>with</c> of a <c>with</c> expression (FMT-SPACE-025), and no space between the
/// <c>with</c> of a collection-expression element and its argument list (FMT-SPACE-028)
/// </summary>
internal sealed class WithSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether the token is the <c>with</c> of a <c>with</c> expression
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is such a keyword; otherwise, <see langword="false"/></returns>
    private static bool IsWithKeyword(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.WithKeyword)
               && token.Parent is WithExpressionSyntax;
    }

    /// <summary>
    /// Determines whether the token is the <c>with</c> of a <c>with(…)</c> element of a collection expression, which its
    /// argument list follows directly
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is such a keyword; otherwise, <see langword="false"/></returns>
    private static bool IsWithElementKeyword(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.WithKeyword)
               && token.Parent is WithElementSyntax;
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsWithKeyword(left) || IsWithKeyword(right))
        {
            return 1;
        }

        if (IsWithElementKeyword(left))
        {
            return 0;
        }

        return null;
    }

    #endregion // ISpacingRule
}