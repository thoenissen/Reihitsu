using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space in front of the <c>when</c> of a <c>case</c> guard, a switch-expression arm, and a <c>catch</c> filter
/// (FMT-SPACE-020). The space behind <c>when</c> is not decided by this rule
/// </summary>
internal sealed class WhenSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (right.IsKind(SyntaxKind.WhenKeyword)
            && right.Parent is WhenClauseSyntax or CatchFilterClauseSyntax)
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}