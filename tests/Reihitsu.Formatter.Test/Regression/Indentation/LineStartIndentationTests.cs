using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for the indentation of what starts a line: code with trailing comments, comments, documentation comments, directives, and the content the formatter must not re-indent
/// </summary>
[TestClass]
public class LineStartIndentationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: only the whitespace in front of the line changes and the trailing comment keeps its spacing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-005")]
    public void LineMovesAsAWholeWithItsTrailingComment()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                         var x = 1; // one
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1; // one
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a moved line keeps the spacing between its own tokens and its trailing comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-005")]
    public void WrappedLineKeepsItsInnerSpacingWhenMoved()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                               var x = a; /* one */   // two
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = a; /* one */   // two
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a comment before a statement takes its column and a comment before a closing brace is one level deeper than the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentTakesTheColumnOfTheFollowingCodeOrOneLevelInsideAClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                         // before statement
                                     var x = 1;

                               // before brace
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        // before statement
                                        var x = 1;

                                        // before brace
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment on its own line before a member takes the member's column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeMemberTakesTheMemberColumn()
    {
        const string input = """
                             class C
                             {
                             // first
                                 int _a;

                                           /* second */
                                 int _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // first
                                    int _a;

                                    /* second */
                                    int _b;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a documentation comment behind an opening brace stays there while the member below it is re-indented
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void DelimitedDocumentationBehindOpeningBraceStaysInPlace()
    {
        const string input = """
                             public class TestClass
                             { /** Doc */
                                     public string Description { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                { /** Doc */
                                    public string Description { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: only the first line of a delimited comment is re-indented and its other lines are kept as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-007")]
    public void MultiLineCommentMovesOnlyItsFirstLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                         /* first
                                            second */
                                     var x = 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        /* first
                                               second */
                                        var x = 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: every <c>///</c> line of a documentation comment takes the column of the member it documents
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-008")]
    public void EveryDocumentationLineIsIndentedWithTheDocumentedMember()
    {
        const string input = """
                             class C
                             {
                                     /// <summary>
                               /// Gets the value.
                                         /// </summary>
                                 int _value;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Gets the value.
                                    /// </summary>
                                    int _value;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: indented <c>#if</c>, <c>#endif</c>, and <c>#pragma</c> directives start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-009")]
    public void ConditionalAndPragmaDirectivesMoveToColumnZero()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 0;
                                     #if !NEVER
                                     x = 1;
                                     #endif
                                         #pragma warning disable CS0168
                                     int y;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 0;
                                #if !NEVER
                                        x = 1;
                                #endif
                                #pragma warning disable CS0168
                                        int y;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that indented <c>#nullable</c>, <c>#if</c>, and <c>#endif</c> directives between members start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-009")]
    public void NullableAndConditionalDirectivesBetweenMembersMoveToColumnZero()
    {
        const string input = """
                             class C
                             {
                                 #nullable enable
                                 string _a;
                                     #if !NEVER
                                 int _b;
                                   #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #nullable enable
                                    string _a;
                                #if !NEVER
                                    int _b;
                                #endif
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that indented <c>#define</c> and <c>#undef</c> directives at the start of the file start in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-009")]
    public void DefineAndUndefDirectivesMoveToColumnZero()
    {
        const string input = """
                               #define FEATURE
                                 #undef LEGACY
                             class C
                             {
                                 int _x;
                             }
                             """;
        const string expected = """
                                #define FEATURE
                                #undef LEGACY
                                class C
                                {
                                    int _x;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: region directives take the member column inside a type and the label column between switch sections
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-010")]
    public void RegionDirectivesTakeTheMemberOrLabelColumn()
    {
        const string input = """
                             class C
                             {
                                     #region Fields

                                 private int _x;

                               #endregion // Fields

                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                             #region Cases

                                         case 1:
                                             break;

                                     #endregion // Cases
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields

                                    private int _x;

                                    #endregion // Fields

                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            #region Cases

                                            case 1:
                                                break;

                                            #endregion // Cases
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that region directives inside a method body take the statement column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-010")]
    public void RegionDirectivesInsideABlockTakeTheStatementColumn()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                             #region Body

                                     var x = 1;

                                             #endregion // Body
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        #region Body

                                        var x = 1;

                                        #endregion // Body
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: disabled text and the continuation line of a verbatim string keep their whitespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-011")]
    public void DisabledTextAndVerbatimContinuationLinesAreNotReIndented()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                             #if NEVER
                                         var  y = 2;
                               if (y) {
                             #endif
                                     var s = @"line one
                                   line two";
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the continuation line of an interpolated verbatim string keeps its whitespace while the statement moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-011")]
    public void InterpolatedVerbatimContinuationLineIsNotReIndented()
    {
        const string input = """
                             class C
                             {
                                 void M(int v)
                                 {
                                         var s = $@"line {v}
                                   line two";
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int v)
                                    {
                                        var s = $@"line {v}
                                      line two";
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}