using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests the final output of the formatter for inputs whose structural transforms write only the separators their text needs
/// (FMT-STRUCT-026), and keep the comments they carry where and as the author wrote them (FMT-PIPE-013)
/// </summary>
[TestClass]
public class StructuralTransformSeparatorFinalOutputTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the final output of the first example of FMT-STRUCT-026, which the requirement leaves unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void ConvertedAndBracedClassKeepsItsFinalOutput()
    {
        const string input = """
                             class C
                             {
                                 int Get() => 42;

                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo(); // trailing
                                     else
                                         Bar();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Get()
                                    {
                                        return 42;
                                    }

                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                            Foo(); // trailing
                                        }
                                        else
                                        {
                                            Bar();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the final output of the second example of FMT-STRUCT-026: leading comments, a directive behind an inserted
    /// brace, and a comment that leads the statement behind an inserted brace stay where the author put them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    [Requirement("FMT-PIPE-013")]
    public void BracedBodiesKeepTheirCommentsAndTheDirectiveBehindThemWhereTheAuthorPutThem()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         // leading
                                         Foo();
                                     if (a)
                                         /* x */ /* y */ Foo();
                                     if (a)
                                         Foo();
                             #if DEBUG
                                     Bar();
                             #endif
                                     if (a)
                                         Foo();
                                     // about Bar
                                     Bar();
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

                                        if (a)
                                        {
                                            /* x */ /* y */ Foo();
                                        }

                                        if (a)
                                        {
                                            Foo();
                                        }
                                #if DEBUG
                                        Bar();
                                #endif

                                        if (a)
                                        {
                                            Foo();
                                        }

                                        // about Bar
                                        Bar();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two block comments in front of an unbraced <c>if</c> body stay on the statement's line with the space
    /// between them once the body is braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void TwoLeadingBlockCommentsOfABracedBodyStayOnTheStatementsLine()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         /* x */ /* y */ Foo();
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
                                            /* x */ /* y */ Foo();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line between two comments that lead an unbraced <c>if</c> body is kept once the body is braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void BlankLineBetweenTwoLeadingCommentsOfABracedBodyIsKept()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         // one

                                         // two
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
                                            // one

                                            // two
                                            Foo();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the final output of the third example of FMT-STRUCT-026: the gaps in front of carried trailing comments stay
    /// as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    [Requirement("FMT-PIPE-013")]
    public void GapsInFrontOfCarriedTrailingCommentsStayAsWritten()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();    // x
                                     if (a)
                                         Foo();// y
                                     if (a)
                                         Foo(); /* a */ // b
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
                                            Foo();    // x
                                        }

                                        if (a)
                                        {
                                            Foo();// y
                                        }

                                        if (a)
                                        {
                                            Foo(); /* a */ // b
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between two trailing comments of an unbraced <c>if</c> body is kept once the body is braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void SpaceBetweenTwoTrailingCommentsOfABracedBodyIsKept()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo(); /* a */ // b
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
                                            Foo(); /* a */ // b
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the fourth example of FMT-STRUCT-026 for an indented line: the block comment moved behind <c>return</c> ends up
    /// one space behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void IndentedCommentMovedBehindReturnEndsUpOneSpaceBehindIt()
    {
        const string input = """
                             class C
                             {
                                 int Get() =>
                                     /* c */ 42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Get()
                                    {
                                        return /* c */ 42;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the fourth example of FMT-STRUCT-026 for a line that is not indented: the block comment moved behind
    /// <c>return</c> follows it without a space, as the author wrote none in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CommentAtColumnZeroMovedBehindReturnFollowsItWithoutASpace()
    {
        const string input = """
                             class C
                             {
                                 int Get() =>
                             /* c */ 42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Get()
                                    {
                                        return/* c */ 42;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that accessors and a get-only property written without whitespace keep the final output they have, now that
    /// the transforms write no space around the arrow they create
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CompactAccessorsAndGetOnlyPropertyKeepTheirFinalOutput()
    {
        const string input = """
                             class C
                             {
                                 string _s;

                                 string P{get{return"";}set{_s=value;}}

                                 string Q{get{return"";}}
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    string _s;

                                    string P
                                    {
                                        get => "";
                                        set => _s = value;
                                    }

                                    string Q => "";
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a trailing single-line comment of an unbraced top-level <c>if</c> body at the end of the file stays behind
    /// the statement once the body is braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void TrailingCommentAtTheEndOfTheFileStaysBehindTheBracedStatement()
    {
        const string input = """
                             if (args.Length > 0)
                                 Foo(); // c
                             """;
        const string expected = """
                                if (args.Length > 0)
                                {
                                    Foo(); // c
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}