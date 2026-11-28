using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Tooling.Enumerations;
using Reihitsu.Tooling.Test.Helpers;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for the check of <see cref="FixtureRunner"/> that a fix result compiles under the chosen language version: a result
/// that brings in a compile error the fixture did not have is an invalid result, even when it parses, while a result that
/// compiles, or whose compile errors the fixture already had, is fixed
/// </summary>
[TestClass]
public sealed class FixtureRunnerCompilationTests
{
    #region Constants

    /// <summary>
    /// A method whose non-verbatim interpolation holds an object creation on one line, which compiles under every version
    /// </summary>
    private const string OneLineHoleSource = "class P\n{\n    public int A;\n    public int B;\n}\n\nclass C\n{\n    string M()\n    {\n        var a = $\"a {new P { A = 1, B = 2 }.A} b\";\n\n        return a;\n    }\n}\n";

    /// <summary>
    /// The same method with the object creation written onto several lines inside the hole, which parses under every version
    /// but compiles only from C# 11 on (CS8967 before)
    /// </summary>
    private const string MultiLineHoleSource = "class P\n{\n    public int A;\n    public int B;\n}\n\nclass C\n{\n    string M()\n    {\n        var a = $\"a {new P\n             {\n                 A = 1,\n                 B = 2\n             }.A} b\";\n\n        return a;\n    }\n}\n";

    /// <summary>
    /// The text the analyzer of these tests reports, and that every fake fix of these tests removes
    /// </summary>
    private const string ReportedMarker = "// report";

    /// <summary>
    /// A type whose method body carries the reported marker and compiles
    /// </summary>
    private const string CompilingSource = "internal class Sample\n{\n    internal int M()\n    {\n        // report\n        return 1;\n    }\n}\n";

