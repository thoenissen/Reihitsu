using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting.Utilities;

/// <summary>
/// The removal half of the region phase. It strips <c>#region</c> and <c>#endregion</c> directives
/// that sit inside an element body, reparsing the changed text to rebuild the tree. Naming and
/// endregion synchronization belong to <see cref="RegionNamingRewriter"/>
/// </summary>
internal static class NestedRegionRemovalStep
{
    #region Methods

    /// <summary>
    /// Removes region directives placed within element bodies
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated root</returns>
    public static SyntaxNode Remove(SyntaxNode root, CancellationToken cancellationToken)
    {
        var syntaxTree = root.SyntaxTree;
        var sourceText = syntaxTree?.GetText(cancellationToken) ?? SourceText.From(root.ToFullString());
        var removalSpans = new List<TextSpan>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) == false)
            {
                continue;
            }

            if (TryGetRemovableRegionPair(trivia, cancellationToken, out var endRegionTrivia) == false)
            {
                continue;
            }

            removalSpans.Add(GetLineRemovalSpan(sourceText, trivia));
            removalSpans.Add(GetLineRemovalSpan(sourceText, endRegionTrivia));
        }

        if (removalSpans.Count == 0)
        {
            return root;
        }

        var updatedText = sourceText;

        foreach (var removalSpan in removalSpans.OrderByDescending(static span => span.Start))
        {
            updatedText = updatedText.Replace(removalSpan, string.Empty);
        }

        // When the node is detached from a syntax tree, reparse the changed text instead of
        // dereferencing a null tree
        if (syntaxTree == null)
        {
            return CSharpSyntaxTree.ParseText(updatedText, cancellationToken: cancellationToken).GetRoot(cancellationToken);
        }

        return syntaxTree.WithChangedText(updatedText).GetRoot(cancellationToken);
    }

    /// <summary>
    /// Gets the span covering the whole line a directive sits on, including its line break
    /// </summary>
    /// <param name="sourceText">The source text</param>
    /// <param name="trivia">The directive trivia</param>
    /// <returns>The span to remove</returns>
    private static TextSpan GetLineRemovalSpan(SourceText sourceText, SyntaxTrivia trivia)
    {
        var line = sourceText.Lines.GetLineFromPosition(trivia.Span.Start);
        var removalEnd = line.EndIncludingLineBreak > line.End
                             ? line.EndIncludingLineBreak
                             : line.End;

        return TextSpan.FromBounds(line.Start, removalEnd);
    }

    /// <summary>
    /// Determines whether a <c>#region</c> directive and its matching <c>#endregion</c> may both be removed
    /// </summary>
    /// <param name="regionTrivia">The <c>#region</c> directive trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="endRegionTrivia">The matching <c>#endregion</c> directive trivia when the pair is removable</param>
    /// <returns><see langword="true"/> if both halves of the pair may be removed; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// One qualifying half is enough, and then both are removed. A pair that straddles an
    /// element-body boundary — the <c>#region</c> inside a body and its <c>#endregion</c> outside, or
    /// the reverse — must not lose only its qualifying half, because the orphaned directive turns
    /// source that compiles into source that does not (CS1028). Removing the pair as a unit also
    /// matches <c>RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider</c>, which deletes both halves
    /// once either one is reported, so the formatter and the code fix agree on the same input.
    /// </remarks>
    private static bool TryGetRemovableRegionPair(SyntaxTrivia regionTrivia,
                                                  CancellationToken cancellationToken,
                                                  out SyntaxTrivia endRegionTrivia)
    {
        endRegionTrivia = default;

        if (regionTrivia.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
        {
            return false;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var relatedDirectives = regionDirective.GetRelatedDirectives();

        if (relatedDirectives.Count != 2 || relatedDirectives[1] is not EndRegionDirectiveTriviaSyntax endRegionDirective)
        {
            return false;
        }

        var candidate = endRegionDirective.ParentTrivia;

        if (candidate.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false)
        {
            return false;
        }

        if (RegionDirectiveUtilities.IsWithinElementBody(regionTrivia) == false
            && RegionDirectiveUtilities.IsWithinElementBody(candidate) == false)
        {
            return false;
        }

        endRegionTrivia = candidate;

        return true;
    }

    #endregion // Methods
}