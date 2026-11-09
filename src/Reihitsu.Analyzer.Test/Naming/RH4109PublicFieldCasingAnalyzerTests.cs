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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4109PublicFieldCasingAnalyzer"/> and <see cref="RH4109PublicFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4109PublicFieldCasingAnalyzerTests : BatchCodeFixTestsBase<RH4109PublicFieldCasingAnalyzer, RH4109PublicFieldCasingCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Name of the project of the target framework that does not define <c>NET8_0</c>
    /// </summary>
    private const string TargetWithoutSymbol = "Lib(net9.0)";

    /// <summary>
    /// Name of the project of the target framework that defines <c>NET8_0</c>
    /// </summary>
    private const string TargetWithSymbol = "Lib(net8.0)";

    /// <summary>
    /// Path of the file that declares the field
    /// </summary>
    private const string HolderPath = "/src/Lib/A.cs";

    /// <summary>
    /// Path of the file that reads the field
    /// </summary>
    private const string ReaderPath = "/src/Lib/B.cs";

    /// <summary>
    /// Path of the file of the consuming project
    /// </summary>
    private const string ConsumerPath = "/src/Consumer/C.cs";

    /// <summary>
    /// Source of the file that declares the field
    /// </summary>
    private const string HolderSource = """
                                        public class Holder
                                        {
                                            public int key;
                                        }
                                        """;

    /// <summary>
    /// Source of a file that reads the field only when <c>NET8_0</c> is defined
    /// </summary>
    private const string ConditionalReaderSource = """
                                                   public static class Reader
                                                   {
                                                       public static int Read()
                                                       {
                                                   #if NET8_0
                                                           return new Holder().key;
                                                   #else
                                                           return 0;
                                                   #endif
                                                       }
                                                   }
                                                   """;

    /// <summary>
    /// Source of a file that reads the field in active code
    /// </summary>
    private const string ActiveReaderSource = """
                                              public static class OtherReader
                                              {
                                                  public static int Read()
                                                  {
                                                      return new Holder().key;
                                                  }
                                              }
                                              """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for public fields that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    [Requirement("RH4109-002")]
    public async Task VerifyDiagnosticsForPublicFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        public int {|#0:cacheCount|};

                                        public int GetCount()
                                        {
                                            return cacheCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         public int CacheCount;

                                         public int GetCount()
                                         {
                                             return CacheCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4109PublicFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4109MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase public fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    public async Task VerifyNoDiagnosticsForPascalCasePublicField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        public int CacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies public readonly fields are also covered by the public field rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    [Requirement("RH4109-002")]
    public async Task VerifyDiagnosticsForPublicReadonlyFieldWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        public readonly int {|#0:cacheLimit|} = 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         public readonly int CacheLimit = 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4109PublicFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4109MessageFormat));
    }

    /// <summary>
    /// Verifies that an implicitly private field is not claimed by the public field rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    [Requirement("SHARED-CASING-004")]
    public async Task VerifyNoDiagnosticsForImplicitlyPrivateField()
    {
        const string testCode = """
                                internal class ResourceCache
                                {
                                    int cacheCount;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a public field of a nested private class and a static interface field without modifier belong to the public group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-004")]
    [Requirement("RH4109-001")]
    public async Task ContainingTypeDoesNotChangeTheAccessibilityGroup()
    {
        const string testCode = """
                                public class C
                                {
                                    private class Nested
                                    {
                                        public int {|RH4109:value|};
                                    }
                                }

                                public interface I
                                {
                                    static int {|RH4109:shared|};
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that every declarator of public, public readonly, and public static fields is checked, while constants and implicitly private fields are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    public async Task FieldsOfThePublicGroupThatAreNotConstantsAreChecked()
    {
        const string testCode = """
                                public class C
                                {
                                    public int CacheCount;
                                    public int {|RH4109:itemCount|}, {|RH4109:pageSize|};
                                    public readonly int {|RH4109:cacheLimit|} = 10;
                                    public static int {|RH4109:instances|};
                                    public const int limit = 2;
                                    int cacheValue;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames a public field to PascalCase together with its use, and is withheld for a field whose new name is already taken
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-002")]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesPublicFieldAndWithholdsTheFixWhenTheNewNameIsTaken()
    {
        const string testCode = """
                                public class Invoice
                                {
                                    public int Amount;

                                    public int {|RH4109:amount|};

                                    public int {|RH4109:taxRate|};

                                    public int Total => Amount + amount + taxRate;
                                }
                                """;

        const string fixedCode = """
                                 public class Invoice
                                 {
                                     public int Amount;

                                     public int {|RH4109:amount|};

                                     public int TaxRate;

                                     public int Total => Amount + amount + TaxRate;
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostic that keeps no fix is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;

                         // The withheld field keeps the Fix All loop running for a second pass that changes nothing
                         test.NumberOfFixAllIterations = -2;
                     });
    }

    /// <summary>
    /// Verifies that a field keeps its diagnostic but gets no fix when it is the member of a record's positional parameter of the same name, because after the rename the compiler would declare a public property for the parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldSplitTheFieldFromItsPositionalParameter()
    {
        const string testCode = """
                                public record R(int count)
                                {
                                    public int {|RH4109:count|} = count;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies the multi-targeted shape where both target projects compile the same two files as linked documents and
    /// the reference to the field is active only in the other target's build: started from the target where the
    /// reference is excluded code, no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenALinkedDocumentReferencesTheFieldOnlyInTheOtherTargetsBuild()
    {
        var result = await ApplyFixInMultiTargetSolutionAsync([
                                                                  (TargetWithoutSymbol, [], [(HolderPath, HolderSource), (ReaderPath, ConditionalReaderSource)], []),
                                                                  (TargetWithSymbol, ["NET8_0"], [(HolderPath, HolderSource), (ReaderPath, ConditionalReaderSource)], [])
                                                              ],
                                                              TargetWithoutSymbol).ConfigureAwait(false);

        Assert.AreEqual(0, result.ActionCount, "No fix may be offered while a linked document names the field in code excluded from this target.");
    }

    /// <summary>
    /// Verifies the multi-targeted shape of linked documents started from the target where the reference is active: the
    /// fix is withheld, or it renames the field and the reference in the shared file, and no target's build gets a
    /// compile error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixStartedFromTheTargetWithTheActiveReferenceBreaksNoTargetsBuild()
    {
        var result = await ApplyFixInMultiTargetSolutionAsync([
                                                                  (TargetWithoutSymbol, [], [(HolderPath, HolderSource), (ReaderPath, ConditionalReaderSource)], []),
                                                                  (TargetWithSymbol, ["NET8_0"], [(HolderPath, HolderSource), (ReaderPath, ConditionalReaderSource)], [])
                                                              ],
                                                              TargetWithSymbol).ConfigureAwait(false);

        AssertWithheldOrRenamedEverywhere(result);
    }

    /// <summary>
    /// Verifies the multi-targeted shape where a file is compiled only in the other target's project and references the
    /// field in active code: the fix is withheld, or it also renames that reference, and no target's build gets a
    /// compile error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixBreaksNoTargetsBuildWhenAFileOfOnlyTheOtherTargetReferencesTheField()
    {
        var result = await ApplyFixInMultiTargetSolutionAsync([
                                                                  (TargetWithoutSymbol, [], [(HolderPath, HolderSource)], []),
                                                                  (TargetWithSymbol, ["NET8_0"], [(HolderPath, HolderSource), (ReaderPath, ActiveReaderSource)], [])
                                                              ],
                                                              TargetWithoutSymbol).ConfigureAwait(false);

        AssertWithheldOrRenamedEverywhere(result);
    }

    /// <summary>
    /// Verifies the multi-targeted shape where another project references only the other target's project and uses the
    /// field in active code: the fix is withheld, or it also renames that reference, and no project's build gets a
    /// compile error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixBreaksNoBuildWhenAProjectReferencingOnlyTheOtherTargetUsesTheField()
    {
        var result = await ApplyFixInMultiTargetSolutionAsync([
                                                                  (TargetWithoutSymbol, [], [(HolderPath, HolderSource)], []),
                                                                  (TargetWithSymbol, ["NET8_0"], [(HolderPath, HolderSource)], []),
                                                                  ("Consumer", [], [(ConsumerPath, ActiveReaderSource)], [1])
                                                              ],
                                                              TargetWithoutSymbol).ConfigureAwait(false);

        AssertWithheldOrRenamedEverywhere(result);
    }

    /// <summary>
    /// Verifies the multi-targeted shape of linked documents with Fix All started from the target where the reference is
    /// excluded code: the field is left as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixAllLeavesTheFieldWhenALinkedDocumentReferencesItOnlyInTheOtherTargetsBuild()
    {
        var result = await ApplyFixInMultiTargetSolutionAsync([
                                                                  (TargetWithoutSymbol, [], [(HolderPath, HolderSource), (ReaderPath, ConditionalReaderSource)], []),
                                                                  (TargetWithSymbol, ["NET8_0"], [(HolderPath, HolderSource), (ReaderPath, ConditionalReaderSource)], [])
                                                              ],
                                                              TargetWithoutSymbol,
                                                              fixAll: true).ConfigureAwait(false);

        Assert.IsTrue(result.FixedSources.Where(static source => source.Path == HolderPath).All(static source => source.Text.Contains("public int key;", StringComparison.Ordinal)),
                      "Fix All may not rename the field while a linked document names it in code excluded from this target.");
        AssertWithheldOrRenamedEverywhere(result);
    }

    /// <summary>
    /// Asserts that the fix was withheld, or that it renamed the field and every reference in every project without a
    /// compile error that the solution did not have
    /// </summary>
    /// <param name="result">Result of the fix</param>
    private static void AssertWithheldOrRenamedEverywhere((int ActionCount, ImmutableArray<(string Project, string Path, string Text)> FixedSources, ImmutableArray<string> InitialErrors, ImmutableArray<string> FixedErrors) result)
    {
        Assert.IsEmpty(result.InitialErrors, "The input must compile in every project.");
        Assert.IsEmpty(result.FixedErrors, "The fix may not add a compile error to any project.");

        if (result.ActionCount == 0)
        {
            return;
        }

        foreach (var (project, path, text) in result.FixedSources)
        {
            Assert.DoesNotContain(".key", text, $"The fix left a reference to the old name in {path} of {project}.");
            Assert.DoesNotContain("int key", text, $"The fix left the old declaration in {path} of {project}.");
        }
    }

    /// <summary>
    /// Builds a solution of the given projects, where documents with the same path in several projects are linked
    /// documents of one file as in a multi-targeted project, runs the analyzer in the initiating project, and applies the
    /// first fix offered for the diagnostic on <c>key</c> in that project's copy of the holder file, or Fix All in
    /// document scope when <paramref name="fixAll"/> is set
    /// </summary>
    /// <param name="projects">Projects: name, preprocessor symbols, documents, and the indexes of the referenced projects</param>
    /// <param name="initiatingProject">Name of the project the fix is started from</param>
    /// <param name="fixAll">Whether Fix All is applied instead of the single fix</param>
    /// <returns>
    /// The number of offered actions, or zero for a Fix All that changes no document, the texts after the fix, and the
    /// compile errors before and after it
    /// </returns>
    private static async Task<(int ActionCount, ImmutableArray<(string Project, string Path, string Text)> FixedSources, ImmutableArray<string> InitialErrors, ImmutableArray<string> FixedErrors)> ApplyFixInMultiTargetSolutionAsync((string Name, string[] Symbols, (string Path, string Text)[] Documents, int[] References)[] projects, string initiatingProject, bool fixAll = false)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            var references = (trustedPlatformAssemblies?.Split(Path.PathSeparator) ?? []).Select(static path => MetadataReference.CreateFromFile(path))
                                                                                         .ToList();
            var projectIds = projects.Select(static _ => ProjectId.CreateNewId()).ToList();
            var documentIds = new List<(string Project, string Path, DocumentId Id)>();
            var solution = workspace.CurrentSolution;

            for (var index = 0; index < projects.Length; index++)
            {
                var project = projects[index];
                var projectInfo = ProjectInfo.Create(projectIds[index],
                                                     VersionStamp.Create(),
                                                     project.Name,
                                                     "Lib",
                                                     LanguageNames.CSharp,
                                                     filePath: $"/src/{project.Name}/Lib.csproj",
                                                     compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                                                     parseOptions: new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: project.Symbols),
                                                     metadataReferences: references,
                                                     projectReferences: project.References.Select(referenceIndex => new ProjectReference(projectIds[referenceIndex])));

                solution = solution.AddProject(projectInfo);

                foreach (var (path, text) in project.Documents)
                {
                    var documentId = DocumentId.CreateNewId(projectIds[index]);

                    documentIds.Add((project.Name, path, documentId));
                    solution = solution.AddDocument(DocumentInfo.Create(documentId,
                                                                        Path.GetFileName(path),
                                                                        loader: TextLoader.From(TextAndVersion.Create(SourceText.From(text), VersionStamp.Create())),
                                                                        filePath: path));
                }
            }

            var initiatingDocumentId = documentIds.Single(entry => entry.Project == initiatingProject && entry.Path == HolderPath).Id;
            var initiatingDocument = solution.GetDocument(initiatingDocumentId);

            Assert.IsNotEmpty(initiatingDocument.GetLinkedDocumentIds().Concat([initiatingDocumentId]), "The holder file must be a document of the initiating project.");

            var initialErrors = await GetCompileErrorsAsync(solution).ConfigureAwait(false);
            var compilation = await initiatingDocument.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);
            var diagnostics = await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new RH4109PublicFieldCasingAnalyzer()))
                                               .GetAnalyzerDiagnosticsAsync(CancellationToken.None)
                                               .ConfigureAwait(false);
            var initiatingTree = await initiatingDocument.GetSyntaxTreeAsync(CancellationToken.None).ConfigureAwait(false);
            var diagnostic = diagnostics.Single(item => item.Location.SourceTree == initiatingTree);
            var provider = new RH4109PublicFieldCasingCodeFixProvider();
            var actions = new List<CodeAction>();

            await provider.RegisterCodeFixesAsync(new CodeFixContext(initiatingDocument,
                                                                     diagnostic,
                                                                     (action, _) => actions.Add(action),
                                                                     CancellationToken.None))
                          .ConfigureAwait(false);

            CodeAction appliedAction = null;

            if (fixAll)
            {
                var context = new FixAllContext(initiatingDocument,
                                                provider,
                                                FixAllScope.Document,
                                                actions.FirstOrDefault()?.EquivalenceKey ?? provider.GetType().Name,
                                                provider.FixableDiagnosticIds,
                                                new MultiTargetFixAllDiagnosticProvider(diagnostics),
                                                CancellationToken.None);

                appliedAction = await provider.GetFixAllProvider().GetFixAsync(context).ConfigureAwait(false);
            }
            else if (actions.Count > 0)
            {
                appliedAction = actions[0];
            }

            var fixedSolution = solution;

            if (appliedAction != null)
            {
                fixedSolution = (await appliedAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false)).OfType<ApplyChangesOperation>()
                                                                                                                      .Single()
                                                                                                                      .ChangedSolution;
            }

            var fixedSources = ImmutableArray.CreateBuilder<(string Project, string Path, string Text)>();
            var changed = false;

            foreach (var (project, path, documentId) in documentIds)
            {
                var text = await fixedSolution.GetDocument(documentId).GetTextAsync(CancellationToken.None).ConfigureAwait(false);
                var originalText = await solution.GetDocument(documentId).GetTextAsync(CancellationToken.None).ConfigureAwait(false);

                changed |= text.ToString() != originalText.ToString();

                fixedSources.Add((project, path, text.ToString()));
            }

            var fixedErrors = await GetCompileErrorsAsync(fixedSolution).ConfigureAwait(false);

            // A Fix All action that changes no document is the same outcome as a withheld fix
            var actionCount = actions.Count;

            if (fixAll)
            {
                actionCount = changed
                                  ? 1
                                  : 0;
            }

            return (actionCount, fixedSources.ToImmutable(), initialErrors, fixedErrors);
        }
    }

    /// <summary>
    /// Gets the compile errors of every project of the solution
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <returns>The errors, each with its project name</returns>
    private static async Task<ImmutableArray<string>> GetCompileErrorsAsync(Solution solution)
    {
        var errors = ImmutableArray.CreateBuilder<string>();

        foreach (var project in solution.Projects)
        {
            var compilation = await project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);

            errors.AddRange(compilation.GetDiagnostics(CancellationToken.None)
                                       .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                                       .Select(diagnostic => $"{project.Name}: {diagnostic}"));
        }

        return errors.ToImmutable();
    }

    #endregion // Tests

    #region Types

#pragma warning disable RH2101 // The provider is a focused test adapter owned by this fixture
    /// <summary>
    /// Supplies the diagnostics of the multi-targeted Fix All test
    /// </summary>
    private sealed class MultiTargetFixAllDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        #region Fields

        /// <summary>
        /// Diagnostics
        /// </summary>
        private readonly ImmutableArray<Diagnostic> _diagnostics;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="diagnostics">Diagnostics of the initiating project</param>
        public MultiTargetFixAllDiagnosticProvider(ImmutableArray<Diagnostic> diagnostics)
        {
            _diagnostics = diagnostics;
        }

        #endregion // Constructor

        #region DiagnosticProvider

        /// <inheritdoc/>
        public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            return _diagnostics.Where(diagnostic => diagnostic.Location.SourceTree == tree);
        }

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>([]);
        }

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>(_diagnostics);
        }

        #endregion // DiagnosticProvider
    }

#pragma warning restore RH2101

    #endregion // Types

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class Invoice
                                    {
                                        public int Amount;

                                        public int {|#0:amount|};

                                        public int {|#1:taxRate|};

                                        public int Total => Amount + amount + taxRate;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Invoice
                                     {
                                         public int Amount;

                                         public int {|#0:amount|};

                                         public int TaxRate;

                                         public int Total => Amount + amount + TaxRate;
                                     }
                                 }
                                 """;

        // The target name of the first reported field is already taken by a correctly cased sibling, so that
        // rename is refused while the unrelated second field is still corrected
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4109PublicFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4109MessageFormat, 2),
                                  config =>
                                  {
                                      // "amount" cannot be corrected because the correctly cased sibling already owns the target name,
                                      // so its diagnostic survives. Both paths refuse it through the same conflict check, so the batch
                                      // result needs no separate expectation
                                      config.FixedState.ExpectedDiagnostics.Add(Diagnostic(RH4109PublicFieldCasingAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(AnalyzerResources.RH4109MessageFormat));

                                      // The refused candidate keeps the batch loop running for a second, no-op pass
                                      config.NumberOfFixAllIterations = -2;
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}