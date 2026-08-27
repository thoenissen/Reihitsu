using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Base class for formatter tests with string assertions
/// </summary>
public abstract class FormatterTestsBase
{
    #region Fields

    /// <summary>
    /// The line endings every fixture is exercised against. Running both LF and CRLF makes
    /// line-ending–sensitive offset and width calculations a standard, always-on dimension of the
    /// suite rather than a one-off per bug (issue #330)
    /// </summary>
    protected static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Applies the formatter rule and verifies both first-pass and second-pass results under both
    /// LF and CRLF line endings (issue #330)
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
    /// Applies the formatter rule with the requested end-of-line sequence and parse options, and
    /// verifies that the output matches the expected text, uses the requested ending byte-for-byte,
    /// and is idempotent
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
        }
        else
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var actual = ApplyRule(normalizedInput, endOfLine, parseOptions);

            Assert.AreEqual(normalizedExpected, actual, $"Formatter output mismatch under {endingName} line endings.");
            AssertUsesLineEnding(actual, endOfLine);

            var actualSecondPass = ApplyRule(actual, endOfLine, parseOptions);

            Assert.AreEqual(normalizedExpected, actualSecondPass, $"Formatter is not idempotent under {endingName} line endings.");
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
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions);
        var context = new FormattingContext(endOfLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(), context, CancellationToken.None);

        return result.ToFullString();
    }

    #endregion // Methods
}