using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests that a comment trailing the head of a block statement — the closing parenthesis of its head, or the <c>else</c>
/// keyword — stays behind that token on its line while the opening brace of the body starts the line below
/// </summary>
[TestClass]
public class BlockStatementHeadCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: the comments behind <c>if</c>, <c>else if</c>, and <c>foreach</c> heads stay behind them,
    /// with the brace that shared the comment's line moved onto the line below
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public void SpecExampleKeepsEveryHeadCommentBehindItsHead()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a, bool b, int[] xs)
                                 {
                                     while (b)
                                         if (a) // why
                                         {
                                             Foo();
                                         }

                                     if (a) /* m */ { Foo(); }
                                     else if (b) // c
                                     {
                                         Bar();
                                     }

                                     foreach (var x in xs) /* f */ { Foo(); }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a, bool b, int[] xs)
                                    {
                                        while (b)
                                            if (a) // why
                                            {
                                                Foo();
                                            }

                                        if (a) /* m */
                                        {
                                            Foo();
                                        }
                                        else if (b) // c
                                        {
                                            Bar();
                                        }

                                        foreach (var x in xs) /* f */
                                        {
                                            Foo();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment behind the head of each named block statement, with the brace already on the next line,
    /// stays behind the head and the input is kept unchanged
    /// </summary>
    /// <param name="head">The block statement's head, as it stands in front of the comment</param>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    [DataRow("if (a)")]
    [DataRow("else if (b)")]
    [DataRow("else")]
    [DataRow("while (a)")]
    [DataRow("for (var i = 0; i < 1; i++)")]
    [DataRow("foreach (var x in xs)")]
    [DataRow("using (d)")]
    [DataRow("lock (o)")]
    public void LineCommentBehindHeadWithBraceOnNextLineStays(string head)
    {
        var input = CreateMethod(head,
                                 " // c",
                                 """
                                         {
                                             Foo();
                                         }
                                 """);

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited comment behind the head of each named block statement, with the body's braces on the
    /// comment's line, stays behind the head while the braces move onto lines of their own
    /// </summary>
    /// <param name="head">The block statement's head, as it stands in front of the comment</param>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    [DataRow("if (a)")]
    [DataRow("else if (b)")]
    [DataRow("else")]
    [DataRow("while (a)")]
    [DataRow("for (var i = 0; i < 1; i++)")]
    [DataRow("foreach (var x in xs)")]
    [DataRow("using (d)")]
    [DataRow("lock (o)")]
    public void DelimitedCommentBehindHeadWithBraceOnItsLineStays(string head)
    {
        var input = CreateMethod(head, " /* c */ { Foo(); }", string.Empty);
        var expected = CreateMethod(head,
                                    " /* c */",
                                    """
                                            {
                                                Foo();
                                            }
                                    """);

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change doc's first visible output change: a line comment behind an <c>if</c> condition behind another
    /// statement stays behind the condition
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public void LineCommentBehindIfConditionBehindStatementStays()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     Foo();

                                     if (a) // why
                                     {
                                         Foo();
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the change doc's second visible output change: a delimited comment behind an <c>if</c> condition with the
    /// body on the same line stays behind the condition, and the body gets its own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public void DelimitedCommentBehindIfConditionWithBodyOnItsLineStays()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     Foo();

                                     if (a) /* m */ { Foo(); }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        Foo();

                                        if (a) /* m */
                                        {
                                            Foo();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported input and the change doc's third visible output change: an <c>if</c> that is the body of a
    /// loop keeps the comment behind its condition and is kept unchanged in one run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public void LineCommentBehindIfConditionInLoopBodyStays()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a, bool b)
                                 {
                                     while (b)
                                         if (a) // why
                                         {
                                             Foo();
                                         }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the change doc's fourth visible output change: an <c>if</c> whose unbraced body gets braces keeps the
    /// comment behind its condition
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public void LineCommentBehindIfConditionWithUnbracedBodyStays()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     Foo();

                                     if (a) // c
                                         Foo();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        Foo();

                                        if (a) // c
                                        {
                                            Foo();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment behind an <c>if</c> condition that is the first statement of its block stays behind
    /// the condition
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public void LineCommentBehindFirstIfConditionStays()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a) // c
                                     {
                                         Foo();
                                     }
                                     else // d
                                     {
                                         Bar();
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Builds a method whose last statement is the block statement with the given head, behind a statement and a blank
    /// line. For an <c>else</c> head, an <c>if</c> statement with a block stands in front of it
    /// </summary>
    /// <param name="head">The block statement's head</param>
    /// <param name="behindHead">The text written behind the head on its line</param>
    /// <param name="body">The lines of the body below the head, without a final line break; empty when there are none</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateMethod(string head, string behindHead, string body)
    {
        var prefix = head.StartsWith("else", StringComparison.Ordinal)
                         ? "        if (a)\n        {\n            Foo();\n        }\n"
                         : string.Empty;
        var bodyLines = string.IsNullOrEmpty(body)
                            ? string.Empty
                            : $"{body.Replace("\r\n", "\n")}\n";

        return $"class C\n{{\n    void M(bool a, bool b, int[] xs, System.IDisposable d, object o)\n    {{\n        Foo();\n\n{prefix}        {head}{behindHead}\n{bodyLines}    }}\n}}";
    }

    #endregion // Methods
}