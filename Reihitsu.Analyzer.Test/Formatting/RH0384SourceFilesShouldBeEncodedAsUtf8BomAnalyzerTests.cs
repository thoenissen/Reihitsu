using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0384SourceFilesShouldBeEncodedAsUtf8BomAnalyzer"/> and <see cref="RH0384SourceFilesShouldBeEncodedAsUtf8BomCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0384SourceFilesShouldBeEncodedAsUtf8BomAnalyzerTests : AnalyzerTestsBase<RH0384SourceFilesShouldBeEncodedAsUtf8BomAnalyzer, RH0384SourceFilesShouldBeEncodedAsUtf8BomCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that UTF-8 BOM encoded files do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUtf8BomEncodedFilesDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData,
                     test => ConfigureDocumentEncoding(test, testData, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)));
    }

    /// <summary>
    /// Verifies that UTF-8 files without BOM are detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUtf8WithoutBomIsDetected()
    {
        const string testData = """
                                {|#0:internal|} class TestClass
                                {
                                }
                                """;

        await Verify(testData,
                     test => ConfigureDocumentEncoding(test, testData, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)),
                     Diagnostics(RH0384SourceFilesShouldBeEncodedAsUtf8BomAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
    }

    /// <summary>
    /// Verifies that UTF-16 encoded files are detected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUtf16EncodedFilesAreDetected()
    {
        const string testData = """
                                {|#0:internal|} class TestClass
                                {
                                }
                                """;

        await Verify(testData,
                     test => ConfigureDocumentEncoding(test, testData, Encoding.Unicode),
                     Diagnostics(RH0384SourceFilesShouldBeEncodedAsUtf8BomAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix rewrites the document using UTF-8 with BOM.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixRewritesDocumentAsUtf8Bom()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                """;

        var document = CreateDocument(testData, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var diagnostics = await GetDiagnosticsAsync(document);

        Assert.HasCount(1, diagnostics);

        var fixedDocument = await ApplyCodeFixAsync(document, diagnostics[0]);
        var fixedText = await fixedDocument.GetTextAsync(CancellationToken.None).ConfigureAwait(false);

        Assert.AreEqual(testData, fixedText.ToString());
        Assert.IsNotNull(fixedText.Encoding);
        CollectionAssert.AreEqual(new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble(), fixedText.Encoding.GetPreamble());
        Assert.AreEqual(Encoding.UTF8.WebName, fixedText.Encoding.WebName);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Applies the given encoding to the single test document.
    /// </summary>
    /// <param name="test">Test configuration</param>
    /// <param name="source">Source code</param>
    /// <param name="encoding">Encoding to apply</param>
    private static void ConfigureDocumentEncoding(CSharpAnalyzerVerifierTest<RH0384SourceFilesShouldBeEncodedAsUtf8BomAnalyzer> test, string source, Encoding encoding)
    {
        test.TestState.Sources.Clear();
        test.TestState.Sources.Add(("/0/Test0.cs", source));
        test.SolutionTransforms.Add((solution, projectId) => ApplyDocumentEncoding(solution, projectId, encoding));
    }

    /// <summary>
    /// Applies an encoding to the single document in the specified solution.
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <param name="encoding">Encoding to apply</param>
    /// <returns>Transformed solution</returns>
    private static Solution ApplyDocumentEncoding(Solution solution, ProjectId projectId, Encoding encoding)
    {
        var project = solution.GetProject(projectId);
        var document = project?.Documents.Single();
        var sourceText = document?.GetTextAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

        if (document == null || sourceText == null)
        {
            return solution;
        }

        var updatedSourceText = SourceText.From(sourceText.ToString(), encoding, sourceText.ChecksumAlgorithm);

        return document.WithText(updatedSourceText).Project.Solution;
    }

    /// <summary>
    /// Creates a document with the specified encoding.
    /// </summary>
    /// <param name="source">Source code</param>
    /// <param name="encoding">Document encoding</param>
    /// <returns>Created document</returns>
    private static Document CreateDocument(string source, Encoding encoding)
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var solution = workspace.CurrentSolution
                                .AddProject(ProjectInfo.Create(projectId,
                                                               VersionStamp.Create(),
                                                               "TestProject",
                                                               "TestProject",
                                                               LanguageNames.CSharp,
                                                               filePath: "TestProject.csproj",
                                                               compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                                                               parseOptions: new CSharpParseOptions(LanguageVersion.Preview)))
                                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
                                .AddDocument(documentId, "Test0.cs", SourceText.From(source, encoding), filePath: "Test0.cs");

        return solution.GetDocument(documentId);
    }

    /// <summary>
    /// Runs the analyzer and returns its diagnostics.
    /// </summary>
    /// <param name="document">Document</param>
    /// <returns>Produced diagnostics</returns>
    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Document document)
    {
        var compilation = await document.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);

        Assert.IsNotNull(compilation);

        return await compilation.WithAnalyzers([new RH0384SourceFilesShouldBeEncodedAsUtf8BomAnalyzer()],
                                               new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty))
                                .GetAnalyzerDiagnosticsAsync(CancellationToken.None)
                                .ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the first registered code fix to the specified document and diagnostic.
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>Updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, Diagnostic diagnostic)
    {
        var codeActions = new List<CodeAction>();
        var codeFixProvider = new RH0384SourceFilesShouldBeEncodedAsUtf8BomCodeFixProvider();
        var context = new CodeFixContext(document,
                                         diagnostic,
                                         (action, _) => codeActions.Add(action),
                                         CancellationToken.None);

        await codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

        Assert.HasCount(1, codeActions);

        var operations = await codeActions[0].GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
        var applyChangesOperation = operations.OfType<ApplyChangesOperation>().Single();

        return applyChangesOperation.ChangedSolution.GetDocument(document.Id);
    }

    #endregion // Methods
}