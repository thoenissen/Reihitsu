using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Reproduction-gate counterpart and code-fix measurements for issue #528. RH8301 and its code fix own
/// the same documentation-exterior suffix policy as the formatter's line-prefix normalization, through
/// <see cref="Reihitsu.Core.DocumentationCommentUtilities.NormalizeExteriorSuffix(string)"/>, but neither
/// uses a regular expression or a match timeout to reach it, so this records whether growing the same
/// kind of large, malformed documentation comment produces a timeout-shaped failure on this path too
/// </summary>
[TestClass]
public class Issue528RH8301CounterpartStressTests : AnalyzerTestsBase<RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer, RH8301DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider>
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Constants

    /// <summary>
    /// Scratch file every measurement is appended to (shared with the formatter-side stress tests)
    /// </summary>
    private const string ResultsPath = "/tmp/claude-0/-home-user-Reihitsu/a5396c88-3fc2-51c4-a9ef-4a4265d4eae4/scratchpad/issue528-timings.txt";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Runs the RH8301 analyzer (via <see cref="AnalyzerTestsBase{TAnalyzer,TCodeFix}.ApplyCodeFixAsync"/>,
    /// which first collects every analyzer diagnostic over the whole document and then applies the code
    /// fix for the first one) over documents with a growing number of malformed lines, and records the
    /// elapsed time for each size
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ManyMalformedLinesAnalyzerAndCodeFixScalingIsMeasured()
    {
        // Arrange
        int[] sizes = [1_000, 4_000, 16_000];

        // Act & Assert
        foreach (var size in sizes)
        {
            var source = BuildManyMalformedLinesSource(size);
            var stopwatch = Stopwatch.StartNew();
            var fixedSource = await ApplyCodeFixAsync(source);

            stopwatch.Stop();

            Assert.IsFalse(string.IsNullOrEmpty(fixedSource));
            Assert.AreNotEqual(source, fixedSource);
            File.AppendAllText(ResultsPath, $"RH8301Counterpart\tsize={size}\telapsedMs={stopwatch.ElapsedMilliseconds}\toutputLength={fixedSource.Length}" + System.Environment.NewLine);
        }
    }

    /// <summary>
    /// Builds a class with the requested number of malformed <c>///</c> lines (missing the separating
    /// space), so every line reports an RH8301 diagnostic
    /// </summary>
    /// <param name="lineCount">Number of malformed lines</param>
    /// <returns>Source text with the generated malformed documentation comment</returns>
    private static string BuildManyMalformedLinesSource(int lineCount)
    {
        var builder = new StringBuilder();

        builder.AppendLine("internal class TestClass");
        builder.AppendLine("{");

        for (var index = 0; index < lineCount; index++)
        {
            builder.Append("    ///content").Append(index).AppendLine();
        }

        builder.AppendLine("    void Method()");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    #endregion // Methods
}