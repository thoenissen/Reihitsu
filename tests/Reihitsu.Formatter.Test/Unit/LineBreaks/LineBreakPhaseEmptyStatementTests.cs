using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests that <see cref="LineBreakPhase"/>, run alone, splits an empty statement from the statement next to it where a
/// blank-line requirement separates the two, without inserting the blank line, and leaves the pair as written otherwise
/// </summary>
[TestClass]
public class LineBreakPhaseEmptyStatementTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a statement a blank-line requirement separates from the empty statement in front of it starts the line
    /// below that empty statement, with no blank line between them
    /// </summary>
    /// <param name="body">The statements of the method body, one line each, separated by <c>|</c></param>
    /// <param name="lineStart">The text the split statement's line starts with</param>
    /// <param name="previousLineStart">The text the line directly above it starts with</param>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [DataRow("a++;|; return a;", "return a;", ";", DisplayName = "return behind an empty statement")]
    [DataRow("a++;|; throw null;", "throw null;", ";", DisplayName = "throw behind an empty statement")]
    [DataRow("a++;|; if (a > 0) { }", "if (a > 0)", ";", DisplayName = "if behind an empty statement")]
    [DataRow("a++;|; var b = 1;", "var b = 1;", ";", DisplayName = "local declaration behind an empty statement")]
    [DataRow("while (a > 0) { ; break; }", "break;", ";", DisplayName = "break behind an empty statement")]
    [DataRow("if (a > 0) { } ;", ";", "}", DisplayName = "empty statement behind a closing brace")]
    public void StatementSeparatedByBlankLineRequirementStartsItsLine(string body, string lineStart, string previousLineStart)
    {
        AssertLineStartsWithoutBlankLineAbove(CreateMethod("int", body), lineStart, previousLineStart);
    }

    /// <summary>
    /// Verifies that a <c>yield return</c> behind an empty statement starts the line below it, with no blank line between
    /// them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void YieldBehindEmptyStatementStartsItsLine()
    {
        AssertLineStartsWithoutBlankLineAbove(CreateMethod("System.Collections.Generic.IEnumerable<int>", "a++;|; yield return a;"), "yield return a;", ";");
    }

    /// <summary>
    /// Verifies that a <c>return</c> behind an empty statement in a switch section starts the line below it, with no blank
    /// line between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void ReturnBehindEmptyStatementInSwitchSectionStartsItsLine()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             ; return 1;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;

        AssertLineStartsWithoutBlankLineAbove(input, "return 1;", ";");
    }

    /// <summary>
    /// Verifies that a <c>throw</c> behind an empty statement on a section's label line starts the line below the label,
    /// with no blank line between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void ThrowBehindEmptyStatementOnLabelLineStartsItsLine()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: ; throw null;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;

        AssertLineStartsWithoutBlankLineAbove(input, "throw null;", "case 1:");
    }

    /// <summary>
    /// Verifies that an empty statement and an expression statement that no blank-line requirement separates are left on
    /// one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void EmptyStatementAndExpressionStatementAreLeftAsWritten()
    {
        AssertUnchanged(CreateMethod("void", "a++;|; a--;|; Foo();"));
    }

    /// <summary>
    /// Verifies that an empty statement in front of the terminal <c>break</c> of a switch section, which no blank-line
    /// requirement separates from it, is left on the <c>break</c>'s line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void EmptyStatementInFrontOfTerminalBreakIsLeftAsWritten()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             ; break;
                                     }
                                 }
                             }
                             """;

        AssertUnchanged(input);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Builds a class with one method whose body holds the given lines
    /// </summary>
    /// <param name="returnType">The method's return type</param>
    /// <param name="body">The statements of the method body, one line each, separated by <c>|</c></param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateMethod(string returnType, string body)
    {
        var lines = string.Concat(body.Split('|').Select(line => $"        {line}\n"));

        return $"class C\n{{\n    {returnType} M(int a)\n    {{\n{lines}    }}\n}}";
    }

    /// <summary>
    /// Runs the phase alone under LF and CRLF and verifies that it changes nothing
    /// </summary>
    /// <param name="input">The C# source text</param>
    private void AssertUnchanged(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalized = NormalizeLineEndings(input, endOfLine);

            Assert.AreEqual(normalized, ExecutePhase(normalized, endOfLine), $"The line-break phase changed the input under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Runs the phase alone under LF and CRLF and verifies that a line of the output starts with the given text and that the
    /// line directly above it, not blank, starts with the other given text
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="lineStart">The text the moved statement's line starts with, after its indentation</param>
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