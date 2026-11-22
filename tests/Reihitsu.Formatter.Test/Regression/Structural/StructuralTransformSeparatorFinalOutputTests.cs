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

    /// <summary>
    /// Verifies that a hand-written <c>get=&gt;"";</c> and a converted setter end up spaced alike: one space on each side of the
    /// hand-written arrow and of the arrow the transform creates
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    [Requirement("FMT-SPACE-017")]
    public void HandWrittenArrowAndConvertedAccessorArrowAreSpacedAlike()
    {
        const string input = """
                             class C
                             {
                                 string _s;

                                 string P
                                 {
                                     get=>"";
                                     set{_s = value;}
                                 }
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
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the final output of the <c>return</c> example of FMT-STRUCT-026: one space behind every created <c>return</c>,
    /// whether the transform wrote it as a separator or the spacing phase writes it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void EveryCreatedReturnEndsUpOneSpaceInFrontOfItsExpression()
    {
        const string input = """
                             class C
                             {
                                 int A() => a;

                                 int B() => \u0061;

                                 int C() => @a;

                                 int D() => @"s";

                                 int E() => (a);
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A()
                                    {
                                        return a;
                                    }

                                    int B()
                                    {
                                        return \u0061;
                                    }

                                    int C()
                                    {
                                        return @a;
                                    }

                                    int D()
                                    {
                                        return @"s";
                                    }

                                    int E()
                                    {
                                        return (a);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the inputs of the removed examples of FMT-STRUCT-026 that start from a comment behind the arrow and a comment
    /// moved behind the created <c>return</c> are refused as unsupported: the input is returned unchanged, and the first comment
    /// behind an arrow is reported. The example with a comment directly at the arrow is refused under FMT-UNSUP-013
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int Get() =>\n        /* c */ 42;", "/* c */", DisplayName = "Indented block comment in front of the expression")]
    [DataRow("    int Get() =>\n/* c */ 42;", "/* c */", DisplayName = "Block comment in front of the expression at the start of its line")]
    [DataRow("    int Get() => // c\n        /* d */ 42;", "// c", DisplayName = "Line comment behind the arrow and a block comment in front of the expression")]
    [DataRow("    int Get2() => // c\n        // d\n        42;", "// c", DisplayName = "Line comment behind the arrow and a comment line")]
    [DataRow("    void M()\n    {\n        int L() => // c\n            /* d */ 42;\n    }", "// c", DisplayName = "Local function")]
    [DataRow("    public static C operator +(C x, C y) => // c\n        /* d */ x;", "// c", DisplayName = "Operator")]
    [DataRow("    public static implicit operator int(C c) => // c\n        /* d */ 0;", "// c", DisplayName = "Conversion operator")]
    [DataRow("    void Run() => // c\n        /* d */ Foo();", "// c", DisplayName = "Converted expression statement")]
    [DataRow("    int Get() =>\n        // c\n        42;", "// c", DisplayName = "Comment line moved in front of the created return")]
    public void CommentsBehindTheArrowOfTheRemovedExamplesAreUnsupported(string members, string comment)
    {
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, $"class C\n{{\n{members}\n}}", comment);
    }

    #endregion // Tests
}