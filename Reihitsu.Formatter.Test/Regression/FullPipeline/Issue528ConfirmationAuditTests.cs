using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.DocumentationComments;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Escalated-confirmation measurements for issue #528. Re-measures the call site the issue names with
/// warm-up and repeated samples, attributes the full-pipeline cost to individual phases, exercises the
/// repository's own sources through the named phase (the regime the reported failure occurred in), and
/// searches for an input size at which the production call site actually raises
/// <see cref="RegexMatchTimeoutException"/>
/// </summary>
[TestClass]
public class Issue528ConfirmationAuditTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Scratch file every measurement is appended to
    /// </summary>
    private const string ResultsPath = "/tmp/claude-0/-home-user-Reihitsu/a5396c88-3fc2-51c4-a9ef-4a4265d4eae4/scratchpad/issue528-audit.txt";

    /// <summary>
    /// Verbatim copy of the line-prefix pattern used by the call site the issue names, so the regex can
    /// be measured apart from the rest of the phase
    /// </summary>
    private const string LinePrefixPattern = @"(?:\A|(?<=\r\n)|(?<=[\r\n\u0085\u2028\u2029]))(?<indent>[^\S\r\n\u0085\u2028\u2029]*)(?<prefix>///)(?<suffix>[^\r\n\u0085\u2028\u2029]*)(?=\r\n|\r|\n|\u0085|\u2028|\u2029|$)";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Measures the isolated line-prefix regex with the production timeout as the comment grows, taking
    /// the fastest of several warmed samples so scheduler and collector noise cannot hide the shape of
    /// the curve. Records whether the regex itself raises the timeout at any size
    /// </summary>
    [TestMethod]
    public void IsolatedLinePrefixRegexScalingIsMeasuredWithWarmup()
    {
        // Arrange
        int[] sizes = [16_000, 64_000, 256_000, 512_000];

        // Act
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var size in sizes)
            {
                var commentText = BuildCommentTextOnly(size, endOfLine);

                Record($"RegexOnly\tsize={size}\tending={DescribeLineEnding(endOfLine)}\tchars={commentText.Length}\t{MeasureRegexOnly(commentText)}");
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Probes the line-prefix regex with deliberately tighter match timeouts on a large input and on a
    /// repository-sized input, so it is decidable whether the timeout is consumed by cumulative regex
    /// work across the whole replace or only by a single match attempt
    /// </summary>
    [TestMethod]
    public void LinePrefixRegexIsProbedWithTighterMatchTimeouts()
    {
        // Arrange
        var largeComment = BuildCommentTextOnly(512_000, "\n");
        var repositorySizedComment = BuildCommentTextOnly(6, "\n");
        TimeSpan[] timeouts = [TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(10)];

        // Act
        foreach (var timeout in timeouts)
        {
            Record($"TimeoutProbe	input=large	chars={largeComment.Length}	timeoutMs={timeout.TotalMilliseconds}	{ProbeRegex(largeComment, timeout)}");
            Record($"TimeoutProbe	input=repositorySized	chars={repositorySizedComment.Length}	timeoutMs={timeout.TotalMilliseconds}	{ProbeRegex(repositorySizedComment, timeout)}");
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Runs the production phase at growing sizes and records, for each one, whether
    /// <see cref="RegexMatchTimeoutException"/> escapes the call site the issue names
    /// </summary>
    [TestMethod]
    public void ProductionPhaseIsSearchedForTheNamedTimeoutException()
    {
        // Arrange
        int[] sizes = [64_000, 256_000, 512_000, 1_024_000];

        // Act
        foreach (var size in sizes)
        {
            foreach (var endOfLine in _lineEndings)
            {
                Record($"PhaseTimeoutSearch\tsize={size}\tending={DescribeLineEnding(endOfLine)}\t{RunPhaseAndDescribeOutcome(size, endOfLine, TestContext.CancellationToken)}");
            }
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Runs every source file the self-hosting test formats through the named phase alone and records
    /// the worst observed cost and the largest documentation comment, so the reported failure's own
    /// regime - this repository's sources - is measured instead of a synthetic giant
    /// </summary>
    [TestMethod]
    public void RepositorySourcesAreMeasuredThroughTheNamedPhase()
    {
        // Arrange
        var solutionRoot = FindSolutionRootDirectory();
        var worstPhaseMilliseconds = -1L;
        var worstPhaseFile = string.Empty;
        var worstRegexTicks = -1L;
        var worstRegexFile = string.Empty;
        var largestCommentLength = 0;
        var largestCommentFile = string.Empty;
        var fileCount = 0;

        // Act
        foreach (var file in EnumerateRepositorySourceFiles(solutionRoot))
        {
            TestContext.CancellationToken.ThrowIfCancellationRequested();

            var content = File.ReadAllText(file, Encoding.UTF8);

            foreach (var endOfLine in _lineEndings)
            {
                var normalized = NormalizeLineEndings(content, endOfLine);
                var tree = CSharpSyntaxTree.ParseText(normalized, cancellationToken: TestContext.CancellationToken);

                if (tree.GetDiagnostics(TestContext.CancellationToken).Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
                {
                    continue;
                }

                fileCount++;

                var root = tree.GetRoot(TestContext.CancellationToken);
                var stopwatch = Stopwatch.StartNew();

                new DocumentationCommentFormattingPhase().Execute(root, new FormattingContext(endOfLine), TestContext.CancellationToken);
                stopwatch.Stop();

                if (stopwatch.ElapsedMilliseconds > worstPhaseMilliseconds)
                {
                    worstPhaseMilliseconds = stopwatch.ElapsedMilliseconds;
                    worstPhaseFile = $"{Path.GetRelativePath(solutionRoot, file)} [{DescribeLineEnding(endOfLine)}]";
                }

                foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
                {
                    if (trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineDocumentationCommentTrivia) == false)
                    {
                        continue;
                    }

                    var commentText = normalized.Substring(trivia.FullSpan.Start, trivia.FullSpan.Length);
                    var regexStopwatch = Stopwatch.StartNew();

                    Regex.Replace(commentText, LinePrefixPattern, match => match.Value, RegexOptions.None, TimeSpan.FromSeconds(2));
                    regexStopwatch.Stop();

                    if (regexStopwatch.ElapsedTicks > worstRegexTicks)
                    {
                        worstRegexTicks = regexStopwatch.ElapsedTicks;
                        worstRegexFile = $"{Path.GetRelativePath(solutionRoot, file)} [{DescribeLineEnding(endOfLine)}]";
                    }

                    if (commentText.Length > largestCommentLength)
                    {
                        largestCommentLength = commentText.Length;
                        largestCommentFile = Path.GetRelativePath(solutionRoot, file);
                    }
                }
            }
        }

        Record($"RepositoryRegime\tfileRuns={fileCount}\tworstPhaseMs={worstPhaseMilliseconds}\tworstPhaseFile={worstPhaseFile}");
        Record($"RepositoryRegime\tworstRegexMs={worstRegexTicks * 1000.0 / Stopwatch.Frequency:F3}\tworstRegexFile={worstRegexFile}");
        Record($"RepositoryRegime\tlargestCommentChars={largestCommentLength}\tlargestCommentFile={largestCommentFile}");

        // Assert
        Assert.IsGreaterThan(0, fileCount);
    }

    /// <summary>
    /// Attributes the full-pipeline cost of a growing documentation comment to individual phases, so the
    /// superlinear full-pipeline curve the previous run set aside is assigned to a phase by measurement
    /// </summary>
    [TestMethod]
    public void FullPipelineCostIsAttributedToIndividualPhases()
    {
        // Arrange
        int[] sizes = [4_000, 16_000, 64_000];
        const string endOfLine = "\n";

        // Act
        foreach (var size in sizes)
        {
            var source = NormalizeLineEndings(BuildManyLinesSource(size), endOfLine);
            var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
            var measurements = new List<string>();
            var stopwatch = Stopwatch.StartNew();
            var previous = 0L;

            FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken),
                                       new FormattingContext(endOfLine),
                                       (phaseName, _, _) =>
                                       {
                                           var now = stopwatch.ElapsedMilliseconds;

                                           measurements.Add($"{phaseName}={now - previous}");
                                           previous = now;
                                       },
                                       TestContext.CancellationToken);
            stopwatch.Stop();

            Record($"PhaseAttribution\tsize={size}\ttotalMs={stopwatch.ElapsedMilliseconds}\t{string.Join(" ", measurements)}");
        }

        // Assert
        Assert.IsTrue(File.Exists(ResultsPath));
    }

    /// <summary>
    /// Runs the line-prefix regex once with the requested match timeout and describes the outcome
    /// </summary>
    /// <param name="commentText">Documentation comment text to normalize</param>
    /// <param name="matchTimeout">Match timeout to apply</param>
    /// <returns>A description of the elapsed time, or of the exception the regex raised</returns>
    private static string ProbeRegex(string commentText, TimeSpan matchTimeout)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            Regex.Replace(commentText, LinePrefixPattern, match => match.Value, RegexOptions.None, matchTimeout);
            stopwatch.Stop();

            return $"OK	elapsedMs={stopwatch.ElapsedMilliseconds}";
        }
        catch (RegexMatchTimeoutException exception)
        {
            return $"THREW={exception.GetType().Name}	matchTimeout={exception.MatchTimeout}";
        }
    }

    /// <summary>
    /// Measures the isolated line-prefix regex, taking the fastest of several warmed samples
    /// </summary>
    /// <param name="commentText">Documentation comment text to normalize</param>
    /// <returns>A description of the measured samples, or of the timeout the regex raised</returns>
    private static string MeasureRegexOnly(string commentText)
    {
        try
        {
            var best = long.MaxValue;
            var samples = new List<long>();

            for (var attempt = 0; attempt < 4; attempt++)
            {
                var stopwatch = Stopwatch.StartNew();

                Regex.Replace(commentText, LinePrefixPattern, match => match.Value, RegexOptions.None, TimeSpan.FromSeconds(2));
                stopwatch.Stop();
                samples.Add(stopwatch.ElapsedMilliseconds);

                if (stopwatch.ElapsedMilliseconds < best)
                {
                    best = stopwatch.ElapsedMilliseconds;
                }
            }

            return $"bestMs={best}\tsamples={string.Join(",", samples)}";
        }
        catch (RegexMatchTimeoutException exception)
        {
            return $"THREW={exception.GetType().Name}\tmatchTimeout={exception.MatchTimeout}";
        }
    }

    /// <summary>
    /// Runs the production phase over a generated comment of the requested size and describes the outcome
    /// </summary>
    /// <param name="lineCount">Number of content lines inside the summary</param>
    /// <param name="endOfLine">Line ending under test</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A description of the elapsed time, or of the exception the phase raised</returns>
    private static string RunPhaseAndDescribeOutcome(int lineCount, string endOfLine, CancellationToken cancellationToken)
    {
        try
        {
            var source = NormalizeLineEndings(BuildManyLinesSource(lineCount), endOfLine);
            var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
            var stopwatch = Stopwatch.StartNew();
            var result = new DocumentationCommentFormattingPhase().Execute(tree.GetRoot(cancellationToken), new FormattingContext(endOfLine), cancellationToken);

            stopwatch.Stop();

            return $"chars={source.Length}\telapsedMs={stopwatch.ElapsedMilliseconds}\toutputChars={result.ToFullString().Length}\tOK";
        }
        catch (RegexMatchTimeoutException exception)
        {
            return $"THREW={exception.GetType().Name}\tmatchTimeout={exception.MatchTimeout}";
        }
        catch (OutOfMemoryException)
        {
            return "SKIPPED=OutOfMemoryException";
        }
    }

    /// <summary>
    /// Builds only the documentation comment text of a class with the requested number of
    /// already-well-formed content lines, without the surrounding declaration
    /// </summary>
    /// <param name="lineCount">Number of content lines inside the summary</param>
    /// <param name="endOfLine">Line ending to build with</param>
    /// <returns>The documentation comment text</returns>
    private static string BuildCommentTextOnly(int lineCount, string endOfLine)
    {
        var builder = new StringBuilder();

        builder.Append("    /// <summary>").Append(endOfLine);

        for (var index = 0; index < lineCount; index++)
        {
            builder.Append("    /// ").Append('x', 40).Append(endOfLine);
        }

        builder.Append("    /// </summary>").Append(endOfLine);

        return builder.ToString();
    }

    /// <summary>
    /// Builds a class with a single <c>&lt;summary&gt;</c> documentation comment containing the
    /// requested number of already-well-formed content lines
    /// </summary>
    /// <param name="lineCount">Number of content lines inside the summary</param>
    /// <returns>Source text with the generated documentation comment</returns>
    private static string BuildManyLinesSource(int lineCount)
    {
        var builder = new StringBuilder();

        builder.AppendLine("class C");
        builder.AppendLine("{");
        builder.AppendLine("    /// <summary>");

        for (var index = 0; index < lineCount; index++)
        {
            builder.Append("    /// ").Append('x', 40).AppendLine();
        }

        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    void M()");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    /// <summary>
    /// Finds the solution root directory by walking up from the test assembly's location
    /// </summary>
    /// <returns>The absolute path to the solution root directory</returns>
    private static string FindSolutionRootDirectory()
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
    /// Enumerates the same source files the self-hosting idempotency test formats
    /// </summary>
    /// <param name="solutionRoot">The absolute path to the solution root</param>
    /// <returns>An enumerable of absolute file paths</returns>
    private static IEnumerable<string> EnumerateRepositorySourceFiles(string solutionRoot)
    {
        string[] sourceDirectories = ["Reihitsu.Core", "Reihitsu.Analyzer", "Reihitsu.Analyzer.CodeFixes", "Reihitsu.Analyzer.Test", "Reihitsu.Cli", "Reihitsu.Cli.Test", "Reihitsu.Formatter", "Reihitsu.Formatter.Test"];

        foreach (var sourceDirectory in sourceDirectories)
        {
            var fullPath = Path.Combine(solutionRoot, sourceDirectory);

            if (Directory.Exists(fullPath) == false)
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(fullPath, "*.cs", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(solutionRoot, file);

                if (relativePath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                    || relativePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    || relativePath.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
                    || relativePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return file;
            }
        }
    }

    /// <summary>
    /// Appends a single observation to the results scratch file
    /// </summary>
    /// <param name="line">Observation to append</param>
    private static void Record(string line)
    {
        File.AppendAllText(ResultsPath, line + System.Environment.NewLine);
    }

    #endregion // Methods
}