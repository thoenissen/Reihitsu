using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Test.Verifiers;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Base class for formatter validation tests that assert the formatter clears analyzer diagnostics. Every formatter pass is the
/// shared <see cref="FormatterPass"/> of the formatter tests, so every input is parsed as those tests parse it, asserted not to
/// be refused as unsupported, since a refused input comes back unchanged and would pass an expectation that the formatter keeps
/// it, and checked for phase trees that do not parse back to themselves (FMT-PIPE-021) or add elastic trivia (FMT-PIPE-034).
/// Every output the formatter gives is also run through the phase-level stability check (FMT-PIPE-027) of
/// <see cref="PhaseStabilityCheck"/>, under the line ending and parse options it was formatted with
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public abstract class FormatterTestsBase<TAnalyzer> : AnalyzerTestsBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Methods

    /// <summary>
    /// Verifies that analyzer-clean source remains unchanged and analyzer-clean after formatting
    /// </summary>
    /// <param name="source">The source text to verify</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task VerifyFormatter(string source)
    {
        await VerifyFormatter(source, null, Environment.NewLine, null);
        await VerifyFormatter(AlternateLineEndings(source), null, AlternateLineEndings(Environment.NewLine), null);
    }

    /// <summary>
    /// Verifies that the formatter fixes a rule violation and remains stable on a second pass under LF and CRLF line endings
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixedSource">The expected formatted source text</param>
    /// <param name="expected">The expected diagnostics before formatting</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task VerifyFormatter(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        await VerifyFormatter(source, fixedSource, Environment.NewLine, expected);
        await VerifyFormatter(AlternateLineEndings(source), AlternateLineEndings(fixedSource), AlternateLineEndings(Environment.NewLine), expected);
    }

    /// <summary>
    /// Verifies that the formatter fixes a rule violation and remains stable on a second pass under LF and CRLF line endings,
    /// with additional configuration of every analyzer run, such as a reference the source needs to compile
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixedSource">The expected formatted source text</param>
    /// <param name="onConfigure">Additional configuration of every analyzer run</param>
    /// <param name="expected">The expected diagnostics before formatting</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task VerifyFormatter(string source, string fixedSource, Action<CSharpAnalyzerVerifierTest<TAnalyzer>> onConfigure, params DiagnosticResult[] expected)
    {
        await VerifyFormatter(source, fixedSource, Environment.NewLine, onConfigure, expected);
        await VerifyFormatter(AlternateLineEndings(source), AlternateLineEndings(fixedSource), AlternateLineEndings(Environment.NewLine), onConfigure, expected);
    }

    /// <summary>
    /// Verifies that the formatter keeps a shape the analyzer reports as written, under LF and CRLF line endings: the
    /// analyzer reports exactly the diagnostics marked in the source, and formatting the source changes nothing
    /// </summary>
    /// <param name="source">The source text, with markup for every expected diagnostic</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task VerifyFormatterKeepsReportedShape(string source)
    {
        await VerifyFormatterKeepsReportedShape(source, Environment.NewLine);
        await VerifyFormatterKeepsReportedShape(AlternateLineEndings(source), AlternateLineEndings(Environment.NewLine));
    }

    /// <summary>
    /// Creates an expected diagnostic with an explicit span
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="startLine">Start line (1-based)</param>
    /// <param name="startColumn">Start column (1-based)</param>
    /// <param name="endLine">End line (1-based)</param>
    /// <param name="endColumn">End column (1-based)</param>
    /// <param name="message">Expected diagnostic message</param>
    /// <returns>The expected diagnostic result</returns>
    protected static DiagnosticResult ExpectedDiagnostic(string diagnosticId, int startLine, int startColumn, int endLine, int endColumn, string message)
    {
        return Diagnostic(diagnosticId).WithSpan(startLine, startColumn, endLine, endColumn)
                                       .WithMessage(message);
    }

    /// <summary>
    /// Verifies that the formatter fixes a rule violation and remains stable on a second pass under LF and CRLF line endings
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixedSource">The expected formatted source text</param>
    /// <param name="endOfLine">End of line sequence</param>
    /// <param name="expected">Expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFormatter(string source, string fixedSource, string endOfLine, DiagnosticResult[] expected)
    {
        await VerifyFormatter(source, fixedSource, endOfLine, null, expected);
    }

    /// <summary>
    /// Verifies that the formatter fixes a rule violation under the given line ending without refusing the input or its output as
    /// unsupported, that a second pass leaves the output unchanged, and that no formatter phase run alone changes the output
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixedSource">The expected formatted source text</param>
    /// <param name="endOfLine">End of line sequence</param>
    /// <param name="onConfigure">Additional configuration of every analyzer run, or <see langword="null"/> for none</param>
    /// <param name="expected">Expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFormatter(string source, string fixedSource, string endOfLine, Action<CSharpAnalyzerVerifierTest<TAnalyzer>> onConfigure, DiagnosticResult[] expected)
    {
        fixedSource ??= source;
        onConfigure ??= _ =>
                        {
                        };

        await Verify(source, onConfigure, expected ?? []);

        var formatted = VerifyFormatterFixCore(source, fixedSource, endOfLine);

        await Verify(formatted, onConfigure);

        var reformatted = VerifyFormatterFixCore(formatted, fixedSource, endOfLine);

        await Verify(reformatted, onConfigure);

        PhaseStabilityCheck.AssertNoPhaseChanges(reformatted, endOfLine, null);
    }

    /// <summary>
    /// Verifies under the given line ending that the analyzer reports exactly the diagnostics marked in the source, that
    /// the formatter does not refuse the source as unsupported, that formatting it changes nothing, and that no formatter
    /// phase run alone changes it
    /// </summary>
    /// <param name="source">The source text, with markup for every expected diagnostic</param>
    /// <param name="endOfLine">End of line sequence</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFormatterKeepsReportedShape(string source, string endOfLine)
    {
        await Verify(source);

        var formatted = VerifyFormatterFixCore(source, StripMarkup(source), endOfLine);

        PhaseStabilityCheck.AssertNoPhaseChanges(formatted, endOfLine, null);
    }

    /// <summary>
    /// Regex that strips Roslyn analyzer-test markup from source text
    /// </summary>
    /// <returns>The markup-stripping regex</returns>
    /// <remarks>
    /// Only ever matches in-repository test fixture source and carries no nested quantifier capable of
    /// pathological backtracking, so a wall-clock timeout would guard against nothing and only risks a spurious
    /// <see cref="RegexMatchTimeoutException"/> when the test host is under load; it uses
    /// <see cref="Regex.InfiniteMatchTimeout"/>
    /// </remarks>
    private static Regex MarkupRegex()
    {
        return new Regex(@"\{\|[^:|]+:(.*?)\|\}|\[\|(.*?)\|\]", RegexOptions.Singleline, Regex.InfiniteMatchTimeout);
    }

    /// <summary>
    /// Removes analyzer-test markup from the provided source text
    /// </summary>
    /// <param name="source">Source text that may contain markup</param>
    /// <returns>The source text without markup</returns>
    private static string StripMarkup(string source)
    {
        return MarkupRegex().Replace(source, match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);
    }

    /// <summary>
    /// Returns the alternative line-ending sequence for the provided text
    /// </summary>
    /// <param name="text">Text for which to return alternative line endings</param>
    /// <returns>The alternative line-ending sequence</returns>
    private static string AlternateLineEndings(string text)
    {
        const string placeholder = "␍␊";

        return text.Replace("\r\n", placeholder)
                   .Replace("\n", "\r\n")
                   .Replace(placeholder, "\n");
    }

    /// <summary>
    /// Runs one shared formatter pass with the default parse options, which fails when the formatter refuses the input as
    /// unsupported or a phase's tree does not round-trip, and asserts that its output equals the expected fixed text
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixed">The expected formatted source text</param>
    /// <param name="endOfLine">The line-ending sequence for the formatting context</param>
    /// <returns>The formatted text</returns>
    private static string VerifyFormatterFixCore(string source, string @fixed, string endOfLine)
    {
        var formatted = FormatterPass.Format(StripMarkup(source), endOfLine, null);

        Assert.AreEqual(@fixed, formatted, "Formatter output should match the expected fixed code.");

        return formatted;
    }

    #endregion // Methods
}