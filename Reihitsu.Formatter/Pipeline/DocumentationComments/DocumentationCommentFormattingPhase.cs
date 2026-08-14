using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.DocumentationComments.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Pipeline.DocumentationComments;

/// <summary>
/// Normalizes XML documentation comments to repository-specific layout rules. Element-level layout
/// decisions live in <see cref="DocCommentElementNormalizer"/>; this phase locates the candidate
/// elements, splices in their normalized text — repeating until no candidate is left, because only
/// the outermost ones can be rebuilt per round — and fixes the <c>///</c> line prefixes
/// </summary>
internal sealed class DocumentationCommentFormattingPhase : IFormattingPhase
{
    #region Constants

    /// <summary>
    /// Upper bound on the rebuild rounds a single comment may take. Each round resolves one nesting
    /// level, so real documentation finishes in one or two; the bound only guards against a rebuild
    /// that would otherwise keep reporting the same element as a candidate
    /// </summary>
    private const int MaximumNormalizationRounds = 16;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Determines whether a documentation exterior follows non-whitespace source on the same line
    /// </summary>
    /// <param name="documentationCommentTrivia">Documentation comment trivia</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> when the documentation comment starts after other source text</returns>
    private static bool IsAfterSourceOnSameLine(SyntaxTrivia documentationCommentTrivia, SourceText sourceText)
    {
        var line = sourceText.Lines.GetLineFromPosition(documentationCommentTrivia.FullSpan.Start);
        var prefix = sourceText.ToString(TextSpan.FromBounds(line.Start, documentationCommentTrivia.FullSpan.Start));

        return prefix.Any(character => char.IsWhiteSpace(character) == false && character != '\uFEFF');
    }

