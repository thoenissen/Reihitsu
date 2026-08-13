using System.Diagnostics;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.DocumentationComments;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Reproduction-gate measurements for issue #528: builds large documentation comments (many
/// <c>///</c> lines, long single lines, and both together, plus a nested-element sibling shape) and
/// records the wall-clock cost of formatting them through the full pipeline, under both LF and CRLF.
/// Timing observations are written to a fixed scratch file so the raw numbers survive minimal-verbosity
/// test output
/// </summary>
[TestClass]
public class Issue528DocumentationCommentStressTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Scratch file every measurement is appended to
    /// </summary>
    private const string ResultsPath = "/tmp/claude-0/-home-user-Reihitsu/a5396c88-3fc2-51c4-a9ef-4a4265d4eae4/scratchpad/issue528-timings.txt";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Measures formatting cost as the number of already-well-formed <c>///</c> content lines inside a
    /// single <c>&lt;summary&gt;</c> grows, under both LF and CRLF. This exercises only the single-round
    /// path (no element requires rebuilding) so the whole cost sits in the line-prefix
    /// <c>Regex.Replace</c> call
    /// </summary>
    [TestMethod]
    public void ManyDocumentationLinesScalingIsMeasured()
    {
        // Arrange
        int[] sizes = [1_000, 4_000, 16_000, 64_000];

        // Act
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var size in sizes)
            {
                var source = NormalizeLineEndings(BuildManyLinesSource(size, 40), endOfLine);
                var stopwatch = Stopwatch.StartNew();
                var result = ApplyRule(source, endOfLine);

                stopwatch.Stop();
                RecordTiming("ManyLines", size, endOfLine, stopwatch.ElapsedMilliseconds, result.Length);
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Measures formatting cost as the length of a single <c>///</c> content line grows, under both LF
    /// and CRLF
    /// </summary>
    [TestMethod]
    public void LongDocumentationLineScalingIsMeasured()
    {
        // Arrange
        int[] sizes = [2_000, 20_000, 200_000, 2_000_000];

        // Act
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var size in sizes)
            {
                var source = NormalizeLineEndings(BuildManyLinesSource(1, size), endOfLine);
                var stopwatch = Stopwatch.StartNew();
                var result = ApplyRule(source, endOfLine);

                stopwatch.Stop();
                RecordTiming("LongLine", size, endOfLine, stopwatch.ElapsedMilliseconds, result.Length);
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Measures formatting cost as both the number of lines and the length of each line grow together,
    /// under both LF and CRLF
    /// </summary>
    [TestMethod]
    public void ManyLongDocumentationLinesScalingIsMeasured()
    {
        // Arrange
        int[] sizes = [500, 2_000, 8_000, 32_000];

        // Act
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var size in sizes)
            {
                var source = NormalizeLineEndings(BuildManyLinesSource(size, 200), endOfLine);
                var stopwatch = Stopwatch.StartNew();
                var result = ApplyRule(source, endOfLine);

                stopwatch.Stop();
                RecordTiming("ManyLongLines", size, endOfLine, stopwatch.ElapsedMilliseconds, result.Length);
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Measures formatting cost as the number of lines inside a nested <c>&lt;code&gt;</c> element
    /// grows, under both LF and CRLF. This exercises the multi-round <c>NormalizeElementsUntilStable</c>
    /// rebuild path rather than the single-round path
    /// </summary>
    [TestMethod]
    public void NestedElementScalingIsMeasured()
    {
        // Arrange
        int[] sizes = [500, 2_000, 8_000, 32_000];

        // Act
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var size in sizes)
            {
                var source = NormalizeLineEndings(BuildNestedElementSource(size), endOfLine);
                var stopwatch = Stopwatch.StartNew();
                var result = ApplyRule(source, endOfLine);

                stopwatch.Stop();
                RecordTiming("NestedElement", size, endOfLine, stopwatch.ElapsedMilliseconds, result.Length);
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Measures the cost of <see cref="DocumentationCommentFormattingPhase"/> alone (not the full
    /// pipeline) as the number of already-well-formed <c>///</c> content lines inside a single
    /// <c>&lt;summary&gt;</c> grows, under both LF and CRLF. Isolates the phase the issue names from
    /// every other pipeline phase that also runs over the same file when going through
    /// <see cref="FormatterTestsBase.ApplyRule(string, string)"/>
    /// </summary>
    [TestMethod]
    public void ManyDocumentationLinesScalingIsMeasuredForThePhaseAlone()
    {
        // Arrange
        int[] sizes = [4_000, 16_000, 64_000, 256_000];

        // Act
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var size in sizes)
            {
                var source = NormalizeLineEndings(BuildManyLinesSource(size, 40), endOfLine);
                var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
                var context = new FormattingContext(endOfLine);
                var stopwatch = Stopwatch.StartNew();
                var result = new DocumentationCommentFormattingPhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);

                stopwatch.Stop();
                RecordTiming("PhaseOnlyManyLines", size, endOfLine, stopwatch.ElapsedMilliseconds, result.ToFullString().Length);
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Refines <see cref="ManyDocumentationLinesScalingIsMeasuredForThePhaseAlone"/> with intermediate
    /// sizes between 16,000 and 64,000 lines under CRLF, to check whether the ratio observed there was
    /// a sustained trend or an isolated measurement
    /// </summary>
    [TestMethod]
    public void ManyDocumentationLinesFineGrainedScalingIsMeasuredForThePhaseAloneUnderCrlf()
    {
        // Arrange
        int[] sizes = [16_000, 24_000, 32_000, 48_000, 64_000];
        const string endOfLine = "\r\n";

        // Act
        foreach (var size in sizes)
        {
            var source = NormalizeLineEndings(BuildManyLinesSource(size, 40), endOfLine);
            var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
            var context = new FormattingContext(endOfLine);
            var stopwatch = Stopwatch.StartNew();
            var result = new DocumentationCommentFormattingPhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);

            stopwatch.Stop();
            RecordTiming("PhaseOnlyFineGrained", size, endOfLine, stopwatch.ElapsedMilliseconds, result.ToFullString().Length);
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Runs the same growing sizes as <see cref="ManyDocumentationLinesScalingIsMeasured"/> through the
    /// full pipeline, but with plain <c>//</c> comments instead of <c>///</c> documentation comments, so
    /// any superlinear cost that is a property of file size or trivia volume in general - rather than of
    /// the documentation-comment path the issue names - shows up here too
    /// </summary>
    [TestMethod]
    public void ManyPlainCommentLinesScalingIsMeasuredThroughTheFullPipeline()
    {
        // Arrange
        int[] sizes = [4_000, 16_000, 64_000];

        // Act
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var size in sizes)
            {
                var source = NormalizeLineEndings(BuildManyPlainCommentLinesSource(size, 40), endOfLine);
                var stopwatch = Stopwatch.StartNew();
                var result = ApplyRule(source, endOfLine);

                stopwatch.Stop();
                RecordTiming("PlainComments", size, endOfLine, stopwatch.ElapsedMilliseconds, result.Length);
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Measures the cost of <see cref="DocumentationCommentFormattingPhase"/> alone as the number of
    /// content lines grows, when every line is malformed (missing the separating space and interspersed
    /// with blank documentation lines) so the line-prefix <c>Regex.Replace</c> match evaluator actually
    /// calls <c>NormalizeExteriorSuffix</c> on every line instead of taking the no-op path the
    /// well-formed sizing tests exercise. This is the trivia shape the well-formed example omits
    /// </summary>
    [TestMethod]
    public void ManyMalformedDocumentationLinesScalingIsMeasuredForThePhaseAlone()
    {
        // Arrange
        int[] sizes = [4_000, 16_000, 64_000];

        // Act
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var size in sizes)
            {
                var source = NormalizeLineEndings(BuildManyMalformedLinesSource(size), endOfLine);
                var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
                var context = new FormattingContext(endOfLine);
                var stopwatch = Stopwatch.StartNew();
                var result = new DocumentationCommentFormattingPhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);

                stopwatch.Stop();
                RecordTiming("PhaseOnlyMalformed", size, endOfLine, stopwatch.ElapsedMilliseconds, result.ToFullString().Length);
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Builds a class with a single <c>&lt;summary&gt;</c> documentation comment containing the
    /// requested number of already-well-formed content lines
    /// </summary>
    /// <param name="lineCount">Number of content lines inside the summary</param>
    /// <param name="lineLength">Approximate length of each content line, in characters</param>
    /// <returns>Source text with the generated documentation comment</returns>
    private static string BuildManyLinesSource(int lineCount, int lineLength)
    {
        var builder = new StringBuilder();

        builder.AppendLine("class C");
        builder.AppendLine("{");
        builder.AppendLine("    /// <summary>");

        for (var index = 0; index < lineCount; index++)
        {
            builder.Append("    /// ").Append('x', lineLength).AppendLine();
        }

        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    void M()");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    /// <summary>
    /// Builds a class with a single <c>&lt;summary&gt;</c> documentation comment containing the
    /// requested number of malformed content lines: every third line is missing the separating space
    /// after <c>///</c>, and every fifth line is a bare <c>///</c> with no content, so the line-prefix
    /// match evaluator has real per-line normalization work to do instead of the well-formed no-op path
    /// </summary>
    /// <param name="lineCount">Number of content lines inside the summary</param>
    /// <returns>Source text with the generated malformed documentation comment</returns>
    private static string BuildManyMalformedLinesSource(int lineCount)
    {
        var builder = new StringBuilder();

        builder.AppendLine("class C");
        builder.AppendLine("{");
        builder.AppendLine("    /// <summary>");

        for (var index = 0; index < lineCount; index++)
        {
            if (index % 5 == 0)
            {
                builder.AppendLine("    ///");
            }
            else if (index % 3 == 0)
            {
                builder.Append("    ///content").Append(index).AppendLine();
            }
            else
            {
                builder.Append("    /// content").Append(index).AppendLine();
            }
        }

        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    void M()");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    /// <summary>
    /// Builds a class preceded by the requested number of plain <c>//</c> comment lines (not
    /// documentation comments), so the same file size and line count can be exercised through the full
    /// pipeline without any <c>///</c> content at all
    /// </summary>
    /// <param name="lineCount">Number of plain comment lines</param>
    /// <param name="lineLength">Approximate length of each comment line, in characters</param>
    /// <returns>Source text with the generated plain comments</returns>
    private static string BuildManyPlainCommentLinesSource(int lineCount, int lineLength)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < lineCount; index++)
        {
            builder.Append("// ").Append('x', lineLength).AppendLine();
        }

        builder.AppendLine("class C");
        builder.AppendLine("{");
        builder.AppendLine("    void M()");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    /// <summary>
    /// Builds a class whose documentation comment nests a <c>&lt;code&gt;</c> element inside a
    /// <c>&lt;remarks&gt;</c> element, both containing the requested number of lines, so the
    /// multi-round rebuild path in <c>NormalizeElementsUntilStable</c> is exercised instead of the
    /// single-round path
    /// </summary>
    /// <param name="lineCount">Number of content lines inside the nested code element</param>
    /// <returns>Source text with the generated nested documentation comment</returns>
    private static string BuildNestedElementSource(int lineCount)
    {
        var builder = new StringBuilder();

        builder.AppendLine("class C");
        builder.AppendLine("{");
        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// Text");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    /// <remarks>Lead");
        builder.AppendLine("    /// <code>");

        for (var index = 0; index < lineCount; index++)
        {
            builder.Append("    /// line").Append(index).AppendLine();
        }

        builder.AppendLine("    /// </code>");
        builder.AppendLine("    /// </remarks>");
        builder.AppendLine("    void M()");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    /// <summary>
    /// Appends a single timing observation to the results scratch file
    /// </summary>
    /// <param name="label">Label describing the scenario</param>
    /// <param name="size">Size parameter driving the input</param>
    /// <param name="endOfLine">Line ending under test</param>
    /// <param name="elapsedMilliseconds">Measured elapsed time</param>
    /// <param name="outputLength">Length of the formatted output</param>
    private static void RecordTiming(string label, int size, string endOfLine, long elapsedMilliseconds, int outputLength)
    {
        var endingName = DescribeLineEnding(endOfLine);
        var line = $"{label}\tsize={size}\tending={endingName}\telapsedMs={elapsedMilliseconds}\toutputLength={outputLength}";

        File.AppendAllText(ResultsPath, line + System.Environment.NewLine);
    }

    #endregion // Methods
}