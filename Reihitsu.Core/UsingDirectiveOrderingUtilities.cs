using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core.Enumerations;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods for using directive ordering analyzers and code fixes
/// </summary>
public static class UsingDirectiveOrderingUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the using directive is global
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns><see langword="true"/> if the directive is global</returns>
    public static bool IsGlobalUsing(UsingDirectiveSyntax usingDirective)
    {
        return usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
    }

    /// <summary>
    /// Determines whether the using directive imports a System namespace
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns><see langword="true"/> if the directive imports a System namespace</returns>
    public static bool IsSystemNamespaceUsing(UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias != null
            || usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
            || usingDirective.Name == null)
        {
            return false;
        }

        var namespaceName = StripGlobalQualifier(usingDirective.Name.ToString());

        return namespaceName == "System" || namespaceName.StartsWith("System.", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the root namespace segment for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The first namespace segment (after stripping a leading <c>global::</c> qualifier), or an empty string</returns>
    public static string GetRootNamespace(UsingDirectiveSyntax usingDirective)
    {
        var name = StripGlobalQualifier(usingDirective.Name?.ToString() ?? string.Empty);
        var dotIndex = name.IndexOf('.');

        return dotIndex >= 0 ? name.Substring(0, dotIndex) : name;
    }

    /// <summary>
    /// Gets the using directive group
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The group</returns>
    public static UsingDirectiveOrderingGroup GetUsingDirectiveGroup(UsingDirectiveSyntax usingDirective)
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
    public static string GetSortKey(UsingDirectiveSyntax usingDirective)
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
    public static int CompareSortKeys(string left, string right)
    {
        return StringComparer.OrdinalIgnoreCase.Compare(left, right);
    }

    /// <summary>
    /// Gets the using-type ordering slot used to separate regular, static and alias directives
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The using-type order (regular before static before alias)</returns>
    public static int GetUsingTypeOrder(UsingDirectiveSyntax usingDirective)
    {
        return GetUsingDirectiveGroup(usingDirective) switch
               {
                   UsingDirectiveOrderingGroup.Static => 1,
                   UsingDirectiveOrderingGroup.Alias => 2,
                   _ => 0,
               };
    }

    /// <summary>
    /// Gets the namespace ordering key for a using directive, keeping the <c>System</c> group first
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The namespace ordering key</returns>
    public static string GetNamespaceGroupOrderKey(UsingDirectiveSyntax usingDirective)
    {
        var rootNamespace = GetRootNamespace(usingDirective);

        return string.Equals(rootNamespace, "System", StringComparison.OrdinalIgnoreCase)
                   ? string.Empty
                   : rootNamespace;
    }

    /// <summary>
    /// Determines whether two using directives belong to the same group (same using type and root namespace)
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
    /// Computes the canonical (sorted and grouped) order shared by the analyzers, code fixes and formatter
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns>The canonically ordered list</returns>
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
    /// Strips a leading <c>global::</c> alias qualifier from a using name
    /// </summary>
    /// <param name="name">Rendered using name</param>
    /// <returns>The name without a leading <c>global::</c> qualifier</returns>
    private static string StripGlobalQualifier(string name)
    {
        const string globalQualifier = "global::";

        return name.StartsWith(globalQualifier, StringComparison.Ordinal)
                   ? name.Substring(globalQualifier.Length)
                   : name;
    }

    /// <summary>
    /// Gets the diagnostic location for the using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>Diagnostic location</returns>
    public static Location GetDiagnosticLocation(UsingDirectiveSyntax usingDirective)
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
    public static SyntaxList<UsingDirectiveSyntax> GetUsings(SyntaxNode scope)
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
    public static SyntaxNode WithUsings(SyntaxNode scope, SyntaxList<UsingDirectiveSyntax> usingDirectives)
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
    public static SyntaxList<UsingDirectiveSyntax> OrderUsings(SyntaxList<UsingDirectiveSyntax> usingDirectives)
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
    public static bool TryGetUsingDirectiveScope(SyntaxNode root, Diagnostic diagnostic, out SyntaxNode scope)
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
        var orderedUsings = ComputeCanonicalOrder(SyntaxFactory.List(usingDirectives));

        // Reassigning leading trivia positionally would move comments onto a different directive once the
        // members are reordered. When any member carries a comment, the directives keep their own leading
        // trivia so the comment stays with the directive it was written for
        if (usingDirectives.Any(usingDirective => HasCommentTrivia(usingDirective.GetLeadingTrivia())))
        {
            return orderedUsings;
        }

        var leadingTriviaByGroup = usingDirectives.GroupBy(GetUsingDirectiveGroup)
                                                  .ToDictionary(group => group.Key,
                                                                group => group.Select(usingDirective => usingDirective.GetLeadingTrivia())
                                                                              .ToList());
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

    /// <summary>
    /// Determines whether the trivia list contains a comment
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns><see langword="true"/> if the trivia list contains a comment</returns>
    private static bool HasCommentTrivia(SyntaxTriviaList trivia)
    {
        return trivia.Any(currentTrivia => currentTrivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                                           || currentTrivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                                           || currentTrivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                           || currentTrivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
    }

    #endregion // Methods
}