using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5104CommentsMustBeOnTheirOwnLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5104CommentsMustBeOnTheirOwnLineFormatterTests : FormatterTestsBase<RH5104CommentsMustBeOnTheirOwnLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter keeps a comment behind an <see langword="if"/> condition, and that RH5104 still reports it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-001")]
    [Requirement("FMT-BREAK-036")]
    public async Task VerifyFormatterKeepsCommentBehindIfConditionAndItStaysReported()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method(bool left, bool right)
                                 {
                                     if (left == right) {|#0:// Compare the values.|}
                                     {
                                         return;
                                     }
                                 }
                             }
                             """;

        await VerifyFormatterKeepsReportedCode(input, "\n");
        await VerifyFormatterKeepsReportedCode(input.Replace("\r\n", "\n").Replace("\n", "\r\n"), "\r\n");
    }

    /// <summary>
    /// Verifies that two comments sharing a line without code, which the formatter keeps, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    public async Task VerifyFormatterKeepsTwoCommentsOnALineWithoutCodeAndTheyAreNotReported()
    {
        const string testData = """"
                                internal class Example
                                {
                                    void Method()
                                    {
                                        /* a */ /* b */
                                        Method();
                                    }
                                }
                                """";

        await VerifyFormatter(testData);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Verifies that the formatter leaves reported code unchanged on two passes and that RH5104 still reports it
    /// </summary>
    /// <param name="source">The source text, including analyzer-test markup for the expected diagnostic</param>
    /// <param name="endOfLine">The line-ending sequence of the source and of the formatting context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFormatterKeepsReportedCode(string source, string endOfLine)
    {
        var expected = Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat);

        await Verify(source, expected);

        var input = source.Replace("{|#0:", string.Empty).Replace("|}", string.Empty);
        var formatted = Format(input, endOfLine);

        Assert.AreEqual(input, formatted, "Formatter output should keep the comment behind the condition, where RH5104 reports it.");

        Assert.AreEqual(input, Format(formatted, endOfLine), "Formatter output should be stable on a second pass.");
    }

    /// <summary>
    /// Runs the formatting pipeline over the source text
    /// </summary>
    /// <param name="source">The source text without markup</param>
    /// <param name="endOfLine">The line-ending sequence for the formatting context</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string source, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var context = new FormattingContext(endOfLine);

        return FormattingPipeline.Execute(tree.GetRoot(), context, CancellationToken.None).ToFullString();
    }

    #endregion // Methods
}