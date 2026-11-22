using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// One formatter pass of a test fixture, shared by the formatter tests and the analyzer tests' formatter-parity helpers: the
/// text is parsed and given its formatting context as <see cref="SecondPassInput"/> does, run through the production pipeline,
/// and checked. A pass fails when the formatter refuses the text as unsupported, because a refused input comes back unchanged
/// and would otherwise pass an expectation that the formatter keeps it, and, for text without syntax errors, when a phase's
/// tree does not parse back to itself (FMT-PIPE-021) or adds elastic trivia (FMT-PIPE-034)
/// </summary>
public static class FormatterPass
{
    #region Methods

    /// <summary>
    /// Formats the text once and fails when the formatter refuses it or when a phase's tree does not round-trip
    /// </summary>
    /// <param name="text">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <returns>The formatted source text</returns>
    public static string Format(string text, string endOfLine, CSharpParseOptions parseOptions)
    {
        var (root, context) = SecondPassInput.Create(text, endOfLine, parseOptions);

        if (root.ContainsDiagnostics
            && root.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            var result = FormattingPipeline.Execute(root, context, CancellationToken.None);

            AssertNotRefused(result);

            return result.Output.ToFullString();
        }

        var (checkedResult, failures) = PhaseTreeRoundTrip.ExecuteAndCompare(root, context, (CSharpParseOptions)root.SyntaxTree.Options);

        AssertNotRefused(checkedResult);

        if (failures.Count > 0)
        {
            Assert.Fail(string.Join(Environment.NewLine, failures));
        }

        return checkedResult.Output.ToFullString();
    }

    /// <summary>
    /// Fails, naming the reason, when the formatter refused the input as unsupported
    /// </summary>
    /// <param name="result">The result of the pipeline</param>
    private static void AssertNotRefused(FormattingResult<SyntaxNode> result)
    {
        Assert.IsNull(result.Unsupported, $"The formatter must not refuse the input as unsupported: {result.Unsupported?.Reason}");
    }

    #endregion // Methods
}