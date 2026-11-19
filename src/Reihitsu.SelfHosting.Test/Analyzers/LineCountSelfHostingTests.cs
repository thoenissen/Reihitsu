using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Analyzer;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.SelfHosting.Test.Utilities;

namespace Reihitsu.SelfHosting.Test.Analyzers;

/// <summary>
/// Verifies that the self-hosting scan judges the repository's code by the line-count rules RH2008 and RH2111 as the
/// repository's build does: each test project as test code, every other project as production code, with the limits of
/// the repository's root <c>reihitsu.json</c>
/// </summary>
[TestClass]
public sealed class LineCountSelfHostingTests
{
    #region Constants

    /// <summary>
    /// Assembly name of a scanned test project
    /// </summary>
    private const string TestProjectAssemblyName = "Reihitsu.Core.Test";

    /// <summary>
    /// Assembly name of a scanned production project
    /// </summary>
    private const string ProductionProjectAssemblyName = "Reihitsu.Core";

    /// <summary>
    /// File name of the synthetic source file added to a scanned compilation
    /// </summary>
    private const string SampleFileName = "LineCountSelfHostingSample.cs";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The repository's test projects, by project name: every project that references <c>Microsoft.NET.Test.Sdk</c>
    /// </summary>
    private static readonly string[] _testProjectNames = [
                                                             "Reihitsu.Analyzer.Test",
                                                             "Reihitsu.ArchitectureTests",
                                                             "Reihitsu.Cli.Test",
                                                             "Reihitsu.Core.Test",
                                                             "Reihitsu.Formatter.Test",
                                                             "Reihitsu.Playground.Test",
                                                             "Reihitsu.SelfHosting.Test",
                                                             "Reihitsu.Tooling.Test"
                                                         ];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that the scan's project model marks exactly the repository's test projects as test projects
    /// </summary>
    [TestMethod]
    public void ProjectModelMarksExactlyTheTestProjectsAsTestProjects()
    {
        // Arrange
        var sourceDirectory = Path.Combine(RepositoryRoot.Find(), "src");

        // Act
        var testProjectNames = Directory.EnumerateFiles(sourceDirectory, "*.csproj", SearchOption.AllDirectories)
                                        .Where(path => RepositoryRoot.IsInBuildOutputPath(path) == false)
                                        .Select(SelfHostingProject.Load)
                                        .Where(project => project.IsTestProject)
                                        .Select(project => project.ProjectName)
                                        .Order(StringComparer.Ordinal)
                                        .ToList();

        // Assert
        Assert.AreSequenceEqual(_testProjectNames, testProjectNames);
    }

    /// <summary>
    /// Verifies that the scan judges a method in a test project by the default test-method limit: 120 lines of code are
    /// not reported and 121 are
    /// </summary>
    [TestMethod]
    public void ScanJudgesMethodInTestProjectByTheTestMethodLimit()
    {
        // Act
        var violations = ScanSample(new RH2008MethodsShouldNotExceedTheLineLimitAnalyzer(), TestProjectAssemblyName, CreateMethodsAround(120));

        // Assert
        Assert.HasCount(1, violations, string.Join("\n", violations));
        Assert.EndsWith("[RH2008] (RH2008MethodsShouldNotExceedTheLineLimitAnalyzer) This member has 121 lines of code, which exceeds the limit of 120.", violations[0]);
    }

    /// <summary>
    /// Verifies that the scan judges a method in a production project by the default production-method limit: 60 lines
    /// of code are not reported and 61 are
    /// </summary>
    [TestMethod]
    public void ScanJudgesMethodInProductionProjectByTheProductionMethodLimit()
    {
        // Act
        var violations = ScanSample(new RH2008MethodsShouldNotExceedTheLineLimitAnalyzer(), ProductionProjectAssemblyName, CreateMethodsAround(60));

        // Assert
        Assert.HasCount(1, violations, string.Join("\n", violations));
        Assert.EndsWith("[RH2008] (RH2008MethodsShouldNotExceedTheLineLimitAnalyzer) This member has 61 lines of code, which exceeds the limit of 60.", violations[0]);
    }

    /// <summary>
    /// Verifies that the scan judges a type in a test project by the default test-type limit, which the repository's
    /// production-type limit does not lower: 3000 lines of code are not reported and 3001 are
    /// </summary>
    [TestMethod]
    public void ScanJudgesTypeInTestProjectByTheTestTypeLimit()
    {
        // Act
        var violations = ScanSample(new RH2111TypesShouldNotExceedTheLineLimitAnalyzer(), TestProjectAssemblyName, CreateTypesAround(3000));

        // Assert
        Assert.HasCount(1, violations, string.Join("\n", violations));
        Assert.EndsWith("[RH2111] (RH2111TypesShouldNotExceedTheLineLimitAnalyzer) This type has 3001 lines of code, which exceeds the limit of 3000.", violations[0]);
    }

    /// <summary>
    /// Verifies that the scan judges a type in a production project by the repository's own production-type limit of
    /// 500 from its root <c>reihitsu.json</c>: 500 lines of code are not reported and 501 are
    /// </summary>
    [TestMethod]
    public void ScanJudgesTypeInProductionProjectByTheRepositoryTypeLimit()
    {
        // Act
        var violations = ScanSample(new RH2111TypesShouldNotExceedTheLineLimitAnalyzer(), ProductionProjectAssemblyName, CreateTypesAround(500));

        // Assert
        Assert.HasCount(1, violations, string.Join("\n", violations));
        Assert.EndsWith("[RH2111] (RH2111TypesShouldNotExceedTheLineLimitAnalyzer) This type has 501 lines of code, which exceeds the limit of 500.", violations[0]);
    }

