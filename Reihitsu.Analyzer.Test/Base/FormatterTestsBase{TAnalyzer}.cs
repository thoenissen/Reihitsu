using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Base class for formatter validation tests that assert the formatter clears analyzer diagnostics
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public abstract class FormatterTestsBase<TAnalyzer> : AnalyzerTestsBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Fields

    /// <summary>
    /// Line endings used by formatter-stability tests
    /// </summary>
    private static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that the formatter fixes the targeted rule violation using analyzer-style expected diagnostics
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixedSource">The expected formatted source text</param>
    /// <param name="expected">The expected diagnostics before formatting</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task VerifyFormatterFix(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        await VerifyFormatterFix(source, fixedSource, null, expected);
    }

    /// <summary>
    /// Verifies that the formatter fixes the targeted rule violation using analyzer-style expected diagnostics
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixedSource">The expected formatted source text</param>
    /// <param name="transformParseOptions">Optional parse-option transformation</param>
    /// <param name="expected">The expected diagnostics before formatting</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task VerifyFormatterFix(string source, string fixedSource, Func<CSharpParseOptions, CSharpParseOptions> transformParseOptions, params DiagnosticResult[] expected)
    {
        Assert.IsNotEmpty(expected, "Diagnostics are required!");

        if (transformParseOptions == null)
        {
            await Verify(source, expected);
        }
        else
        {
            await Verify(source,
                         test => test.SolutionTransforms.Add((solution, projectId) => ApplyParseOptionsToTestProject(solution, projectId, transformParseOptions)),
                         expected);
        }

        var formatted = await VerifyFormatterFixCore(source, fixedSource, transformParseOptions);

        await Verify(formatted);
    }

    /// <summary>
    /// Verifies that the formatter fixes a rule violation and remains stable on a second pass under LF and CRLF line endings
    /// </summary>
    /// <param name="source">The source text before formatting, including analyzer-test markup</param>
    /// <param name="fixedSource">The expected formatted source text</param>
    /// <param name="expected">The expected diagnostics before formatting</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task VerifyFormatterFixAndIdempotency(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        Assert.IsNotEmpty(expected, "Diagnostics are required!");

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedSource = NormalizeLineEndings(source, endOfLine);
            var normalizedFixedSource = NormalizeLineEndings(fixedSource, endOfLine);

            await Verify(normalizedSource, expected);

            var formatted = await VerifyFormatterFixCore(normalizedSource, normalizedFixedSource, null, endOfLine);

            await Verify(formatted);

            var reformatted = await VerifyFormatterFixCore(formatted, normalizedFixedSource, null, endOfLine);

            await Verify(reformatted);
        }
    }

    /// <summary>
    /// Verifies that analyzer-clean source remains unchanged and analyzer-clean after formatting
    /// </summary>
    /// <param name="source">The source text to verify</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected static async Task VerifyFormatterStability(string source)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalizedSource = NormalizeLineEndings(source, endOfLine);

            await Verify(normalizedSource);

            var formatted = await VerifyFormatterFixCore(normalizedSource, normalizedSource, null, endOfLine);

            await Verify(formatted);
        }
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
    /// Regex that strips Roslyn analyzer-test markup from source text
    /// </summary>
    /// <returns>The markup-stripping regex</returns>
    private static Regex MarkupRegex()
    {
        return new Regex(@"\{\|[^:|]+:(.*?)\|\}|\[\|(.*?)\|\]", RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));
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
    /// Normalizes all line endings in the provided text to the requested sequence
    /// </summary>
    /// <param name="text">Text to normalize</param>
    /// <param name="endOfLine">Target line-ending sequence</param>
    /// <returns>The normalized text</returns>
    private static string NormalizeLineEndings(string text, string endOfLine)
    {
        var lineFeedOnly = text.Replace("\r\n", "\n");

        return endOfLine == "\n" ? lineFeedOnly : lineFeedOnly.Replace("\n", endOfLine);
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
                                                             string endOfLine = null)
    {
        var input = StripMarkup(source);
        var parseOptions = transformParseOptions?.Invoke(CSharpParseOptions.Default) ?? CSharpParseOptions.Default;
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions);
        var context = new FormattingContext(endOfLine ?? Environment.NewLine);
        var formatted = FormattingPipeline.Execute(await tree.GetRootAsync(), context, CancellationToken.None).ToFullString();

        Assert.AreEqual(@fixed, formatted, "Formatter output should match the expected fixed code.");

        return formatted;
    }

    /// <summary>
    /// Applies transformed parse options to the formatter test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <param name="transformParseOptions">Parse-option transformation</param>
    /// <returns>The updated solution</returns>
    private static Solution ApplyParseOptionsToTestProject(Solution solution, ProjectId projectId, Func<CSharpParseOptions, CSharpParseOptions> transformParseOptions)
    {
        var project = solution.GetProject(projectId);

        if (project?.ParseOptions is CSharpParseOptions parseOptions)
        {
            solution = solution.WithProjectParseOptions(projectId, transformParseOptions(parseOptions));
        }

        return solution;
    }

    #endregion // Methods
}