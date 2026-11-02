using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests that <see cref="LineBreakPhase"/>, run alone, starts a statement that stands behind the closing line of a comment
/// that spans several lines on the line below that comment where a blank-line requirement separates it from the statement
/// in front, without inserting the blank line, so the blank-line phase that runs after it adds only the blank line
/// </summary>
[TestClass]
public class LineBreakPhaseCommentTailStatementTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a statement behind a comment's closing line, which a blank-line requirement separates from the
    /// statement in front, starts the line below the comment's closing line, with no blank line between them
    /// </summary>
    /// <param name="body">The lines of the method body, separated by <c>|</c></param>
    /// <param name="lineStart">The text the split statement's line starts with</param>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    [DataRow("a++; /* x|*/ return a;", "return a;", DisplayName = "return behind an expression statement")]
    [DataRow("a++;|; /* x|*/ return a;", "return a;", DisplayName = "return behind an empty statement")]
    [DataRow("if (a > 0) { A(); } /* q|*/ B();", "B();", DisplayName = "expression statement behind a closing brace")]
    [DataRow("while (a > 0) { A(); /* x|*/ break; }", "break;", DisplayName = "break in a loop body")]
    [DataRow("int b = a; /* x|*/ Foo(b);", "Foo(b);", DisplayName = "expression statement behind a local declaration")]
    [DataRow("a++; /* x|*/ var b = a;", "var b = a;", DisplayName = "local declaration behind an expression statement")]
    public void StatementBehindCommentTailStartsTheLineBelowIt(string body, string lineStart)
    {
        AssertLineStartsBelowCommentTail(CreateMethod(body), lineStart);
    }

    /// <summary>
    /// Verifies that a <c>return</c> behind a comment's closing line, which trails an empty statement on a section label's
    /// line, starts the line below the comment's closing line, with no blank line between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void ReturnBehindCommentTailOfEmptyStatementInSwitchSectionStartsTheLineBelowIt()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: ; /* a
                                         */ return 1;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;

        AssertLineStartsBelowCommentTail(input, "return 1;");
    }

    /// <summary>
    /// Verifies that a <c>return</c> behind a comment's closing line, which trails the first statement of a section, starts
    /// the line below the comment's closing line, with no blank line between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void ReturnBehindCommentTailInSwitchSectionStartsTheLineBelowIt()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: A(); /* x
                                         */ return 1;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;

        AssertLineStartsBelowCommentTail(input, "return 1;");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Builds a class with one method whose body holds the given lines
    /// </summary>
    /// <param name="body">The lines of the method body, separated by <c>|</c></param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateMethod(string body)
    {
        var lines = string.Concat(body.Split('|').Select(line => $"        {line}\n"));

        return $"class C\n{{\n    int M(int a)\n    {{\n{lines}    }}\n}}";
    }

    /// <summary>
    /// Runs the phase alone under LF and CRLF and verifies that a line of the output starts with the given text, that the
    /// line directly above it, not blank, is the comment's closing line and holds nothing but <c>*/</c>
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="lineStart">The text the moved statement's line starts with, after its indentation</param>
    private void AssertLineStartsBelowCommentTail(string input, string lineStart)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var output = ExecutePhase(NormalizeLineEndings(input, endOfLine), endOfLine);

            AssertUsesLineEnding(output, endOfLine);

            var lines = output.Split(endOfLine);
            var index = Array.FindIndex(lines, line => line.TrimStart().StartsWith(lineStart, StringComparison.Ordinal));

            Assert.IsGreaterThan(0, index, $"No line of the phase output under {endingName} line endings starts with '{lineStart}':{Environment.NewLine}{output}");
            Assert.AreEqual("*/", lines[index - 1].Trim(), $"The line above '{lineStart}' is not the comment's closing line under {endingName} line endings:{Environment.NewLine}{output}");
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