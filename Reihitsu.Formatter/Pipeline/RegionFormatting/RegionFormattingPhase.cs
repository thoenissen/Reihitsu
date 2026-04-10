using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting;

/// <summary>
/// Region formatting — capitalizes region descriptions and synchronizes
/// endregion comments to match their corresponding region name.
/// </summary>
internal static class RegionFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies region formatting rules to the given syntax node.
    /// </summary>
    /// <param name="root">The syntax node to format.</param>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The formatted syntax node.</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var regions = new Stack<SyntaxTrivia>();
        var pairs = new List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                regions.Push(trivia);
            }
            else if (trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) && regions.Count > 0)
            {
                pairs.Add((regions.Pop(), trivia));
            }
        }

        var replacements = new Dictionary<SyntaxTrivia, SyntaxTrivia>();

        foreach (var (region, endRegion) in pairs)
        {
            var regionDirective = (RegionDirectiveTriviaSyntax)region.GetStructure();
            var regionName = GetRegionName(regionDirective);

            if (string.IsNullOrWhiteSpace(regionName))
            {
                continue;
            }

            var capitalizedName = CapitalizeFirstLetter(regionName);

            if (regionName != capitalizedName)
            {
                replacements[region] = BuildRegionTrivia(regionDirective, capitalizedName);
            }

            var endRegionDirective = (EndRegionDirectiveTriviaSyntax)endRegion.GetStructure();
            var expectedComment = " // " + capitalizedName;
            var currentComment = GetEndRegionComment(endRegionDirective);

            if (currentComment != expectedComment)
            {
                replacements[endRegion] = BuildEndRegionTrivia(endRegionDirective, expectedComment);
            }
        }

        if (replacements.Count == 0)
        {
            return root;
        }

        return root.ReplaceTrivia(replacements.Keys, (oldTrivia, _) => replacements[oldTrivia]);
    }

    /// <summary>
    /// Extracts the region name from a <c>#region</c> directive.
    /// </summary>
    /// <param name="directive">The region directive syntax.</param>
    /// <returns>The region name, or <see langword="null"/> if no name is found.</returns>
    private static string GetRegionName(RegionDirectiveTriviaSyntax directive)
    {
        foreach (var trivia in directive.EndOfDirectiveToken.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.PreprocessingMessageTrivia))
            {
                return trivia.ToString().TrimStart();
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the trailing comment text from an <c>#endregion</c> directive.
    /// </summary>
    /// <param name="directive">The endregion directive syntax.</param>
    /// <returns>The trailing comment text.</returns>
    private static string GetEndRegionComment(EndRegionDirectiveTriviaSyntax directive)
    {
        var combined = directive.EndRegionKeyword.TrailingTrivia.ToFullString()
                       + directive.EndOfDirectiveToken.LeadingTrivia.ToFullString();

        return combined;
    }

    /// <summary>
    /// Returns the given text with its first letter capitalized.
    /// </summary>
    /// <param name="text">The text to capitalize.</param>
    /// <returns>The text with the first letter in upper case.</returns>
    private static string CapitalizeFirstLetter(string text)
    {
        if (text.Length == 0 || char.IsUpper(text[0]))
        {
            return text;
        }

        return char.ToUpperInvariant(text[0]) + text.Substring(1);
    }

    /// <summary>
    /// Builds a replacement <c>#region</c> trivia with the specified name.
    /// </summary>
    /// <param name="original">The original region directive.</param>
    /// <param name="newName">The new region name.</param>
    /// <returns>The replacement trivia.</returns>
    private static SyntaxTrivia BuildRegionTrivia(RegionDirectiveTriviaSyntax original, string newName)
    {
        var cleanKeyword = original.RegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var endToken = original.EndOfDirectiveToken;
        var newEndToken = endToken.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(" " + newName)));
        var newDirective = original.WithRegionKeyword(cleanKeyword).WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    /// <summary>
    /// Builds a replacement <c>#endregion</c> trivia with the specified comment.
    /// </summary>
    /// <param name="original">The original endregion directive.</param>
    /// <param name="comment">The comment to attach.</param>
    /// <returns>The replacement trivia.</returns>
    private static SyntaxTrivia BuildEndRegionTrivia(EndRegionDirectiveTriviaSyntax original, string comment)
    {
        var cleanKeyword = original.EndRegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var endToken = original.EndOfDirectiveToken;
        var newEndToken = endToken.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(comment)));
        var newDirective = original.WithEndRegionKeyword(cleanKeyword).WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    #endregion // Methods
}