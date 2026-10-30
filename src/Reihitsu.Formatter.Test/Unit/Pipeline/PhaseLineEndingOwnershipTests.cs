using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests that observe every phase of the production pipeline: after the preparation phase, no phase receives or returns an
/// end-of-line trivia, a line break that ends a directive, or a newline of a documentation comment other than the detected
/// line ending, while the line breaks inside literals, comments, and disabled text keep their characters
/// </summary>
[TestClass]
public class PhaseLineEndingOwnershipTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Placeholder for a line break that the line-ending rules rewrite and that the test replaces with a foreign terminator
    /// </summary>
    private const string ForeignLineBreakPlaceholder = "¶";

    /// <summary>
    /// Placeholder for a line break inside a literal, a comment, or disabled text, which keeps its characters
    /// </summary>
    private const string KeptLineBreakPlaceholder = "¤";

    /// <summary>
    /// Data-row value that selects the other of LF and CRLF as the foreign terminator
    /// </summary>
    private const string OtherLineEnding = "other";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the spec example: wrapping a statement with a leading comment in braces writes the line breaks behind the
    /// opening brace and behind the comment with the detected line ending, so the output of every phase holds only that line
    /// ending, under LF and under CRLF
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-019")]
    public void BracesAroundAStatementWithALeadingCommentAreWrittenWithTheDetectedLineEnding()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         // leading
                                         Foo();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                            // leading
                                            Foo();
                                        }
                                    }
                                }
                                """;

        AssertEveryPhaseUsesTheDetectedLineEnding(input, expected);
    }

    /// <summary>
    /// Verifies that the line breaks written when braces are added around a branch with one or two leading single-line
    /// comments, a leading block comment, an <c>else</c> branch with a leading comment, or an <c>else if</c> chain are the
    /// detected line ending in the output of every phase
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    /// <param name="expected">The expected output with LF line endings</param>
    [TestMethod]
    [Requirement("FMT-PIPE-019")]
    [DataRow("class C\n{\n    void M(bool a)\n    {\n        if (a)\n            // c1\n            // c2\n            Foo();\n    }\n}",
             "class C\n{\n    void M(bool a)\n    {\n        if (a)\n        {\n            // c1\n            // c2\n            Foo();\n        }\n    }\n}")]
    [DataRow("class C\n{\n    void M(bool a)\n    {\n        if (a)\n            /* c */\n            Foo();\n    }\n}",
             "class C\n{\n    void M(bool a)\n    {\n        if (a)\n        {\n            /* c */\n            Foo();\n        }\n    }\n}")]
    [DataRow("class C\n{\n    void M(bool a)\n    {\n        if (a)\n            Foo();\n        else\n            // e\n            Bar();\n    }\n}",
             "class C\n{\n    void M(bool a)\n    {\n        if (a)\n        {\n            Foo();\n        }\n        else\n        {\n            // e\n            Bar();\n        }\n    }\n}")]
    [DataRow("class C\n{\n    void M(bool a, bool b)\n    {\n        if (a)\n            // c1\n            Foo();\n        else if (b)\n            // c2\n            Bar();\n    }\n}",
             "class C\n{\n    void M(bool a, bool b)\n    {\n        if (a)\n        {\n            // c1\n            Foo();\n        }\n        else if (b)\n        {\n            // c2\n            Bar();\n        }\n    }\n}")]
    public void BracesAroundBranchesWithLeadingCommentsAreWrittenWithTheDetectedLineEnding(string input, string expected)
    {
        AssertEveryPhaseUsesTheDetectedLineEnding(input, expected);
    }

    /// <summary>
    /// Verifies that the line breaks ending region, conditional, and pragma directives, the newlines of a documentation
    /// comment, and the line breaks next to comments are the detected line ending in the output of every phase, also where
    /// phases add blank lines, wrap a statement in braces, and indent next to those directives and comments
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-019")]
    public void LineBreaksAroundDirectivesAndCommentsAreTheDetectedLineEndingInEveryPhase()
    {
        const string input = """
                             class C
                             {
                                 #region fields
                                 private int _a; // trailing
                                 #endregion
                                 /// <summary>
                                 /// Doc
                                 /// </summary>
                                 /// <param name="a">A</param>
                                 private void M(bool a)
                                 {
                             #if false
                                     Disabled();
                             #else
                                     Enabled();
                             #endif
                             #pragma warning disable CS0168
                                     /* block */
                                     if (a)
                                         // leading
                                         Foo();
                             #pragma warning restore CS0168
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields

                                    private int _a; // trailing

                                    #endregion // Fields

                                    /// <summary>
                                    /// Doc
                                    /// </summary>
                                    /// <param name="a">A</param>
                                    private void M(bool a)
                                    {
                                #if false
                                        Disabled();
                                #else
                                        Enabled();

                                #endif
                                #pragma warning disable CS0168
                                        /* block */
                                        if (a)
                                        {
                                            // leading
                                            Foo();
                                        }
                                #pragma warning restore CS0168
                                    }
                                }
                                """;

        AssertEveryPhaseUsesTheDetectedLineEnding(input, expected);
    }

    /// <summary>
    /// Verifies that a foreign line break between tokens, at the end of a directive, and inside a documentation comment of an
    /// LF or CRLF file is rewritten by the preparation phase, so no later phase receives or returns it
    /// </summary>
    /// <param name="terminator">The foreign terminator, or <c>other</c> for the other of LF and CRLF</param>
    [TestMethod]
    [Requirement("FMT-PIPE-019")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    [DataRow(OtherLineEnding)]
    public void ForeignLineBreaksOfTheInputAreRewrittenBeforeEveryLaterPhase(string terminator)
    {
        const string input = "class C\n{\n    #region Fields¶¶    private int _a;¶    private int _b;\n\n    #endregion // Fields\n\n    /// <summary>¶    /// Doc¶    /// </summary>\n    private void M()\n    {\n    }\n}";
        const string expected = "class C\n{\n    #region Fields\n\n    private int _a;\n    private int _b;\n\n    #endregion // Fields\n\n    /// <summary>\n    /// Doc\n    /// </summary>\n    private void M()\n    {\n    }\n}";

        AssertEveryPhaseUsesTheDetectedLineEnding(input, expected, terminator);
    }

    /// <summary>
    /// Verifies that a lone <c>\r</c> or a U+2028 inside a verbatim string, inside a multi-line raw string, inside a block
    /// comment, and inside disabled text of an LF or CRLF file keeps its characters through every phase, while every line
    /// break the line-ending rules cover is the detected line ending in the output of every phase
    /// </summary>
    /// <param name="terminator">The terminator inside the literals, the comment, and the disabled text</param>
    [TestMethod]
    [Requirement("FMT-PIPE-019")]
    [Requirement("FMT-EOL-004")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreaksInsideLiteralsCommentsAndDisabledTextKeepTheirCharacters(string terminator)
    {
        const string input = "class C\n{\n    private string _s = @\"a¤b\";\n    private string _r = \"\"\"\n                        a¤                        b\n                        \"\"\";\n    private int _x; /* a¤b */\n#if false\n    a¤b\n#endif\n}";

        AssertEveryPhaseUsesTheDetectedLineEnding(input, input, keptTerminator: terminator);
    }

    /// <summary>
    /// Verifies that a documentation element on the last line of a file without a final line break, which the documentation
    /// phase expands onto several lines, is written with the detected line ending, under LF and under CRLF, so a second pass
    /// leaves the output unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-019")]
    [Requirement("FMT-PIPE-004")]
    public void ExpandedDocumentationElementOnTheLastLineIsWrittenWithTheDetectedLineEnding()
    {
        const string input = "class C\n{\n}\n/// <summary>a</summary>";
        const string expected = "class C;\n\n/// <summary>\n/// a\n/// </summary>";

        AssertEveryPhaseUsesTheDetectedLineEnding(input, expected);
    }

    /// <summary>
    /// Formats the input through the observed production pipeline under LF and CRLF and asserts that every phase after the
    /// preparation phase receives and returns only the detected line ending in the line breaks the line-ending rules cover,
    /// that the LineEndings sub-phase returns only that line ending, that the output matches the expected text, that a
    /// second pass leaves it unchanged, and that no phase run alone changes it
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings, where <c>¶</c> marks a foreign line break and <c>¤</c> a kept one</param>
    /// <param name="expectedWithLf">The expected output with LF line endings, where <c>¤</c> marks a kept line break</param>
    /// <param name="foreignTerminator">The terminator that replaces <c>¶</c>, or <c>other</c> for the other of LF and CRLF</param>
    /// <param name="keptTerminator">The terminator that replaces <c>¤</c></param>
    private static void AssertEveryPhaseUsesTheDetectedLineEnding(string inputWithLf, string expectedWithLf, string foreignTerminator = null, string keptTerminator = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var input = NormalizeLineEndings(inputWithLf, endOfLine).Replace(ForeignLineBreakPlaceholder, GetForeignTerminator(foreignTerminator, endOfLine))
                                                                    .Replace(KeptLineBreakPlaceholder, keptTerminator);
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine).Replace(KeptLineBreakPlaceholder, keptTerminator);
            var phases = ObservedPipeline.Run(input, endOfLine);

            for (var index = 2; index < phases.Count; index++)
            {
                AssertNoForeignLineBreak(phases[index].Name, "received", phases[index].Input, endOfLine);
                AssertNoForeignLineBreak(phases[index].Name, "returned", phases[index].Output, endOfLine);
            }

            Assert.AreEqual(ObservedPipeline.TabReplacementPhaseName, phases[0].Name, "The first phase of the pipeline should be the TabReplacement sub-phase of the preparation phase.");
            Assert.AreEqual(ObservedPipeline.LineEndingsPhaseName, phases[1].Name, "The second phase of the pipeline should be the LineEndings sub-phase of the preparation phase.");

            AssertNoForeignLineBreak(phases[1].Name, "returned", phases[1].Output, endOfLine);

            Assert.AreEqual(Escape(expected), Escape(phases[^1].Output.ToFullString()), $"Formatter output mismatch under {endingName} line endings.");

            var secondPass = ObservedPipeline.Run(expected, endOfLine);

            Assert.AreEqual(Escape(expected), Escape(secondPass[^1].Output.ToFullString()), $"Formatter is not idempotent under {endingName} line endings.");

            PhaseStabilityCheck.Verify(expected, endOfLine, null);
        }
    }

    /// <summary>
    /// Returns the terminator that replaces the foreign-line-break placeholder
    /// </summary>
    /// <param name="foreignTerminator">The terminator a test asked for, <c>other</c>, or <see langword="null"/></param>
    /// <param name="endOfLine">The line ending of the file</param>
    /// <returns>The terminator, the other of LF and CRLF for <c>other</c>, or the file's line ending when none was asked for</returns>
    private static string GetForeignTerminator(string foreignTerminator, string endOfLine)
    {
        if (foreignTerminator == null)
        {
            return endOfLine;
        }

        if (foreignTerminator == OtherLineEnding)
        {
            return endOfLine == "\n" ? "\r\n" : "\n";
        }

        return foreignTerminator;
    }

    /// <summary>
    /// Asserts that the node holds no line break the line-ending rules cover that differs from the line ending
    /// </summary>
    /// <param name="phaseName">The name of the phase</param>
    /// <param name="direction">Whether the phase received or returned the node</param>
    /// <param name="node">The node to scan</param>
    /// <param name="endOfLine">The detected line ending</param>
    private static void AssertNoForeignLineBreak(string phaseName, string direction, SyntaxNode node, string endOfLine)
    {
        var findings = ObservedPipeline.FindForeignLineBreaks(node, endOfLine);

        Assert.IsEmpty(findings, $"{phaseName} {direction} a line break other than {DescribeLineEnding(endOfLine)}: {string.Join("; ", findings)}");
    }

    /// <summary>
    /// Makes line terminators visible, so an assertion message shows which one differs
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The text with its line terminators escaped</returns>
    private static string Escape(string text)
    {
        return text.Replace("\r", "\\r")
                   .Replace("\n", "\\n")
                   .Replace("\u0085", "\\u0085")
                   .Replace("\u2028", "\\u2028")
                   .Replace("\u2029", "\\u2029");
    }

    #endregion // Methods
}