using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// The ordering and grouping policy half of the using-directive ordering phase. It decides the
/// canonical order of a using block and whether two adjacent directives belong to the same group,
/// without touching any leading trivia. The policy is owned by
/// <see cref="UsingDirectiveOrderingUtilities"/> so the analyzers, code fixes and formatter share a
/// single source of truth. Rebuilding the layout belongs to <see cref="UsingLeadingTriviaBuilder"/>
/// </summary>
internal static class UsingGrouping
{
    #region Methods

    /// <summary>
    /// Determines whether two using directives belong to the same formatter group
    /// </summary>
    /// <param name="left">Left using directive</param>
    /// <param name="right">Right using directive</param>
    /// <returns><see langword="true"/> if both directives belong to the same group</returns>
    public static bool AreInSameGroup(UsingDirectiveSyntax left, UsingDirectiveSyntax right)
    {
        return UsingDirectiveOrderingUtilities.AreInSameGroup(left, right);
    }

    /// <summary>
    /// Computes the canonical order for the given using directives
    /// </summary>
    /// <param name="usingDirectives">Using directives to order</param>
    /// <returns>The canonical order</returns>
    public static List<UsingDirectiveSyntax> ComputeCanonicalOrder(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return UsingDirectiveOrderingUtilities.ComputeCanonicalOrder(usingDirectives);
    }

    #endregion // Methods
}