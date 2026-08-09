using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Tooling.Enumerations;

namespace Reihitsu.Tooling;

/// <summary>
/// Runs the code-fix surface over a single fixture under a single line ending
/// </summary>
public static class FixtureRunner
{
    #region Methods

    /// <summary>
    /// Analyzes the fixture and applies the resolved code fix until the diagnostic is no longer reported or the
    /// iteration cap is reached. Convergence is decided by the diagnostic disappearing, not by two consecutive
    /// passes producing the same text: a fixture carrying several occurrences legitimately changes on every
    /// iteration while still converging
    /// </summary>
    /// <param name="target">Analyzer and code fix provider the diagnostic ID resolved to</param>
    /// <param name="source">Fixture source</param>
    /// <param name="lineEnding">Line ending the fixture is normalized to before it is analyzed</param>
    /// <param name="maximumIterations">Maximum number of code actions applied before the run is abandoned</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The observations of this fixture and line-ending arm</returns>
    public static async Task<FixtureRunResult> RunAsync(CodeFixTarget target,
                                                        string source,
                                                        string lineEnding,
                                                        int maximumIterations,
                                                        CancellationToken cancellationToken = default)
    {
        var normalized = LineEndingUtilities.Normalize(source, lineEnding);

        if (HasSyntaxErrors(normalized, cancellationToken))
        {
            return Create(FixtureOutcome.ParseError, 0, 0, normalized, normalized, lineEnding);
        }

        var current = normalized;
        var iterations = 0;
        var registeredActions = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var workspace = new AdhocWorkspace())
            {
                var (document, documentId) = CreateDocument(workspace, current);

                var diagnostics = await AnalyzeAsync(document, target, cancellationToken).ConfigureAwait(false);

                if (diagnostics.IsEmpty)
                {
                    return Create(iterations == 0 ? FixtureOutcome.NoDiagnostic : FixtureOutcome.Fixed,
                                  iterations,
                                  registeredActions,
                                  normalized,
                                  current,
                                  lineEnding);
                }

                if (iterations >= maximumIterations)
                {
                    return Create(FixtureOutcome.NotConverged, iterations, registeredActions, normalized, current, lineEnding);
                }

                var actions = await RegisterActionsAsync(document, target, diagnostics[0], cancellationToken).ConfigureAwait(false);

                if (actions.Count == 0)
                {
                    return Create(FixtureOutcome.NoFixOffered, iterations, registeredActions, normalized, current, lineEnding);
                }

                if (iterations == 0)
                {
                    registeredActions = actions.Count;
                }

                var applied = await ApplyAsync(actions[0], documentId, cancellationToken).ConfigureAwait(false);

                iterations++;

                if (string.Equals(applied, current, StringComparison.Ordinal))
                {
                    // The action produced no textual change, so another iteration would repeat it forever
                    return Create(FixtureOutcome.NotConverged, iterations, registeredActions, normalized, current, lineEnding);
                }

                current = applied;
            }

