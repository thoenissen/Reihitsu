using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Helper methods for using directive ordering analyzers and code fixes
/// </summary>
internal static class UsingDirectiveOrderingUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the using directive is global
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns><see langword="true"/> if the directive is global</returns>
    internal static bool IsGlobalUsing(UsingDirectiveSyntax usingDirective)
    {
        return usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
    }

    /// <summary>
    /// Determines whether the using directive imports a System namespace
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns><see langword="true"/> if the directive imports a System namespace</returns>
    internal static bool IsSystemNamespaceUsing(UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias != null
            || usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
            || usingDirective.Name == null)
        {
            return false;
        }

        var namespaceName = usingDirective.Name.ToString();

        return namespaceName == "System" || namespaceName.StartsWith("System.", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the using directive group
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The group</returns>
    internal static UsingDirectiveOrderingGroup GetUsingDirectiveGroup(UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias != null)
        {
            return UsingDirectiveOrderingGroup.Alias;
        }

        if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
        {
            return UsingDirectiveOrderingGroup.Static;
        }

        return IsSystemNamespaceUsing(usingDirective)
                   ? UsingDirectiveOrderingGroup.SystemNamespace
                   : UsingDirectiveOrderingGroup.OtherNamespace;
    }

    /// <summary>
    /// Gets the sort key for the using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The sort key</returns>
    internal static string GetSortKey(UsingDirectiveSyntax usingDirective)
    {
        return usingDirective.Alias != null
                   ? usingDirective.Alias.Name.Identifier.ValueText
                   : usingDirective.Name?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Compares two sort keys using the repository's using-ordering rules
    /// </summary>
    /// <param name="left">Left sort key</param>
    /// <param name="right">Right sort key</param>
    /// <returns>A value less than zero when <paramref name="left"/> sorts before <paramref name="right"/></returns>
    internal static int CompareSortKeys(string left, string right)
    {
        return StringComparer.OrdinalIgnoreCase.Compare(left, right);
    }

    /// <summary>
    /// Gets the diagnostic location for the using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>Diagnostic location</returns>
    internal static Location GetDiagnosticLocation(UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias != null)
        {
            return usingDirective.Alias.Name.Identifier.GetLocation();
        }

        return usingDirective.Name?.GetLocation() ?? usingDirective.GetLocation();
    }

    /// <summary>
    /// Gets the usings from the given scope
    /// </summary>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <returns>The using directives</returns>
    internal static SyntaxList<UsingDirectiveSyntax> GetUsings(SyntaxNode scope)
    {
        return scope switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.Usings,
                   BaseNamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.Usings,
                   _ => throw new ArgumentOutOfRangeException(nameof(scope)),
               };
    }

    /// <summary>
    /// Applies the using directives to the given scope
    /// </summary>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns>The updated scope</returns>
    internal static SyntaxNode WithUsings(SyntaxNode scope, SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return scope switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.WithUsings(usingDirectives),
                   BaseNamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.WithUsings(usingDirectives),
                   _ => throw new ArgumentOutOfRangeException(nameof(scope)),
               };
    }

    /// <summary>
    /// Reorders the using directives in canonical group and sort order
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns>The reordered using directives</returns>
    internal static SyntaxList<UsingDirectiveSyntax> OrderUsings(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        var orderedGlobalUsings = OrderSubset(usingDirectives.Where(IsGlobalUsing).ToList());
        var orderedLocalUsings = OrderSubset(usingDirectives.Where(obj => IsGlobalUsing(obj) == false).ToList());
        var reorderedUsings = new List<UsingDirectiveSyntax>(usingDirectives.Count);
        var globalUsingIndex = 0;
        var localUsingIndex = 0;

        foreach (var usingDirective in usingDirectives)
        {
            reorderedUsings.Add(IsGlobalUsing(usingDirective)
                                    ? orderedGlobalUsings[globalUsingIndex++]
                                    : orderedLocalUsings[localUsingIndex++]);
        }

        return SyntaxFactory.List(reorderedUsings);
    }

    /// <summary>
    /// Finds the using directive and its containing scope for a diagnostic
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="scope">Containing scope</param>
    /// <returns><see langword="true"/> if both nodes were found</returns>
    internal static bool TryGetUsingDirectiveScope(SyntaxNode root, Diagnostic diagnostic, out SyntaxNode scope)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        var usingDirective = diagnosticNode?.AncestorsAndSelf()
                                           .OfType<UsingDirectiveSyntax>()
                                           .FirstOrDefault();
        scope = usingDirective?.Parent;

        return usingDirective != null && scope is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax;
    }

    /// <summary>
    /// Orders a homogeneous global or local using subset
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns>The ordered subset</returns>
    private static List<UsingDirectiveSyntax> OrderSubset(IReadOnlyList<UsingDirectiveSyntax> usingDirectives)
    {
        var leadingTriviaByGroup = usingDirectives.GroupBy(GetUsingDirectiveGroup)
                                                  .ToDictionary(group => group.Key,
                                                                group => group.Select(usingDirective => usingDirective.GetLeadingTrivia())
                                                                              .ToList());
        var orderedUsings = usingDirectives.Select((usingDirective, directiveIndex) => new
                                                                                       {
                                                                                           UsingDirective = usingDirective,
                                                                                           DirectiveIndex = directiveIndex,
                                                                                       })
                                           .OrderBy(obj => (int)GetUsingDirectiveGroup(obj.UsingDirective))
                                           .ThenBy(obj => GetSortKey(obj.UsingDirective), StringComparer.OrdinalIgnoreCase)
                                           .ThenBy(obj => obj.DirectiveIndex)
                                           .Select(obj => obj.UsingDirective)
                                           .ToList();
        var groupCounts = new Dictionary<UsingDirectiveOrderingGroup, int>();

        for (var usingIndex = 0; usingIndex < orderedUsings.Count; usingIndex++)
        {
            var usingDirective = orderedUsings[usingIndex];
            var usingDirectiveGroup = GetUsingDirectiveGroup(usingDirective);
            groupCounts.TryGetValue(usingDirectiveGroup, out var groupIndex);
            var groupLeadingTrivia = leadingTriviaByGroup[usingDirectiveGroup];
            var leadingTrivia = groupIndex < groupLeadingTrivia.Count
                                    ? groupLeadingTrivia[groupIndex]
                                    : groupLeadingTrivia[groupLeadingTrivia.Count - 1];

            orderedUsings[usingIndex] = usingDirective.WithLeadingTrivia(leadingTrivia);
            groupCounts[usingDirectiveGroup] = groupIndex + 1;
        }

        return orderedUsings;
    }

    #endregion // Methods
}