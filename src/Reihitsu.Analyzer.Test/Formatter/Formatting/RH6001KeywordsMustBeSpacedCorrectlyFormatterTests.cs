using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6001KeywordsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6001KeywordsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6001KeywordsMustBeSpacedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        {|#0:if|}(true)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's output for an <c>if</c> keyword directly followed by a documentation comment stays
    /// unchanged and raises no diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-002")]
    public async Task VerifyFormatterOutputWithStrayDocumentationCommentBehindKeywordIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    internal void M(bool c, int[] items)
                                    {
                                        if/// x
                                        (c)
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that the formatter's output for an <c>if</c> keyword followed by a block comment and a documentation comment
    /// stays unchanged and raises no diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-002")]
    public async Task VerifyFormatterOutputWithBlockCommentAndStrayDocumentationCommentBehindKeywordIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    internal void M(bool c)
                                    {
                                        if/* c *//// x
                                        (c)
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that the formatter puts one space behind a <c>fixed</c> keyword glued to its parenthesis, which clears the
    /// diagnostic, and that formatting its output again changes nothing, under LF and CRLF line endings. The fixture needs
    /// an <c>unsafe</c> compilation, so it is checked here rather than through <see cref="FormatterTestsBase{TAnalyzer}.VerifyFormatter(string, string, Microsoft.CodeAnalysis.Testing.DiagnosticResult[])"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-001")]
    public async Task VerifyFormatterFixesGluedFixedKeyword()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    internal unsafe void Method(int[] items)
                                    {
                                        {|#0:fixed|}(int* p = items)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     internal unsafe void Method(int[] items)
                                     {
                                         fixed (int* p = items)
                                         {
                                         }
                                     }
                                 }
                                 """;
        const string plainSource = """
                                   internal class TestClass
                                   {
                                       internal unsafe void Method(int[] items)
                                       {
                                           fixed(int* p = items)
                                           {
                                           }
                                       }
                                   }
                                   """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject),
                     Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat));

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var expected = fixedData.Replace("\r\n", "\n").Replace("\n", endOfLine);
            var firstPass = Format(plainSource.Replace("\r\n", "\n").Replace("\n", endOfLine));

            Assert.AreEqual(expected, firstPass, $"Unexpected formatter output under {(endOfLine == "\n" ? "LF" : "CRLF")} line endings.");
            Assert.AreEqual(firstPass, Format(firstPass), $"The second pass must not change the output under {(endOfLine == "\n" ? "LF" : "CRLF")} line endings.");

            await Verify(firstPass, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Formats the source with the formatter
    /// </summary>
    /// <param name="source">The source text</param>
    /// <returns>The formatted text</returns>
    private static string Format(string source)
    {
        return ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(source), CancellationToken.None)
                                .GetText()
                                .ToString();
    }

    #endregion // Methods
}