using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Spaces the colon that ends an attribute target: no space in front of it and one space behind it, whatever the target
/// - a keyword such as <c>return</c> or <c>assembly</c>, or an identifier such as <c>field</c> (FMT-SPACE-016)
/// </summary>
internal sealed class AttributeTargetColonSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether the token is the colon of an attribute target specifier
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token ends an attribute target; otherwise, <see langword="false"/></returns>
    private static bool IsAttributeTargetColon(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.ColonToken)
               && token.Parent is AttributeTargetSpecifierSyntax;
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsAttributeTargetColon(right))
        {
            return 0;
        }

        if (IsAttributeTargetColon(left))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}