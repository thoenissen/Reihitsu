using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Self-hosting tests that run all Reihitsu analyzers over the analyzer's own source code.
/// These tests verify that analyzers report no violations on correctly-formatted code,
/// serving as regression tests to detect unintended analyzer behavior changes.
/// </summary>
[TestClass]
public class SelfHostingTests
{
    #region Constants

    /// <summary>
    /// Directories to scan for C# files (relative to the solution root).
    /// These are limited to the analyzer projects to avoid conflicts with Formatter self-hosting tests.
    /// </summary>
    private static readonly string[] SourceDirectories = ["Reihitsu.Analyzer", "Reihitsu.Analyzer.CodeFixes"];

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that all Reihitsu analyzers report no violations on the analyzer's own source code.
    /// Any violations found indicate a potential regression in analyzer behavior.
    /// </summary>
    [TestMethod]
    public void AnalyzersReportNoViolationsOnOwnSourceCode()
    {
        var solutionRoot = FindSolutionRoot();
        var sourceFiles = EnumerateSourceFiles(solutionRoot).ToList();
        var analyzers = DiscoverAnalyzers().ToList();

        if (analyzers.Count == 0)
        {
            Assert.Fail("No DiagnosticAnalyzer types were discovered. Check analyzer discovery logic.");
        }

        var syntaxTrees = new List<SyntaxTree>();
        var diagnosticsByFile = new Dictionary<string, List<(Diagnostic, string AnalyzerName)>>();

        foreach (var file in sourceFiles)
        {
            TestContext.CancellationTokenSource.Token.ThrowIfCancellationRequested();

            var content = File.ReadAllText(file);
            var syntaxTree = CSharpSyntaxTree.ParseText(content, path: file, cancellationToken: TestContext.CancellationTokenSource.Token);

            if (syntaxTree.GetDiagnostics(TestContext.CancellationTokenSource.Token).Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                Assert.Fail($"Failed to parse source file with syntax errors: {file}");
            }

            syntaxTrees.Add(syntaxTree);
        }

        if (syntaxTrees.Count == 0)
        {
            Assert.Fail("No valid source files were found or parsed. Check EnumerateSourceFiles and parse logic.");
        }

        var compilation = CSharpCompilation.Create("Reihitsu.Analyzer")
                                           .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                           .AddReferences(GetCompilationReferences())
                                           .AddSyntaxTrees(syntaxTrees);

        AssertNoCompilationErrors(compilation, solutionRoot, TestContext.CancellationTokenSource.Token);

        foreach (var analyzer in analyzers)
        {
            TestContext.CancellationTokenSource.Token.ThrowIfCancellationRequested();

            var diagnostics = RunAnalyzer(compilation, analyzer, TestContext.CancellationTokenSource.Token);

            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Location.SourceTree != null)
                {
                    var filePath = diagnostic.Location.SourceTree.FilePath;

                    if (diagnosticsByFile.ContainsKey(filePath) == false)
                    {
                        diagnosticsByFile[filePath] = [];
                    }

                    diagnosticsByFile[filePath].Add((diagnostic, analyzer.GetType().Name));
                }
            }
        }

        if (diagnosticsByFile.Count > 0)
        {
            var failures = new List<string>();

            foreach (var (file, diagnostics) in diagnosticsByFile.OrderBy(x => x.Key))
            {
                var relativePath = Path.GetRelativePath(solutionRoot, file);

                foreach (var (diagnostic, analyzerName) in diagnostics.OrderBy(x => x.Item1.Location.SourceSpan.Start))
                {
                    var lineSpan = diagnostic.Location.GetLineSpan();
                    var lineNumber = lineSpan.StartLinePosition.Line + 1;
                    var message = $"{relativePath}:{lineNumber} [{diagnostic.Id}] ({analyzerName}) {diagnostic.GetMessage()}";

                    failures.Add(message);
                }
            }

            Assert.Fail($"Analyzers reported {failures.Count} violation(s):\n{string.Join("\n", failures)}");
        }
    }

    /// <summary>
    /// Finds the solution root directory by walking up from the test assembly's location.
    /// </summary>
    /// <returns>The absolute path to the solution root directory.</returns>
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
    /// Enumerates all C# source files in the configured source directories,
    /// excluding auto-generated files and bin/obj directories.
    /// </summary>
    /// <param name="solutionRoot">The absolute path to the solution root.</param>
    /// <returns>An enumerable of absolute file paths.</returns>
    private static IEnumerable<string> EnumerateSourceFiles(string solutionRoot)
    {
        foreach (var dir in SourceDirectories)
        {
            var fullPath = Path.Combine(solutionRoot, dir);

            if (Directory.Exists(fullPath) == false)
            {
                Assert.Fail($"Source directory does not exist: {fullPath}");
            }

            foreach (var file in Directory.EnumerateFiles(fullPath, "*.cs", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(solutionRoot, file);

                if (relativePath.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                    || relativePath.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                {
                    continue;
                }

                if (relativePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (relativePath.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return file;
            }
        }
    }

    /// <summary>
    /// Discovers all DiagnosticAnalyzer types from the Reihitsu.Analyzer assembly.
    /// </summary>
    /// <returns>An enumerable of DiagnosticAnalyzer types.</returns>
    private static IEnumerable<DiagnosticAnalyzer> DiscoverAnalyzers()
    {
        var analyzerAssembly = typeof(Reihitsu.Analyzer.Base.DiagnosticAnalyzerBase<>).Assembly;

        var analyzerTypes = analyzerAssembly.GetTypes()
                                            .Where(type => type.IsAbstract is false
                                                           && type.IsInterface is false
                                                           && typeof(DiagnosticAnalyzer).IsAssignableFrom(type)
                                                           && type.GetCustomAttribute<DiagnosticAnalyzerAttribute>() is not null);

        foreach (var analyzerType in analyzerTypes)
        {
            DiagnosticAnalyzer analyzer = null;

            try
            {
                analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
            }
            catch
            {
                Assert.Fail($"Failed to create instance of analyzer type: {analyzerType.FullName}. Check that it has a public parameterless constructor.");
            }

            yield return analyzer;
        }
    }

    /// <summary>
    /// Runs a single analyzer on the given compilation and collects diagnostics.
    /// </summary>
    /// <param name="compilation">The compilation to analyze.</param>
    /// <param name="analyzer">The analyzer to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An enumerable of reported diagnostics.</returns>
    private static ImmutableArray<Diagnostic> RunAnalyzer(CSharpCompilation compilation, DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
    {
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken).Result;
    }

    /// <summary>
    /// Gets the references needed for compilation.
    /// </summary>
    /// <returns>An enumerable of MetadataReferences.</returns>
    private static IEnumerable<MetadataReference> GetCompilationReferences()
    {
        var referencePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddTrustedPlatformAssemblyReferences(referencePaths);
        AddLoadedAssemblyReferences(referencePaths);
        AddReferencedAssemblies(typeof(Reihitsu.Analyzer.Base.DiagnosticAnalyzerBase<>).Assembly, referencePaths);
        AddReferencedAssemblies(typeof(DiagnosticAnalyzer).Assembly, referencePaths);

        return referencePaths.Select(path => MetadataReference.CreateFromFile(path));
    }

    /// <summary>
    /// Asserts that the self-hosting compilation can be fully bound.
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="solutionRoot">Solution root path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private static void AssertNoCompilationErrors(Compilation compilation, string solutionRoot, CancellationToken cancellationToken)
    {
        var errors = compilation.GetDiagnostics(cancellationToken)
                                .Where(obj => obj.Severity == DiagnosticSeverity.Error)
                                .OrderBy(obj => obj.Location.GetLineSpan().Path, StringComparer.OrdinalIgnoreCase)
                                .ThenBy(obj => obj.Location.SourceSpan.Start)
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

        Assert.Fail($"Self-hosting compilation reported {errors.Count} error(s). This usually indicates missing references or invalid source:\n{string.Join("\n", messages)}");
    }

    /// <summary>
    /// Adds trusted platform assemblies to the compilation reference set.
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
    /// Adds currently loaded assemblies to the compilation reference set.
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
    /// Adds the assembly and its transitive referenced assemblies.
    /// </summary>
    /// <param name="rootAssembly">Root assembly</param>
    /// <param name="referencePaths">Reference paths</param>
    private static void AddReferencedAssemblies(Assembly rootAssembly, HashSet<string> referencePaths)
    {
        var queue = new Queue<Assembly>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        queue.Enqueue(rootAssembly);

        while (queue.Count > 0)
        {
            var assembly = queue.Dequeue();
            var assemblyName = assembly.FullName ?? assembly.GetName().Name;

            if (string.IsNullOrWhiteSpace(assemblyName)
                || visited.Add(assemblyName) == false)
            {
                continue;
            }

            AddAssemblyReferencePath(assembly, referencePaths);

            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                try
                {
                    queue.Enqueue(Assembly.Load(referencedAssemblyName));
                }
                catch
                {
                    Assert.Fail($"Failed to load referenced assembly: {referencedAssemblyName.FullName}. Check that all dependencies are available.");
                }
            }
        }
    }

    /// <summary>
    /// Adds a single assembly location if it is usable as metadata reference.
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