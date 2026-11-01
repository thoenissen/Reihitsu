using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space on each side of every <c>=&gt;</c>: the arrow of a lambda, of an expression body of a member, accessor,
/// or local function, and of a switch-expression arm (FMT-SPACE-017)
/// </summary>
internal sealed class ArrowSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (left.IsKind(SyntaxKind.EqualsGreaterThanToken)
            || right.IsKind(SyntaxKind.EqualsGreaterThanToken))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}