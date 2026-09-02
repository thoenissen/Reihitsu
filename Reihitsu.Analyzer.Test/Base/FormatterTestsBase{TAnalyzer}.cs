using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Base class for formatter validation tests that assert the formatter clears analyzer diagnostics
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
        fixedSource ??= source;

        await Verify(source, expected);

        var formatted = await VerifyFormatterFixCore(source, fixedSource, null, endOfLine);

        await Verify(formatted);

        var reformatted = await VerifyFormatterFixCore(formatted, fixedSource, null, endOfLine);

        await Verify(reformatted);
    }

    /// <summary>
    /// Regex that strips Roslyn analyzer-test markup from source text
    /// </summary>
    /// <returns>The markup-stripping regex</returns>
    private static Regex MarkupRegex()
    {
        return new Regex(@"\{\|[^:|]+:(.*?)\|\}|\[\|(.*?)\|\]", RegexOptions.Singleline, TimeSpan.FromSeconds(2));
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
    /// Runs the formatter, verifies the fixed output, and asserts that no analyzer diagnostics remain
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixed">The expected formatted source text</param>
    /// <param name="transformParseOptions">Optional parse-option transformation</param>
    /// <param name="endOfLine">Optional line-ending sequence for the formatting context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<string> VerifyFormatterFixCore(string source,
                                                             string @fixed,
                                                             Func<CSharpParseOptions, CSharpParseOptions> transformParseOptions,
                                                             string endOfLine)
    {
        var input = StripMarkup(source);
        var parseOptions = transformParseOptions?.Invoke(CSharpParseOptions.Default) ?? CSharpParseOptions.Default;
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions);
        var context = new FormattingContext(endOfLine);
        var formatted = FormattingPipeline.Execute(await tree.GetRootAsync(), context, CancellationToken.None).ToFullString();

        Assert.AreEqual(@fixed, formatted, "Formatter output should match the expected fixed code.");

        return formatted;
    }

    #endregion // Methods
}