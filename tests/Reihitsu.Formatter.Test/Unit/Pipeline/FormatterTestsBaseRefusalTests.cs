using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests of the formatter test helpers of <see cref="FormatterTestsBase"/>: a helper that formats a fixture fails when the
/// formatter refuses the fixture as unsupported, because a refused input comes back unchanged and would otherwise pass an
/// expectation that the formatter keeps it. The refused fixture is otherwise a fixed point of every phase run alone, so no
/// other check of the helpers fails on it first
/// </summary>
[TestClass]
public class FormatterTestsBaseRefusalTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Formatted source the formatter refuses as unsupported, for a comment inside a field declaration
    /// </summary>
    private const string RefusedSource = """
                                         internal class TestClass
                                         {
                                             private int /* comment */ _value;
                                         }
                                         """;

    /// <summary>
    /// The same source without the comment, which the formatter does not refuse and keeps as written
    /// </summary>
    private const string KeptSource = """
                                      internal class TestClass
                                      {
                                          private int _value;
                                      }
                                      """;

    /// <summary>
    /// Source with a syntax error, a missing semicolon, that the formatter refuses as unsupported for a comment inside a
    /// field declaration
    /// </summary>
    private const string RefusedSourceWithSyntaxError = """
                                                        internal class TestClass
                                                        {
                                                            private int /* comment */ _value
                                                        }
                                                        """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase.AssertRuleResult(string, string, Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)"/>
    /// without an expected text fails, naming the refusal, when the formatter refuses the input
    /// </summary>
    [TestMethod]
    public void AssertRuleResultWithoutExpectedTextFailsWhenTheFormatterRefusesTheInput()
    {
        var exception = Assert.ThrowsExactly<AssertFailedException>(() => AssertRuleResult(RefusedSource));

        Assert.Contains(VariableDeclarationCommentReason, exception.Message);
    }

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase.AssertRuleResult(string, string, Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)"/>
    /// with the input as the expected text fails, naming the refusal, when the formatter refuses the input
    /// </summary>
    [TestMethod]
    public void AssertRuleResultWithTheInputAsExpectedTextFailsWhenTheFormatterRefusesTheInput()
    {
        var exception = Assert.ThrowsExactly<AssertFailedException>(() => AssertRuleResult(RefusedSource, RefusedSource));

        Assert.Contains(VariableDeclarationCommentReason, exception.Message);
    }

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase.AssertSecondPassIsStable"/> fails, naming the refusal, when the formatter
    /// refuses the input
    /// </summary>
    [TestMethod]
    public void AssertSecondPassIsStableFailsWhenTheFormatterRefusesTheInput()
    {
        var exception = Assert.ThrowsExactly<AssertFailedException>(() => AssertSecondPassIsStable(RefusedSource));

        Assert.Contains(VariableDeclarationCommentReason, exception.Message);
    }

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase.AssertBlankLinesAbove"/> fails, naming the refusal, when the formatter
    /// refuses the input
    /// </summary>
    [TestMethod]
    public void AssertBlankLinesAboveFailsWhenTheFormatterRefusesTheInput()
    {
        var exception = Assert.ThrowsExactly<AssertFailedException>(() => AssertBlankLinesAbove(RefusedSource, ("{", false)));

        Assert.Contains(VariableDeclarationCommentReason, exception.Message);
    }

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase.FormatWithPipeline"/> fails, naming the refusal, when the formatter refuses
    /// the input, under LF and CRLF line endings
    /// </summary>
    [TestMethod]
    public void FormatWithPipelineFailsWhenTheFormatterRefusesTheInput()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var exception = Assert.ThrowsExactly<AssertFailedException>(() => FormatWithPipeline(RefusedSource, endOfLine));

            Assert.Contains(VariableDeclarationCommentReason, exception.Message, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase.FormatWithPipeline"/> fails, naming the refusal, when the formatter refuses an
    /// input with a syntax error, which the helpers format without the round-trip check, under LF and CRLF line endings
    /// </summary>
    [TestMethod]
    public void FormatWithPipelineFailsWhenTheFormatterRefusesAnInputWithASyntaxError()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var exception = Assert.ThrowsExactly<AssertFailedException>(() => FormatWithPipeline(RefusedSourceWithSyntaxError, endOfLine));

            Assert.Contains(VariableDeclarationCommentReason, exception.Message, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that every helper still passes for the same source without the comment, which the formatter does not refuse
    /// and keeps as written
    /// </summary>
    [TestMethod]
    public void HelpersPassWhenTheFormatterKeepsTheInput()
    {
        AssertRuleResult(KeptSource);
        AssertRuleResult(KeptSource, KeptSource);
        AssertSecondPassIsStable(KeptSource);
        AssertBlankLinesAbove(KeptSource, ("{", false));

        foreach (var endOfLine in _lineEndings)
        {
            Assert.AreEqual(NormalizeLineEndings(KeptSource, endOfLine), FormatWithPipeline(KeptSource, endOfLine));
        }
    }

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase.FormatWithPipeline"/> still passes for an input with a syntax error that the
    /// formatter does not refuse, under LF and CRLF line endings
    /// </summary>
    [TestMethod]
    public void FormatWithPipelinePassesWhenTheFormatterDoesNotRefuseAnInputWithASyntaxError()
    {
        var source = RefusedSourceWithSyntaxError.Replace("/* comment */ ", string.Empty);

        foreach (var endOfLine in _lineEndings)
        {
            FormatWithPipeline(source, endOfLine);
        }
    }

    #endregion // Methods
}