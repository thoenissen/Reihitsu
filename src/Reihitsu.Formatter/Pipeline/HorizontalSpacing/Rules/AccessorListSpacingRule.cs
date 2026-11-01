using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space behind the <c>{</c> of a property, indexer, or event accessor list and between two accessors of such a
/// list on one line (FMT-SPACE-022). The space in front of the list's <c>}</c> is decided by
/// <see cref="SingleSpaceSpacingRule"/>
/// </summary>
internal sealed class AccessorListSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether the gap follows the opening brace of an accessor list or separates two of its accessors
    /// </summary>
    /// <param name="left">The token in front of the gap</param>
    /// <param name="right">The token behind the gap</param>
    /// <returns><see langword="true"/> if the gap is such a gap; otherwise, <see langword="false"/></returns>
    private static bool IsAccessorListGap(SyntaxToken left, SyntaxToken right)
    {
        if (left.IsKind(SyntaxKind.OpenBraceToken)
            && left.Parent is AccessorListSyntax)
        {
            return true;
        }

        return right.Parent?.FirstAncestorOrSelf<AccessorDeclarationSyntax>() is { Parent: AccessorListSyntax accessorList } accessor
               && right == accessor.GetFirstToken()
               && accessorList.Accessors.IndexOf(accessor) > 0
               && left == accessor.GetFirstToken().GetPreviousToken();
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsAccessorListGap(left, right))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}