    /// <summary>
    /// Determines whether significant trivia owned by the following token precedes documentation on its physical line
    /// </summary>
    /// <param name="documentationCommentTrivia">Documentation comment trivia</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> when the owning token carries same-line source before the documentation comment</returns>
    private static bool HasOwningLeadingSourceOnSameLine(SyntaxTrivia documentationCommentTrivia, SourceText sourceText)
    {
        var line = sourceText.Lines.GetLineFromPosition(documentationCommentTrivia.FullSpan.Start);

        foreach (var leadingSpan in documentationCommentTrivia.Token.LeadingTrivia.Select(static leadingTrivia => leadingTrivia.FullSpan))
        {
            var overlapStart = Math.Max(line.Start, leadingSpan.Start);
            var overlapEnd = Math.Min(documentationCommentTrivia.FullSpan.Start, leadingSpan.End);

            if (overlapStart >= overlapEnd)
            {
                continue;
            }

            var overlap = sourceText.ToString(TextSpan.FromBounds(overlapStart, overlapEnd));

            if (overlap.Any(character => char.IsWhiteSpace(character) == false && character != '\uFEFF'))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Inserts line breaks before documentation trivia in their owning token's leading trivia
    /// </summary>
    /// <param name="token">Owning token</param>
    /// <param name="documentationCommentTrivia">Documentation comment trivia owned by the token</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The token with the documentation comment moved onto a line of its own</returns>
    private static SyntaxToken MoveBeforeOwningToken(SyntaxToken token, IReadOnlyCollection<SyntaxTrivia> documentationCommentTrivia, string endOfLine)
    {
        var leadingTrivia = token.LeadingTrivia;
        var documentationIndices = documentationCommentTrivia.Select(trivia => leadingTrivia.IndexOf(trivia))
                                                             .Where(index => index >= 0)
                                                             .OrderByDescending(index => index)
                                                             .ToList();

        foreach (var originalDocumentationIndex in documentationIndices)
        {
            var documentationIndex = originalDocumentationIndex;

            if (documentationIndex > 0 && leadingTrivia[documentationIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                leadingTrivia = leadingTrivia.RemoveAt(documentationIndex - 1);
                documentationIndex--;
            }

            leadingTrivia = leadingTrivia.Insert(documentationIndex, SyntaxFactory.EndOfLine(endOfLine));
        }

        return token.WithLeadingTrivia(leadingTrivia);
    }

    /// <summary>
    /// Moves off-position single-line documentation trivia onto a line of its own before its following token
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The root with trailing documentation comments moved before their Roslyn owner</returns>
    private static SyntaxNode RelocateOffPositionDocumentationComments(SyntaxNode root, SourceText sourceText, string endOfLine, CancellationToken cancellationToken)
    {
        var owningRelocations = new Dictionary<SyntaxToken, List<SyntaxTrivia>>();
        var previousTokenRelocationCounts = new Dictionary<SyntaxToken, int>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();
            RegisterRelocation(root, trivia, sourceText, owningRelocations, previousTokenRelocationCounts);
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var owningRelocation in owningRelocations)
        {
            replacements[owningRelocation.Key] = MoveBeforeOwningToken(owningRelocation.Key, owningRelocation.Value, endOfLine);
        }

        foreach (var previousTokenRelocation in previousTokenRelocationCounts)
        {
            var currentPreviousToken = replacements.TryGetValue(previousTokenRelocation.Key, out var replacement) ? replacement : previousTokenRelocation.Key;
            var trailingTrivia = currentPreviousToken.TrailingTrivia;

            if (trailingTrivia.Count > 0 && trailingTrivia[trailingTrivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                trailingTrivia = trailingTrivia.RemoveAt(trailingTrivia.Count - 1);
            }

            for (var relocationIndex = 0; relocationIndex < previousTokenRelocation.Value; relocationIndex++)
            {
                trailingTrivia = trailingTrivia.Add(SyntaxFactory.EndOfLine(endOfLine));
            }

            replacements[previousTokenRelocation.Key] = currentPreviousToken.WithTrailingTrivia(trailingTrivia);
        }

        return replacements.Count == 0
                   ? root
                   : root.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Classifies and registers one off-position documentation comment relocation
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="trivia">Trivia candidate</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="owningRelocations">Relocations that split the owning token's leading trivia</param>
    /// <param name="previousTokenRelocationCounts">Line breaks to append to previous tokens</param>
    private static void RegisterRelocation(SyntaxNode root,
                                           SyntaxTrivia trivia,
                                           SourceText sourceText,
                                           IDictionary<SyntaxToken, List<SyntaxTrivia>> owningRelocations,
                                           IDictionary<SyntaxToken, int> previousTokenRelocationCounts)
    {
        if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) == false
            || IsAfterSourceOnSameLine(trivia, sourceText) == false)
        {
            return;
        }

        var owningToken = trivia.Token;

        // Relocating the comment above its owning token is what makes it document that token. When the owner
        // does not open a node there is nothing to document - the comment is stranded before a ';' or a closing
        // '}' - so moving it changes the author's layout without making the comment mean anything, and the
        // compiler reports CS1587 either way. Leave it where it was written (issues #591, #625).
        if (ReihitsuFormatterHelpers.DocumentsFollowingCode(owningToken) == false)
        {
            return;
        }

        var previousToken = GetPreviousPresentToken(owningToken);

        if (HasOwningLeadingSourceOnSameLine(trivia, sourceText)
            || previousToken.RawKind == 0
            || TokenLocator.ContainsToken(root, previousToken) == false)
        {
            AddOwningRelocation(owningRelocations, owningToken, trivia);

            return;
        }

        previousTokenRelocationCounts.TryGetValue(previousToken, out var relocationCount);
        previousTokenRelocationCounts[previousToken] = relocationCount + 1;
    }

    /// <summary>
    /// Locates the previous non-missing token
    /// </summary>
    /// <param name="owningToken">Token that owns the documentation trivia</param>
    /// <returns>The previous non-missing token, or the default token when none exists</returns>
    private static SyntaxToken GetPreviousPresentToken(SyntaxToken owningToken)
    {
        var previousToken = owningToken.GetPreviousToken(includeZeroWidth: true);

        while (previousToken.RawKind != 0 && previousToken.IsMissing)
        {
            previousToken = previousToken.GetPreviousToken(includeZeroWidth: true);
        }

        return previousToken;
    }

    /// <summary>
    /// Adds a documentation trivia item to the relocations for its owning token
    /// </summary>
    /// <param name="owningRelocations">Relocations grouped by owning token</param>
    /// <param name="owningToken">Owning token</param>
    /// <param name="trivia">Documentation trivia</param>
    private static void AddOwningRelocation(IDictionary<SyntaxToken, List<SyntaxTrivia>> owningRelocations,
                                            SyntaxToken owningToken,
                                            SyntaxTrivia trivia)
    {
        if (owningRelocations.TryGetValue(owningToken, out var documentationTrivia) == false)
        {
            documentationTrivia = [];
            owningRelocations[owningToken] = documentationTrivia;
        }

        documentationTrivia.Add(trivia);
    }

    /// <summary>
    /// Rebuilds the outermost normalization candidates of a documentation comment. A candidate nested
    /// inside another candidate is skipped, because rebuilding the outer element already replaces the
    /// text the inner one occupies and both replacements would overwrite each other
    /// </summary>
    /// <param name="commentText">Text of the documentation comment</param>
    /// <param name="commentSpan">Span the comment text occupies in <paramref name="sourceText"/></param>
    /// <param name="documentationComment">Structured documentation comment</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The rebuilt comment text, or <see langword="null"/> if no element required rebuilding</returns>
    private static string NormalizeOutermostElements(string commentText, TextSpan commentSpan, DocumentationCommentTriviaSyntax documentationComment, SourceText sourceText, CancellationToken cancellationToken)
    {
        var candidates = documentationComment.DescendantNodes()
                                             .OfType<XmlElementSyntax>()
                                             .Where(obj => DocCommentElementNormalizer.RequiresNormalization(obj, sourceText))
                                             .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        var candidateSet = new HashSet<XmlElementSyntax>(candidates);
        var topLevelCandidates = candidates.Where(obj => obj.Ancestors().OfType<XmlElementSyntax>().Any(candidateSet.Contains) == false)
                                           .OrderByDescending(obj => obj.Span.Start)
                                           .ToList();
        var normalizedCommentText = commentText;

        foreach (var element in topLevelCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var replacementText = DocCommentElementNormalizer.BuildReplacement(element, sourceText);
            var relativeStart = element.Span.Start - commentSpan.Start;
            var relativeEnd = element.Span.End - commentSpan.Start;

            normalizedCommentText = $"{normalizedCommentText.Substring(0, relativeStart)}{replacementText}{normalizedCommentText.Substring(relativeEnd)}";
        }

        return normalizedCommentText;
    }

    /// <summary>
    /// Attempts to read the rebuilt comment text back into a structured documentation comment, so the
    /// next round measures the elements against the text that was just written rather than the original
    /// </summary>
    /// <param name="commentText">Rebuilt comment text</param>
    /// <param name="commentTrivia">Parsed documentation comment trivia</param>
    /// <param name="documentationComment">Structured documentation comment</param>
    /// <returns><see langword="true"/> when the text parses back into a single documentation comment covering all of it</returns>
    private static bool TryReadBackDocumentationComment(string commentText, out SyntaxTrivia commentTrivia, out DocumentationCommentTriviaSyntax documentationComment)
    {
        var parsedTrivia = SyntaxFactory.ParseLeadingTrivia(commentText);

        if (parsedTrivia.Count > 0
            && parsedTrivia[0].IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
            && parsedTrivia[0].GetStructure() is DocumentationCommentTriviaSyntax structure
            && parsedTrivia[0].FullSpan == new TextSpan(0, commentText.Length))
        {
            commentTrivia = parsedTrivia[0];
            documentationComment = structure;

            return true;
        }

        commentTrivia = default;
        documentationComment = null;

        return false;
    }

    /// <summary>
    /// Rebuilds normalization candidates until none is left. Only the outermost candidates can be
    /// rebuilt in one round, so a <c>&lt;code&gt;</c> that needs alignment inside a <c>&lt;remarks&gt;</c>
    /// that needs alignment used to be reached by the next pipeline run instead, which broke the
    /// two-run convergence invariant (issue #434). A round that changes nothing ends the loop, so the
    /// cost is one extra parse per nesting level and none at all for the common single-candidate comment
    /// </summary>
    /// <param name="documentationCommentTrivia">Documentation comment trivia</param>
    /// <param name="documentationComment">Structured documentation comment</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The rebuilt comment text, or <see langword="null"/> if no element required rebuilding</returns>
    private static string NormalizeElementsUntilStable(SyntaxTrivia documentationCommentTrivia, DocumentationCommentTriviaSyntax documentationComment, SourceText sourceText, CancellationToken cancellationToken)
    {
        var currentComment = documentationComment;
        var currentSourceText = sourceText;
        var currentSpan = documentationCommentTrivia.FullSpan;
        var currentText = sourceText.ToString(currentSpan);
        string normalizedCommentText = null;

        for (var round = 0; round < MaximumNormalizationRounds; round++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var roundText = NormalizeOutermostElements(currentText, currentSpan, currentComment, currentSourceText, cancellationToken);

            if (roundText == null)
            {
                break;
            }

            normalizedCommentText = roundText;

            if (TryReadBackDocumentationComment(roundText, out var roundTrivia, out currentComment) == false)
            {
                break;
            }

            currentSourceText = SourceText.From(roundText);
            currentSpan = roundTrivia.FullSpan;
            currentText = roundText;
        }

        return normalizedCommentText;
    }

    /// <summary>
    /// Normalizes a documentation comment if any supported XML element requires it
    /// </summary>
    /// <param name="documentationCommentTrivia">Documentation comment trivia</param>
    /// <param name="documentationComment">Structured documentation comment</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The normalized comment text, or <see langword="null"/> if no change is required</returns>
    private static string NormalizeDocumentationComment(SyntaxTrivia documentationCommentTrivia, DocumentationCommentTriviaSyntax documentationComment, SourceText sourceText, CancellationToken cancellationToken)
    {
        var rebuiltCommentText = NormalizeElementsUntilStable(documentationCommentTrivia, documentationComment, sourceText, cancellationToken);
        var normalizedCommentText = rebuiltCommentText ?? sourceText.ToString(documentationCommentTrivia.FullSpan);
        var changed = rebuiltCommentText != null;

        // A deterministic single-pass scan, so how long this takes depends only on the length of the comment.
        // The pattern it replaced was linear too, but it carried a wall-clock match timeout, which made success
        // depend on process scheduling rather than on the input: ordinary CI jitter under coverage and parallel
        // test execution could abort a normalization that had no real work left to do
        var normalizedLinePrefixes = DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes(normalizedCommentText, cancellationToken);

        if (normalizedLinePrefixes != normalizedCommentText)
        {
            normalizedCommentText = normalizedLinePrefixes;
            changed = true;
        }

        return changed ? normalizedCommentText : null;
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <summary>
    /// Applies XML documentation comment formatting to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var sourceText = root.SyntaxTree?.GetText(cancellationToken) ?? SourceText.From(root.ToFullString());
        var relocatedRoot = RelocateOffPositionDocumentationComments(root, sourceText, context.EndOfLine, cancellationToken);

        if (ReferenceEquals(relocatedRoot, root) == false)
        {
            root = relocatedRoot;
            sourceText = SourceText.From(root.ToFullString());
        }

        var replacements = new Dictionary<SyntaxTrivia, SyntaxTrivia>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) == false
                || trivia.GetStructure() is not DocumentationCommentTriviaSyntax documentationComment)
            {
                continue;
            }

            var updatedCommentText = NormalizeDocumentationComment(trivia, documentationComment, sourceText, cancellationToken);

            if (updatedCommentText == null)
            {
                continue;
            }

            var leadingTrivia = SyntaxFactory.ParseLeadingTrivia(updatedCommentText);

            if (leadingTrivia.Count > 0)
            {
                replacements[trivia] = leadingTrivia[0];
            }
        }

        return replacements.Count == 0 ? root : root.ReplaceTrivia(replacements.Keys, (oldTrivia, _) => replacements[oldTrivia]);
    }

    #endregion // IFormattingPhase
}