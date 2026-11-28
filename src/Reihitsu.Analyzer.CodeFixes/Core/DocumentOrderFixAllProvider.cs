using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Fix All for code fixes whose guard depends on the code around the diagnostic, so that two fixes that are each safe
/// alone can together write the code the guard exists to prevent, and for fixes that rewrite or move code holding other
/// diagnostics. The fixes of a document are applied one after another in document order, and each fix is registered
/// again by its provider on the code the fixes in front of it produced, so its guard decides on that code, as applying
/// the fixes one after another does (SHARED-FIX-002). A diagnostic whose fix the provider withholds on that code, or on
/// the starting code, stays as it is. A diagnostic inside code that an earlier fix rewrote or moved is found again on the changed document, by
/// running the rule's analyzer on it, and fixed where that fix left it, until no diagnostic whose fix is offered remains.
/// In the solution-scoped mode, each fix keeps every document its fix changes, such as the other parts of a partial type
/// that a rename reaches, and the documents are fixed one after another on the solution the fixes before them produced
/// </summary>
internal sealed class DocumentOrderFixAllProvider : FixAllProvider
{
    #region Fields

    /// <summary>
    /// Analyzer that reports the diagnostics the code fix corrects
    /// </summary>
    private readonly DiagnosticAnalyzer _analyzer;

    /// <summary>
    /// Title of the Fix All action in the solution-scoped mode, or <see langword="null"/> in the document-scoped mode
    /// </summary>
    private readonly string _solutionScopedTitle;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="analyzer">Analyzer that reports the diagnostics the code fix corrects</param>
    /// <param name="solutionScopedTitle">Title of the Fix All action in the solution-scoped mode, or <see langword="null"/></param>
    private DocumentOrderFixAllProvider(DiagnosticAnalyzer analyzer, string solutionScopedTitle)
    {
        _analyzer = analyzer;
        _solutionScopedTitle = solutionScopedTitle;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Creates the Fix All provider for the code fix of an analyzer's diagnostics
    /// </summary>
    /// <param name="analyzer">Analyzer that reports the diagnostics the code fix corrects</param>
    /// <returns>The Fix All provider</returns>
    public static FixAllProvider Create(DiagnosticAnalyzer analyzer)
    {
        return FixAllProvider.Create(new DocumentOrderFixAllProvider(analyzer, null).ApplyFixAllAsync);
    }

    /// <summary>
    /// Creates the solution-scoped Fix All provider for a code fix that also changes other documents than its diagnostic's
    /// own: every change of a fix is kept, and each document is fixed on the solution the fixes before it produced
    /// </summary>
    /// <param name="analyzer">Analyzer that reports the diagnostics the code fix corrects</param>
    /// <param name="title">Title of the Fix All action</param>
    /// <returns>The Fix All provider</returns>
    public static FixAllProvider CreateSolutionScoped(DiagnosticAnalyzer analyzer, string title)
    {
        return new DocumentOrderFixAllProvider(analyzer, title);
    }

    /// <summary>
    /// Applies the fixes of the diagnostics in document order, each registered on the code the fixes in front of it
    /// produced
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Document</param>
    /// <param name="diagnostics">Diagnostics of the document</param>
    /// <param name="keepsSolution">Whether each fix keeps the changes of its whole solution (solution-scoped mode)</param>
    /// <returns>The changed document, whether a fix was applied, and whether a fix changed code that held a later diagnostic</returns>
    private static async Task<(Document Document, bool HasFixed, bool HasLostDiagnostic)> ApplyPassAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics, bool keepsSolution)
    {
        var cancellationToken = context.CancellationToken;
        var orderedDiagnostics = diagnostics.Where(static diagnostic => diagnostic.Location.IsInSource)
                                            .OrderBy(static diagnostic => diagnostic.Location.SourceSpan.Start)
                                            .ThenBy(static diagnostic => diagnostic.Location.SourceSpan.End)
                                            .ToList();
        var currentSpans = orderedDiagnostics.Select(static diagnostic => (TextSpan?)diagnostic.Location.SourceSpan).ToList();
        var currentDocument = document;
        var currentText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var hasFixed = false;
        var hasLostDiagnostic = false;

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
            // need not be written out again for every fix; the solution-scoped mode keeps the fix's whole solution instead
            currentText = currentText.WithChanges(changes);
            currentDocument = keepsSolution
                                  ? changedDocument
                                  : currentDocument.WithText(currentText);
            hasFixed = true;

            for (var laterIndex = diagnosticIndex + 1; laterIndex < currentSpans.Count; laterIndex++)
            {
                if (currentSpans[laterIndex] != null)
                {
                    currentSpans[laterIndex] = MapSpan(currentSpans[laterIndex].Value, changes);
                    hasLostDiagnostic |= currentSpans[laterIndex] == null;
                }
            }
        }

