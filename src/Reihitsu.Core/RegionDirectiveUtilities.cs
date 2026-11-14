using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    /// Gets the text written behind the <c>endregion</c> keyword of an endregion directive, trimmed, with a leading
    /// <c>//</c> kept as written
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>The trimmed text, or an empty string when only whitespace follows the keyword</returns>
    public static string GetEndRegionText(EndRegionDirectiveTriviaSyntax directive)
    {
        return $"{directive.EndRegionKeyword.TrailingTrivia.ToFullString()}{directive.EndOfDirectiveToken.LeadingTrivia.ToFullString()}".Trim();
    }

    /// <summary>
    /// Gets the description of an endregion directive: its text (<see cref="GetEndRegionText"/>) without a leading <c>//</c>
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    public static string GetEndRegionDescription(EndRegionDirectiveTriviaSyntax directive)
    {
        var description = GetEndRegionText(directive);

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
        return GetTypeRegions(typeDeclaration).Where(pair => pair.NestingDepth == 1)
                                              .Select(pair => (pair.Region, pair.EndRegion))
                                              .ToList();
    }

    /// <summary>
    /// Gets every region pair that belongs to the current type (<see cref="GetOwningTypeDeclaration"/>): its top-level
    /// regions and the regions nested in them, each with its nesting depth, <c>1</c> for a top-level region
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Region pairs with their nesting depth in closing-directive traversal order</returns>
    public static List<(SyntaxTrivia Region, SyntaxTrivia EndRegion, int NestingDepth)> GetTypeRegions(TypeDeclarationSyntax typeDeclaration)
    {
        return GetRegionPairsWithDepth(typeDeclaration,
                                       directiveTrivia => IsWithinElementBody(directiveTrivia) == false
                                                          && GetOwningTypeDeclaration(directiveTrivia) == typeDeclaration);
    }

    /// <summary>
    /// Gets the declaration a region directive belongs to: the innermost class, struct, interface, record, record struct,
    /// or enum declaration whose span holds the directive
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns>The declaration, or <see langword="null"/> when no type declaration holds the directive</returns>
    public static BaseTypeDeclarationSyntax GetOwningTypeDeclaration(SyntaxTrivia directiveTrivia)
    {
        for (var node = directiveTrivia.Token.Parent; node != null; node = node.Parent)
        {
            if (node is BaseTypeDeclarationSyntax typeDeclaration
                && typeDeclaration.Span.Contains(directiveTrivia.SpanStart))
            {
                return typeDeclaration;
            }
        }

        return null;
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
    /// Tries to find the region that judges a member by its description: the innermost region that encloses the member,
    /// when the member lies in a top-level region
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Region pairs of the member's type with their nesting depth (<see cref="GetTypeRegions"/>)</param>
    /// <param name="region">Innermost enclosing region pair</param>
    /// <returns><see langword="true"/> if the member lies in a top-level region</returns>
    public static bool TryFindInnermostContainingRegion(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion, int NestingDepth)> regions, out (SyntaxTrivia Region, SyntaxTrivia EndRegion) region)
    {
        region = default;

        var containingRegions = regions.Where(currentRegion => Contains((currentRegion.Region, currentRegion.EndRegion), memberDeclaration))
                                       .ToList();

        if (containingRegions.Any(currentRegion => currentRegion.NestingDepth == 1) == false)
        {
            return false;
        }

        var innermostRegion = containingRegions.OrderByDescending(currentRegion => currentRegion.NestingDepth)
                                               .First();

        region = (innermostRegion.Region, innermostRegion.EndRegion);

        return true;
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
    /// Determines whether a directive lies in a branch — <c>#if</c>, <c>#elif</c>, or <c>#else</c> — of a conditional
    /// directive that starts inside the given node
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="scope">Node whose conditional directives are considered</param>
    /// <returns><see langword="true"/> if an open branch of a conditional directive inside the node encloses the directive</returns>
    public static bool IsInConditionalBranch(SyntaxTrivia directiveTrivia, SyntaxNode scope)
    {
        if (directiveTrivia.GetStructure() is not DirectiveTriviaSyntax directive)
        {
            return false;
        }

        var closedConditionals = 0;

        for (var previousDirective = directive.GetPreviousDirective(); previousDirective != null && previousDirective.SpanStart >= scope.SpanStart; previousDirective = previousDirective.GetPreviousDirective())
        {
            switch (previousDirective)
            {
                case EndIfDirectiveTriviaSyntax:
                    closedConditionals++;
                    break;

                case IfDirectiveTriviaSyntax when closedConditionals > 0:
                    closedConditionals--;
                    break;

                case IfDirectiveTriviaSyntax:
                case ElifDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax when closedConditionals == 0:
                    return true;
            }
        }

        return false;
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