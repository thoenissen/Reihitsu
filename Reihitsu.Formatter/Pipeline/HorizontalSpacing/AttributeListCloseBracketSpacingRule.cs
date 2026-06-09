using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Requires exactly one space after the closing bracket of an attribute list, for example
/// <c>[Obsolete] void M()</c>. This is the highest-precedence rule so an attribute close bracket
/// is always followed by a single space regardless of what the following token is
/// </summary>
internal sealed class AttributeListCloseBracketSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsAttributeListCloseBracket(left))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule

    #region Methods

    /// <summary>
    /// Determines whether the token is the closing bracket of an attribute list
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token closes an attribute list; otherwise, <see langword="false"/></returns>
    private static bool IsAttributeListCloseBracket(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.CloseBracketToken) && token.Parent is AttributeListSyntax;
    }

    #endregion // Methods
}