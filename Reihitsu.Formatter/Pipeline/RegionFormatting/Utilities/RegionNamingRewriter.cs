using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting.Utilities;

/// <summary>
/// The region phase's naming step. It pairs each <c>#region</c> with its <c>#endregion</c>, capitalizes
/// the region description and synchronizes the <c>#endregion</c> comment through trivia replacement
/// without changing directive placement
/// </summary>
/// <remarks>
/// Unlike the layout steps around it, this one deliberately reaches directives inside a branch the
/// compiler skipped. RH7301 and RH7302 report those directives, so refusing to name them would leave
/// a diagnostic the formatter declines to fix. The layout steps have no such counterpart — the
/// blank-line rules stay silent inside disabled code — which is why they skip it instead (issue #434)
/// </remarks>
internal static class RegionNamingRewriter
{
    #region Methods

    /// <summary>
    /// Capitalizes region descriptions and synchronizes endregion comments
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated root</returns>
    public static SyntaxNode Rewrite(SyntaxNode root, CancellationToken cancellationToken)
    {
        var replacements = new Dictionary<SyntaxTrivia, SyntaxTrivia>();

        // The inclusion policy keeps cancellation responsive while the shared engine scans region directives and
        // still includes every region directive in pairing.
        var pairs = RegionDirectiveUtilities.GetRegionPairs(root,
                                                            _ =>
                                                            {
                                                                cancellationToken.ThrowIfCancellationRequested();

                                                                return true;
                                                            });

        foreach (var (region, endRegion) in pairs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var regionDirective = (RegionDirectiveTriviaSyntax)region.GetStructure();
            var regionName = RegionDirectiveUtilities.GetRegionDescription(regionDirective);

            if (regionName.Length == 0)
            {
                continue;
            }

            var capitalizedName = CapitalizeFirstLetter(regionName);

            if (regionName != capitalizedName)
            {
                replacements[region] = BuildRegionTrivia(regionDirective, capitalizedName);
            }

            var endRegionDirective = (EndRegionDirectiveTriviaSyntax)endRegion.GetStructure();
            var canonicalEndRegion = RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia(endRegionDirective, capitalizedName);

            if (endRegion.ToFullString() != canonicalEndRegion.ToFullString())
            {
                replacements[endRegion] = canonicalEndRegion;
            }
        }

        return replacements.Count > 0
                   ? root.ReplaceTrivia(replacements.Keys, (oldTrivia, _) => replacements[oldTrivia])
                   : root;
    }

    /// <summary>
    /// Returns the given text with its first letter capitalized
    /// </summary>
    /// <param name="text">The text to capitalize</param>
    /// <returns>The text with the first letter in upper case</returns>
    private static string CapitalizeFirstLetter(string text)
    {
        if (text.Length == 0 || char.IsUpper(text[0]))
        {
            return text;
        }

        return char.ToUpperInvariant(text[0]) + text.Substring(1);
    }

    /// <summary>
    /// Builds a replacement <c>#region</c> trivia with the specified name
    /// </summary>
    /// <param name="original">The original region directive</param>
    /// <param name="newName">The new region name</param>
    /// <returns>The replacement trivia</returns>
    private static SyntaxTrivia BuildRegionTrivia(RegionDirectiveTriviaSyntax original, string newName)
    {
        var cleanKeyword = original.RegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var endToken = original.EndOfDirectiveToken;
        var newEndToken = endToken.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(" " + newName)));
        var newDirective = original.WithRegionKeyword(cleanKeyword).WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    #endregion // Methods
}