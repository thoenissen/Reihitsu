using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Runs a rule document's violation/correction example against the shipped analyzer and code fix
/// </summary>
internal static class RuleDocumentationExampleVerifier
{
    #region Fields

    /// <summary>
    /// Maximum number of code-fix applications attempted before treating the fix as non-converging
    /// </summary>
    private const int MaxFixIterations = 10;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies a single rule document example
    /// </summary>
    /// <param name="example">Example to verify</param>
    /// <param name="analyzers">Every shipped analyzer, keyed by diagnostic ID</param>
    /// <param name="codeFixProviders">Every shipped code-fix provider</param>
    /// <returns>The verification result</returns>
    internal static RuleDocumentationExampleResult Verify(RuleDocumentationExample example,
                                                          IReadOnlyDictionary<string, DiscoveredAnalyzer> analyzers,
                                                          IReadOnlyList<DiscoveredCodeFixProvider> codeFixProviders)
    {
        if (analyzers.TryGetValue(example.DiagnosticId, out var discoveredAnalyzer) == false)
        {
            return Failure(example, RuleDocumentationExampleOutcome.NoMatchingAnalyzer, $"No shipped analyzer reports diagnostic '{example.DiagnosticId}'.");
        }

        var analyzer = CreateAnalyzer(discoveredAnalyzer.AnalyzerType);

        if (TryParseCompilationUnit(example.Violation, out var violationParseError) == false)
        {
            return Failure(example, RuleDocumentationExampleOutcome.ExampleDoesNotParse, $"Violation example does not parse as a compilation unit: {violationParseError}");
        }

        if (TryParseCompilationUnit(example.Correction, out var correctionParseError) == false)
        {
            return Failure(example, RuleDocumentationExampleOutcome.ExampleDoesNotParse, $"Correction example does not parse as a compilation unit: {correctionParseError}");
        }

        var violationDiagnosticCount = CountDiagnostics(example.Violation, analyzer, example.DiagnosticId);

        if (violationDiagnosticCount < 1)
        {
            return Failure(example, RuleDocumentationExampleOutcome.ViolationDoesNotReport, $"Violation example reported '{example.DiagnosticId}' {violationDiagnosticCount} time(s); expected at least once.");
        }

        var correctionDiagnosticCount = CountDiagnostics(example.Correction, analyzer, example.DiagnosticId);

        if (correctionDiagnosticCount != 0)
        {
            return Failure(example, RuleDocumentationExampleOutcome.CorrectionStillReports, $"Correction example reported '{example.DiagnosticId}' {correctionDiagnosticCount} time(s); expected zero.");
        }

        if (example.HasCodeFix == false)
        {
            return Success(example);
        }

        var matchingCodeFixProviderTypes = codeFixProviders.Where(codeFix => codeFix.DiagnosticId == example.DiagnosticId)
                                                           .Select(codeFix => codeFix.CodeFixProviderType)
                                                           .ToArray();

        if (matchingCodeFixProviderTypes.Length == 0)
        {
            return Failure(example, RuleDocumentationExampleOutcome.NoMatchingCodeFix, $"Metadata advertises a code fix for '{example.DiagnosticId}' but none is shipped.");
        }

        string fixedText;

        try
        {
            fixedText = ApplyCodeFixUntilResolved(example.Violation, analyzer, matchingCodeFixProviderTypes, example.DiagnosticId, out var iterations);

            if (iterations > MaxFixIterations)
            {
                return Failure(example, RuleDocumentationExampleOutcome.CodeFixDoesNotConverge, $"Code fix did not converge within {MaxFixIterations} iterations.");
            }
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return Failure(example, RuleDocumentationExampleOutcome.CodeFixDoesNotConverge, $"Code fix threw: {exception.GetType().Name}: {exception.Message}");
        }

        if (string.Equals(fixedText, example.Correction, StringComparison.Ordinal) == false)
        {
            return Failure(example, RuleDocumentationExampleOutcome.CodeFixOutputDiffers, $"Code fix output does not match the documented correction character for character.{Environment.NewLine}--- expected ---{Environment.NewLine}{example.Correction}{Environment.NewLine}--- actual ---{Environment.NewLine}{fixedText}");
        }

        return Success(example);
    }

    /// <summary>
    /// Creates a single analyzer instance
    /// </summary>
    /// <param name="analyzerType">Analyzer type</param>
    /// <returns>The created analyzer</returns>
    private static DiagnosticAnalyzer CreateAnalyzer(Type analyzerType)
    {
        return Activator.CreateInstance(analyzerType) as DiagnosticAnalyzer
                   ?? throw new InvalidOperationException($"Failed to create analyzer type '{analyzerType.FullName}'.");
    }

    /// <summary>
    /// Attempts to parse the given source as a standalone compilation unit, without wrapping it in any
    /// enclosing type or member
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="error">Description of the first parse error, when parsing fails</param>
    /// <returns><see langword="true"/> when the source parses without syntax errors; otherwise, <see langword="false"/></returns>
    private static bool TryParseCompilationUnit(string source, out string error)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var firstError = syntaxTree.GetDiagnostics().FirstOrDefault(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        error = firstError?.ToString() ?? string.Empty;

        return firstError == null;
    }

    /// <summary>
    /// Counts how many times the analyzer reports the given diagnostic ID on the provided source
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="analyzer">Analyzer to run</param>
    /// <param name="diagnosticId">Diagnostic ID to count</param>
    /// <returns>The number of matching diagnostics</returns>
    private static int CountDiagnostics(string source, DiagnosticAnalyzer analyzer, string diagnosticId)
    {
        var compilation = CreateCompilation(source);
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
        var diagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(CancellationToken.None).GetAwaiter().GetResult();

        return diagnostics.Count(diagnostic => diagnostic.Id == diagnosticId);
    }

