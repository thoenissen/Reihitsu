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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// The compilations of every self-hostable project of the solution. They are built once per test run and shared by
/// every analyzer row; <see cref="CSharpCompilation"/> is immutable, so concurrent rows can analyze the same instance
/// </summary>
internal sealed class SelfHostingSolution
{
    #region Fields

    /// <summary>
    /// The shared instance, built on first use; <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> lets exactly
    /// one thread build it while concurrent rows wait for the result, and a build failure is rethrown to every row
    /// </summary>
    private static readonly Lazy<SelfHostingSolution> _instance = new(Create, LazyThreadSafetyMode.ExecutionAndPublication);

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SelfHostingSolution"/> class
    /// </summary>
    /// <param name="solutionRoot">Solution root path</param>
    /// <param name="compilations">Project compilations, in project-path order</param>
    private SelfHostingSolution(string solutionRoot, ImmutableArray<CSharpCompilation> compilations)
    {
        SolutionRoot = solutionRoot;
        Compilations = compilations;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The shared instance
    /// </summary>
    internal static SelfHostingSolution Instance => _instance.Value;

    /// <summary>
    /// Absolute path to the solution root
    /// </summary>
    internal string SolutionRoot { get; }

    /// <summary>
    /// One compilation per self-hostable project, each holding only that project's own syntax trees
    /// </summary>
    internal ImmutableArray<CSharpCompilation> Compilations { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Builds the compilations of every self-hostable project
    /// </summary>
    /// <returns>The created instance</returns>
    private static SelfHostingSolution Create()
    {
        var solutionRoot = RepositoryRoot.Find();
        var projects = DiscoverProjects(solutionRoot).ToArray();

        if (projects.Length == 0)
        {
            Assert.Fail("No C# projects with source files were discovered. Check project discovery logic.");
        }

        var projectsByFilePath = projects.ToDictionary(project => project.ProjectFilePath, StringComparer.OrdinalIgnoreCase);
        var compilationCache = new Dictionary<string, CSharpCompilation>(StringComparer.OrdinalIgnoreCase);
        var compilationStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var compilations = projects.Select(project => CreateCompilation(project, projectsByFilePath, compilationCache, compilationStack, solutionRoot))
                                   .ToImmutableArray();

        return new SelfHostingSolution(solutionRoot, compilations);
    }

    /// <summary>
    /// Discovers all C# projects in the repository that contain source files
    /// </summary>
    /// <param name="solutionRoot">The absolute path to the solution root</param>
    /// <returns>Discovered projects</returns>
    private static IEnumerable<SelfHostingProject> DiscoverProjects(string solutionRoot)
    {
        var candidateProjects = Directory.EnumerateFiles(solutionRoot, "*.csproj", SearchOption.AllDirectories)
                                         .Where(path => RepositoryRoot.IsInBuildOutputPath(path) == false)
                                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                                         .Select(SelfHostingProject.Load)
                                         .ToArray();
        var unhostableProjectFilePaths = candidateProjects.Where(project => IsSelfHostable(project) == false)
                                                          .Select(project => project.ProjectFilePath)
                                                          .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var project in candidateProjects)
        {
            if (unhostableProjectFilePaths.Contains(project.ProjectFilePath))
            {
                continue;
            }

            if (project.ProjectReferencePaths.Any(unhostableProjectFilePaths.Contains))
            {
                continue;
            }

            if (EnumerateSourceFiles(project.ProjectDirectoryPath, solutionRoot).Any())
            {
                yield return project;
            }
        }
    }

    /// <summary>
    /// Determines whether a project's real compilation can be modeled by this harness, which parses raw source
    /// files and literal project-file metadata without running MSBuild targets or source generators
    /// </summary>
    /// <param name="project">Project metadata</param>
    /// <returns><see langword="true"/> if the project uses the plain C# SDK; otherwise, <see langword="false"/></returns>
    private static bool IsSelfHostable(SelfHostingProject project)
    {
        return string.Equals(project.Sdk, "Microsoft.NET.Sdk", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a compilation for the specified project, including transitive project references
    /// </summary>
    /// <param name="project">Project metadata</param>
    /// <param name="projectsByFilePath">All discovered projects keyed by project file path</param>
    /// <param name="compilationCache">Compilation cache</param>
    /// <param name="compilationStack">Projects currently being compiled</param>
    /// <param name="solutionRoot">Solution root path</param>
    /// <returns>The created compilation</returns>
    private static CSharpCompilation CreateCompilation(SelfHostingProject project,
                                                       IReadOnlyDictionary<string, SelfHostingProject> projectsByFilePath,
                                                       IDictionary<string, CSharpCompilation> compilationCache,
                                                       ISet<string> compilationStack,
                                                       string solutionRoot)
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
            var syntaxTrees = ParseSourceTrees(project, solutionRoot).ToArray();

            if (syntaxTrees.Length == 0)
            {
                Assert.Fail($"Project '{project.ProjectName}' contains no source files for self-hosting.");
            }

            var compilation = CSharpCompilation.Create(project.AssemblyName)
                                               .WithOptions(new CSharpCompilationOptions(project.OutputKind))
                                               .AddReferences(GetCompilationReferences(project, projectsByFilePath, compilationCache, compilationStack, solutionRoot))
                                               .AddSyntaxTrees(syntaxTrees);

            AssertNoCompilationErrors(compilation, project, solutionRoot);

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

            if (RepositoryRoot.IsInBuildOutputPath(relativePath)
                || RepositoryRoot.IsGeneratedSourceFile(relativePath))
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
    /// <returns>Parsed syntax trees</returns>
    private static IEnumerable<SyntaxTree> ParseSourceTrees(SelfHostingProject project, string solutionRoot)
    {
        foreach (var file in EnumerateSourceFiles(project.ProjectDirectoryPath, solutionRoot))
        {
            var content = File.ReadAllText(file);
            var syntaxTree = CSharpSyntaxTree.ParseText(content, path: file);

            if (syntaxTree.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                Assert.Fail($"Failed to parse source file with syntax errors: {file}");
            }

            yield return syntaxTree;
        }
    }

    /// <summary>
    /// Gets the references needed for compilation
    /// </summary>
    /// <param name="project">Project metadata</param>
    /// <param name="projectsByFilePath">All discovered projects keyed by project file path</param>
    /// <param name="compilationCache">Compilation cache</param>
    /// <param name="compilationStack">Projects currently being compiled</param>
    /// <param name="solutionRoot">Solution root path</param>
    /// <returns>An enumerable of metadata references</returns>
    private static List<MetadataReference> GetCompilationReferences(SelfHostingProject project,
                                                                    IReadOnlyDictionary<string, SelfHostingProject> projectsByFilePath,
                                                                    IDictionary<string, CSharpCompilation> compilationCache,
                                                                    ISet<string> compilationStack,
                                                                    string solutionRoot)
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

            references.Add(CreateCompilation(referencedProject, projectsByFilePath, compilationCache, compilationStack, solutionRoot).ToMetadataReference());
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
    private static void AssertNoCompilationErrors(CSharpCompilation compilation, SelfHostingProject project, string solutionRoot)
    {
        var errors = compilation.GetDiagnostics()
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