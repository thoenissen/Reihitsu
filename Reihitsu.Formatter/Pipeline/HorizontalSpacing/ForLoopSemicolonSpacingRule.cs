using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Requires exactly one space after each semicolon in a for-loop header, for example
/// <c>for (var i = 0; i &lt; n; i++)</c>
/// </summary>
internal sealed class ForLoopSemicolonSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (left.IsKind(SyntaxKind.SemicolonToken) && left.Parent is ForStatementSyntax)
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}