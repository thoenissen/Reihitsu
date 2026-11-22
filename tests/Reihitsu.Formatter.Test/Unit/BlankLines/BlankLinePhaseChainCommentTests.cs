using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests that <see cref="BlankLinePhase"/>, run alone, decides a comment in front of a chain link on the lines it finds and
/// does not predict where the line-break phase breaks the link off a comment's closing line
/// </summary>
[TestClass]
public class BlankLinePhaseChainCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a comment spanning several lines whose closing line holds the chain link behind it is glued to the
    /// link while the link stands on that line, so the phase inserts no blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void CommentGluedToChainLinkOnItsClosingLineGetsNoBlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()
                                              /* c
                                               */ .D();
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that the same comment, once the chain link starts the line below the comment's closing line, is on its own
    /// line and gets the blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void CommentInFrontOfChainLinkOnItsOwnLineGetsBlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()
                                              /* c
                                               */
                                              .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* c
                                                  */
                                                 .D();
                                    }
                                }
                                """;

        foreach (var endOfLine in _lineEndings)
        {
            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), ExecutePhase(NormalizeLineEndings(input, endOfLine), endOfLine), $"Unexpected phase output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Runs the phase alone under LF and CRLF and verifies that it changes nothing
    /// </summary>
    /// <param name="input">The C# source text</param>
    private void AssertUnchanged(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalized = NormalizeLineEndings(input, endOfLine);

            Assert.AreEqual(normalized, ExecutePhase(normalized, endOfLine), $"The blank-line phase changed the input under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Executes the <see cref="BlankLinePhase"/> alone on the input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The phase's output</returns>
    private string ExecutePhase(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(endOfLine);

        return new BlankLinePhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).ToFullString();
    }

    #endregion // Methods
}