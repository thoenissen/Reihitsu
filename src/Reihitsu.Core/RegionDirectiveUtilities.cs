using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Provides helpers for working with region directives
/// </summary>
public static class RegionDirectiveUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the directive is located within an element body
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if the directive is inside an element body</returns>
    public static bool IsWithinElementBody(SyntaxTrivia directiveTrivia)
    {
        var currentNode = directiveTrivia.Token.Parent;

        while (currentNode != null)
        {
            switch (currentNode)
            {
                case BlockSyntax:
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
    public static string GetRegionDescription(RegionDirectiveTriviaSyntax directive)
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
    public static string GetEndRegionDescription(EndRegionDirectiveTriviaSyntax directive)
    {
        var description = $"{directive.EndRegionKeyword.TrailingTrivia.ToFullString()}{directive.EndOfDirectiveToken.LeadingTrivia.ToFullString()}".Trim();

        if (description.StartsWith("//", StringComparison.Ordinal))
        {
            description = description.Substring(2).TrimStart();
        }

        return description;
    }

    /// <summary>
    /// Creates canonical <c>#endregion // Description</c> trivia while preserving the directive's exterior trivia
    /// </summary>
    /// <param name="directive">Endregion directive to rewrite</param>
    /// <param name="description">Normalized description to write</param>
    /// <returns>Canonical replacement trivia</returns>
    public static SyntaxTrivia CreateCanonicalEndRegionTrivia(EndRegionDirectiveTriviaSyntax directive, string description)
    {
        var replacementDirective = directive.WithEndRegionKeyword(directive.EndRegionKeyword.WithTrailingTrivia(SyntaxFactory.Space,
                                                                                                                SyntaxFactory.Comment($"// {description}")))
                                            .WithEndOfDirectiveToken(directive.EndOfDirectiveToken.WithLeadingTrivia());

        return SyntaxFactory.Trivia(replacementDirective);
    }

    /// <summary>
    /// Enumerates region directives once and pairs included directives using LIFO matching
    /// </summary>
    /// <param name="syntaxRoot">Syntax root whose directives should be paired</param>
    /// <param name="includeDirective">Optional policy applied to each directive before pairing</param>
    /// <returns>Matched pairs in closing-directive traversal order; unmatched directives are omitted</returns>
    public static List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> GetRegionPairs(SyntaxNode syntaxRoot, Func<SyntaxTrivia, bool> includeDirective)
    {
        return GetRegionPairsWithDepth(syntaxRoot, includeDirective).Select(pair => (pair.Region, pair.EndRegion))
                                                                    .ToList();
    }

    /// <summary>
    /// Gets the top-level region pairs declared for the current type
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Region pairs</returns>
    public static List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> GetTopLevelRegions(TypeDeclarationSyntax typeDeclaration)
    {
        var nestedTypeSpans = typeDeclaration.DescendantNodes()
                                             .OfType<TypeDeclarationSyntax>()
                                             .Select(nestedType => nestedType.Span)
                                             .ToList();

        return GetRegionPairsWithDepth(typeDeclaration, directiveTrivia => BelongsToType(typeDeclaration, nestedTypeSpans, directiveTrivia)).Where(pair => pair.NestingDepth == 1)
                                                                                                                                            .Select(pair => (pair.Region, pair.EndRegion))
                                                                                                                                            .ToList();
    }

    /// <summary>
    /// Determines whether the member is contained in any of the provided regions
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Region pairs</param>
    /// <returns><see langword="true"/> if contained in a region</returns>
    public static bool IsWithinRegion(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        return regions.Any(obj => Contains(obj, memberDeclaration));
    }

    /// <summary>
    /// Tries to find the containing top-level region for the member declaration
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Region pairs</param>
    /// <param name="region">Containing region pair</param>
    /// <returns><see langword="true"/> if a containing region was found</returns>
    public static bool TryFindContainingRegion(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions, out (SyntaxTrivia Region, SyntaxTrivia EndRegion) region)
    {
        region = regions.FirstOrDefault(currentRegion => Contains(currentRegion, memberDeclaration));

        return region != default;
    }

    /// <summary>
    /// Gets the index of the containing top-level region for the member declaration, or <c>-1</c> if the
    /// member is not contained in any of the provided regions. The index is stable for the given region
    /// list and can be used as a region-scope key
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Region pairs</param>
    /// <returns>Index of the containing region, or <c>-1</c> if the member is outside any region</returns>
    public static int GetContainingRegionIndex(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        for (var index = 0; index < regions.Count; index++)
        {
            if (Contains(regions[index], memberDeclaration))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// Tries to find the matching region directive
    /// </summary>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="matchingDirectiveTrivia">Matching directive trivia</param>
    /// <returns><see langword="true"/> if a matching directive was found</returns>
    public static bool TryFindMatchingDirective(SyntaxNode syntaxRoot, SyntaxTrivia directiveTrivia, out SyntaxTrivia matchingDirectiveTrivia)
    {
        matchingDirectiveTrivia = default;

        if (syntaxRoot == null || SyntaxTriviaUtilities.IsRegionDirective(directiveTrivia) == false)
        {
            return false;
        }

        foreach (var (region, endRegion) in GetRegionPairs(syntaxRoot, includeDirective: null))
        {
            if (region == directiveTrivia)
            {
                matchingDirectiveTrivia = endRegion;

                return true;
            }

            if (endRegion == directiveTrivia)
            {
                matchingDirectiveTrivia = region;

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the directive belongs to the current type declaration
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="nestedTypeSpans">Spans of the type's nested type declarations</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if the directive belongs to the current type</returns>
    private static bool BelongsToType(TypeDeclarationSyntax typeDeclaration, IReadOnlyList<TextSpan> nestedTypeSpans, SyntaxTrivia directiveTrivia)
    {
        if (IsWithinElementBody(directiveTrivia))
        {
            return false;
        }

        if (typeDeclaration.Span.Contains(directiveTrivia.SpanStart) == false)
        {
            return false;
        }

        return nestedTypeSpans.Any(nestedTypeSpan => nestedTypeSpan.Contains(directiveTrivia.SpanStart)) == false;
    }

    /// <summary>
    /// Determines whether the member declaration sits between the region's <c>#region</c> and
    /// <c>#endregion</c> directives
    /// </summary>
    /// <param name="region">Region pair</param>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns><see langword="true"/> if the member is contained in the region</returns>
    private static bool Contains((SyntaxTrivia Region, SyntaxTrivia EndRegion) region, MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration.SpanStart >= region.Region.Span.End
               && memberDeclaration.Span.End <= region.EndRegion.SpanStart;
    }

    /// <summary>
    /// Enumerates included region directives once and retains each matched pair's opening depth
    /// </summary>
    /// <param name="syntaxRoot">Syntax root whose directives should be paired</param>
    /// <param name="includeDirective">Optional policy applied to each directive before pairing</param>
    /// <returns>Matched pairs with their opening depth in closing-directive traversal order</returns>
    private static List<(SyntaxTrivia Region, SyntaxTrivia EndRegion, int NestingDepth)> GetRegionPairsWithDepth(SyntaxNode syntaxRoot, Func<SyntaxTrivia, bool> includeDirective)
    {
        var pairs = new List<(SyntaxTrivia Region, SyntaxTrivia EndRegion, int NestingDepth)>();

        if (syntaxRoot == null)
        {
            return pairs;
        }

        var regionStack = new Stack<SyntaxTrivia>();

        foreach (var directiveTrivia in syntaxRoot.DescendantTrivia(descendIntoTrivia: true))
        {
            if (SyntaxTriviaUtilities.IsRegionDirective(directiveTrivia) == false
                || (includeDirective != null && includeDirective(directiveTrivia) == false))
            {
                continue;
            }

            if (directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                regionStack.Push(directiveTrivia);
            }
            else if (regionStack.Count > 0)
            {
                var nestingDepth = regionStack.Count;

                pairs.Add((regionStack.Pop(), directiveTrivia, nestingDepth));
            }
        }

        return pairs;
    }

    #endregion // Methods
}