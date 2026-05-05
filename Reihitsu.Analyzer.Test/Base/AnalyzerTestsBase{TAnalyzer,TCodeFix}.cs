using System;
using System.Collections.Generic;
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
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            var versionStamp = VersionStamp.Create();
            var parseOptions = new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(preprocessorSymbols: preprocessorSymbols);
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