    /// <summary>
    /// Repeatedly applies the code fix's first offered action for the target diagnostic until the diagnostic is
    /// no longer reported
    /// </summary>
    /// <param name="source">Initial source text</param>
    /// <param name="analyzer">Analyzer to run</param>
    /// <param name="codeFixProviderTypes">Every shipped code-fix provider type registered for the diagnostic</param>
    /// <param name="diagnosticId">Diagnostic ID to resolve</param>
    /// <param name="iterations">Number of fix applications performed</param>
    /// <returns>The source text once the diagnostic is no longer reported</returns>
    private static string ApplyCodeFixUntilResolved(string source, DiagnosticAnalyzer analyzer, IReadOnlyList<Type> codeFixProviderTypes, string diagnosticId, out int iterations)
    {
        var currentText = source;

        for (iterations = 0; iterations <= MaxFixIterations; iterations++)
        {
            var compilation = CreateCompilation(currentText);
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
            var diagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(CancellationToken.None).GetAwaiter().GetResult();
            var target = diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == diagnosticId);

            if (target == null)
            {
                return currentText;
            }

            currentText = ApplySingleCodeFix(currentText, target, codeFixProviderTypes);
        }

        return currentText;
    }

    /// <summary>
    /// Applies the first code-fix action registered by any of the given provider types for the given diagnostic
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="targetDiagnosticTemplate">Diagnostic reported against the source, used only to locate the same diagnostic against the freshly created document</param>
    /// <param name="codeFixProviderTypes">Code-fix provider types to try, in order</param>
    /// <returns>The source text after applying the fix</returns>
    private static string ApplySingleCodeFix(string source, Diagnostic targetDiagnosticTemplate, IReadOnlyList<Type> codeFixProviderTypes)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            var solution = workspace.CurrentSolution
                                    .AddProject(ProjectInfo.Create(projectId,
                                                                   VersionStamp.Create(),
                                                                   "TestProject",
                                                                   "TestProject",
                                                                   LanguageNames.CSharp,
                                                                   parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
                                                                   compilationOptions: new CSharpCompilationOptions(OutputKind.ConsoleApplication),
                                                                   metadataReferences: GetMetadataReferences()))
                                    .AddDocument(documentId, "Test.cs", SourceText.From(source));
            var document = solution.GetDocument(documentId)
                               ?? throw new InvalidOperationException("Failed to create test document.");
            var diagnostic = Diagnostic.Create(targetDiagnosticTemplate.Descriptor, targetDiagnosticTemplate.Location, targetDiagnosticTemplate.AdditionalLocations);
            var actions = new List<CodeAction>();

            foreach (var codeFixProviderType in codeFixProviderTypes)
            {
                var codeFixProvider = Activator.CreateInstance(codeFixProviderType) as CodeFixProvider
                                          ?? throw new InvalidOperationException($"Failed to create code-fix provider '{codeFixProviderType.FullName}'.");
                var context = new CodeFixContext(document,
                                                 diagnostic,
                                                 (action, _) => actions.Add(action),
                                                 CancellationToken.None);

                codeFixProvider.RegisterCodeFixesAsync(context).GetAwaiter().GetResult();

                if (actions.Count > 0)
                {
                    break;
                }
            }

            if (actions.Count == 0)
            {
                throw new InvalidOperationException("No code fix action was registered for the diagnostic.");
            }

            var operations = actions[0].GetOperationsAsync(CancellationToken.None).GetAwaiter().GetResult();
            var applyChanges = operations.OfType<ApplyChangesOperation>().First();
            var changedDocument = applyChanges.ChangedSolution.GetDocument(documentId)
                                      ?? throw new InvalidOperationException("Failed to resolve the changed document.");
            var text = changedDocument.GetTextAsync(CancellationToken.None).GetAwaiter().GetResult();

            return text.ToString();
        }
    }

    /// <summary>
    /// Creates a standalone compilation for the given source text
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The created compilation</returns>
    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));

        return CSharpCompilation.Create("TestAssembly")
                                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                                .AddReferences(GetMetadataReferences())
                                .AddSyntaxTrees(syntaxTree);
    }

    /// <summary>
    /// Gets the metadata references required to compile a rule document example
    /// </summary>
    /// <returns>Metadata references</returns>
    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var referencePaths = trustedPlatformAssemblies?.Split(Path.PathSeparator)
                                 ?? [];

        return referencePaths.Select(path => MetadataReference.CreateFromFile(path));
    }

    /// <summary>
    /// Creates a passing result
    /// </summary>
    /// <param name="example">Example that was verified</param>
    /// <returns>A passing result</returns>
    private static RuleDocumentationExampleResult Success(RuleDocumentationExample example)
    {
        return new RuleDocumentationExampleResult(example, RuleDocumentationExampleOutcome.Passed, string.Empty);
    }

    /// <summary>
    /// Creates a failing result
    /// </summary>
    /// <param name="example">Example that was verified</param>
    /// <param name="outcome">Failure outcome</param>
    /// <param name="detail">Human-readable detail describing the failure</param>
    /// <returns>A failing result</returns>
    private static RuleDocumentationExampleResult Failure(RuleDocumentationExample example, RuleDocumentationExampleOutcome outcome, string detail)
    {
        return new RuleDocumentationExampleResult(example, outcome, detail);
    }

    #endregion // Methods
}