        return (currentDocument, hasFixed, hasLostDiagnostic);
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
        var firstAction = await GetFirstMatchingActionAsync(context, document, diagnostic).ConfigureAwait(false);

        if (firstAction == null)
        {
            return null;
        }

        var operations = await firstAction.GetOperationsAsync(context.CancellationToken).ConfigureAwait(false);

        return operations.OfType<ApplyChangesOperation>().FirstOrDefault()?.ChangedSolution.GetDocument(document.Id);
    }

    /// <summary>
    /// Registers the provider's fixes for one diagnostic and gets the first registered action that matches the
    /// equivalence key of the Fix All context, or any action when the context has no key
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Current document</param>
    /// <param name="diagnostic">Diagnostic, located in the current document</param>
    /// <returns>The first matching action, or <see langword="null"/> when the provider registers none</returns>
    private static async Task<CodeAction> GetFirstMatchingActionAsync(FixAllContext context, Document document, Diagnostic diagnostic)
    {
        var matchingActions = new List<CodeAction>();
        var fixContext = new CodeFixContext(document,
                                            diagnostic,
                                            (action, _) =>
                                            {
                                                if (context.CodeActionEquivalenceKey == null || action.EquivalenceKey == context.CodeActionEquivalenceKey)
                                                {
                                                    matchingActions.Add(action);
                                                }
                                            },
                                            context.CancellationToken);

        await context.CodeFixProvider.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);

        return matchingActions.FirstOrDefault();
    }

    /// <summary>
    /// Maps the span of a later diagnostic through the changes of a fix. A span that a change overlaps is no longer
    /// known; its diagnostic is found again by the analyzer in the next pass
    /// </summary>
    /// <param name="currentSpan">Span before the changes</param>
    /// <param name="changes">Changes, ordered by their start</param>
    /// <returns>The span after the changes, or <see langword="null"/></returns>
    private static TextSpan? MapSpan(TextSpan currentSpan, List<TextChange> changes)
    {
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

    /// <summary>
    /// Gets the diagnostics of the starting document whose fix the provider offers on that document. A diagnostic whose fix a
    /// guard withholds there is not among the fixes the user asked to apply, also when the fixes in front of it would release
    /// the guard (SHARED-FIX-002)
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Starting document</param>
    /// <param name="diagnostics">Diagnostics of the document</param>
    /// <returns>The diagnostics whose fix is offered</returns>
    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsWithOfferedFixAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics)
    {
        var offered = ImmutableArray.CreateBuilder<Diagnostic>(diagnostics.Length);

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Location.IsInSource == false
                || await GetFirstMatchingActionAsync(context, document, diagnostic).ConfigureAwait(false) != null)
            {
                offered.Add(diagnostic);
            }
        }

        return offered.ToImmutable();
    }

    /// <summary>
    /// Applies the fixes of the diagnostics of one document in document order. A pass applies the fixes of the given
    /// diagnostics, starting with those whose fix the provider offers on the starting document; when a fix in it changed code
    /// that held a later diagnostic, that diagnostic's place is no longer known, so the analyzer runs on the changed document
    /// and another pass applies the fixes of what it reports. The
    /// work ends after a pass that applies no fix or loses no diagnostic. Another pass runs only after a pass that fixed
    /// a diagnostic, so their number is bounded by the number of diagnostics in the starting document, which also stops
    /// a fix that would keep reporting new diagnostics
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Document</param>
    /// <param name="diagnostics">Diagnostics of the document</param>
    /// <returns>The document with every fix applied that its provider offers on the code the earlier fixes produced</returns>
    private async Task<Document> ApplyFixAllAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics)
    {
        var currentDocument = document;
        var currentDiagnostics = await GetDiagnosticsWithOfferedFixAsync(context, document, diagnostics).ConfigureAwait(false);
        var maximumPasses = diagnostics.Length;

        for (var pass = 0; pass < maximumPasses; pass++)
        {
            var result = await ApplyPassAsync(context, currentDocument, currentDiagnostics, _solutionScopedTitle != null).ConfigureAwait(false);

            currentDocument = result.Document;

            if (result.HasFixed == false
                || result.HasLostDiagnostic == false)
            {
                break;
            }

            currentDiagnostics = await GetDiagnosticsAsync(context, currentDocument).ConfigureAwait(false);
        }

        return currentDocument;
    }

    /// <summary>
    /// Gets the diagnostics the analyzer reports in the document for the diagnostic IDs being fixed
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Document</param>
    /// <returns>The diagnostics</returns>
    private async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(FixAllContext context, Document document)
    {
        var cancellationToken = context.CancellationToken;
        var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

        if (compilation == null
            || syntaxTree == null)
        {
            return ImmutableArray<Diagnostic>.Empty;
        }

        var compilationWithAnalyzers = compilation.WithAnalyzers([_analyzer], new CompilationWithAnalyzersOptions(document.Project.AnalyzerOptions, null, false, false));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var syntaxDiagnostics = await compilationWithAnalyzers.GetAnalyzerSyntaxDiagnosticsAsync(syntaxTree, cancellationToken).ConfigureAwait(false);
        var semanticDiagnostics = await compilationWithAnalyzers.GetAnalyzerSemanticDiagnosticsAsync(semanticModel, null, cancellationToken).ConfigureAwait(false);

        return syntaxDiagnostics.Concat(semanticDiagnostics)
                                .Where(diagnostic => context.DiagnosticIds.Contains(diagnostic.Id))
                                .ToImmutableArray();
    }

    /// <summary>
    /// Applies the fixes of every document of the Fix All scope in the solution-scoped mode, one document after another,
    /// each on the solution the fixes of the documents before it produced. A document that an earlier fix changed is
    /// analyzed again, so its diagnostics are located on its current code
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <returns>The solution with every fix applied</returns>
    private async Task<Solution> ApplySolutionFixAllAsync(FixAllContext context)
    {
        var documents = context.Scope switch
                        {
                            FixAllScope.Document when context.Document != null => [context.Document],
                            FixAllScope.Project when context.Project != null => context.Project.Documents.ToList(),
                            FixAllScope.Solution => context.Solution.Projects.SelectMany(static project => project.Documents).ToList(),
                            _ => new List<Document>()
                        };
        var solution = context.Solution;

        foreach (var startingDocument in documents)
        {
            var diagnostics = await context.GetDocumentDiagnosticsAsync(startingDocument).ConfigureAwait(false);
            var currentDocument = solution.GetDocument(startingDocument.Id);

            if (diagnostics.IsEmpty
                || currentDocument == null)
            {
                continue;
            }

            var startingText = await startingDocument.GetTextAsync(context.CancellationToken).ConfigureAwait(false);
            var currentText = await currentDocument.GetTextAsync(context.CancellationToken).ConfigureAwait(false);

            if (currentText.ContentEquals(startingText) == false)
            {
                diagnostics = await GetDiagnosticsAsync(context, currentDocument).ConfigureAwait(false);
            }

            solution = (await ApplyFixAllAsync(context, currentDocument, diagnostics).ConfigureAwait(false)).Project.Solution;
        }

        return solution;
    }

    #endregion // Methods

    #region FixAllProvider

    /// <inheritdoc/>
    /// <remarks>Reached only in the solution-scoped mode; <see cref="Create"/> returns a document-based provider</remarks>
    public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
    {
        return Task.FromResult(CodeAction.Create(_solutionScopedTitle,
                                                 cancellationToken => ApplySolutionFixAllAsync(fixAllContext.WithCancellationToken(cancellationToken)),
                                                 fixAllContext.CodeActionEquivalenceKey));
    }

    #endregion // FixAllProvider
}