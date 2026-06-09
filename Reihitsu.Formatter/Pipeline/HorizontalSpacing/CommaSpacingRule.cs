using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Requires exactly one space after a comma, except inside a rank-only array specifier such as
/// <c>int[,]</c> where the commas stay compact
/// </summary>
internal sealed class CommaSpacingRule : ISpacingRule
{
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

    #region Methods

    /// <summary>
    /// Determines the spacing after a comma token
    /// </summary>
    /// <param name="current">The comma token</param>
    /// <returns>The required number of spaces after the comma</returns>
    private static int GetSpacesAfterComma(SyntaxToken current)
    {
        if (current.Parent is ArrayRankSpecifierSyntax arrayRankSpecifier
            && IsRankOnlyArraySpecifier(arrayRankSpecifier))
        {
            return 0;
        }

        return 1;
    }

    /// <summary>
    /// Determines whether an array rank specifier only declares the rank and does not contain size expressions
    /// </summary>
    /// <param name="arrayRankSpecifier">The array rank specifier to inspect</param>
    /// <returns><see langword="true"/> if all entries are omitted size expressions; otherwise, <see langword="false"/></returns>
    private static bool IsRankOnlyArraySpecifier(ArrayRankSpecifierSyntax arrayRankSpecifier)
    {
        return arrayRankSpecifier.Sizes.All(static size => size.IsKind(SyntaxKind.OmittedArraySizeExpression));
    }

    #endregion // Methods
}