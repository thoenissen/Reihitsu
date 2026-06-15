using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Verifying analyzer and code fixers
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
/// <typeparam name="TCodeFix">Type of the code fixer</typeparam>
public abstract class AnalyzerTestsBase<TAnalyzer, TCodeFix> : AnalyzerTestsBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    #region Methods

    /// <summary>
    /// Verifies the analyzer provides diagnostics which, in combination with the code fix, produce the expected
    /// fixed code
    /// </summary>
    /// <param name="source">The source text to test, which may include markup syntax</param>
    /// <param name="fixedSource">The expected fixed source text. Any remaining diagnostics are defined in markup</param>
    /// <param name="expected">The expected diagnostics. These diagnostics are in addition to any diagnostics defined in markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task Verify(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        await Verify(source, fixedSource, null, expected);
    }

    /// <summary>
    /// Verifies the analyzer provides diagnostics which, in combination with the code fix, produce the expected
    /// fixed code
    /// </summary>
    /// <param name="source">The source text to test, which may include markup syntax</param>
    /// <param name="fixedSource">The expected fixed source text. Any remaining diagnostics are defined in markup</param>
    /// <param name="onConfigure">Additional configuration of the test</param>
    /// <param name="expected">The expected diagnostics. These diagnostics are in addition to any diagnostics defined in markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task Verify(string source, string fixedSource, Action<CSharpCodeFixVerifierTest<TAnalyzer, TCodeFix>> onConfigure, params DiagnosticResult[] expected)
    {
        var test = new CSharpCodeFixVerifierTest<TAnalyzer, TCodeFix>
                   {
                       TestCode = source,
                       FixedCode = fixedSource,
                       ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                   };

        test.ExpectedDiagnostics.AddRange(expected);

        onConfigure?.Invoke(test);

        await test.RunAsync(CancellationToken.None);
    }

    /// <summary>
    /// Normalizes the line endings of the provided text to CRLF, irrespective of how the source file is stored.
    /// This is used by end-of-line regression tests so the document's predominant line ending is CRLF on every
    /// platform, even where <see cref="System.Environment.NewLine"/> is LF
    /// </summary>
    /// <param name="text">Text to normalize</param>
    /// <returns>The text with CRLF line endings</returns>
    protected static string NormalizeToCarriageReturnLineFeed(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\n", "\r\n");
    }

    /// <summary>
    /// Gets the code actions registered for the provided diagnostic on the given source text
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="locationProvider">Callback that selects the diagnostic location from the parsed syntax root</param>
    /// <param name="preprocessorSymbols">Preprocessor symbols to define for parsing</param>
    /// <returns>The registered code actions</returns>
    protected static async Task<List<CodeAction>> GetCodeFixActionsAsync(string source, string diagnosticId, Func<SyntaxNode, Location> locationProvider, params string[] preprocessorSymbols)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var (document, _) = CreateTestDocument(workspace, source, preprocessorSymbols);

            return await RegisterCodeFixActionsAsync(document, diagnosticId, locationProvider).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Runs the analyzer over the provided source, applies the first code action offered for the first fixable
    /// diagnostic and returns the resulting source text. Unlike the markup-based verifier, the raw text is returned
    /// without line-ending normalization, so it can be used to assert that a fix preserves the document's
    /// end-of-line style
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="preprocessorSymbols">Preprocessor symbols to define for parsing</param>
    /// <returns>The source text after applying the first offered code action</returns>
    protected static async Task<string> ApplyCodeFixAsync(string source, params string[] preprocessorSymbols)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var (document, documentId) = CreateTestDocument(workspace, source, preprocessorSymbols);

            var compilation = await document.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false)
                                  ?? throw new InvalidOperationException("Failed to compile test document.");
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new TAnalyzer());
            var diagnostics = await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync(CancellationToken.None).ConfigureAwait(false);
            var codeFixProvider = new TCodeFix();
            var diagnostic = diagnostics.FirstOrDefault(item => codeFixProvider.FixableDiagnosticIds.Contains(item.Id))
                                 ?? throw new InvalidOperationException("The analyzer did not report a fixable diagnostic.");
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document,
                                             diagnostic,
                                             (action, _) => actions.Add(action),
                                             CancellationToken.None);

            await codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

            if (actions.Count == 0)
            {
                throw new InvalidOperationException("No code fix action was registered for the diagnostic.");
            }

            var operations = await actions[0].GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
            var applyChanges = operations.OfType<ApplyChangesOperation>().First();
            var changedDocument = applyChanges.ChangedSolution.GetDocument(documentId)
                                      ?? throw new InvalidOperationException("Failed to resolve the changed document.");
            var text = await changedDocument.GetTextAsync(CancellationToken.None).ConfigureAwait(false);

            return text.ToString();
        }
    }

    /// <summary>
    /// Creates an ad-hoc test document for the provided source
    /// </summary>
    /// <param name="workspace">Workspace hosting the document</param>
    /// <param name="source">Source text</param>
    /// <param name="preprocessorSymbols">Preprocessor symbols to define for parsing</param>
    /// <returns>The created document and its identifier</returns>
    private static (Document Document, DocumentId DocumentId) CreateTestDocument(AdhocWorkspace workspace, string source, string[] preprocessorSymbols)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var versionStamp = VersionStamp.Create();
        var parseOptions = new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(languageVersion: Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest,
                                                                                preprocessorSymbols: preprocessorSymbols);
        var solution = workspace.CurrentSolution
                                .AddProject(ProjectInfo.Create(projectId,
                                                               versionStamp,
                                                               "TestProject",
                                                               "TestProject",
                                                               LanguageNames.CSharp,
                                                               parseOptions: parseOptions,
                                                               metadataReferences: GetMetadataReferences()))
                                .AddDocument(documentId, "Test.cs", SourceText.From(source));
        var document = solution.GetDocument(documentId)
                           ?? throw new InvalidOperationException("Failed to create test document.");

        return (document, documentId);
    }

    /// <summary>
    /// Registers the code fix actions for the provided diagnostic on the given document
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="locationProvider">Callback that selects the diagnostic location from the parsed syntax root</param>
    /// <returns>The registered code actions</returns>
    private static async Task<List<CodeAction>> RegisterCodeFixActionsAsync(Document document, string diagnosticId, Func<SyntaxNode, Location> locationProvider)
    {
        var root = await document.GetSyntaxRootAsync(CancellationToken.None).ConfigureAwait(false)
                       ?? throw new InvalidOperationException("Failed to parse test document.");
        var descriptor = new DiagnosticDescriptor(diagnosticId,
                                                  "Title",
                                                  "Message",
                                                  "Testing",
                                                  DiagnosticSeverity.Warning,
                                                  isEnabledByDefault: true);
        var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(descriptor, locationProvider(root));
        var actions = new List<CodeAction>();
        var codeFixProvider = new TCodeFix();
        var context = new CodeFixContext(document,
                                         diagnostic,
                                         (action, _) => actions.Add(action),
                                         CancellationToken.None);

        await codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

        return actions;
    }

    /// <summary>
    /// Gets the metadata references required for the ad-hoc code-fix test project
    /// </summary>
    /// <returns>Metadata references</returns>
    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var referencePaths = trustedPlatformAssemblies?.Split(Path.PathSeparator)
                                 ?? [];

        return referencePaths.Select(path => MetadataReference.CreateFromFile(path));
    }

    #endregion // Methods
}