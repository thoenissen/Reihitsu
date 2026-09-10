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
    #region Constants

    /// <summary>
    /// The diagnostic ID Roslyn reports when an analyzer throws instead of completing
    /// </summary>
    private const string AnalyzerFailureDiagnosticId = "AD0001";

    /// <summary>
    /// Global analyzer-config content that disables Roslyn's own generated-code heuristic. Every fixture is now
    /// named after its real on-disk path (see <see cref="CreateDocument"/>), and without this override a fixture
    /// whose name happens to match a generated-file pattern (<c>*.g.cs</c>, <c>*.designer.cs</c>, …) would be
    /// silently skipped by every rule that opts out of generated-code analysis — exactly the "silently skipped
    /// fixture is indistinguishable from a fixture that reports nothing" failure this runner exists to avoid
    /// </summary>
    private const string DisableGeneratedCodeHeuristicConfig = "is_global = true\ngenerated_code = false\n";

    /// <summary>
    /// Placeholder path for the analyzer-config document. Roslyn requires an absolute path to parse an
    /// analyzer-config document, but a global config applies to every tree in the project regardless of location,
    /// so the path itself carries no meaning beyond satisfying that requirement
    /// </summary>
    private const string AnalyzerConfigPath = "/fixture-project/.globalconfig";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Analyzes the fixture and applies the resolved code fix until the diagnostic is no longer reported or the
    /// iteration cap is reached. Convergence is decided by the diagnostic disappearing, not by two consecutive
    /// passes producing the same text: a fixture carrying several occurrences legitimately changes on every
    /// iteration while still converging. A fix that replaces the analyzed document's identity (for example a
    /// rename) is followed into the next iteration under its new path, and counts as progress on its own even
    /// when the text is unchanged, so a rename-only fix converges instead of being mistaken for an ineffective one
    /// </summary>
    /// <param name="target">Analyzer and code fix provider the diagnostic ID resolved to</param>
    /// <param name="source">Fixture source</param>
    /// <param name="lineEnding">Line ending the fixture is normalized to before it is analyzed</param>
    /// <param name="maximumIterations">Maximum number of code actions applied before the run is abandoned</param>
    /// <param name="fixturePath">
    /// The fixture's on-disk relative path, forward-slash separated. Used both as the
    /// document identity the first iteration analyzes and to identify the fixture in tooling-failure messages
    /// </param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The observations of this fixture and line-ending arm</returns>
    public static async Task<FixtureRunResult> RunAsync(CodeFixTarget target,
                                                        string source,
                                                        string lineEnding,
                                                        int maximumIterations,
                                                        string fixturePath,
                                                        CancellationToken cancellationToken = default)
    {
        var normalized = FixtureLineEndings.Normalize(source, lineEnding);

        if (HasSyntaxErrors(normalized, cancellationToken))
        {
            return Create(FixtureOutcome.ParseError, 0, 0, normalized, normalized, lineEnding, fixturePath);
        }

        var state = (Current: normalized, DocumentPath: fixturePath, Iterations: 0, RegisteredActions: 0);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var iteration = await RunIterationAsync(target,
                                                    normalized,
                                                    lineEnding,
                                                    maximumIterations,
                                                    fixturePath,
                                                    state,
                                                    cancellationToken).ConfigureAwait(false);

            if (iteration.TerminalResult != null)
            {
                return iteration.TerminalResult;
            }

            state.RegisteredActions = iteration.RegisteredActions;
            state.Iterations++;

            var madeProgress = string.Equals(iteration.AppliedSource, state.Current, StringComparison.Ordinal) == false
                               || string.Equals(iteration.AppliedDocumentPath, state.DocumentPath, StringComparison.Ordinal) == false;

            if (madeProgress == false)
            {
                // The action produced no textual or document-identity change, so another iteration would repeat
                // it forever. This is reported apart from the cap: an ineffective fix is a defect in the rule, a
                // cap is not.
                return Create(FixtureOutcome.NoProgress,
                              state.Iterations,
                              state.RegisteredActions,
                              normalized,
                              state.Current,
                              lineEnding,
                              state.DocumentPath);
            }

            state.Current = iteration.AppliedSource;
            state.DocumentPath = iteration.AppliedDocumentPath;

            if (HasSyntaxErrors(state.Current, cancellationToken))
            {
                return Create(FixtureOutcome.InvalidResult,
                              state.Iterations,
                              state.RegisteredActions,
                              normalized,
                              state.Current,
                              lineEnding,
                              state.DocumentPath);
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
    /// Analyzes one iteration and either returns a terminal result or the source and document path produced by
    /// the first code action
    /// </summary>
    /// <param name="target">Resolved analyzer and code fix target</param>
    /// <param name="normalized">Original normalized fixture source</param>
    /// <param name="lineEnding">Requested fixture line ending</param>
    /// <param name="maximumIterations">Maximum number of code actions</param>
    /// <param name="fixturePath">
    /// The fixture's on-disk relative path, used to identify the fixture in
    /// tooling-failure messages
    /// </param>
    /// <param name="state">Current source, document path, iteration count, and first action count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A terminal result or the source, document path, and action count for the next iteration</returns>
    private static async Task<(FixtureRunResult TerminalResult, string AppliedSource, string AppliedDocumentPath, int RegisteredActions)> RunIterationAsync(CodeFixTarget target,
                                                                                                                                                            string normalized,
                                                                                                                                                            string lineEnding,
                                                                                                                                                            int maximumIterations,
                                                                                                                                                            string fixturePath,
                                                                                                                                                            (string Current, string DocumentPath, int Iterations, int RegisteredActions) state,
                                                                                                                                                            CancellationToken cancellationToken)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var (document, documentId, projectId) = CreateDocument(workspace, state.Current, state.DocumentPath);

            var reported = await AnalyzeAsync(document, target, cancellationToken).ConfigureAwait(false);

            if (reported.Any(diagnostic => diagnostic.Id == AnalyzerFailureDiagnosticId))
            {
                return (Create(FixtureOutcome.AnalyzerFailure,
                               state.Iterations,
                               state.RegisteredActions,
                               normalized,
                               state.Current,
                               lineEnding,
                               state.DocumentPath),
                        null,
                        null,
                        state.RegisteredActions);
            }

            var diagnostics = reported.Where(diagnostic => diagnostic.Id == target.DiagnosticId)
                                      .OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start)
                                      .ToImmutableArray();
            var terminalResult = GetDiagnosticTerminalResult(diagnostics,
                                                             normalized,
                                                             lineEnding,
                                                             maximumIterations,
                                                             state);

            if (terminalResult != null)
            {
                return (terminalResult, null, null, state.RegisteredActions);
            }

            var actions = await RegisterActionsAsync(document, target, diagnostics[0], cancellationToken).ConfigureAwait(false);

            if (actions.Count == 0)
            {
                return (Create(FixtureOutcome.NoFixOffered,
                               state.Iterations,
                               state.RegisteredActions,
                               normalized,
                               state.Current,
                               lineEnding,
                               state.DocumentPath),
                        null,
                        null,
                        state.RegisteredActions);
            }

            var registeredActions = state.Iterations == 0 ? actions.Count : state.RegisteredActions;
            var applied = await ApplyAsync(actions[0], documentId, projectId, fixturePath, lineEnding, state.DocumentPath, cancellationToken).ConfigureAwait(false);

            return (null, applied.Source, applied.DocumentPath, registeredActions);
        }
    }

    /// <summary>
    /// Resolves terminal states determined solely by the reported diagnostics and iteration count
    /// </summary>
    /// <param name="diagnostics">Target diagnostics reported for the current source</param>
    /// <param name="normalized">Original normalized fixture source</param>
    /// <param name="lineEnding">Requested fixture line ending</param>
    /// <param name="maximumIterations">Maximum number of code actions</param>
    /// <param name="state">Current source, document path, iteration count, and first action count</param>
    /// <returns>The terminal result, or <see langword="null"/> when a code action may be applied</returns>
    private static FixtureRunResult GetDiagnosticTerminalResult(ImmutableArray<Diagnostic> diagnostics,
                                                                string normalized,
                                                                string lineEnding,
                                                                int maximumIterations,
                                                                (string Current, string DocumentPath, int Iterations, int RegisteredActions) state)
    {
        if (diagnostics.IsEmpty)
        {
            return Create(state.Iterations == 0 ? FixtureOutcome.NoDiagnostic : FixtureOutcome.Fixed,
                          state.Iterations,
                          state.RegisteredActions,
                          normalized,
                          state.Current,
                          lineEnding,
                          state.DocumentPath);
        }

        return state.Iterations >= maximumIterations
                   ? Create(FixtureOutcome.NotConverged,
                            state.Iterations,
                            state.RegisteredActions,
                            normalized,
                            state.Current,
                            lineEnding,
                            state.DocumentPath)
                   : null;
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
    /// <param name="finalDocumentPath">The document path the fixture was last analyzed under</param>
    /// <returns>The fixture run result</returns>
    private static FixtureRunResult Create(FixtureOutcome outcome,
                                           int iterations,
                                           int registeredActions,
                                           string originalSource,
                                           string finalSource,
                                           string lineEnding,
                                           string finalDocumentPath)
    {
        return new FixtureRunResult(outcome,
                                    iterations,
                                    registeredActions,
                                    originalSource,
                                    finalSource,
                                    FixtureLineEndings.UsesOnly(finalSource, lineEnding),
                                    finalDocumentPath);
    }

    /// <summary>
    /// Runs the target's analyzers over the document and returns every diagnostic they produced, unfiltered.
    /// Filtering to the resolved ID is the caller's job, because Roslyn reports an analyzer that threw as an
    /// AD0001 diagnostic — dropping it here would make a crashed analyzer look like a fixture that reports nothing
    /// </summary>
    /// <param name="document">Document to analyze</param>
    /// <param name="target">Analyzer and code fix provider the diagnostic ID resolved to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Every diagnostic the analyzers reported</returns>
    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(Document document, CodeFixTarget target, CancellationToken cancellationToken)
    {
        var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false)
                              ?? throw new InvalidOperationException("Failed to compile the fixture document.");
        var diagnostics = await compilation.WithAnalyzers(target.Analyzers, document.Project.AnalyzerOptions)
                                           .GetAnalyzerDiagnosticsAsync(cancellationToken)
                                           .ConfigureAwait(false);

        return diagnostics;
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
    /// Applies a code action and returns the resulting document text and path. A code fix that replaces the
    /// document's identity (for example a rename) removes the original <see cref="DocumentId"/> rather than
    /// changing its text, so the fixture project's remaining document is resolved as a fallback instead of
    /// treating that shape as a tooling failure
    /// </summary>
    /// <param name="action">Code action to apply</param>
    /// <param name="documentId">Identifier of the document the action changes</param>
    /// <param name="projectId">
    /// Identifier of the fixture project, used to find a replacement document when the
    /// original identifier no longer resolves
    /// </param>
    /// <param name="fixturePath">
    /// The fixture's on-disk relative path, named in the failure message when the
    /// applied solution carries no single resolvable fixture document
    /// </param>
    /// <param name="lineEnding">Requested fixture line ending, named in the failure message</param>
    /// <param name="currentDocumentPath">The document path the action was applied to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The source text and document path after the action was applied</returns>
    private static async Task<(string Source, string DocumentPath)> ApplyAsync(CodeAction action,
                                                                               DocumentId documentId,
                                                                               ProjectId projectId,
                                                                               string fixturePath,
                                                                               string lineEnding,
                                                                               string currentDocumentPath,
                                                                               CancellationToken cancellationToken)
    {
        var operations = await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
        var applyChanges = operations.OfType<ApplyChangesOperation>().FirstOrDefault()
                               ?? throw new InvalidOperationException("The code action registered no document change.");
        var changedDocument = applyChanges.ChangedSolution.GetDocument(documentId);
        var documentPath = currentDocumentPath;

        if (changedDocument == null)
        {
            var documents = applyChanges.ChangedSolution.GetProject(projectId)?.Documents.ToImmutableArray() ?? [];

            if (documents.Length != 1)
            {
                throw new InvalidOperationException($"Failed to resolve the fixture document for '{fixturePath}' [{FixtureLineEndings.GetName(lineEnding)}]: "
                                                    + $"expected exactly one document after the code action but found {documents.Length}.");
            }

            changedDocument = documents[0];
            documentPath = BuildDocumentPath(changedDocument);
        }

        var text = await changedDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return (text.ToString(), documentPath);
    }

    /// <summary>
    /// Builds a forward-slash separated document path from a document's folders and name
    /// </summary>
    /// <param name="document">Document to describe</param>
    /// <returns>The document's folders and name joined with <c>/</c></returns>
    private static string BuildDocumentPath(Document document)
    {
        return document.Folders.Count == 0 ? document.Name : string.Join('/', document.Folders.Append(document.Name));
    }

    /// <summary>
    /// Creates an ad-hoc document for the fixture source, named after the fixture's own document path so that
    /// analyzers reading the document's file name (such as RH4001) observe the fixture's real identity instead of
    /// a constant. The project also carries a global analyzer config that disables Roslyn's own generated-code
    /// heuristic, so a fixture whose real name looks generated is still analyzed like every other fixture.
    /// References come from the running host rather than from a package restore, so the runner needs no network
    /// access
    /// </summary>
    /// <param name="workspace">Workspace hosting the document</param>
    /// <param name="source">Fixture source</param>
    /// <param name="documentPath">Forward-slash separated document path the fixture is currently analyzed under</param>
    /// <returns>The created document, its identifier, and its project's identifier</returns>
    private static (Document Document, DocumentId DocumentId, ProjectId ProjectId) CreateDocument(AdhocWorkspace workspace, string source, string documentPath)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var configId = DocumentId.CreateNewId(projectId);
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var references = (trustedPlatformAssemblies?.Split(Path.PathSeparator) ?? []).Where(path => string.IsNullOrEmpty(path) == false)
                                                                                     .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path));
        var separatorIndex = documentPath.LastIndexOf('/');
        var name = separatorIndex < 0 ? documentPath : documentPath[(separatorIndex + 1)..];
        var folders = separatorIndex < 0 ? (ImmutableArray<string>)[] : [.. documentPath[..separatorIndex].Split('/')];
        var solution = workspace.CurrentSolution
                                .AddProject(ProjectInfo.Create(projectId,
                                                               VersionStamp.Create(),
                                                               "FixtureProject",
                                                               "FixtureProject",
                                                               LanguageNames.CSharp,
                                                               parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
                                                               metadataReferences: references))
                                .AddDocument(documentId, name, SourceText.From(source), folders, documentPath)
                                .AddAnalyzerConfigDocument(configId, ".globalconfig", SourceText.From(DisableGeneratedCodeHeuristicConfig), filePath: AnalyzerConfigPath);
        var document = solution.GetDocument(documentId)
                           ?? throw new InvalidOperationException("Failed to create the fixture document.");

        return (document, documentId, projectId);
    }

    #endregion // Methods
}