    /// <summary>
    /// Verifies that the scan passes the repository's root <c>reihitsu.json</c> to the analyzers and that RH0001 reports
    /// nothing on it, also at a location outside the source files
    /// </summary>
    [TestMethod]
    public void RepositoryConfigurationIsPassedAndValid()
    {
        // Arrange
        var solution = SelfHostingSolution.Instance;
        var options = solution.GetAnalyzerOptions(ProductionProjectAssemblyName);
        var compilation = solution.Compilations.Single(obj => obj.AssemblyName == ProductionProjectAssemblyName);

        // Act
        var configurationFiles = options.AdditionalFiles
                                        .Where(file => string.Equals(Path.GetFileName(file.Path), "reihitsu.json", StringComparison.OrdinalIgnoreCase))
                                        .Select(file => Path.GetFullPath(file.Path))
                                        .ToList();
        var analysisOptions = new CompilationWithAnalyzersOptions(options, onAnalyzerException: null, concurrentAnalysis: true, logAnalyzerExecutionTime: false);
        var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation, [new RH0001ConfigurationFileMustBeValidAnalyzer()], analysisOptions);
        var diagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(TestContext.CancellationToken)
                                                  .GetAwaiter()
                                                  .GetResult();

        // Assert
        Assert.AreSequenceEqual(new[] { Path.Combine(solution.SolutionRoot, "reihitsu.json") }, configurationFiles);
        Assert.IsEmpty(diagnostics, string.Join("\n", diagnostics));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates a class with a method of exactly <paramref name="limit"/> lines of code and one of
    /// <paramref name="limit"/> + 1 lines
    /// </summary>
    /// <param name="limit">Limit the two methods lie at and above</param>
    /// <returns>Source of the class</returns>
    private static string CreateMethodsAround(int limit)
    {
        var builder = new StringBuilder();

        builder.Append("namespace LineCountSelfHostingSample;\n\ninternal class MethodSample\n{\n");

        AppendMethod(builder, $"M{limit}", limit);
        AppendMethod(builder, $"M{limit + 1}", limit + 1);

        builder.Append("}\n");

        return builder.ToString();
    }

    /// <summary>
    /// Appends a method whose block body holds the given number of lines of code, one statement per line
    /// </summary>
    /// <param name="builder">Builder of the class source</param>
    /// <param name="name">Name of the method</param>
    /// <param name="lines">Lines of code of the body</param>
    private static void AppendMethod(StringBuilder builder, string name, int lines)
    {
        builder.Append("    internal void ").Append(name).Append("()\n    {\n");

        for (var line = 0; line < lines; line++)
        {
            builder.Append("        ").Append(name).Append("();\n");
        }

        builder.Append("    }\n");
    }

    /// <summary>
    /// Creates a class whose body holds exactly <paramref name="limit"/> lines of code and one with
    /// <paramref name="limit"/> + 1 lines
    /// </summary>
    /// <param name="limit">Limit the two classes lie at and above</param>
    /// <returns>Source of the classes</returns>
    private static string CreateTypesAround(int limit)
    {
        var builder = new StringBuilder();

        builder.Append("namespace LineCountSelfHostingSample;\n\n");

        AppendType(builder, $"T{limit}", limit);
        AppendType(builder, $"T{limit + 1}", limit + 1);

        return builder.ToString();
    }

    /// <summary>
    /// Appends a class whose body holds the given number of lines of code, one field per line
    /// </summary>
    /// <param name="builder">Builder of the source</param>
    /// <param name="name">Name of the class</param>
    /// <param name="lines">Lines of code of the body</param>
    private static void AppendType(StringBuilder builder, string name, int lines)
    {
        builder.Append("internal class ").Append(name).Append("\n{\n");

        for (var line = 0; line < lines; line++)
        {
            builder.Append("    internal int F").Append(line).Append(";\n");
        }

        builder.Append("}\n\n");
    }

    /// <summary>
    /// Adds the source as a file of the scanned project with the given assembly name and runs the analyzer over it the
    /// way the scan does, with the analyzer options the scan derives for that project
    /// </summary>
    /// <param name="analyzer">Analyzer to run</param>
    /// <param name="assemblyName">Assembly name of a scanned project</param>
    /// <param name="source">Source of the synthetic file</param>
    /// <returns>The scan's report lines for the synthetic file</returns>
    private List<string> ScanSample(DiagnosticAnalyzer analyzer, string assemblyName, string source)
    {
        var solution = SelfHostingSolution.Instance;
        var compilation = solution.Compilations.Single(obj => obj.AssemblyName == assemblyName);
        var filePath = Path.Combine(solution.SolutionRoot, "src", assemblyName, SampleFileName);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, (CSharpParseOptions)compilation.SyntaxTrees.First().Options, filePath, cancellationToken: TestContext.CancellationToken);
        var violations = AnalyzerViolationReport.Collect(analyzer,
                                                         [compilation.AddSyntaxTrees(syntaxTree)],
                                                         obj => solution.GetAnalyzerOptions(obj.AssemblyName),
                                                         solution.SolutionRoot,
                                                         TestContext.CancellationToken);
        var relativePath = Path.GetRelativePath(solution.SolutionRoot, filePath);

        return violations.Where(violation => violation.StartsWith(relativePath + ":", StringComparison.Ordinal))
                         .ToList();
    }

    #endregion // Methods
}