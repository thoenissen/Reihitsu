using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Unit.Rules.Base;

/// <summary>
/// Base class for formatter tests with normalized string assertions.
/// </summary>
public abstract class FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Applies the formatter rule to the given source text.
    /// </summary>
    /// <param name="input">The source text to format.</param>
    /// <returns>The formatted source text.</returns>
    protected string ApplyRule(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext(Environment.NewLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(), context, CancellationToken.None);

        return result.ToFullString();
    }

    /// <summary>
    /// Applies the formatter rule and verifies both first-pass and second-pass results.
    /// </summary>
    /// <param name="input">The input source text.</param>
    /// <param name="expected">The expected formatted output.</param>
    protected void AssertRuleResult(string input, string expected = null)
    {
        if (string.IsNullOrEmpty(expected))
        {
            var actual = ApplyRule(input);

            Assert.AreEqual(input, actual);
        }
        else
        {
            var actual = ApplyRule(input);

            Assert.AreEqual(expected, actual);

            var actualSecondPass = ApplyRule(actual);

            Assert.AreEqual(expected, actualSecondPass);
        }
    }

    /// <summary>
    /// Asserts that two strings are equal after line-ending normalization.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="message">The optional assertion message.</param>
    protected void AssertNormalized(string expected, string actual, string message = "")
    {
        Assert.AreEqual(Normalize(expected), Normalize(actual), message);
    }

    /// <summary>
    /// Normalizes line endings in the specified text to LF.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The normalized text.</returns>
    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    #endregion // Methods
}