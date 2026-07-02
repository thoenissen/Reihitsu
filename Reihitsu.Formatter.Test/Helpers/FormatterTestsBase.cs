using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    /// Applies the formatter rule to the given source text using <see cref="Environment.NewLine"/>
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <returns>The formatted source text</returns>
    protected static string ApplyRule(string input)
    {
        return ApplyRule(input, Environment.NewLine);
    }

    /// <summary>
    /// Applies the formatter rule to the given source text, threading the requested end-of-line
    /// sequence through the pipeline so the chosen ending is honored verbatim
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    /// <returns>The formatted source text</returns>
    protected static string ApplyRule(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext(endOfLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(), context, CancellationToken.None);

        return result.ToFullString();
    }

    /// <summary>
    /// Applies the formatter rule and verifies both first-pass and second-pass results under both
    /// LF and CRLF line endings (issue #330)
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expected">The expected formatted output, or <see langword="null"/> when the input is already formatted</param>
    protected static void AssertRuleResult(string input, string expected = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            AssertRuleResult(input, expected, endOfLine);
        }
    }

    /// <summary>
    /// Applies the formatter rule with the requested end-of-line sequence and verifies that the
    /// output matches the expected text, uses the requested ending byte-for-byte, and is idempotent
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expected">The expected formatted output, or <see langword="null"/> when the input is already formatted</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    protected static void AssertRuleResult(string input, string expected, string endOfLine)
    {
        var normalizedInput = NormalizeLineEndings(input, endOfLine);
        var endingName = DescribeLineEnding(endOfLine);

        if (string.IsNullOrEmpty(expected))
        {
            var actual = ApplyRule(normalizedInput, endOfLine);

            Assert.AreEqual(normalizedInput, actual, $"Formatter changed already-formatted source under {endingName} line endings.");
            AssertUsesLineEnding(actual, endOfLine);
        }
        else
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var actual = ApplyRule(normalizedInput, endOfLine);

            Assert.AreEqual(normalizedExpected, actual, $"Formatter output mismatch under {endingName} line endings.");
            AssertUsesLineEnding(actual, endOfLine);

            var actualSecondPass = ApplyRule(actual, endOfLine);

            Assert.AreEqual(normalizedExpected, actualSecondPass, $"Formatter is not idempotent under {endingName} line endings.");
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

    #endregion // Methods
}