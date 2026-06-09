using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// The ordering and grouping policy half of the using-directive ordering phase. It decides the
/// canonical order of a using block and whether two adjacent directives belong to the same group,
/// without touching any leading trivia. Rebuilding the layout belongs to
/// <see cref="UsingLeadingTriviaBuilder"/>
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
        return GetUsingTypeOrder(left) == GetUsingTypeOrder(right)
               && string.Equals(GetRootNamespace(left), GetRootNamespace(right), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Computes the canonical order for the given using directives
    /// </summary>
    /// <param name="usingDirectives">Using directives to order</param>
    /// <returns>The canonical order</returns>
    public static List<UsingDirectiveSyntax> ComputeCanonicalOrder(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return usingDirectives.Select((usingDirective, directiveIndex) => new
                                                                          {
                                                                              UsingDirective = usingDirective,
                                                                              DirectiveIndex = directiveIndex,
                                                                          })
                              .OrderBy(obj => GetUsingTypeOrder(obj.UsingDirective))
                              .ThenBy(obj => GetNamespaceGroupOrderKey(obj.UsingDirective), StringComparer.OrdinalIgnoreCase)
                              .ThenBy(obj => GetSortKey(obj.UsingDirective), StringComparer.OrdinalIgnoreCase)
                              .ThenBy(obj => obj.DirectiveIndex)
                              .Select(obj => obj.UsingDirective)
                              .ToList();
    }

    /// <summary>
    /// Gets the namespace ordering key for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The ordering key</returns>
    private static string GetNamespaceGroupOrderKey(UsingDirectiveSyntax usingDirective)
    {
        var rootNamespace = GetRootNamespace(usingDirective);

        return string.Equals(rootNamespace, "System", StringComparison.OrdinalIgnoreCase)
                   ? string.Empty
                   : rootNamespace;
    }

    /// <summary>
    /// Gets the root namespace segment for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The first namespace segment, or an empty string</returns>
    private static string GetRootNamespace(UsingDirectiveSyntax usingDirective)
    {
        var name = usingDirective.Name?.ToString() ?? string.Empty;
        var dotIndex = name.IndexOf('.');

        return dotIndex >= 0 ? name.Substring(0, dotIndex) : name;
    }

    /// <summary>
    /// Gets a stable sort key for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The string key to sort by</returns>
    private static string GetSortKey(UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias != null)
        {
            return $"{usingDirective.Alias.Name}={usingDirective.Name}";
        }

        return usingDirective.Name?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the using-type ordering slot
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The using-type order</returns>
    private static int GetUsingTypeOrder(UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
        {
            return 1;
        }

        if (usingDirective.Alias != null)
        {
            return 2;
        }

        return 0;
    }

    #endregion // Methods
}