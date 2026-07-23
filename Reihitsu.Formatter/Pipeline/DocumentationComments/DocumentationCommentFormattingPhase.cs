using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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

        // The match timeout is a safety net against pathological input, not a performance budget. These prefix
        // patterns are anchored on /// and cannot backtrack catastrophically, so the value is deliberately generous:
        // a tight wall-clock timeout is tripped by ordinary CI scheduling jitter (a GC pause or a thread preemption
        // during one of the many calls the self-hosting tests make), not by real regex work
        var normalizedWhitespaceOnlyPrefixes = Regex.Replace(normalizedCommentText,
                                                             @"(?:\A|(?<=\r\n)|(?<=[\r\n\u0085\u2028\u2029]))(?<indent>[^\S\r\n\u0085\u2028\u2029]*)(?<prefix>///)(?:[^\S\r\n\u0085\u2028\u2029]{2,}|[^\S \r\n\u0085\u2028\u2029])(?=\r\n|\r|\n|\u0085|\u2028|\u2029|$)",
                                                             "${indent}${prefix}",
                                                             RegexOptions.None,
                                                             TimeSpan.FromSeconds(2));
        var normalizedLinePrefixes = Regex.Replace(normalizedWhitespaceOnlyPrefixes,
                                                   @"(?:\A|(?<=\r\n)|(?<=[\r\n\u0085\u2028\u2029]))(?<indent>[^\S\r\n\u0085\u2028\u2029]*)(?<prefix>///)[^\S\r\n\u0085\u2028\u2029]*(?=\S)",
                                                   "${indent}${prefix} ",
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
        var sourceText = root.SyntaxTree.GetText(cancellationToken);
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