    /// <summary>
    /// A type whose field refers to an unknown type (CS0246) and whose method body carries the reported marker
    /// </summary>
    private const string UnknownTypeSource = "internal class Sample\n{\n    private Missing _field;\n\n    internal int M()\n    {\n        // report\n        return 1;\n    }\n}\n";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a fix result which writes a line break inside a non-verbatim interpolation hole is an invalid result
    /// under C# 10, although it parses there, under either line ending
    /// </summary>
    /// <param name="lineEnding">Line ending of the arm</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow(FixtureLineEndings.LineFeed)]
    [DataRow(FixtureLineEndings.CarriageReturnLineFeed)]
    public async Task RunAsyncReportsInvalidResultWhenTheFixBreaksALineInsideAnInterpolationHoleBeforeCSharp11(string lineEnding)
    {
        // Arrange
        var expectedSource = FixtureLineEndings.Normalize(MultiLineHoleSource, lineEnding);
        var parseErrors = CSharpSyntaxTree.ParseText(expectedSource, new CSharpParseOptions(LanguageVersion.CSharp10), cancellationToken: TestContext.CancellationToken)
                                          .GetDiagnostics(TestContext.CancellationToken)
                                          .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        // Act
        var result = await RunHoleFixAsync(lineEnding, LanguageVersion.CSharp10);

        // Assert
        Assert.IsEmpty(parseErrors);
        Assert.AreEqual(FixtureOutcome.InvalidResult, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
        Assert.AreEqual(expectedSource, result.FinalSource);
    }

    /// <summary>
    /// Verifies that the same fix result is fixed from C# 11 on, where it compiles
    /// </summary>
    /// <param name="languageVersion">Language version of the run</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow(LanguageVersion.CSharp11)]
    [DataRow(LanguageVersion.Latest)]
    public async Task RunAsyncReportsFixedWhenTheFixBreaksALineInsideAnInterpolationHoleFromCSharp11(LanguageVersion languageVersion)
    {
        // Act
        var result = await RunHoleFixAsync(FixtureLineEndings.LineFeed, languageVersion);

        // Assert
        Assert.AreEqual(FixtureOutcome.Fixed, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
        Assert.AreEqual(MultiLineHoleSource, result.FinalSource);
    }

    /// <summary>
    /// Verifies that a fix result which refers to an undefined name (CS0103) is an invalid result, so the check covers every
    /// compile error and not only the ones a language version decides
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsInvalidResultWhenTheFixIntroducesAnUndefinedName()
    {
        // Act
        var result = await RunMarkerFixAsync(CompilingSource, "return missing;", "return 1;");

        // Assert
        Assert.AreEqual(FixtureOutcome.InvalidResult, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
    }

    /// <summary>
    /// Verifies that a fix result which brings in only a warning (CS0219, an unused local) is fixed: only errors make a
    /// result invalid
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsFixedWhenTheFixIntroducesOnlyAWarning()
    {
        // Act
        var result = await RunMarkerFixAsync(CompilingSource, "var unused = 1;\n\n        return 1;", "return 1;");

        // Assert
        Assert.AreEqual(FixtureOutcome.Fixed, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
    }

    /// <summary>
    /// Verifies that a fixture which does not compile and reports nothing is still a fixture without a diagnostic: the
    /// fixture's own compile errors are no parse error
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsNoDiagnosticForAFixtureThatDoesNotCompile()
    {
        // Arrange
        var target = new CodeFixTarget(FakeDiagnostic.Id,
                                       [new TextReportingFakeAnalyzer(_ => false)],
                                       new SourceTransformingFakeCodeFix(source => source));

        // Act
        var result = await FixtureRunner.RunAsync(target,
                                                  UnknownTypeSource,
                                                  FixtureLineEndings.LineFeed,
                                                  10,
                                                  "Fixture.cs",
                                                  LanguageVersion.Latest,
                                                  TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(FixtureOutcome.NoDiagnostic, result.Outcome);
        Assert.AreEqual(0, result.Iterations);
    }

    /// <summary>
    /// Verifies that a fix result which keeps a compile error the fixture already had (CS0246) and brings in no other is fixed
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsFixedWhenTheFixKeepsACompileErrorTheFixtureAlreadyHad()
    {
        // Act
        var result = await RunMarkerFixAsync(UnknownTypeSource, "return 2;", "return 1;");

        // Assert
        Assert.AreEqual(FixtureOutcome.Fixed, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
    }

    /// <summary>
    /// Verifies that a fix result which repeats a compile error the fixture already had is fixed. Splitting a field
    /// declaration of an unknown type into one declaration per field reports CS0246 once per declaration, with the same
    /// message, where the fixture reported it once
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsFixedWhenTheFixRepeatsACompileErrorTheFixtureAlreadyHad()
    {
        // Arrange
        var target = new CodeFixTarget(FakeDiagnostic.Id,
                                       [new TextReportingFakeAnalyzer(source => source.Contains("_first, _second", StringComparison.Ordinal))],
                                       new SourceTransformingFakeCodeFix(source => source.Replace("    private Missing _first, _second;\n",
                                                                                                  "    private Missing _first;\n    private Missing _second;\n",
                                                                                                  StringComparison.Ordinal)));

        // Act
        var result = await FixtureRunner.RunAsync(target,
                                                  "internal class Sample\n{\n    private Missing _first, _second;\n}\n",
                                                  FixtureLineEndings.LineFeed,
                                                  10,
                                                  "Fixture.cs",
                                                  LanguageVersion.Latest,
                                                  TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(FixtureOutcome.Fixed, result.Outcome);
        Assert.AreEqual("internal class Sample\n{\n    private Missing _first;\n    private Missing _second;\n}\n", result.FinalSource);
    }

    /// <summary>
    /// Verifies that a fix result which brings in a compile error of another kind (CS0103) into a fixture that already has a
    /// compile error (CS0246) is an invalid result
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsInvalidResultWhenTheFixAddsAnotherCompileErrorToAFixtureThatDoesNotCompile()
    {
        // Act
        var result = await RunMarkerFixAsync(UnknownTypeSource, "return missing;", "return 1;");

        // Assert
        Assert.AreEqual(FixtureOutcome.InvalidResult, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
    }

    /// <summary>
    /// Verifies that a fix result which brings in a compile error with the same ID as the fixture's own (CS0246) but for
    /// another name is an invalid result: the fixture did not have that error
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsInvalidResultWhenTheFixAddsAnErrorWithTheSameIdForAnotherName()
    {
        // Act
        var result = await RunMarkerFixAsync(UnknownTypeSource, "Other other = null;\n\n        return 1;", "return 1;");

        // Assert
        Assert.AreEqual(FixtureOutcome.InvalidResult, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
    }

    /// <summary>
    /// Runs a fix that rewrites the one-line object creation inside the interpolation hole onto several lines
    /// </summary>
    /// <param name="lineEnding">Line ending of the arm</param>
    /// <param name="languageVersion">Language version of the run</param>
    /// <returns>The fixture run result</returns>
    private async Task<FixtureRunResult> RunHoleFixAsync(string lineEnding, LanguageVersion languageVersion)
    {
        var target = new CodeFixTarget(FakeDiagnostic.Id,
                                       [new TextReportingFakeAnalyzer(source => source.Contains("{ A = 1, B = 2 }", StringComparison.Ordinal))],
                                       new SourceTransformingFakeCodeFix(source => FixtureLineEndings.Normalize(MultiLineHoleSource, source.Contains('\r', StringComparison.Ordinal) ? FixtureLineEndings.CarriageReturnLineFeed : FixtureLineEndings.LineFeed)));

        return await FixtureRunner.RunAsync(target,
                                            OneLineHoleSource,
                                            lineEnding,
                                            10,
                                            "Fixture.cs",
                                            languageVersion,
                                            TestContext.CancellationToken);
    }

    /// <summary>
    /// Runs a fix over a line-feed fixture that carries <see cref="ReportedMarker"/>: the fix replaces the marker line and the
    /// statement behind it with the provided replacement, which ends the diagnostic
    /// </summary>
    /// <param name="source">Fixture source</param>
    /// <param name="replacement">Statements that replace the marker line and the original statement</param>
    /// <param name="originalStatement">The statement behind the marker line</param>
    /// <returns>The fixture run result</returns>
    private async Task<FixtureRunResult> RunMarkerFixAsync(string source, string replacement, string originalStatement)
    {
        var markerBlock = $"{ReportedMarker}\n        {originalStatement}";
        var target = new CodeFixTarget(FakeDiagnostic.Id,
                                       [new TextReportingFakeAnalyzer(text => text.Contains(ReportedMarker, StringComparison.Ordinal))],
                                       new SourceTransformingFakeCodeFix(text => text.Replace(markerBlock, replacement, StringComparison.Ordinal)));

        return await FixtureRunner.RunAsync(target,
                                            source,
                                            FixtureLineEndings.LineFeed,
                                            10,
                                            "Fixture.cs",
                                            LanguageVersion.Latest,
                                            TestContext.CancellationToken);
    }

    #endregion // Methods
}