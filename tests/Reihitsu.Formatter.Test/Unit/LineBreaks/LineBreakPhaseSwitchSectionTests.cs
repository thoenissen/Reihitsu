using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests that <see cref="LineBreakPhase"/>, run alone, places switch sections, their labels, and their first statements on
/// lines of their own without inserting a blank line, and leaves an empty first statement as written
/// </summary>
[TestClass]
public class LineBreakPhaseSwitchSectionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the phase-alone example of FMT-BREAK-035: an empty first statement stays on its label's line and the
    /// <c>break</c> next to it stays as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-035")]
    public void EmptyFirstStatementIsLeftAsWritten()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: ; break;
                                     }
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var normalized = NormalizeLineEndings(input, endOfLine);

            Assert.AreEqual(normalized, ExecutePhase(normalized, endOfLine), $"The line-break phase changed the input under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the phase moves a section behind a <c>return</c> onto its own line without a blank line in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    public void SectionBehindReturnStartsItsLineWithoutBlankLine()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             return 1; case 2:
                                             return 2;
                                     }
                                 }
                             }
                             """;

        AssertLineStartsWithoutBlankLineAbove(input, "case 2:", "return 1;");
    }

    /// <summary>
    /// Verifies that the phase moves a section behind a <c>break</c> onto its own line and leaves the blank line in front of
    /// it to the blank-line phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    public void SectionBehindBreakStartsItsLineWithoutBlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             break; case 2:
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertLineStartsWithoutBlankLineAbove(input, "case 2:", "break;");
    }

    /// <summary>
    /// Verifies that the phase moves a label behind another label onto its own line without a blank line in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    public void LabelBehindLabelStartsItsLineWithoutBlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: case 2:
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertLineStartsWithoutBlankLineAbove(input, "case 2:", "case 1:");
    }

    /// <summary>
    /// Verifies that the phase moves a first statement behind its label onto its own line without a blank line in front of
    /// it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-035")]
    public void FirstStatementStartsItsLineWithoutBlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: A();
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertLineStartsWithoutBlankLineAbove(input, "A();", "case 1:");
    }

    /// <summary>
    /// Runs the phase alone under LF and CRLF and verifies that a line of the output starts with the given text and that the
    /// line directly above it, not blank, starts with the other given text
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="lineStart">The text the moved token's line starts with, after its indentation</param>
    /// <param name="previousLineStart">The text the line directly above starts with, after its indentation</param>
    private void AssertLineStartsWithoutBlankLineAbove(string input, string lineStart, string previousLineStart)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var output = ExecutePhase(NormalizeLineEndings(input, endOfLine), endOfLine);

            AssertUsesLineEnding(output, endOfLine);

            var lines = output.Split(endOfLine);
            var index = Array.FindIndex(lines, line => line.TrimStart().StartsWith(lineStart, StringComparison.Ordinal));

            Assert.IsGreaterThan(0, index, $"No line of the phase output under {endingName} line endings starts with '{lineStart}':{Environment.NewLine}{output}");
            Assert.IsTrue(lines[index - 1].TrimStart().StartsWith(previousLineStart, StringComparison.Ordinal), $"The line above '{lineStart}' does not start with '{previousLineStart}' under {endingName} line endings:{Environment.NewLine}{output}");
        }
    }

    /// <summary>
    /// Executes the <see cref="LineBreakPhase"/> alone on the input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The phase's output</returns>
    private string ExecutePhase(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(endOfLine);

        return new LineBreakPhase().Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).ToFullString();
    }

    #endregion // Methods
}