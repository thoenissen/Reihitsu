using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.SelfHosting.Test.Utilities;

namespace Reihitsu.SelfHosting.Test.Analyzers;

/// <summary>
/// Verifies the analyzer rows of the self-hosting scan and the report a failing row prints
/// </summary>
[TestClass]
public sealed class AnalyzerViolationReportTests
{
    #region Fields

    /// <summary>
    /// Root that the synthetic source files are placed under; it is only used as a path prefix and never created
    /// </summary>
    private static readonly string _solutionRoot = Path.Combine(Path.GetTempPath(), "reihitsu-self-hosting-report");

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the analyzer row names are distinct, so a row name identifies exactly one analyzer, and that a known analyzer has a row
    /// </summary>
    [TestMethod]
    public void AnalyzerRowsNameEachAnalyzerTypeExactlyOnce()
    {
        // Act
        var names = AnalyzerSelfHostingTests.AnalyzerNames.ToList();

        // Assert
        Assert.Contains(nameof(RH4106PrivateFieldCasingAnalyzer), names);
        Assert.HasCount(names.Count, names.Distinct(StringComparer.Ordinal).ToList(), "Two analyzer rows share a name.");
    }

    /// <summary>
    /// Verifies that the rows are exactly the C# analyzers Roslyn's own discovery finds in the analyzer assembly, the way
    /// the compiler loads them from the package: no analyzer the compiler runs goes without a row, and no row runs a type
    /// the compiler would not load
    /// </summary>
    [TestMethod]
    public void AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly()
    {
        // Arrange
        var analyzerAssembly = typeof(DiagnosticAnalyzerBase).Assembly;
        var reference = new AnalyzerFileReference(analyzerAssembly.Location, new LoadedAnalyzerAssemblyLoader(analyzerAssembly));
        var loadFailures = new List<string>();

        reference.AnalyzerLoadFailed += (_, args) => loadFailures.Add($"{args.TypeName}: {args.ErrorCode} {args.Message}");

        // Act
        var compilerAnalyzerNames = reference.GetAnalyzers(LanguageNames.CSharp)
                                             .Select(analyzer => analyzer.GetType().Name)
                                             .ToHashSet(StringComparer.Ordinal);
        var rowNames = AnalyzerSelfHostingTests.AnalyzerNames.ToHashSet(StringComparer.Ordinal);
        var analyzersWithoutRow = compilerAnalyzerNames.Except(rowNames, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var rowsWithoutAnalyzer = rowNames.Except(compilerAnalyzerNames, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();

        // Assert
        Assert.IsEmpty(loadFailures, $"Roslyn failed to load analyzers:\n{string.Join("\n", loadFailures)}");
        Assert.IsGreaterThan(0, compilerAnalyzerNames.Count, "Roslyn found no analyzer in the analyzer assembly.");
        Assert.IsEmpty(analyzersWithoutRow, $"Analyzers the compiler loads but no row runs: {string.Join(", ", analyzersWithoutRow)}");
        Assert.IsEmpty(rowsWithoutAnalyzer, $"Rows for types the compiler does not load as analyzers: {string.Join(", ", rowsWithoutAnalyzer)}");
    }

    /// <summary>
    /// Verifies that the line-count rules RH2008 and RH2111 have a row: the repository's own build judges its code by
    /// both rules, so the scan judges its source against them too
    /// </summary>
    [TestMethod]
    public void LineCountAnalyzersHaveARow()
    {
        // Act
        var names = AnalyzerSelfHostingTests.AnalyzerNames.ToList();

        // Assert
        Assert.Contains(nameof(RH2008MethodsShouldNotExceedTheLineLimitAnalyzer), names);
        Assert.Contains(nameof(RH2111TypesShouldNotExceedTheLineLimitAnalyzer), names);
    }

    /// <summary>
    /// Verifies that the complexity rule RH2009 has a row like every other analyzer
    /// </summary>
    [TestMethod]
    public void ComplexityAnalyzerHasARow()
    {
        // Act
        var names = AnalyzerSelfHostingTests.AnalyzerNames.ToList();

        // Assert
        Assert.Contains(nameof(RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer), names);
    }

    /// <summary>
    /// Verifies that the scan leaves out what RH2009 reports, so the repository's own code is not held to the
    /// complexity rule yet, while the analyzer itself does report the same code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReportLeavesOutComplexityViolations()
    {
        // Arrange
        const string source = """
                              namespace Sample;

                              internal class Holder
                              {
                                  internal int Score(int[] values)
                                  {
                                      int total = 0;

                                      foreach (var value in values)
                                      {
                                          if (value > 0)
                                          {
                                              if (value % 2 == 0)
                                              {
                                                  for (int i = 0; i < value; i++)
                                                  {
                                                      if (i % 3 == 0)
                                                      {
                                                          total += i;
                                                      }
                                                      else if (i % 5 == 0)
                                                      {
                                                          total -= i;
                                                      }
                                                  }
                                              }
                                          }
                                      }

                                      return total;
                                  }
                              }
                              """;
        var compilation = CreateCompilation(("Sample/Holder.cs", source));
        var analyzer = new RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer();

        // Act
        var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation,
                                                                    ImmutableArray.Create<DiagnosticAnalyzer>(analyzer),
                                                                    new CompilationWithAnalyzersOptions(null, null, true, false));
        var reported = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(TestContext.CancellationToken);
        var violations = AnalyzerViolationReport.Collect(analyzer, [compilation], _solutionRoot, TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(reported.Any(diagnostic => diagnostic.Id == RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer.DiagnosticId),
                      "RH2009 does not report the sample, so the report cannot show that it leaves the diagnostic out.");
        Assert.IsEmpty(violations);
    }

    /// <summary>
    /// Verifies that a row creates the analyzer its name designates
    /// </summary>
    [TestMethod]
    public void AnalyzerRowNameCreatesThatAnalyzer()
    {
        // Act
        var analyzer = AnalyzerCatalog.CreateAnalyzer(nameof(RH4106PrivateFieldCasingAnalyzer));

        // Assert
        Assert.IsInstanceOfType<RH4106PrivateFieldCasingAnalyzer>(analyzer);
    }

    /// <summary>
    /// Verifies that a violation is reported with its solution-relative file, its one-based line, its rule ID, and the analyzer name
    /// </summary>
    [TestMethod]
    public void ReportNamesFileLineRuleAndAnalyzerOfAViolation()
    {
        // Arrange
        const string source = """
                              namespace Sample;

                              internal class Holder
                              {
                                  private int Value;

                                  internal int Get() => Value;
                              }
                              """;
        var compilation = CreateCompilation(("Sample/Holder.cs", source));

        // Act
        var violations = AnalyzerViolationReport.Collect(new RH4106PrivateFieldCasingAnalyzer(), [compilation], _solutionRoot, TestContext.CancellationToken);

        // Assert
        Assert.HasCount(1, violations);
        Assert.StartsWith($"{Path.Combine("Sample", "Holder.cs")}:5 [RH4106] (RH4106PrivateFieldCasingAnalyzer) ", violations[0]);
    }

    /// <summary>
    /// Verifies that code without a violation produces an empty report
    /// </summary>
    [TestMethod]
    public void ReportIsEmptyForCodeWithoutViolations()
    {
        // Arrange
        const string source = """
                              namespace Sample;

                              internal class Holder
                              {
                                  private int _value;

                                  internal int Get() => _value;
                              }
                              """;
        var compilation = CreateCompilation(("Sample/Holder.cs", source));

        // Act
        var violations = AnalyzerViolationReport.Collect(new RH4106PrivateFieldCasingAnalyzer(), [compilation], _solutionRoot, TestContext.CancellationToken);

        // Assert
        Assert.IsEmpty(violations);
    }

    /// <summary>
    /// Verifies that violations from several compilations are all reported, ordered by file and then by line
    /// </summary>
    [TestMethod]
    public void ReportListsViolationsOfEveryCompilationOrderedByFileAndLine()
    {
        // Arrange
        const string laterFile = """
                                 namespace Sample;

                                 internal class Later
                                 {
                                     private int Second;
                                     private int First;

                                     internal int Get() => First + Second;
                                 }
                                 """;
        const string earlierFile = """
                                   namespace Sample;

                                   internal class Earlier
                                   {
                                       private int Only;

                                       internal int Get() => Only;
                                   }
                                   """;
        var laterCompilation = CreateCompilation(("Sample/B.cs", laterFile));
        var earlierCompilation = CreateCompilation(("Sample/A.cs", earlierFile));

        // Act
        var violations = AnalyzerViolationReport.Collect(new RH4106PrivateFieldCasingAnalyzer(), [laterCompilation, earlierCompilation], _solutionRoot, TestContext.CancellationToken);

        // Assert
        Assert.AreSequenceEqual(new[]
                                { $"{Path.Combine("Sample", "A.cs")}:5",
                                    $"{Path.Combine("Sample", "B.cs")}:5",
                                    $"{Path.Combine("Sample", "B.cs")}:6"
                                },
                                violations.Select(violation => violation[..violation.IndexOf(' ', StringComparison.Ordinal)]).ToList());
    }

    /// <summary>
    /// Creates a compilation over synthetic source files placed under <see cref="_solutionRoot"/>
    /// </summary>
    /// <param name="files">Relative file paths with forward slashes, and their contents</param>
    /// <returns>The compilation</returns>
    private static CSharpCompilation CreateCompilation(params (string RelativePath, string Source)[] files)
    {
        var syntaxTrees = files.Select(file => CSharpSyntaxTree.ParseText(file.Source, path: Path.Combine(_solutionRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                                                                                    .Select(path => MetadataReference.CreateFromFile(path));

        return CSharpCompilation.Create("SelfHostingReportSample", syntaxTrees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion // Methods
}