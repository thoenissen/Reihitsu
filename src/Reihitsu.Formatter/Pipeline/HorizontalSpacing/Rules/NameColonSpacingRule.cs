using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Spaces the colon of a name colon or an expression colon - a named argument, a named attribute argument, a named tuple
/// element, and the name of a property or positional subpattern: no space in front of it and one space behind it
/// (FMT-SPACE-019)
/// </summary>
internal sealed class NameColonSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether the token is the colon of a name colon or an expression colon
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is such a colon; otherwise, <see langword="false"/></returns>
    private static bool IsNameColon(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.ColonToken)
               && token.Parent is NameColonSyntax or ExpressionColonSyntax;
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsNameColon(right))
        {
            return 0;
        }

        if (IsNameColon(left))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}