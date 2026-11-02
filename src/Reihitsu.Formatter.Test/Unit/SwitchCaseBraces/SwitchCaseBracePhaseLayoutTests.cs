using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.SwitchCaseBraces;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.SwitchCaseBraces;

/// <summary>
/// Tests that <see cref="SwitchCaseBracePhase"/>, run alone on input whose lines are already broken, places the braces it
/// inserts on the lines of a section's block, writes no blank line, and carries only the blank lines that lead a comment
/// </summary>
[TestClass]
public class SwitchCaseBracePhaseLayoutTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies on the FMT-SWITCH-015 example that the opening brace starts the line below the label, the statements the
    /// lines below it, the closing brace the line below the last statement, and the terminal <c>break</c> the line below
    /// the closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-015")]
    public void InsertedBracesStartTheLinesOfASectionBlock()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertLineTexts(input, "class C", "{", "void M(int x)", "{", "switch (x)", "{", "case 1:", "{", "A();", "B();", "}", "break;", "}", "}", "}");
    }

    /// <summary>
    /// Verifies that the opening brace starts the line below the comment trailing the label, and the closing brace the line
    /// below the comment trailing the last statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-015")]
    public void InsertedBracesStartTheirLinesBehindTrailingComments()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: // label
                                             A();
                                             B(); // last
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertLineTexts(input, "class C", "{", "void M(int x)", "{", "switch (x)", "{", "case 1: // label", "{", "A();", "B(); // last", "}", "break;", "}", "}", "}");
    }

    /// <summary>
    /// Verifies on the FMT-SWITCH-016 example that brace insertion keeps every blank line it carries — the one behind the
    /// label, the run of two in front of the comment that leads the terminal <c>break</c>, and the one between the
    /// sections — and that the line breaks it writes form none
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-016")]
    public void BraceInsertionWritesAndRemovesNoBlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:

                                             // a
                                             A();
                                             B();


                                             // b
                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertBlankLinesKept(input, 4);
    }

    /// <summary>
    /// Verifies that brace insertion keeps a blank line in front of a documentation comment, a delimited documentation
    /// comment, or a comment that leads the first statement on its line
    /// </summary>
    /// <param name="leadingComment">The comment that leads the first statement</param>
    [TestMethod]
    [Requirement("FMT-SWITCH-016")]
    [DataRow("/// d\n                A();", DisplayName = "documentation comment")]
    [DataRow("/** e */\n                A();", DisplayName = "delimited documentation comment")]
    [DataRow("/* c */ A();", DisplayName = "comment on the statement's line")]
    public void BraceInsertionKeepsBlankLineInFrontOfFirstStatementComment(string leadingComment)
    {
        var input = $"class C\n{{\n    void M(int x)\n    {{\n        switch (x)\n        {{\n            case 1:\n\n                {leadingComment}\n                B();\n                break;\n        }}\n    }}\n}}";

        AssertBlankLinesKept(input, 1);
    }

    /// <summary>
    /// Verifies on the second FMT-SWITCH-016 example that brace insertion does not carry a blank line without a comment in
    /// front of the first statement or the terminal <c>break</c>, and keeps the blank line between the sections, which
    /// crosses no new brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-016")]
    public void BraceInsertionDoesNotCarryBlankLinesWithoutComment()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:

                                             A();
                                             B();

                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertLineTexts(input, "class C", "{", "void M(int x)", "{", "switch (x)", "{", "case 1:", "{", "A();", "B();", "}", "break;", string.Empty, "case 2:", "{", "C();", "}", "break;", "}", "}", "}");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the lines that hold text other than a lone brace, without indentation
    /// </summary>
    /// <param name="lines">The lines</param>
    /// <returns>The trimmed lines with text</returns>
    private static string[] GetTextLinesWithoutBraces(string[] lines)
    {
        return lines.Select(line => line.Trim())
                    .Where(line => line.Length > 0 && line != "{" && line != "}")
                    .ToArray();
    }

    /// <summary>
    /// Runs the phase alone under LF and CRLF and verifies the text of every line of the output, without its indentation,
    /// in order
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="expectedLines">The text of every output line, without indentation</param>
    private void AssertLineTexts(string input, params string[] expectedLines)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ExecutePhase(NormalizeLineEndings(input, endOfLine), endOfLine);

            AssertUsesLineEnding(output, endOfLine);

            var lines = output.Split(endOfLine).Select(line => line.Trim()).ToArray();

            Assert.AreSequenceEqual(expectedLines, lines, $"Unexpected lines of the phase output under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
        }
    }

    /// <summary>
    /// Runs the phase alone under LF and CRLF, verifies that it inserted braces, and that the output holds as many blank
    /// lines as the input and the same lines with text apart from the inserted braces, in order
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="blankLineCount">The number of blank lines of the input</param>
    private void AssertBlankLinesKept(string input, int blankLineCount)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var normalized = NormalizeLineEndings(input, endOfLine);
            var output = ExecutePhase(normalized, endOfLine);

            AssertUsesLineEnding(output, endOfLine);

            var inputLines = normalized.Split(endOfLine);
            var outputLines = output.Split(endOfLine);

            Assert.AreEqual(blankLineCount, inputLines.Count(string.IsNullOrWhiteSpace), "The fixture holds an unexpected number of blank lines.");
            Assert.IsGreaterThan(inputLines.Length, outputLines.Length, $"The phase inserted no braces under {endingName} line endings:{Environment.NewLine}{output}");
            Assert.AreEqual(blankLineCount, outputLines.Count(string.IsNullOrWhiteSpace), $"The phase changed the number of blank lines under {endingName} line endings:{Environment.NewLine}{output}");
            Assert.AreSequenceEqual(GetTextLinesWithoutBraces(inputLines), GetTextLinesWithoutBraces(outputLines), $"The phase changed the lines with text under {endingName} line endings:{Environment.NewLine}{output}");
        }
    }

    /// <summary>
    /// Executes the <see cref="SwitchCaseBracePhase"/> alone on the input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The phase's output</returns>
    private string ExecutePhase(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(endOfLine);

        return new SwitchCaseBracePhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).ToFullString();
    }

    #endregion // Methods
}