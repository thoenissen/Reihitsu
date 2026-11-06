using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Fix All for code fixes whose guard depends on the code around the diagnostic, so that two fixes that are each safe
/// alone can together write the code the guard exists to prevent. The fixes of a document are applied one after another
/// in document order, and each fix is registered again by its provider on the code the fixes in front of it produced,
/// so its guard decides on that code, as applying the fixes one after another does (SHARED-FIX-002). A diagnostic whose
/// fix the provider withholds on that code stays as it is
/// </summary>
internal static class DocumentOrderFixAllProvider
{
    #region Properties

    /// <summary>
    /// The Fix All provider
    /// </summary>
    public static FixAllProvider Instance { get; } = FixAllProvider.Create(ApplyFixAllAsync);

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Applies the fixes of the diagnostics of one document in document order
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Document</param>
    /// <param name="diagnostics">Diagnostics of the document</param>
    /// <returns>The document with every fix applied that its provider offers on the code the earlier fixes produced</returns>
    private static async Task<Document> ApplyFixAllAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics)
    {
        var cancellationToken = context.CancellationToken;
        var orderedDiagnostics = diagnostics.Where(static diagnostic => diagnostic.Location.IsInSource)
                                            .OrderBy(static diagnostic => diagnostic.Location.SourceSpan.Start)
                                            .ThenBy(static diagnostic => diagnostic.Location.SourceSpan.End)
                                            .ToList();
        var currentSpans = orderedDiagnostics.Select(static diagnostic => (TextSpan?)diagnostic.Location.SourceSpan).ToList();
        var currentDocument = document;
        var currentText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        for (var diagnosticIndex = 0; diagnosticIndex < orderedDiagnostics.Count; diagnosticIndex++)
        {
            if (currentSpans[diagnosticIndex] is not { } currentSpan
                || await currentDocument.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false) is not { } currentTree)
            {
                continue;
            }

            var diagnostic = orderedDiagnostics[diagnosticIndex];
            var changedDocument = await GetFixedDocumentAsync(context, currentDocument, Diagnostic.Create(diagnostic.Descriptor, Location.Create(currentTree, currentSpan), diagnostic.Properties)).ConfigureAwait(false);

            if (changedDocument == null)
            {
                continue;
            }

            var changes = (await changedDocument.GetTextChangesAsync(currentDocument, cancellationToken).ConfigureAwait(false)).OrderBy(static change => change.Span.Start)
                                                                                                                               .ToList();

            if (changes.Count == 0)
            {
                continue;
            }

            // The next document is built from the changed text, so that its tree is parsed incrementally and its text
            // need not be written out again for every fix
            currentText = currentText.WithChanges(changes);
            currentDocument = currentDocument.WithText(currentText);

            for (var laterIndex = diagnosticIndex + 1; laterIndex < currentSpans.Count; laterIndex++)
            {
                currentSpans[laterIndex] = MapSpan(currentSpans[laterIndex], changes);
            }
        }

        return currentDocument;
    }

    /// <summary>
    /// Gets the document the provider's fix for one diagnostic produces, when the provider offers the fix on that
    /// document
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Current document</param>
    /// <param name="diagnostic">Diagnostic, located in the current document</param>
    /// <returns>The fixed document, or <see langword="null"/> when no fix is offered</returns>
    private static async Task<Document> GetFixedDocumentAsync(FixAllContext context, Document document, Diagnostic diagnostic)
    {
        CodeAction codeAction = null;
        var fixContext = new CodeFixContext(document,
                                            diagnostic,
                                            (action, _) =>
                                            {
                                                if (codeAction == null
                                                    && (context.CodeActionEquivalenceKey == null || action.EquivalenceKey == context.CodeActionEquivalenceKey))
                                                {
                                                    codeAction = action;
                                                }
                                            },
                                            context.CancellationToken);

        await context.CodeFixProvider.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);

        if (codeAction == null)
        {
            return null;
        }

        var operations = await codeAction.GetOperationsAsync(context.CancellationToken).ConfigureAwait(false);

        return operations.OfType<ApplyChangesOperation>().FirstOrDefault()?.ChangedSolution.GetDocument(document.Id);
    }

    /// <summary>
    /// Maps the span of a later diagnostic through the changes of a fix. A span that a change overlaps is no longer
    /// known, and its diagnostic is left as it is
    /// </summary>
    /// <param name="span">Span before the changes</param>
    /// <param name="changes">Changes, ordered by their start</param>
    /// <returns>The span after the changes, or <see langword="null"/></returns>
    private static TextSpan? MapSpan(TextSpan? span, List<TextChange> changes)
    {
        if (span is not { } currentSpan)
        {
            return null;
        }

        var offset = 0;

        foreach (var change in changes)
        {
            if (change.Span.End <= currentSpan.Start)
            {
                offset += change.NewText.Length - change.Span.Length;

                continue;
            }

            if (change.Span.Start >= currentSpan.End)
            {
                break;
            }

            return null;
        }

        return new TextSpan(currentSpan.Start + offset, currentSpan.Length);
    }

    #endregion // Methods
}