            if (HasSyntaxErrors(current, cancellationToken))
            {
                return Create(FixtureOutcome.InvalidResult, iterations, registeredActions, normalized, current, lineEnding);
            }
        }
    }

    /// <summary>
    /// Determines whether the provided source contains syntax errors
    /// </summary>
    /// <param name="source">Source to parse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the source does not parse cleanly; otherwise, <see langword="false"/></returns>
    public static bool HasSyntaxErrors(string source, CancellationToken cancellationToken = default)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source,
                                                    new CSharpParseOptions(LanguageVersion.Latest),
                                                    cancellationToken: cancellationToken);

        return syntaxTree.GetDiagnostics(cancellationToken).Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    /// <summary>
    /// Builds the result of a fixture run and records whether the line ending survived the fix
    /// </summary>
    /// <param name="outcome">How the fixture ended</param>
    /// <param name="iterations">Number of times a code action was applied</param>
    /// <param name="registeredActions">Number of code actions registered for the first fixed diagnostic</param>
    /// <param name="originalSource">Source the fixture was analyzed from</param>
    /// <param name="finalSource">Source after the last applied code action</param>
    /// <param name="lineEnding">Line ending the arm requested</param>
    /// <returns>The fixture run result</returns>
    private static FixtureRunResult Create(FixtureOutcome outcome,
                                           int iterations,
                                           int registeredActions,
                                           string originalSource,
                                           string finalSource,
                                           string lineEnding)
    {
        return new FixtureRunResult(outcome,
                                    iterations,
                                    registeredActions,
                                    originalSource,
                                    finalSource,
                                    LineEndingUtilities.UsesOnly(finalSource, lineEnding));
    }

    /// <summary>
    /// Runs the target's analyzers over the document and returns the diagnostics carrying the resolved ID
    /// </summary>
    /// <param name="document">Document to analyze</param>
    /// <param name="target">Analyzer and code fix provider the diagnostic ID resolved to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The reported diagnostics, in source order</returns>
    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(Document document, CodeFixTarget target, CancellationToken cancellationToken)
    {
        var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false)
                              ?? throw new InvalidOperationException("Failed to compile the fixture document.");
        var diagnostics = await compilation.WithAnalyzers(target.Analyzers)
                                           .GetAnalyzerDiagnosticsAsync(cancellationToken)
                                           .ConfigureAwait(false);

        return diagnostics.Where(diagnostic => diagnostic.Id == target.DiagnosticId)
                          .OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start)
                          .ToImmutableArray();
    }

    /// <summary>
    /// Collects the code actions the provider registers for the provided diagnostic
    /// </summary>
    /// <param name="document">Document the diagnostic was reported on</param>
    /// <param name="target">Analyzer and code fix provider the diagnostic ID resolved to</param>
    /// <param name="diagnostic">Diagnostic to offer a fix for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The registered code actions</returns>
    private static async Task<List<CodeAction>> RegisterActionsAsync(Document document,
                                                                     CodeFixTarget target,
                                                                     Diagnostic diagnostic,
                                                                     CancellationToken cancellationToken)
    {
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), cancellationToken);

        await target.CodeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

        return actions;
    }

    /// <summary>
    /// Applies a code action and returns the resulting document text
    /// </summary>
    /// <param name="action">Code action to apply</param>
    /// <param name="documentId">Identifier of the document the action changes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The source text after the action was applied</returns>
    private static async Task<string> ApplyAsync(CodeAction action, DocumentId documentId, CancellationToken cancellationToken)
    {
        var operations = await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
        var applyChanges = operations.OfType<ApplyChangesOperation>().FirstOrDefault()
                               ?? throw new InvalidOperationException("The code action registered no document change.");
        var changedDocument = applyChanges.ChangedSolution.GetDocument(documentId)
                                  ?? throw new InvalidOperationException("Failed to resolve the changed fixture document.");
        var text = await changedDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return text.ToString();
    }

    /// <summary>
    /// Creates an ad-hoc document for the fixture source. References come from the running host rather than from
    /// a package restore, so the runner needs no network access
    /// </summary>
    /// <param name="workspace">Workspace hosting the document</param>
    /// <param name="source">Fixture source</param>
    /// <returns>The created document and its identifier</returns>
    private static (Document Document, DocumentId DocumentId) CreateDocument(AdhocWorkspace workspace, string source)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var references = (trustedPlatformAssemblies?.Split(Path.PathSeparator) ?? []).Where(path => string.IsNullOrEmpty(path) == false)
                                                                                     .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path));
        var solution = workspace.CurrentSolution
                                .AddProject(ProjectInfo.Create(projectId,
                                                               VersionStamp.Create(),
                                                               "FixtureProject",
                                                               "FixtureProject",
                                                               LanguageNames.CSharp,
                                                               parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
                                                               metadataReferences: references))
                                .AddDocument(documentId, "Fixture.cs", SourceText.From(source));
        var document = solution.GetDocument(documentId)
                           ?? throw new InvalidOperationException("Failed to create the fixture document.");

        return (document, documentId);
    }

    #endregion // Methods
}