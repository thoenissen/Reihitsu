using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.DocumentationComments;

/// <summary>
/// Normalizes XML documentation comments to repository-specific layout rules. Element-level layout
/// decisions live in <see cref="DocCommentElementNormalizer"/>; this phase locates the candidate
/// elements, splices in their normalized text and fixes the <c>///</c> line prefixes
/// </summary>
internal sealed class DocumentationCommentFormattingPhase : IFormattingPhase
{
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
    /// Inserts a line break before documentation trivia in its owning token's leading trivia
    /// </summary>
    /// <param name="token">Owning token</param>
    /// <param name="documentationCommentTrivia">Documentation comment trivia</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The token with the documentation comment moved onto a line of its own</returns>
    private static SyntaxToken MoveBeforeOwningToken(SyntaxToken token, SyntaxTrivia documentationCommentTrivia, string endOfLine)
    {
        var leadingTrivia = token.LeadingTrivia;
        var documentationIndex = leadingTrivia.IndexOf(documentationCommentTrivia);

        if (documentationIndex < 0)
        {
            return token;
        }

        if (documentationIndex > 0 && leadingTrivia[documentationIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            leadingTrivia = leadingTrivia.RemoveAt(documentationIndex - 1);
            documentationIndex--;
        }

        leadingTrivia = leadingTrivia.Insert(documentationIndex, SyntaxFactory.EndOfLine(endOfLine));

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
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) == false
                || IsAfterSourceOnSameLine(trivia, sourceText) == false)
            {
                continue;
            }

            var previousToken = trivia.Token.GetPreviousToken(includeZeroWidth: true);
            var previousTokenIsInScope = previousToken.RawKind != 0
                                         && previousToken.SpanStart >= root.SpanStart
                                         && previousToken.Span.End <= root.Span.End;

            if (previousTokenIsInScope == false)
            {
                var owningToken = trivia.Token;
                var currentOwningToken = replacements.TryGetValue(owningToken, out var owningReplacement) ? owningReplacement : owningToken;

                replacements[owningToken] = MoveBeforeOwningToken(currentOwningToken, trivia, endOfLine);

                continue;
            }

            var currentToken = replacements.TryGetValue(previousToken, out var replacement) ? replacement : previousToken;
            var trailingTrivia = currentToken.TrailingTrivia;

            if (trailingTrivia.Count > 0 && trailingTrivia[trailingTrivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                trailingTrivia = trailingTrivia.RemoveAt(trailingTrivia.Count - 1);
            }

            trailingTrivia = trailingTrivia.Add(SyntaxFactory.EndOfLine(endOfLine));
            replacements[previousToken] = currentToken.WithTrailingTrivia(trailingTrivia);
        }

        return replacements.Count == 0
                   ? root
                   : root.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
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
        var candidates = documentationComment.DescendantNodes()
                                             .OfType<XmlElementSyntax>()
                                             .Where(obj => DocCommentElementNormalizer.RequiresNormalization(obj, sourceText))
                                             .ToList();
        var normalizedCommentText = sourceText.ToString(documentationCommentTrivia.FullSpan);
        var changed = false;

        if (candidates.Count > 0)
        {
            var candidateSet = new HashSet<XmlElementSyntax>(candidates);
            var topLevelCandidates = candidates.Where(obj => obj.Ancestors().OfType<XmlElementSyntax>().Any(candidateSet.Contains) == false)
                                               .OrderByDescending(obj => obj.Span.Start)
                                               .ToList();

            foreach (var element in topLevelCandidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var replacementText = DocCommentElementNormalizer.BuildReplacement(element, sourceText);
                var relativeStart = element.Span.Start - documentationCommentTrivia.FullSpan.Start;
                var relativeEnd = element.Span.End - documentationCommentTrivia.FullSpan.Start;

                normalizedCommentText = $"{normalizedCommentText.Substring(0, relativeStart)}{replacementText}{normalizedCommentText.Substring(relativeEnd)}";
                changed = true;
            }
        }

        // The match timeout is a safety net against pathological input, not a performance budget. This prefix
        // pattern is anchored on /// and cannot backtrack catastrophically, so the value is deliberately generous:
        // a tight wall-clock timeout is tripped by ordinary CI scheduling jitter (a GC pause or a thread preemption
        // during one of the many calls the self-hosting tests make), not by real regex work
        var normalizedLinePrefixes = Regex.Replace(normalizedCommentText,
                                                   @"(?:\A|(?<=\r\n)|(?<=[\r\n\u0085\u2028\u2029]))(?<indent>[^\S\r\n\u0085\u2028\u2029]*)(?<prefix>///)(?<suffix>[^\r\n\u0085\u2028\u2029]*)(?=\r\n|\r|\n|\u0085|\u2028|\u2029|$)",
                                                   obj => $"{obj.Groups["indent"].Value}{obj.Groups["prefix"].Value}{DocumentationCommentUtilities.NormalizeExteriorSuffix(obj.Groups["suffix"].Value)}",
                                                   RegexOptions.None,
                                                   TimeSpan.FromSeconds(2));

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