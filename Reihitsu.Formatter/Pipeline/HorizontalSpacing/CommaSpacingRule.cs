using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        if (current.Parent is ArrayRankSpecifierSyntax arrayRankSpecifier
            && IsRankOnlyArraySpecifier(arrayRankSpecifier))
        {
            return 0;
        }

        if (current.Parent is TypeArgumentListSyntax typeArgumentList
            && IsUnboundGenericType(typeArgumentList))
        {
            return 0;
        }

        return 1;
    }

    /// <summary>
    /// Determines whether a type argument list belongs to an unbound generic type such as <c>Dictionary&lt;,&gt;</c>
    /// </summary>
    /// <param name="typeArgumentList">The type argument list to inspect</param>
    /// <returns><see langword="true"/> if any type argument is omitted; otherwise, <see langword="false"/></returns>
    private static bool IsUnboundGenericType(TypeArgumentListSyntax typeArgumentList)
    {
        return typeArgumentList.Arguments.Any(static argument => argument.IsKind(SyntaxKind.OmittedTypeArgument));
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