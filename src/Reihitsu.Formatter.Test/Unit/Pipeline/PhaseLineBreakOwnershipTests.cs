using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests that whether two tokens stand on one line is decided before the blank-line phase, that the phases after it
/// change neither those line breaks nor the blank lines, and that no phase after the line-break phase moves a comment
/// onto a line of its own or off one
/// </summary>
[TestClass]
public class PhaseLineBreakOwnershipTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The input of the FMT-PIPE-025 example: switch sections that share one line
    /// </summary>
    private const string SwitchSectionsOnOneLine = """
                                                   class C
                                                   {
                                                       void M(int x)
                                                       {
                                                           switch (x) { case 1: break; case 2: break; }
                                                       }
                                                   }
                                                   """;

    /// <summary>
    /// The input of the FMT-PIPE-026 example: a comment behind the condition of an <c>if</c> that is a loop body
    /// </summary>
    private const string CommentBehindIfInLoopBody = """
                                                     class C
                                                     {
                                                         void M(bool a, bool b)
                                                         {
                                                             while (b)
                                                                 if (a) // why
                                                                 {
                                                                     A();
                                                                 }
                                                         }
                                                     }
                                                     """;

    /// <summary>
    /// Statements next to an empty statement that a blank-line requirement separates, in a block and in a switch section
    /// </summary>
    private const string EmptyStatementPairs = """
                                               class C
                                               {
                                                   int M(int a)
                                                   {
                                                       a++;
                                                       ; return a;
                                                   }

                                                   void N(bool a)
                                                   {
                                                       if (a) { } ;
                                                   }

                                                   int O(int x)
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

    /// <summary>
    /// Statements and a section that share a line with the statement or section in front of them, around a closing brace
    /// and a <c>break</c>
    /// </summary>
    private const string SameLineStatementsAndSections = """
                                                         class C
                                                         {
                                                             void M(int a)
                                                             {
                                                                 a++;
                                                                 if (a > 0) { a++; } a++;
                                                                 switch (a)
                                                                 {
                                                                     case 1:
                                                                         a++;
                                                                         break; case 2:
                                                                         break;
                                                                 }
                                                             }
                                                         }
                                                         """;

    /// <summary>
    /// A switch whose braced sections carry blank lines and comments around the first statement and the terminal
    /// <c>break</c>
    /// </summary>
    private const string BracedSectionWithComments = """
                                                     class C
                                                     {
                                                         void M(int x)
                                                         {
                                                             switch (x)
                                                             {
                                                                 case 1: // label

                                                                     // a
                                                                     A();
                                                                     B(); // b


                                                                     // c
                                                                     /* d */ break; // e

                                                                 case 2:
                                                                     C();
                                                                     break;
                                                             }
                                                         }
                                                     }
                                                     """;

    /// <summary>
    /// A chain link behind the closing line of a comment that spans several lines
    /// </summary>
    private const string ChainLinkBehindMultiLineComment = """
                                                           class C
                                                           {
                                                               void M(dynamic a)
                                                               {
                                                                   A();
                                                                   var r = a.B()
                                                                            .C()
                                                                            /* c
                                                                             */ .D();
                                                               }
                                                           }
                                                           """;

    /// <summary>
    /// Statements behind the closing line of a comment that spans several lines, in blocks and in switch sections, with
    /// and without a blank-line requirement between them and the statement in front
    /// </summary>
    private const string StatementsBehindMultiLineCommentTail = """
                                                                class C
                                                                {
                                                                    int M1(int a)
                                                                    {
                                                                        a++; /* x
                                                                        */ return a;
                                                                    }

                                                                    int M2(int a)
                                                                    {
                                                                        a++;
                                                                        ; /* x
                                                                        */ return a;
                                                                    }

                                                                    void M3(int a)
                                                                    {
                                                                        if (a > 0) { A(); } /* q
                                                                        */ B();
                                                                    }

                                                                    void M4(int a)
                                                                    {
                                                                        while (a > 0) { A(); /* x
                                                                        */ break; }
                                                                    }

                                                                    void M5(int a)
                                                                    {
                                                                        int b = a; /* x
                                                                        */ Foo(b);
                                                                    }

                                                                    int M6(int x)
                                                                    {
                                                                        switch (x)
                                                                        {
                                                                            case 1: ; /* a
                                                                            */ return 1;
                                                                            default:
                                                                                return 0;
                                                                        }
                                                                    }

                                                                    int M7(int x)
                                                                    {
                                                                        switch (x)
                                                                        {
                                                                            case 1: A(); /* x
                                                                            */ return 1;
                                                                            default:
                                                                                return 0;
                                                                        }
                                                                    }

                                                                    void M8()
                                                                    {
                                                                        A(); /* x
                                                                        */ B();
                                                                    }

                                                                    void M9(int x)
                                                                    {
                                                                        switch (x)
                                                                        {
                                                                            case 1: A(); /* x
                                                                            */ break;
                                                                        }
                                                                    }
                                                                }
                                                                """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies the FMT-PIPE-025 example: sections on one line get their own lines and the blank line behind
    /// <c>break</c> in one run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-025")]
    public void SwitchSectionsOnOneLineGetLinesAndBlankLineInOneRun()
    {
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                break;

                                            case 2:
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(SwitchSectionsOnOneLine, expected);
    }

    /// <summary>
    /// Verifies the FMT-PIPE-026 example and the reported input: the comment behind the condition stays behind it, and
    /// the input is kept unchanged in one run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-026")]
    public void CommentBehindIfConditionInLoopBodyIsKeptInOneRun()
    {
        AssertRuleResult(CommentBehindIfInLoopBody);
    }

    /// <summary>
    /// Verifies that the blank-line phase does not change whether two tokens with text stand on one line
    /// </summary>
    /// <param name="fixture">The name of the fixture to format</param>
    [TestMethod]
    [Requirement("FMT-PIPE-025")]
    [DataRow(nameof(SwitchSectionsOnOneLine))]
    [DataRow(nameof(EmptyStatementPairs))]
    [DataRow(nameof(SameLineStatementsAndSections))]
    [DataRow(nameof(BracedSectionWithComments))]
    [DataRow(nameof(ChainLinkBehindMultiLineComment))]
    [DataRow(nameof(StatementsBehindMultiLineCommentTail))]
    public void BlankLinePhaseKeepsWhichTokensShareALine(string fixture)
    {
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var (phaseName, before, after) in ObservePhases(GetFixture(fixture), endOfLine))
            {
                if (phaseName != "BlankLinePhase")
                {
                    continue;
                }

                Assert.AreSequenceEqual(GetTokenLineShape(before, false),
                                        GetTokenLineShape(after, false),
                                        $"The blank-line phase changed whether two tokens share a line under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{before}{Environment.NewLine}---{Environment.NewLine}{after}");
            }
        }
    }

    /// <summary>
    /// Verifies that the blank-line phase adds and removes only blank lines: its output, with blank lines removed and
    /// whitespace at the end of each line trimmed, equals its input treated the same way. A line break the phase wrote
    /// behind the closing line of a comment that spans several lines, where that comment's last line already holds a
    /// token, shows here although the token's physical line does not change
    /// </summary>
    /// <param name="fixture">The name of the fixture to format</param>
    [TestMethod]
    [Requirement("FMT-PIPE-025")]
    [DataRow(nameof(SwitchSectionsOnOneLine))]
    [DataRow(nameof(EmptyStatementPairs))]
    [DataRow(nameof(SameLineStatementsAndSections))]
    [DataRow(nameof(BracedSectionWithComments))]
    [DataRow(nameof(ChainLinkBehindMultiLineComment))]
    [DataRow(nameof(StatementsBehindMultiLineCommentTail))]
    public void BlankLinePhaseChangesOnlyBlankLines(string fixture)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var isBlankLinePhaseObserved = false;

            foreach (var (phaseName, before, after) in ObservePhases(GetFixture(fixture), endOfLine))
            {
                if (phaseName != "BlankLinePhase")
                {
                    continue;
                }

                isBlankLinePhaseObserved = true;

                Assert.AreSequenceEqual(GetNonBlankLines(before, endOfLine),
                                        GetNonBlankLines(after, endOfLine),
                                        $"The blank-line phase changed more than blank lines under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{before}{Environment.NewLine}---{Environment.NewLine}{after}");
            }

            Assert.IsTrue(isBlankLinePhaseObserved, "The pipeline reported no blank-line phase.");
        }
    }

    /// <summary>
    /// Verifies that the phases after the blank-line phase change neither whether two tokens with text stand on one line
    /// nor the number of blank lines between them
    /// </summary>
    /// <param name="fixture">The name of the fixture to format</param>
    [TestMethod]
    [Requirement("FMT-PIPE-025")]
    [DataRow(nameof(SwitchSectionsOnOneLine))]
    [DataRow(nameof(EmptyStatementPairs))]
    [DataRow(nameof(SameLineStatementsAndSections))]
    [DataRow(nameof(BracedSectionWithComments))]
    [DataRow(nameof(ChainLinkBehindMultiLineComment))]
    [DataRow(nameof(StatementsBehindMultiLineCommentTail))]
    public void PhasesAfterBlankLinePhaseKeepLineBreaksAndBlankLines(string fixture)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var isAfterBlankLinePhase = false;

            foreach (var (phaseName, before, after) in ObservePhases(GetFixture(fixture), endOfLine))
            {
                if (isAfterBlankLinePhase)
                {
                    Assert.AreSequenceEqual(GetTokenLineShape(before, true),
                                            GetTokenLineShape(after, true),
                                            $"{phaseName} changed a line break or a blank line between two tokens under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{before}{Environment.NewLine}---{Environment.NewLine}{after}");
                }

                isAfterBlankLinePhase |= phaseName == "BlankLinePhase";
            }

            Assert.IsTrue(isAfterBlankLinePhase, "The pipeline reported no blank-line phase.");
        }
    }

    /// <summary>
    /// Verifies that no phase after the line-break phase changes whether a comment starts its line
    /// </summary>
    /// <param name="fixture">The name of the fixture to format</param>
    [TestMethod]
    [Requirement("FMT-PIPE-026")]
    [DataRow(nameof(CommentBehindIfInLoopBody))]
    [DataRow(nameof(BracedSectionWithComments))]
    [DataRow(nameof(ChainLinkBehindMultiLineComment))]
    [DataRow(nameof(SameLineStatementsAndSections))]
    [DataRow(nameof(StatementsBehindMultiLineCommentTail))]
    public void PhasesAfterLineBreakPhaseKeepWhetherCommentsStartTheirLine(string fixture)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var isAfterLineBreakPhase = false;

            foreach (var (phaseName, before, after) in ObservePhases(GetFixture(fixture), endOfLine))
            {
                if (isAfterLineBreakPhase)
                {
                    Assert.AreSequenceEqual(GetCommentLineStarts(before),
                                            GetCommentLineStarts(after),
                                            $"{phaseName} changed whether a comment starts its line under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{before}{Environment.NewLine}---{Environment.NewLine}{after}");
                }

                isAfterLineBreakPhase |= phaseName == "LineBreakPhase";
            }

            Assert.IsTrue(isAfterLineBreakPhase, "The pipeline reported no line-break phase.");
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the source text of a fixture by its constant's name
    /// </summary>
    /// <param name="fixture">The constant's name</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string GetFixture(string fixture)
    {
        return fixture switch
               {
                   nameof(SwitchSectionsOnOneLine) => SwitchSectionsOnOneLine,
                   nameof(CommentBehindIfInLoopBody) => CommentBehindIfInLoopBody,
                   nameof(EmptyStatementPairs) => EmptyStatementPairs,
                   nameof(SameLineStatementsAndSections) => SameLineStatementsAndSections,
                   nameof(BracedSectionWithComments) => BracedSectionWithComments,
                   nameof(ChainLinkBehindMultiLineComment) => ChainLinkBehindMultiLineComment,
                   nameof(StatementsBehindMultiLineCommentTail) => StatementsBehindMultiLineCommentTail,
                   _ => throw new ArgumentOutOfRangeException(nameof(fixture), fixture, null)
               };
    }

    /// <summary>
    /// Describes, for every token with text, its text and how it stands to the token with text in front of it: whether
    /// it shares that token's line and, when asked, how many blank lines stand between them
    /// </summary>
    /// <param name="text">The source text</param>
    /// <param name="countBlankLines">Whether the description includes the blank lines between the tokens</param>
    /// <returns>One entry per token with text</returns>
    private static List<string> GetTokenLineShape(string text, bool countBlankLines)
    {
        var sourceText = SourceText.From(text);
        var root = CSharpSyntaxTree.ParseText(sourceText).GetRoot();
        var shape = new List<string>();
        SyntaxToken? previous = null;

        foreach (var token in root.DescendantTokens(descendIntoTrivia: false))
        {
            if (token.Span.IsEmpty)
            {
                continue;
            }

            if (previous is { } previousToken)
            {
                var previousLine = sourceText.Lines.GetLineFromPosition(previousToken.Span.End).LineNumber;
                var line = sourceText.Lines.GetLineFromPosition(token.SpanStart).LineNumber;
                var blankLines = 0;

                for (var lineNumber = previousLine + 1; lineNumber < line; lineNumber++)
                {
                    if (string.IsNullOrWhiteSpace(sourceText.Lines[lineNumber].ToString()))
                    {
                        blankLines++;
                    }
                }

                var relation = line == previousLine ? "same line" : "new line";

                shape.Add(countBlankLines ? $"{token.Text} ({relation}, {blankLines} blank)" : $"{token.Text} ({relation})");
            }
            else
            {
                shape.Add(token.Text);
            }

            previous = token;
        }

        return shape;
    }

    /// <summary>
    /// Gets the lines of a text that are not blank, each with the whitespace at its end trimmed
    /// </summary>
    /// <param name="text">The source text</param>
    /// <param name="endOfLine">The line ending the text uses</param>
    /// <returns>The lines that are not blank, in order</returns>
    private static List<string> GetNonBlankLines(string text, string endOfLine)
    {
        var lines = new List<string>();

        foreach (var line in text.Split(endOfLine))
        {
            if (string.IsNullOrWhiteSpace(line) == false)
            {
                lines.Add(line.TrimEnd());
            }
        }

        return lines;
    }

    /// <summary>
    /// Describes, for every comment, its text and whether it starts its line
    /// </summary>
    /// <param name="text">The source text</param>
    /// <returns>One entry per comment, in source order</returns>
    private static List<string> GetCommentLineStarts(string text)
    {
        var sourceText = SourceText.From(text);
        var root = CSharpSyntaxTree.ParseText(sourceText).GetRoot();
        var comments = new List<string>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: false))
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) == false
                && trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) == false
                && trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) == false
                && trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) == false)
            {
                continue;
            }

            var line = sourceText.Lines.GetLineFromPosition(trivia.SpanStart);
            var textInFront = sourceText.ToString(TextSpan.FromBounds(line.Start, trivia.SpanStart));
            var firstLine = trivia.ToString().Split('\n')[0].TrimEnd('\r');

            comments.Add($"{firstLine} ({(string.IsNullOrWhiteSpace(textInFront) ? "starts its line" : "behind code")})");
        }

        return comments;
    }

    /// <summary>
    /// Runs the whole pipeline on the input and records the input and output text of every reported phase
    /// </summary>
    /// <param name="input">The source text, with LF line endings</param>
    /// <param name="endOfLine">The line ending to format with</param>
    /// <returns>The phase name and the input and output text of each phase, in runtime order</returns>
    private List<(string PhaseName, string Before, string After)> ObservePhases(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);
        var phases = new List<(string PhaseName, string Before, string After)>();

        FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken),
                                   new FormattingContext(endOfLine),
                                   (phaseName, before, after) => phases.Add((phaseName, before.ToFullString(), after.ToFullString())),
                                   TestContext.CancellationToken);

        return phases;
    }

    #endregion // Methods
}