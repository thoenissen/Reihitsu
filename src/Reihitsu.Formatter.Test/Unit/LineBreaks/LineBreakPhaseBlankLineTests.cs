using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests that <see cref="LineBreakPhase"/>, run alone, keeps the blank lines of every gap it does not join
/// </summary>
[TestClass]
public class LineBreakPhaseBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the phase keeps the blank line above a block statement after a closing brace and the blank line above
    /// a comment glued to an initializer's brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void KeepsBlankLinesAboveBlockStatementAndGluedComment()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                     }

                                     {
                                     }

                                     var p = new P
                                             {
                                                 Child =

                                                 /* c */ {
                                                             X = 2
                                                         }
                                             };
                                 }
                             }
                             """;

        AssertPhaseKeepsInput(input);
    }

    /// <summary>
    /// Verifies that the phase keeps the blank line above a block statement after a statement that does not end with a
    /// closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void KeepsBlankLineAboveBlockStatementAfterOtherStatement()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A();

                                     {
                                         B();
                                     }
                                 }
                             }
                             """;

        AssertPhaseKeepsInput(input);
    }

    /// <summary>
    /// Verifies that the phase keeps the blank line between an own-line comment and a closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void KeepsBlankLineBetweenCommentAndClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;

                                     // c

                                 }
                             }
                             """;

        AssertPhaseKeepsInput(input);
    }

    /// <summary>
    /// Verifies that the phase keeps the blank line between an own-line comment and a method body's opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void KeepsBlankLineBetweenCommentAndOpeningBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()

                                 // c

                                 {
                                 }
                             }
                             """;

        AssertPhaseKeepsInput(input);
    }

    /// <summary>
    /// Verifies that the phase keeps the blank line before the closing bracket of a collection expression
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void KeepsBlankLineBeforeClosingBracketOfCollectionExpression()
    {
        const string input = """
                             class C
                             {
                                 int[] F = [
                                               1,
                                               2

                                           ];
                             }
                             """;

        AssertPhaseKeepsInput(input);
    }

    /// <summary>
    /// Verifies that the phase keeps the blank line above a comment glued to a closing brace and inserts none in front of
    /// the directive above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void InsertsNoBlankLineInFrontOfDirectiveAboveGluedComment()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #pragma warning disable CS0168

                                 /* d */ }
                             }
                             """;

        AssertPhaseKeepsInput(input);
    }

    /// <summary>
    /// Verifies that the phase keeps the blank line between a directive and a block's closing brace, and the one above the
    /// directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void KeepsBlankLinesAroundDirectiveBeforeClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;

                             #if DEBUG
                                     a--;
                             #endif

                                 }
                             }
                             """;

        AssertPhaseKeepsInput(input);
    }

    /// <summary>
    /// Verifies that where the phase moves an opening brace onto its own line it inserts no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void InsertsNoBlankLineWhereItMovesBraceOntoNewLine()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a) {
                                     }
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var output = ExecutePhase(NormalizeLineEndings(input, endOfLine), endOfLine);
            var lines = output.Split(endOfLine);

            Assert.DoesNotContain(line => string.IsNullOrWhiteSpace(line), lines, $"The phase inserted a blank line under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
            Assert.HasCount(9, lines, $"The phase should have moved the brace onto a line of its own under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
        }
    }

    /// <summary>
    /// Verifies that where the phase joins a lone binary operator with its right operand below a comment on its own line,
    /// it inserts no blank line above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void InsertsNoBlankLineAboveCommentLineWhereItJoinsLoneBinaryOperator()
    {
        const string input = """
                             class C
                             {
                                 bool M(bool a, bool b)
                                 {
                                     var p = a
                                             // between
                                             &&
                                             b;
                                     return p;
                                 }
                             }
                             """;

        AssertPhaseJoinsWithoutBlankLine(input, "&&");
    }

    /// <summary>
    /// Verifies that where the phase joins a lone conditional <c>:</c> with the false branch below a comment on its own line,
    /// it inserts no blank line above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-030")]
    public void InsertsNoBlankLineAboveCommentLineWhereItJoinsLoneConditionalColon()
    {
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     var p = a
                                             ? 1
                                             // c
                                             :
                                             2;
                                     return p;
                                 }
                             }
                             """;

        AssertPhaseJoinsWithoutBlankLine(input, ":");
    }

    /// <summary>
    /// Runs the phase alone on the input under LF and CRLF line endings and asserts that it joined the lone operator with
    /// the operand below it and inserted no blank line
    /// </summary>
    /// <param name="input">The input with LF line endings, which holds no blank line</param>
    /// <param name="operatorText">The text of the lone operator</param>
    private void AssertPhaseJoinsWithoutBlankLine(string input, string operatorText)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ExecutePhase(NormalizeLineEndings(input, endOfLine), endOfLine);
            var lines = output.Split(endOfLine);

            Assert.DoesNotContain(line => string.IsNullOrWhiteSpace(line), lines, $"The phase inserted a blank line under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
            Assert.DoesNotContain(line => line.Trim() == operatorText, lines, $"The phase should have joined the lone operator under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
        }
    }

    /// <summary>
    /// Runs the phase alone on the input under LF and CRLF line endings and asserts that it returns the input unchanged
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    private void AssertPhaseKeepsInput(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalized = NormalizeLineEndings(input, endOfLine);

            Assert.AreEqual(normalized, ExecutePhase(normalized, endOfLine), $"The line-break phase changed the input under {DescribeLineEnding(endOfLine)} line endings.");
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