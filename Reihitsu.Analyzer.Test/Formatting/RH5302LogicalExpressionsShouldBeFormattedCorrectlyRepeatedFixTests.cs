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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Reproduction scratch tests for issue #725: repeated application of the RH5302 code fix on a trailing
/// logical operator ("condition1 &amp;&amp;" / "condition2 ||" both at end of line)
/// </summary>
[TestClass]
public class RH5302LogicalExpressionsShouldBeFormattedCorrectlyRepeatedFixTests : AnalyzerTestsBase<RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer, RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Reproduces the issue's scenario: repeatedly applying the code fix to the diagnostic on the second
    /// (trailing) logical operator and observing whether the diagnostic count converges from 2 to 0/1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRepeatedCodeFixApplicationOnTrailingOrOperatorConverges()
    {
        const string initialSource = """
                                     internal class Example
                                     {
                                         bool condition1;
                                         bool condition2;
                                         bool condition3;

                                         void Run()
                                         {
                                             if (condition1 &&
                                                 condition2 ||
                                                 condition3)
                                             {
                                             }
                                         }
                                     }
                                     """;

        var current = initialSource;
        var diagnosticCounts = new List<int>();
        var conditionTwoLines = new List<string>
                                {
                                    GetLineContaining(current, "condition2")
                                };

        for (var iteration = 0; iteration < 10; iteration++)
        {
            var (next, diagnosticCount, orDiagnosticFound) = await AdvanceOrOperatorAsync(current);

            diagnosticCounts.Add(diagnosticCount);

            if (orDiagnosticFound == false)
            {
                break;
            }

            current = next;
            conditionTwoLines.Add(GetLineContaining(current, "condition2"));
        }

        var conditionTwoLinesReport = string.Join("\n", conditionTwoLines.Select((line, index) => $"  [{index}] (len={line.Length}) \"{line}\""));
        var report = $"Diagnostic counts across applications: [{string.Join(", ", diagnosticCounts)}]\ncondition2 line across applications:\n{conditionTwoLinesReport}\nFinal source:\n{current}";
        var minimumDiagnosticCount = diagnosticCounts.Min();

        Assert.IsLessThanOrEqualTo(1, minimumDiagnosticCount, report);
    }

    #endregion // Tests

    #region Helpers

    /// <summary>
    /// Returns the line of <paramref name="text"/> that contains <paramref name="needle"/>
    /// </summary>
    /// <param name="text">Text to search</param>
    /// <param name="needle">Substring to look for</param>
    /// <returns>The first matching line, or an empty string if none matched</returns>
    private static string GetLineContaining(string text, string needle)
    {
        return text.Split('\n').FirstOrDefault(line => line.Contains(needle) && line.Contains("bool") == false) ?? string.Empty;
    }

    /// <summary>
    /// Runs the analyzer on <paramref name="source"/>, locates the diagnostic reported for the logical-OR
    /// operator (if any) and applies the code fix's first registered action for it
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The resulting text, the diagnostic count observed before the fix, and whether an OR-operator diagnostic was found</returns>
    private static async Task<(string Text, int DiagnosticCount, bool OrDiagnosticFound)> AdvanceOrOperatorAsync(string source)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var (document, documentId) = CreateDocument(workspace, source);

            var root = await document.GetSyntaxRootAsync(CancellationToken.None).ConfigureAwait(false)
                           ?? throw new InvalidOperationException("Failed to parse test document.");
            var compilation = await document.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false)
                                  ?? throw new InvalidOperationException("Failed to compile test document.");
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer());
            var diagnostics = await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync(CancellationToken.None).ConfigureAwait(false);
            var orDiagnostic = diagnostics.FirstOrDefault(diagnostic => IsOrOperatorDiagnostic(root, diagnostic));

            if (orDiagnostic == null)
            {
                return (source, diagnostics.Length, false);
            }

            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document,
                                             orDiagnostic,
                                             (action, _) => actions.Add(action),
                                             CancellationToken.None);
            var codeFixProvider = new RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider();

            await codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

            if (actions.Count == 0)
            {
                return (source, diagnostics.Length, true);
            }

            var operations = await actions[0].GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
            var applyChanges = operations.OfType<ApplyChangesOperation>().First();
            var changedDocument = applyChanges.ChangedSolution.GetDocument(documentId)
                                      ?? throw new InvalidOperationException("Failed to resolve the changed document.");
            var text = await changedDocument.GetTextAsync(CancellationToken.None).ConfigureAwait(false);

            return (text.ToString(), diagnostics.Length, true);
        }
    }

    /// <summary>
    /// Determines whether the provided diagnostic was reported for a logical-OR binary expression's operator
    /// token
    /// </summary>
    /// <param name="root">Syntax root the diagnostic was reported against</param>
    /// <param name="diagnostic">Diagnostic to check</param>
    /// <returns><see langword="true"/> if the diagnostic's token is the operator of a logical-OR expression</returns>
    private static bool IsOrOperatorDiagnostic(SyntaxNode root, Diagnostic diagnostic)
    {
        var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

        return token.Parent is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.LogicalOrExpression);
    }

    /// <summary>
    /// Creates an ad-hoc test document for the provided source
    /// </summary>
    /// <param name="workspace">Workspace hosting the document</param>
    /// <param name="source">Source text</param>
    /// <returns>The created document and its identifier</returns>
    private static (Document Document, DocumentId DocumentId) CreateDocument(AdhocWorkspace workspace, string source)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var versionStamp = VersionStamp.Create();
        var parseOptions = new CSharpParseOptions(languageVersion: LanguageVersion.Latest);
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
    /// Gets the metadata references required for the ad-hoc test project
    /// </summary>
    /// <returns>Metadata references</returns>
    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var referencePaths = trustedPlatformAssemblies?.Split(Path.PathSeparator)
                                 ?? [];

        return referencePaths.Select(path => MetadataReference.CreateFromFile(path));
    }

    #endregion // Helpers
}