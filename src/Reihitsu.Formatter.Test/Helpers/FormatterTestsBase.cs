using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Base class for formatter tests with string assertions
/// </summary>
public abstract class FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The reason the formatter gives for a comment inside a variable declaration (FMT-UNSUP-004)
    /// </summary>
    protected const string VariableDeclarationCommentReason = "a comment between a variable declaration's own tokens";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The line endings every fixture is exercised against. Running both LF and CRLF makes
    /// line-ending–sensitive offset and width calculations a standard, always-on dimension of the
    /// suite rather than a one-off per bug
    /// </summary>
    protected static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test. Declared once here, so every formatter test class uses this property
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Starts the phase-level stability check of the current test
    /// </summary>
    [TestInitialize]
    public void BeginPhaseStabilityCheck()
    {
        PhaseStabilityCheck.BeginTest();
    }

    /// <summary>
    /// Ends the phase-level stability check of the current test
    /// </summary>
    [TestCleanup]
    public void EndPhaseStabilityCheck()
    {
        PhaseStabilityCheck.EndTest();
    }

    /// <summary>
    /// Applies the formatter rule and verifies both first-pass and second-pass results under both
    /// LF and CRLF line endings, and that no phase of the pipeline, run alone, changes the final output
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expected">The expected formatted output, or <see langword="null"/> when the input is already formatted</param>
    /// <param name="parseOptions">The parse options to use, or <see langword="null"/> for the defaults</param>
    protected static void AssertRuleResult(string input, string expected = null, CSharpParseOptions parseOptions = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            AssertRuleResult(input, expected, endOfLine, parseOptions);
        }
    }

    /// <summary>
    /// Formats the input once and verifies, under both LF and CRLF line endings, that a second pass leaves the output
    /// of the first pass unchanged and that no phase of the pipeline, run alone, changes that output. Used where a
    /// requirement leaves the exact output open but the result must still be a fixed point of the formatter
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="parseOptions">The parse options to use, or <see langword="null"/> for the defaults</param>
    protected static void AssertSecondPassIsStable(string input, CSharpParseOptions parseOptions = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var firstPass = ApplyRule(NormalizeLineEndings(input, endOfLine), endOfLine, parseOptions);

            AssertUsesLineEnding(firstPass, endOfLine);

            var secondPass = ApplyRule(firstPass, endOfLine, parseOptions);

            Assert.AreEqual(firstPass, secondPass, $"Formatter is not idempotent under {endingName} line endings.");

            PhaseStabilityCheck.Verify(secondPass, endOfLine, parseOptions);
        }
    }

    /// <summary>
    /// Formats the input once under LF and CRLF line endings and verifies only whether the line above each named line of
    /// the output is blank, then verifies that a second pass is stable as <see cref="AssertSecondPassIsStable"/> does.
    /// Used where a requirement decides the blank lines of a fixture but no requirement decides the columns of its lines
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expectations">For each line, identified as the first output line whose text without leading whitespace starts with <c>LineStart</c>, whether the line above it is blank</param>
    protected static void AssertBlankLinesAbove(string input, params (string LineStart, bool IsBlankAbove)[] expectations)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var output = ApplyRule(NormalizeLineEndings(input, endOfLine), endOfLine);
            var lines = output.Split(endOfLine);

            foreach (var (lineStart, isBlankAbove) in expectations)
            {
                var index = Array.FindIndex(lines, line => line.TrimStart().StartsWith(lineStart, StringComparison.Ordinal));

                Assert.IsGreaterThan(0, index, $"The output under {endingName} line endings has no line starting with '{lineStart}' below its first line:{Environment.NewLine}{output}");
                Assert.AreEqual(isBlankAbove, string.IsNullOrWhiteSpace(lines[index - 1]), $"Unexpected blank line state above '{lineStart}' under {endingName} line endings:{Environment.NewLine}{output}");
            }
        }

        AssertSecondPassIsStable(input);
    }

    /// <summary>
    /// Returns the span of the given occurrence of a shape's text in a source text
    /// </summary>
    /// <param name="text">The source text</param>
    /// <param name="shapeText">The text of the shape</param>
    /// <param name="occurrence">The zero-based occurrence</param>
    /// <returns>The span of the shape</returns>
    protected static TextSpan GetShapeSpan(string text, string shapeText, int occurrence = 0)
    {
        var index = -1;

        for (var current = 0; current <= occurrence; current++)
        {
            index = text.IndexOf(shapeText, index + 1, StringComparison.Ordinal);

            Assert.IsGreaterThanOrEqualTo(0, index, $"The text '{shapeText}' must occur in the input.");
        }

        return new TextSpan(index, shapeText.Length);
    }

    /// <summary>
    /// Asserts that a reported location is the span of the given occurrence of a shape's text: it starts where the shape starts,
    /// and its text is the shape's text, apart from a line break a single-line comment ends with
    /// </summary>
    /// <param name="location">The reported location</param>
    /// <param name="text">The source text</param>
    /// <param name="shapeText">The text of the shape</param>
    /// <param name="occurrence">The zero-based occurrence</param>
    /// <param name="endingName">The name of the line ending the text uses, for the message</param>
    protected static void AssertLocationIsShape(Location location, string text, string shapeText, int occurrence, string endingName)
    {
        var expected = GetShapeSpan(text, shapeText, occurrence);

        Assert.AreEqual(expected.Start, location.SourceSpan.Start, $"The location must start where the shape starts under {endingName} line endings.");
        Assert.AreEqual(shapeText, text.Substring(location.SourceSpan.Start, location.SourceSpan.Length).TrimEnd('\r', '\n'), $"The location must cover the shape under {endingName} line endings.");
    }

    /// <summary>
    /// Rewrites every line break in the given text to the requested end-of-line sequence
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <param name="endOfLine">The target end-of-line sequence</param>
    /// <returns>The text using the requested line endings</returns>
    protected static string NormalizeLineEndings(string text, string endOfLine)
    {
        var lineFeedOnly = text.Replace("\r\n", "\n");

        return endOfLine == "\n" ? lineFeedOnly : lineFeedOnly.Replace("\n", endOfLine);
    }

    /// <summary>
    /// Verifies that the given text uses the requested end-of-line sequence for every line break
    /// </summary>
    /// <param name="text">The formatted text to inspect</param>
    /// <param name="endOfLine">The end-of-line sequence every line break must use</param>
    protected static void AssertUsesLineEnding(string text, string endOfLine)
    {
        if (endOfLine == "\n")
        {
            Assert.IsFalse(text.Contains('\r'), "Formatted output must not contain a carriage return when LF line endings are requested.");
        }
        else
        {
            var withoutCrlf = text.Replace("\r\n", string.Empty);

            Assert.IsFalse(withoutCrlf.Contains('\n') || withoutCrlf.Contains('\r'), "Formatted output must use CRLF for every line break.");
        }
    }

    /// <summary>
    /// Returns a short, human-readable name for the given end-of-line sequence
    /// </summary>
    /// <param name="endOfLine">The end-of-line sequence</param>
    /// <returns><c>LF</c> or <c>CRLF</c></returns>
    protected static string DescribeLineEnding(string endOfLine)
    {
        return endOfLine == "\n" ? "LF" : "CRLF";
    }

    /// <summary>
    /// Formats the input with <see cref="ReihitsuFormatter.FormatSyntaxTree"/> under LF and CRLF line endings and verifies that
    /// the formatter refuses it as unsupported: the output is the input unchanged, and the result names the reason of
    /// FMT-UNSUP-004 and the span of the shape, which is the given occurrence of <paramref name="shapeText"/> in the input
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="shapeText">The text of the unsupported shape the formatter has to report, such as the comment</param>
    /// <param name="occurrence">The zero-based occurrence of <paramref name="shapeText"/> in the input that is reported</param>
    /// <param name="parseOptions">The parse options to use, or <see langword="null"/> for the defaults</param>
    protected void AssertUnsupported(string input, string shapeText, int occurrence = 0, CSharpParseOptions parseOptions = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var text = NormalizeLineEndings(input, endOfLine);
            var tree = CSharpSyntaxTree.ParseText(text, parseOptions, cancellationToken: TestContext.CancellationToken);

            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.AreEqual(text, result.Output.GetText(TestContext.CancellationToken).ToString(), $"The formatter must return a refused input unchanged under {endingName} line endings.");
            Assert.IsNotNull(result.Unsupported, $"The formatter must report the input as unsupported under {endingName} line endings.");
            Assert.AreEqual(VariableDeclarationCommentReason, result.Unsupported.Reason);
            Assert.AreSame(tree, result.Unsupported.Location.SourceTree, "The location must be in the caller's syntax tree.");
            AssertLocationIsShape(result.Unsupported.Location, text, NormalizeLineEndings(shapeText, endOfLine), occurrence, endingName);
        }
    }

    /// <summary>
    /// Formats the input with <see cref="ReihitsuFormatter.FormatSyntaxTree"/> under LF and CRLF line endings and verifies that
    /// the formatter does not refuse it as unsupported, and that its output is a fixed point of the formatter as
    /// <see cref="AssertSecondPassIsStable"/> verifies. A test that knows the exact output asserts it with
    /// <see cref="AssertRuleResult(string, string, CSharpParseOptions)"/> in addition
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="parseOptions">The parse options to use, or <see langword="null"/> for the defaults</param>
    protected void AssertSupported(string input, CSharpParseOptions parseOptions = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), parseOptions, cancellationToken: TestContext.CancellationToken);

            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported, $"The formatter must not refuse the input under {DescribeLineEnding(endOfLine)} line endings: {result.Unsupported?.Reason}");
        }

        AssertSecondPassIsStable(input, parseOptions);
    }

    /// <summary>
    /// Applies the formatter rule with the requested end-of-line sequence and parse options, and
    /// verifies that the output matches the expected text, uses the requested ending byte-for-byte,
    /// is idempotent, and that no phase of the pipeline, run alone, changes it
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expected">The expected formatted output, or <see langword="null"/> when the input is already formatted</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    /// <param name="parseOptions">The parse options to use, or <see langword="null"/> for the defaults</param>
    private static void AssertRuleResult(string input, string expected, string endOfLine, CSharpParseOptions parseOptions)
    {
        var normalizedInput = NormalizeLineEndings(input, endOfLine);
        var endingName = DescribeLineEnding(endOfLine);

        if (string.IsNullOrEmpty(expected))
        {
            var actual = ApplyRule(normalizedInput, endOfLine, parseOptions);

            Assert.AreEqual(normalizedInput, actual, $"Formatter changed already-formatted source under {endingName} line endings.");
            AssertUsesLineEnding(actual, endOfLine);

            PhaseStabilityCheck.Verify(actual, endOfLine, parseOptions);
        }
        else
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var actual = ApplyRule(normalizedInput, endOfLine, parseOptions);

            Assert.AreEqual(normalizedExpected, actual, $"Formatter output mismatch under {endingName} line endings.");
            AssertUsesLineEnding(actual, endOfLine);

            var actualSecondPass = ApplyRule(actual, endOfLine, parseOptions);

            Assert.AreEqual(normalizedExpected, actualSecondPass, $"Formatter is not idempotent under {endingName} line endings.");

            PhaseStabilityCheck.Verify(actualSecondPass, endOfLine, parseOptions);
        }
    }

    /// <summary>
    /// Applies the formatter rule with the requested end-of-line sequence and parse options. Fixtures
    /// whose source carries conditional compilation pass the symbols they need here, so the
    /// line-ending policy stays in this base instead of being restated per test class
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    /// <param name="parseOptions">The parse options to use, or <see langword="null"/> for the defaults</param>
    /// <returns>The formatted source text</returns>
    private static string ApplyRule(string input, string endOfLine, CSharpParseOptions parseOptions = null)
    {
        var (root, context) = SecondPassInput.Create(input, endOfLine, parseOptions);

        if (root.ContainsDiagnostics
            && root.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            return FormattingPipeline.Execute(root, context, CancellationToken.None).Output.ToFullString();
        }

        // Every phase's tree has to parse back to itself with the fixture's parse options (FMT-PIPE-021), so every
        // fixture of the formatter tests checks it on each pass
        var (result, failures) = PhaseTreeRoundTrip.ExecuteAndCompare(root, context, (CSharpParseOptions)root.SyntaxTree.Options);

        if (failures.Count > 0)
        {
            Assert.Fail(string.Join(Environment.NewLine, failures));
        }

        return result.ToFullString();
    }

    #endregion // Methods
}