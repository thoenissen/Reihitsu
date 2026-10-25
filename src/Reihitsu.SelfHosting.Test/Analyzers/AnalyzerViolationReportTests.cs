using System;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    /// Verifies that there is one row per analyzer, each named by the analyzer type, so a row name identifies exactly one analyzer
    /// </summary>
    [TestMethod]
    public void AnalyzerRowsNameEachAnalyzerTypeExactlyOnce()
    {
        // Act
        var names = AnalyzerSelfHostingTests.AnalyzerNames.ToList();

        // Assert
        Assert.Contains(nameof(RH4106PrivateFieldCasingAnalyzer), names);
        Assert.HasCount(names.Count, names.Distinct(StringComparer.Ordinal).ToList(), "Two analyzer rows share a name.");
        Assert.HasCount(AnalyzerCatalog.DiscoverAnalyzerTypes().Count(), names);
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