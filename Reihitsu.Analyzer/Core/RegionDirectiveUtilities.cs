using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Provides helpers for working with region directives
/// </summary>
internal static class RegionDirectiveUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the directive is located within an element body
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if the directive is inside an element body</returns>
    internal static bool IsWithinElementBody(SyntaxTrivia directiveTrivia)
    {
        var currentNode = directiveTrivia.Token.Parent;

        while (currentNode != null)
        {
            switch (currentNode)
            {
                case BlockSyntax { Parent: not TypeDeclarationSyntax and not NamespaceDeclarationSyntax and not FileScopedNamespaceDeclarationSyntax and not CompilationUnitSyntax }:
                case AccessorListSyntax:
                case AnonymousFunctionExpressionSyntax:
                case LocalFunctionStatementSyntax:
                case StatementSyntax:
                    return true;

                case TypeDeclarationSyntax:
                case NamespaceDeclarationSyntax:
                case FileScopedNamespaceDeclarationSyntax:
                case CompilationUnitSyntax:
                    return false;

                default:
                    currentNode = currentNode.Parent;
                    break;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the description of a region directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    internal static string GetRegionDescription(RegionDirectiveTriviaSyntax directive)
    {
        var messageTrivia = directive.EndOfDirectiveToken.LeadingTrivia.FirstOrDefault(static trivia => trivia.IsKind(SyntaxKind.PreprocessingMessageTrivia));

        return messageTrivia == default
                   ? string.Empty
                   : messageTrivia.ToString().Trim();
    }

    /// <summary>
    /// Gets the description of an endregion directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    internal static string GetEndRegionDescription(EndRegionDirectiveTriviaSyntax directive)
    {
        var description = $"{directive.EndRegionKeyword.TrailingTrivia.ToFullString()}{directive.EndOfDirectiveToken.LeadingTrivia.ToFullString()}".Trim();

        if (description.StartsWith("//", StringComparison.Ordinal))
        {
            description = description.Substring(2).TrimStart();
        }

        return description;
    }

    /// <summary>
    /// Gets the top-level region pairs declared for the current type
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Region pairs</returns>
    internal static List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> GetTopLevelRegions(TypeDeclarationSyntax typeDeclaration)
    {
        var regions = new List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)>();
        var regionStack = new Stack<SyntaxTrivia>();

        foreach (var directiveTrivia in typeDeclaration.DescendantTrivia(descendIntoTrivia: true)
                                                       .Where(trivia => trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                                                                        || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)))
        {
            if (BelongsToType(typeDeclaration, directiveTrivia) == false)
            {
                continue;
            }

            if (directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                regionStack.Push(directiveTrivia);
            }
            else if (regionStack.Count > 1)
            {
                regionStack.Pop();
            }
            else if (regionStack.Count > 0)
            {
                regions.Add((regionStack.Pop(), directiveTrivia));
            }
        }

        return regions;
    }

    /// <summary>
    /// Determines whether the member is contained in any of the provided regions
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Region pairs</param>
    /// <returns><see langword="true"/> if contained in a region</returns>
    internal static bool IsWithinRegion(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        return regions.Any(obj => memberDeclaration.SpanStart >= obj.Region.Span.End
                                  && memberDeclaration.Span.End <= obj.EndRegion.SpanStart);
    }

    /// <summary>
    /// Tries to find the containing top-level region for the member declaration
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Region pairs</param>
    /// <param name="region">Containing region pair</param>
    /// <returns><see langword="true"/> if a containing region was found</returns>
    internal static bool TryFindContainingRegion(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions, out (SyntaxTrivia Region, SyntaxTrivia EndRegion) region)
    {
        foreach (var currentRegion in regions)
        {
            if (memberDeclaration.SpanStart >= currentRegion.Region.Span.End
                && memberDeclaration.Span.End <= currentRegion.EndRegion.SpanStart)
            {
                region = currentRegion;

                return true;
            }
        }

        region = default;

        return false;
    }

    /// <summary>
    /// Tries to find the matching region directive
    /// </summary>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="matchingDirectiveTrivia">Matching directive trivia</param>
    /// <returns><see langword="true"/> if a matching directive was found</returns>
    internal static bool TryFindMatchingDirective(SyntaxNode syntaxRoot, SyntaxTrivia directiveTrivia, out SyntaxTrivia matchingDirectiveTrivia)
    {
        matchingDirectiveTrivia = default;

        if (syntaxRoot == null || IsRegionDirective(directiveTrivia) == false)
        {
            return false;
        }

        var directives = syntaxRoot.DescendantTrivia(descendIntoTrivia: true)
                                   .Where(trivia => trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                                                    || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                                   .ToList();
        var directiveIndex = directives.FindIndex(currentDirective => currentDirective == directiveTrivia);

        if (directiveIndex < 0)
        {
            return false;
        }

        if (directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
        {
            return TryFindMatchingEndRegion(directives, directiveIndex, out matchingDirectiveTrivia);
        }

        return TryFindMatchingRegion(directives, directiveIndex, out matchingDirectiveTrivia);
    }

    /// <summary>
    /// Determines whether the directive belongs to the current type declaration
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if the directive belongs to the current type</returns>
    private static bool BelongsToType(TypeDeclarationSyntax typeDeclaration, SyntaxTrivia directiveTrivia)
    {
        if (IsWithinElementBody(directiveTrivia))
        {
            return false;
        }

        if (typeDeclaration.Span.Contains(directiveTrivia.SpanStart) == false)
        {
            return false;
        }

        return typeDeclaration.DescendantNodes()
                              .OfType<TypeDeclarationSyntax>()
                              .Any(nestedType => nestedType.Span.Contains(directiveTrivia.SpanStart)) == false;
    }

    /// <summary>
    /// Determines whether a directive trivia is #region or #endregion
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if supported</returns>
    private static bool IsRegionDirective(SyntaxTrivia directiveTrivia)
    {
        return directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
               || directiveTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia);
    }

    /// <summary>
    /// Tries to find the matching #endregion for a #region
    /// </summary>
    /// <param name="directives">Directive list</param>
    /// <param name="directiveIndex">Index of #region</param>
    /// <param name="matchingDirectiveTrivia">Matching #endregion</param>
    /// <returns><see langword="true"/> if found</returns>
    private static bool TryFindMatchingEndRegion(List<SyntaxTrivia> directives, int directiveIndex, out SyntaxTrivia matchingDirectiveTrivia)
    {
        matchingDirectiveTrivia = default;

        var nestedRegionCount = 0;

        for (var directivePosition = directiveIndex + 1; directivePosition < directives.Count; directivePosition++)
        {
            var currentDirective = directives[directivePosition];

            if (currentDirective.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                nestedRegionCount++;
            }
            else if (nestedRegionCount == 0)
            {
                matchingDirectiveTrivia = currentDirective;

                return true;
            }
            else
            {
                nestedRegionCount--;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to find the matching #region for an #endregion
    /// </summary>
    /// <param name="directives">Directive list</param>
    /// <param name="directiveIndex">Index of #endregion</param>
    /// <param name="matchingDirectiveTrivia">Matching #region</param>
    /// <returns><see langword="true"/> if found</returns>
    private static bool TryFindMatchingRegion(List<SyntaxTrivia> directives, int directiveIndex, out SyntaxTrivia matchingDirectiveTrivia)
    {
        matchingDirectiveTrivia = default;

        var nestedEndRegionCount = 0;

        for (var directivePosition = directiveIndex - 1; directivePosition >= 0; directivePosition--)
        {
            var currentDirective = directives[directivePosition];

            if (currentDirective.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            {
                nestedEndRegionCount++;
            }
            else if (nestedEndRegionCount == 0)
            {
                matchingDirectiveTrivia = currentDirective;

                return true;
            }
            else
            {
                nestedEndRegionCount--;
            }
        }

        return false;
    }

    #endregion // Methods
}