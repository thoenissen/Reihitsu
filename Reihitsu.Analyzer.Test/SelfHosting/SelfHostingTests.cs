using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Self-hosting tests that run all Reihitsu analyzers over the solution's own source code
/// These tests verify that analyzers report no violations on correctly-formatted code,
/// serving as regression tests to detect unintended analyzer behavior changes
/// </summary>
[TestClass]
public class SelfHostingTests
{
    #region Constants

    /// <summary>
    /// Diagnostic IDs excluded from self-hosting because the relevant source tree has not been migrated yet
    /// </summary>
    private static readonly ImmutableHashSet<string> _excludedDiagnosticIds = ["RH8402", "RH7409", "RH7410", "RH7411", "RH7412"];

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that all Reihitsu analyzers report no violations on the solution's own source code
    /// Any violations found indicate a potential regression in analyzer behavior
    /// </summary>
    [TestMethod]
    public void AnalyzersReportNoViolationsOnOwnSourceCode()
    {
        var solutionRoot = FindSolutionRoot();
        var analyzers = DiscoverAnalyzers().Where(IsIncludedInSelfHosting).ToList();
        var projects = DiscoverProjects(solutionRoot).ToArray();
        var projectsByFilePath = projects.ToDictionary(project => project.ProjectFilePath, StringComparer.OrdinalIgnoreCase);
        var compilationCache = new Dictionary<string, CSharpCompilation>(StringComparer.OrdinalIgnoreCase);
        var compilationStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var diagnosticsByFile = new Dictionary<string, List<(Diagnostic Diagnostic, string AnalyzerName)>>();

        if (analyzers.Count == 0)
        {
            Assert.Fail("No DiagnosticAnalyzer types were discovered. Check analyzer discovery logic.");
        }

        if (projects.Length == 0)
        {
            Assert.Fail("No C# projects with source files were discovered. Check project discovery logic.");
        }

        foreach (var project in projects)
        {
            TestContext.CancellationToken.ThrowIfCancellationRequested();

            var compilation = CreateCompilation(project, projectsByFilePath, compilationCache, compilationStack, solutionRoot, TestContext.CancellationToken);

            foreach (var analyzer in analyzers)
            {
                TestContext.CancellationToken.ThrowIfCancellationRequested();

                var diagnostics = RunAnalyzer(compilation, analyzer, TestContext.CancellationToken);

                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic.Location.SourceTree == null)
                    {
                        continue;
                    }

                    var filePath = diagnostic.Location.SourceTree.FilePath;

                    if (diagnosticsByFile.ContainsKey(filePath) == false)
                    {
                        diagnosticsByFile[filePath] = [];
                    }

                    diagnosticsByFile[filePath].Add((diagnostic, analyzer.GetType().Name));
                }
            }
        }

        if (diagnosticsByFile.Count == 0)
        {
            return;
        }

        var failures = new List<string>();

        foreach (var (file, diagnostics) in diagnosticsByFile.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase))
        {
            var relativePath = Path.GetRelativePath(solutionRoot, file);

            foreach (var (diagnostic, analyzerName) in diagnostics.OrderBy(entry => entry.Diagnostic.Location.SourceSpan.Start))
            {
                var lineSpan = diagnostic.Location.GetLineSpan();
                var lineNumber = lineSpan.StartLinePosition.Line + 1;

                failures.Add($"{relativePath}:{lineNumber} [{diagnostic.Id}] ({analyzerName}) {diagnostic.GetMessage()}");
            }
        }

        Assert.Fail($"Analyzers reported {failures.Count} violation(s):\n{string.Join("\n", failures)}");
    }

    /// <summary>
    /// Finds the solution root directory by walking up from the test assembly's location
    /// </summary>
    /// <returns>The absolute path to the solution root directory</returns>
    private static string FindSolutionRoot()
    {
        var directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "Reihitsu.sln")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new InvalidOperationException("Could not find solution root (Reihitsu.sln).");
    }

    /// <summary>
    /// Discovers all C# projects in the repository that contain source files
    /// </summary>
    /// <param name="solutionRoot">The absolute path to the solution root</param>
    /// <returns>Discovered projects</returns>
    private static IEnumerable<SelfHostingProject> DiscoverProjects(string solutionRoot)
    {
        foreach (var projectFile in Directory.EnumerateFiles(solutionRoot, "*.csproj", SearchOption.AllDirectories)
                                             .Where(path => IsInBuildOutputPath(path) == false)
                                             .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var project = SelfHostingProject.Load(projectFile);

            if (EnumerateSourceFiles(project.ProjectDirectoryPath, solutionRoot).Any())
            {
                yield return project;
            }
        }
    }

    /// <summary>
    /// Creates a compilation for the specified project, including transitive project references
    /// </summary>
    /// <param name="project">Project metadata</param>
    /// <param name="projectsByFilePath">All discovered projects keyed by project file path</param>
    /// <param name="compilationCache">Compilation cache</param>
    /// <param name="compilationStack">Projects currently being compiled</param>
    /// <param name="solutionRoot">Solution root path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created compilation</returns>
    private static CSharpCompilation CreateCompilation(SelfHostingProject project,
                                                       IReadOnlyDictionary<string, SelfHostingProject> projectsByFilePath,
                                                       IDictionary<string, CSharpCompilation> compilationCache,
                                                       ISet<string> compilationStack,
                                                       string solutionRoot,
                                                       CancellationToken cancellationToken)
    {
        if (compilationCache.TryGetValue(project.ProjectFilePath, out var cachedCompilation))
        {
            return cachedCompilation;
        }

        if (compilationStack.Add(project.ProjectFilePath) == false)
        {
            Assert.Fail($"Detected a cyclic project-reference graph while compiling '{project.ProjectName}'.");
        }

        try
        {
            var syntaxTrees = ParseSourceTrees(project, solutionRoot, cancellationToken).ToArray();

            if (syntaxTrees.Length == 0)
            {
                Assert.Fail($"Project '{project.ProjectName}' contains no source files for self-hosting.");
            }

            var compilation = CSharpCompilation.Create(project.AssemblyName)
                                               .WithOptions(new CSharpCompilationOptions(project.OutputKind))
                                               .AddReferences(GetCompilationReferences(project, projectsByFilePath, compilationCache, compilationStack, solutionRoot, cancellationToken))
                                               .AddSyntaxTrees(syntaxTrees);

            AssertNoCompilationErrors(compilation, project, solutionRoot, cancellationToken);

            compilationCache[project.ProjectFilePath] = compilation;

            return compilation;
        }
        finally
        {
            compilationStack.Remove(project.ProjectFilePath);
        }
    }

    /// <summary>
    /// Enumerates all C# source files in the project directory, excluding generated files and build outputs
    /// </summary>
    /// <param name="projectDirectoryPath">Project directory path</param>
    /// <param name="solutionRoot">The absolute path to the solution root</param>
    /// <returns>An enumerable of absolute file paths</returns>
    private static IEnumerable<string> EnumerateSourceFiles(string projectDirectoryPath, string solutionRoot)
    {
        foreach (var file in Directory.EnumerateFiles(projectDirectoryPath, "*.cs", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(solutionRoot, file);

            if (IsInBuildOutputPath(relativePath))
            {
                continue;
            }

            if (relativePath.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
                || relativePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
                || relativePath.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            yield return file;
        }
    }

    /// <summary>
    /// Parses the source files in the specified project into syntax trees
    /// </summary>
    /// <param name="project">Project metadata</param>
    /// <param name="solutionRoot">Solution root path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parsed syntax trees</returns>
    private static IEnumerable<SyntaxTree> ParseSourceTrees(SelfHostingProject project, string solutionRoot, CancellationToken cancellationToken)
    {
        foreach (var file in EnumerateSourceFiles(project.ProjectDirectoryPath, solutionRoot))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = File.ReadAllText(file);
            var syntaxTree = CSharpSyntaxTree.ParseText(content, path: file, cancellationToken: cancellationToken);

            if (syntaxTree.GetDiagnostics(cancellationToken).Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                Assert.Fail($"Failed to parse source file with syntax errors: {file}");
            }

            yield return syntaxTree;
        }
    }

    /// <summary>
    /// Discovers all DiagnosticAnalyzer types from the Reihitsu.Analyzer assembly
    /// </summary>
    /// <returns>An enumerable of DiagnosticAnalyzer types</returns>
    private static IEnumerable<DiagnosticAnalyzer> DiscoverAnalyzers()
    {
        return typeof(DiagnosticAnalyzerBase<>).Assembly
                                               .GetTypes()
                                               .Where(type => type.IsAbstract is false
                                                              && type.IsInterface is false
                                                              && typeof(DiagnosticAnalyzer).IsAssignableFrom(type)
                                                              && type.GetCustomAttribute<DiagnosticAnalyzerAttribute>() is not null)
                                               .Select(CreateAnalyzer);
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
    /// Determines whether the analyzer should participate in self-hosting validation
    /// </summary>
    /// <param name="analyzer">Analyzer</param>
    /// <returns><see langword="true"/> if the analyzer is included in self-hosting</returns>
    private static bool IsIncludedInSelfHosting(DiagnosticAnalyzer analyzer)
    {
        return analyzer.SupportedDiagnostics.Any(diagnostic => _excludedDiagnosticIds.Contains(diagnostic.Id)) == false;
    }

    /// <summary>
    /// Runs a single analyzer on the given compilation and collects diagnostics
    /// </summary>
    /// <param name="compilation">The compilation to analyze</param>
    /// <param name="analyzer">The analyzer to run</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An enumerable of reported diagnostics</returns>
    private static ImmutableArray<Diagnostic> RunAnalyzer(CSharpCompilation compilation, DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
    {
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken).Result;
    }

    /// <summary>
    /// Gets the references needed for compilation
    /// </summary>
    /// <param name="project">Project metadata</param>
    /// <param name="projectsByFilePath">All discovered projects keyed by project file path</param>
    /// <param name="compilationCache">Compilation cache</param>
    /// <param name="compilationStack">Projects currently being compiled</param>
    /// <param name="solutionRoot">Solution root path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An enumerable of metadata references</returns>
    private static List<MetadataReference> GetCompilationReferences(SelfHostingProject project,
                                                                    IReadOnlyDictionary<string, SelfHostingProject> projectsByFilePath,
                                                                    IDictionary<string, CSharpCompilation> compilationCache,
                                                                    ISet<string> compilationStack,
                                                                    string solutionRoot,
                                                                    CancellationToken cancellationToken)
    {
        var referencePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddTrustedPlatformAssemblyReferences(referencePaths);
        AddLoadedAssemblyReferences(referencePaths);
        AddPackageReferencesFromAssets(project, referencePaths);
        referencePaths.RemoveWhere(referencePath => projectsByFilePath.Values
                                                                      .Select(projectMetadata => projectMetadata.AssemblyName)
                                                                      .Contains(Path.GetFileNameWithoutExtension(referencePath), StringComparer.OrdinalIgnoreCase));

        var references = referencePaths.Select(path => MetadataReference.CreateFromFile(path))
                                       .Cast<MetadataReference>()
                                       .ToList();

        foreach (var projectReferencePath in project.ProjectReferencePaths)
        {
            if (projectsByFilePath.TryGetValue(projectReferencePath, out var referencedProject) == false)
            {
                Assert.Fail($"Project '{project.ProjectName}' references undiscovered project '{projectReferencePath}'.");
            }

            references.Add(CreateCompilation(referencedProject, projectsByFilePath, compilationCache, compilationStack, solutionRoot, cancellationToken).ToMetadataReference());
        }

        return references;
    }

    /// <summary>
    /// Adds compile-time package references from the project's assets file
    /// </summary>
    /// <param name="project">Project metadata</param>
    /// <param name="referencePaths">Reference paths</param>
    private static void AddPackageReferencesFromAssets(SelfHostingProject project, HashSet<string> referencePaths)
    {
        if (File.Exists(project.ProjectAssetsPath) == false)
        {
            Assert.Fail($"Could not find project assets file for '{project.ProjectName}': {project.ProjectAssetsPath}");
        }

        using (var projectAssetsDocument = JsonDocument.Parse(File.ReadAllText(project.ProjectAssetsPath)))
        {
            var rootElement = projectAssetsDocument.RootElement;
            var targetElement = GetAssetsTarget(rootElement, project);
            var libraryElement = rootElement.GetProperty("libraries");
            var packageFolderPaths = rootElement.GetProperty("packageFolders")
                                                .EnumerateObject()
                                                .Select(packageFolder => packageFolder.Name)
                                                .ToArray();

            foreach (var targetLibrary in targetElement.EnumerateObject())
            {
                if (libraryElement.TryGetProperty(targetLibrary.Name, out var library) == false)
                {
                    continue;
                }

                if (string.Equals(library.GetProperty("type").GetString(),
                                  "package",
                                  StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                if (targetLibrary.Value.TryGetProperty("compile", out var compileAssets) == false)
                {
                    continue;
                }

                var packagePath = library.GetProperty("path").GetString();

                if (string.IsNullOrWhiteSpace(packagePath))
                {
                    continue;
                }

                foreach (var compileAsset in compileAssets.EnumerateObject())
                {
                    var compileAssetPath = compileAsset.Name.Replace('/', Path.DirectorySeparatorChar);

                    if (string.Equals(Path.GetFileName(compileAssetPath), "_._", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    foreach (var packageFolderPath in packageFolderPaths)
                    {
                        var fullPath = Path.Combine(packageFolderPath, packagePath, compileAssetPath);

                        if (File.Exists(fullPath))
                        {
                            if (ContainsReferenceAssembly(referencePaths, fullPath))
                            {
                                break;
                            }

                            referencePaths.Add(fullPath);

                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determines whether the reference set already contains an assembly with the same simple name
    /// </summary>
    /// <param name="referencePaths">Reference paths</param>
    /// <param name="candidatePath">Candidate assembly path</param>
    /// <returns><see langword="true"/> if the assembly name is already present; otherwise, <see langword="false"/></returns>
    private static bool ContainsReferenceAssembly(HashSet<string> referencePaths, string candidatePath)
    {
        var candidateAssemblyName = Path.GetFileNameWithoutExtension(candidatePath);

        return referencePaths.Any(referencePath => string.Equals(Path.GetFileNameWithoutExtension(referencePath), candidateAssemblyName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the assets target that matches the project's target framework
    /// </summary>
    /// <param name="rootElement">Assets root element</param>
    /// <param name="project">Project metadata</param>
    /// <returns>The matching target element</returns>
    private static JsonElement GetAssetsTarget(JsonElement rootElement, SelfHostingProject project)
    {
        var targets = rootElement.GetProperty("targets")
                                 .EnumerateObject()
                                 .ToArray();

        foreach (var target in targets)
        {
            if (target.Name.StartsWith(project.TargetFramework, StringComparison.OrdinalIgnoreCase))
            {
                return target.Value;
            }
        }

        if (targets.Length == 1)
        {
            return targets[0].Value;
        }

        Assert.Fail($"Could not find a target named '{project.TargetFramework}' in assets file '{project.ProjectAssetsPath}'.");

        return default;
    }

    /// <summary>
    /// Asserts that the self-hosting compilation can be fully bound
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="project">Project metadata</param>
    /// <param name="solutionRoot">Solution root path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private static void AssertNoCompilationErrors(CSharpCompilation compilation, SelfHostingProject project, string solutionRoot, CancellationToken cancellationToken)
    {
        var errors = compilation.GetDiagnostics(cancellationToken)
                                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                                .OrderBy(diagnostic => diagnostic.Location.GetLineSpan().Path, StringComparer.OrdinalIgnoreCase)
                                .ThenBy(diagnostic => diagnostic.Location.SourceSpan.Start)
                                .ToList();

        if (errors.Count == 0)
        {
            return;
        }

        var messages = new List<string>();

        foreach (var diagnostic in errors.Take(200))
        {
            if (diagnostic.Location.SourceTree != null)
            {
                var lineSpan = diagnostic.Location.GetLineSpan();
                var relativePath = Path.GetRelativePath(solutionRoot, lineSpan.Path);
                var lineNumber = lineSpan.StartLinePosition.Line + 1;

                messages.Add($"{relativePath}:{lineNumber} [{diagnostic.Id}] {diagnostic.GetMessage()}");
            }
            else
            {
                messages.Add($"[no source] [{diagnostic.Id}] {diagnostic.GetMessage()}");
            }
        }

        Assert.Fail($"Self-hosting compilation for project '{project.ProjectName}' reported {errors.Count} error(s). This usually indicates missing references or invalid source:\n{string.Join("\n", messages)}");
    }

    /// <summary>
    /// Determines whether a path is within a build output directory
    /// </summary>
    /// <param name="path">Path to inspect</param>
    /// <returns><see langword="true"/> if the path is inside bin or obj; otherwise, <see langword="false"/></returns>
    private static bool IsInBuildOutputPath(string path)
    {
        return path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
               || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds trusted platform assemblies to the compilation reference set
    /// </summary>
    /// <param name="referencePaths">Reference paths</param>
    private static void AddTrustedPlatformAssemblyReferences(HashSet<string> referencePaths)
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;

        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            return;
        }

        foreach (var assemblyPath in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            referencePaths.Add(assemblyPath);
        }
    }

    /// <summary>
    /// Adds currently loaded assemblies to the compilation reference set
    /// </summary>
    /// <param name="referencePaths">Reference paths</param>
    private static void AddLoadedAssemblyReferences(HashSet<string> referencePaths)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            AddAssemblyReferencePath(assembly, referencePaths);
        }
    }

    /// <summary>
    /// Adds a single assembly location if it is usable as a metadata reference
    /// </summary>
    /// <param name="assembly">Assembly</param>
    /// <param name="referencePaths">Reference paths</param>
    private static void AddAssemblyReferencePath(Assembly assembly, HashSet<string> referencePaths)
    {
        if (assembly.IsDynamic)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(assembly.Location))
        {
            return;
        }

        if (File.Exists(assembly.Location) == false)
        {
            return;
        }

        referencePaths.Add(assembly.Location);
    }

    #endregion // Methods
}