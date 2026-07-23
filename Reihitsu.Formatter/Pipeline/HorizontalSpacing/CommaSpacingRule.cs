using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Requires exactly one space after a comma, except inside a rank-only array specifier such as
/// <c>int[,]</c> or an unbound generic type such as <c>Dictionary&lt;,&gt;</c> where the commas stay compact
/// </summary>
internal sealed class CommaSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines the spacing after a comma token
    /// </summary>
    /// <param name="current">The comma token</param>
    /// <returns>The required number of spaces after the comma</returns>
    private static int GetSpacesAfterComma(SyntaxToken current)
    {
        return CommaSpacingUtilities.IsSpacingExempt(current) ? 0 : 1;
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (left.IsKind(SyntaxKind.CommaToken))
        {
            return GetSpacesAfterComma(left);
        }

        return null;
    }

    #endregion // ISpacingRule
}