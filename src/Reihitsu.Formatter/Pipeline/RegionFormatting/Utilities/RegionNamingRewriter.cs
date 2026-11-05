using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting.Utilities;

/// <summary>
/// The region phase's naming step. It pairs each <c>#region</c> with its <c>#endregion</c> and writes the text of both:
/// a <c>#region</c> with a description as <c>#region</c>, one space, and the capitalized, trimmed description, with the
/// matching <c>#endregion</c> comment; a region without a description as the bare keyword, with the text behind its
/// <c>#endregion</c> kept behind one space. It replaces only directive trivia and never changes directive placement
/// </summary>
/// <remarks>
/// Unlike the layout steps around it, this one deliberately reaches directives inside a branch the
/// compiler skipped. RH7301 and RH7302 report those directives, so refusing to name them would leave
/// a diagnostic the formatter declines to fix. The layout steps have no such counterpart — the
/// blank-line rules stay silent inside disabled code — which is why they skip it instead
/// </remarks>
internal static class RegionNamingRewriter
{
    #region Methods

    /// <summary>
    /// Writes every paired region directive in its canonical form: a described <c>#region</c> as <c>#region</c>, one
    /// space, and the capitalized description (FMT-REGION-001, FMT-REGION-007) with its <c>#endregion</c> comment
    /// (FMT-REGION-002); an undescribed <c>#region</c> as the bare keyword, and its <c>#endregion</c> as the bare keyword
    /// or the keyword, one space, and its trimmed text (FMT-REGION-004)
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
            var endRegionDirective = (EndRegionDirectiveTriviaSyntax)endRegion.GetStructure();
            var regionName = RegionDirectiveUtilities.GetRegionDescription(regionDirective);

            SyntaxTrivia canonicalRegion;
            SyntaxTrivia canonicalEndRegion;

            if (regionName.Length == 0)
            {
                canonicalRegion = BuildRegionTrivia(regionDirective, string.Empty);
                canonicalEndRegion = BuildUndescribedEndRegionTrivia(endRegionDirective);
            }
            else
            {
                var capitalizedName = CapitalizeFirstLetter(regionName);

                canonicalRegion = BuildRegionTrivia(regionDirective, capitalizedName);
                canonicalEndRegion = RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia(endRegionDirective, capitalizedName);
            }

            AddReplacementIfChanged(replacements, region, canonicalRegion);
            AddReplacementIfChanged(replacements, endRegion, canonicalEndRegion);
        }

        return replacements.Count > 0
                   ? root.ReplaceTrivia(replacements.Keys, (oldTrivia, _) => replacements[oldTrivia])
                   : root;
    }

    /// <summary>
    /// Records a replacement for a directive whose canonical text differs from its text, so a directive already in
    /// canonical form keeps its trivia
    /// </summary>
    /// <param name="replacements">The replacements collected so far</param>
    /// <param name="original">The directive trivia as it stands</param>
    /// <param name="canonical">The directive trivia in canonical form</param>
    private static void AddReplacementIfChanged(Dictionary<SyntaxTrivia, SyntaxTrivia> replacements, SyntaxTrivia original, SyntaxTrivia canonical)
    {
        if (original.ToFullString() != canonical.ToFullString())
        {
            replacements[original] = canonical;
        }
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
    /// Builds a replacement <c>#region</c> trivia: the keyword, one space, and the given name, or the bare keyword when
    /// the name is empty. Whitespace between the keyword and the name and behind the name is not written
    /// </summary>
    /// <param name="original">The original region directive</param>
    /// <param name="newName">The new region name, trimmed, or an empty string for a region without a description</param>
    /// <returns>The replacement trivia</returns>
    private static SyntaxTrivia BuildRegionTrivia(RegionDirectiveTriviaSyntax original, string newName)
    {
        var cleanKeyword = original.RegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var messageTrivia = newName.Length == 0
                                ? SyntaxTriviaList.Empty
                                : SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(" " + newName));
        var newEndToken = original.EndOfDirectiveToken.WithLeadingTrivia(messageTrivia);
        var newDirective = original.WithRegionKeyword(cleanKeyword).WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    /// <summary>
    /// Builds a replacement <c>#endregion</c> trivia for a region without a description: the keyword, one space, and the
    /// text written behind the keyword, trimmed and otherwise kept as written, or the bare keyword when no text follows it
    /// </summary>
    /// <param name="original">The original endregion directive</param>
    /// <returns>The replacement trivia</returns>
    private static SyntaxTrivia BuildUndescribedEndRegionTrivia(EndRegionDirectiveTriviaSyntax original)
    {
        var text = RegionDirectiveUtilities.GetEndRegionText(original);
        var cleanKeyword = original.EndRegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var messageTrivia = text.Length == 0
                                ? SyntaxTriviaList.Empty
                                : SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(" " + text));
        var newEndToken = original.EndOfDirectiveToken.WithLeadingTrivia(messageTrivia);
        var newDirective = original.WithEndRegionKeyword(cleanKeyword).WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    #endregion // Methods
}