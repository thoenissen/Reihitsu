using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Base class for formatter tests with string assertions.
/// </summary>
public abstract class FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Applies the formatter rule to the given source text.
    /// </summary>
    /// <param name="input">The source text to format.</param>
    /// <returns>The formatted source text.</returns>
    protected static string ApplyRule(string input)
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

    #endregion // Methods
}