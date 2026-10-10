using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Requires exactly one space after a comma, except inside a rank-only array specifier such as
/// <c>int[,]</c>, an unbound generic type such as <c>Dictionary&lt;,&gt;</c>, or an interpolated-string
/// alignment component such as <c>$"{value,-10}"</c>, where the commas stay compact
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
        return CommaSpacingUtilities.GetDesiredSpacesAfter(current);
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