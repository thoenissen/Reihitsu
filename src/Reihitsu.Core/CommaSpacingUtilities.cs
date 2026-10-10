using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared comma-spacing policy used by the analyzer and formatter
/// </summary>
public static class CommaSpacingUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a comma belongs to syntax that must remain compact
    /// </summary>
    /// <param name="commaToken">Comma token to inspect</param>
    /// <returns><see langword="true"/> if the comma is exempt from normal value-separator spacing; otherwise, <see langword="false"/></returns>
    public static bool IsSpacingExempt(SyntaxToken commaToken)
    {
        return (commaToken.Parent is ArrayRankSpecifierSyntax arrayRankSpecifier
                && IsRankOnlyArraySpecifier(arrayRankSpecifier))
               || (commaToken.Parent is TypeArgumentListSyntax typeArgumentList
                   && IsUnboundGenericType(typeArgumentList));
    }

    /// <summary>
    /// Determines the number of spaces required after a comma. Unlike <see cref="IsSpacingExempt"/>, this
    /// covers commas that stay subject to spacing analysis but whose required spacing after the comma is
    /// zero rather than one, such as the alignment comma of an interpolated-string alignment component
    /// </summary>
    /// <param name="commaToken">Comma token to inspect</param>
    /// <returns>The number of spaces required after the comma</returns>
    public static int GetDesiredSpacesAfter(SyntaxToken commaToken)
    {
        if (IsSpacingExempt(commaToken))
        {
            return 0;
        }

        return commaToken.Parent is InterpolationAlignmentClauseSyntax ? 0 : 1;
    }

    /// <summary>
    /// Determines whether an array rank specifier declares only rank without size expressions
    /// </summary>
    /// <param name="arrayRankSpecifier">Array rank specifier to inspect</param>
    /// <returns><see langword="true"/> if every size is omitted; otherwise, <see langword="false"/></returns>
    private static bool IsRankOnlyArraySpecifier(ArrayRankSpecifierSyntax arrayRankSpecifier)
    {
        return arrayRankSpecifier.Sizes.All(static size => size.IsKind(SyntaxKind.OmittedArraySizeExpression));
    }

    /// <summary>
    /// Determines whether a type argument list belongs to an unbound generic type
    /// </summary>
    /// <param name="typeArgumentList">Type argument list to inspect</param>
    /// <returns><see langword="true"/> if any type argument is omitted; otherwise, <see langword="false"/></returns>
    private static bool IsUnboundGenericType(TypeArgumentListSyntax typeArgumentList)
    {
        return typeArgumentList.Arguments.Any(static argument => argument.IsKind(SyntaxKind.OmittedTypeArgument));
    }

    #endregion // Methods
}