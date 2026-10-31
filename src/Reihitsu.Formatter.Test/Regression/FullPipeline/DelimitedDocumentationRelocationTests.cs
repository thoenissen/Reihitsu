using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests for delimited documentation comments written behind code: the comments that start a new line
/// below a blank line, the code and comments that stay around them, and the comments that stay where they were written
/// </summary>
[TestClass]
public class DelimitedDocumentationRelocationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: a delimited documentation comment behind an enum member starts a new line
    /// below a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-BLANK-012")]
    public void MovesDelimitedDocumentationBehindEnumMemberBelowBlankLine()
    {
        // Arrange
        const string input = """
                             enum E
                             {
                                 First, /** doc */
                                 Second
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    First,

                                    /** doc */
                                    Second
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a delimited documentation comment behind a block comment that trails a field
    /// starts a new line below a blank line, the block comment stays behind the field, and the documented field stays
    /// behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-BLANK-012")]
    public void MovesDelimitedDocumentationBehindTrailingBlockCommentAndKeepsCodeBehindIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /* c */ /** doc */ int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A; /* c */

                                    /** doc */ int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a field, with the documented field behind it on its line,
    /// starts a new line below a blank line and keeps the field behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-BLANK-012")]
    public void MovesDelimitedDocumentationBehindFieldAndKeepsCodeBehindIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** doc */ int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** doc */ int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written directly behind the semicolon of a field starts a new
    /// line below a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void MovesDelimitedDocumentationWrittenDirectlyBehindField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A;/** doc */ int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** doc */ int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment separated from the field before it by a tab starts a new line
    /// below a blank line, with no whitespace left behind the field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void MovesDelimitedDocumentationSeparatedFromFieldByTab()
    {
        // Arrange
        const string input = "class C\n{\n    int A;\t/** doc */ int B;\n}";
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** doc */ int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a multi-line block comment that trails a field starts a
    /// new line below a blank line, and the block comment keeps its text
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void MovesDelimitedDocumentationBehindMultiLineTrailingBlockComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /* a
                                    b */ /** doc */ int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A; /* a
                                       b */

                                    /** doc */ int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment whose text spans several lines starts a new line below a blank
    /// line and keeps its text, including the whitespace at the start of its continuation lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-DOC-012")]
    public void MovesMultiLineDelimitedDocumentationWithoutRewritingItsText()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** <summary>
                                        Gets B.
                                        </summary> */
                                 int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** <summary>
                                           Gets B.
                                           </summary> */
                                    int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that of two delimited documentation comments behind a field, with the documented field behind them,
    /// only the first starts a new line, and the second stays behind it
    /// </summary>
    [TestMethod]
    public void MovesFirstOfTwoDelimitedDocumentationCommentsAndKeepsSecondBehindIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** a */ /** b */ int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** a */ /** b */ int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that of two delimited documentation comments that end the line of a field, only the first starts a new
    /// line, and the second stays behind it
    /// </summary>
    [TestMethod]
    public void MovesFirstOfTwoDelimitedDocumentationCommentsEndingFieldLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** a */ /** b */
                                 int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** a */ /** b */
                                    int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment behind a moved delimited documentation comment stays behind it on its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void MovesDelimitedDocumentationAndKeepsBlockCommentBehindIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** d */ /* c */ int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** d */ /* c */ int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a field, followed by a line comment on the next line,
    /// starts a new line below a blank line, and the line comment stays below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void MovesDelimitedDocumentationAboveLineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** d */
                                 // c
                                 int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** d */
                                    // c
                                    int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment followed by a single-line documentation comment on the line of a
    /// field starts a new line below a blank line, and the single-line comment starts a line of its own below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    [Requirement("FMT-DOC-002")]
    public void MovesDelimitedDocumentationAndSplitsFollowingSingleLineDocumentation()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** a */ /// b
                                 int B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** a */
                                    /// b
                                    int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment followed by a region directive starts a new line below a blank
    /// line and the region keeps its directives
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void MovesDelimitedDocumentationFollowedByRegionDirective()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** d */
                             #region Values
                                 int B;
                             #endregion // Values
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** d */

                                    #region Values

                                    int B;

                                    #endregion // Values
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment followed by an active conditional section starts a new line
    /// below a blank line and the directives stay where they are
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-001")]
    public void MovesDelimitedDocumentationFollowedByActiveConditionalSection()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** d */
                             #if !DEBUG
                                 int B;
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /** d */
                                #if !DEBUG
                                    int B;
                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment whose following member is disabled text, so that the closing
    /// brace follows it, stays where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void KeepsDelimitedDocumentationFollowedByInactiveConditionalSection()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /** d */
                             #if DEBUG
                                 int B;
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a delimited documentation comment behind a statement and before the closing
    /// brace stays where it was written and gets no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    [Requirement("FMT-BLANK-012")]
    public void KeepsStrandedDelimitedDocumentationBeforeClosingBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1; /** stray */
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a documentation comment the author put on its own line below a statement
    /// gets one blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void SeparatesStrandedDocumentationOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     /// stray
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        /// stray
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that ends the line of the opening brace of a type stays where it
    /// is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void KeepsDelimitedDocumentationEndingOpeningBraceLine()
    {
        // Arrange
        const string input = """
                             class C
                             { /** doc */
                                 void M()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the opening brace of a type written on the type's line,
    /// with a member behind it, goes onto the next line together with that member
    /// </summary>
    [TestMethod]
    public void CarriesDelimitedDocumentationWithCodeBehindOpeningBraceOfTypeOnSameLine()
    {
        // Arrange
        const string input = """
                             class C { /** d */ void M() { } }
                             """;
        const string expected = """
                                class C
                                {
                                    /** d */ void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the opening brace of a type on its own line, with a
    /// member behind it, goes onto the next line together with that member
    /// </summary>
    [TestMethod]
    public void CarriesDelimitedDocumentationWithCodeBehindOpeningBraceOfType()
    {
        // Arrange
        const string input = """
                             class C
                             { /** d */ void M() { } }
                             """;
        const string expected = """
                                class C
                                {
                                    /** d */ void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the opening brace of a block, with a statement behind it,
    /// goes onto the next line together with that statement
    /// </summary>
    [TestMethod]
    public void CarriesDelimitedDocumentationWithCodeBehindOpeningBraceOfBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0) { /** d */ a++; }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (a > 0)
                                        {
                                            /** d */ a++;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment on a line of its own stays behind that
    /// comment
    /// </summary>
    [TestMethod]
    public void KeepsDelimitedDocumentationBehindBlockCommentOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A;

                                 /* c */ /** d */
                                 int B;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a banner comment at the start of the file stays behind it
    /// </summary>
    [TestMethod]
    public void KeepsDelimitedDocumentationBehindBannerCommentAtFileStart()
    {
        // Arrange
        const string input = """
                             /* banner */ /** doc */
                             class C
                             {
                                 int A;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a case label, with a statement behind it on the label's
    /// line, stays where it was written
    /// </summary>
    [TestMethod]
    public void KeepsDelimitedDocumentationWithStatementBehindCaseLabel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int v)
                                 {
                                     switch (v)
                                     {
                                         case 1: /** d */ M(2);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement, with the next statement behind it, starts a
    /// new line below a blank line, as it does today; no requirement decides this position yet
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBehindStatementBeforeNextStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a = 1; /** d */ a = 2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a = 1;

                                        /** d */ a = 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment inside an argument list, before an argument, starts a new line
    /// below a blank line, as it does today; no requirement decides this position yet
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationInsideArgumentList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N(int a, int b)
                                 {
                                     N(1, /** d */ 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N(int a, int b)
                                    {
                                        N(1,

                                          /** d */ 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the closing brace of an if statement, before its else
    /// clause, starts a new line below a blank line, as it does today; no requirement decides this position yet
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBeforeElseClause()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                         M(a);
                                     } /** d */ else
                                     {
                                         M(a);
                                     }
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
                                            M(a);
                                        }

                                        /** d */ else
                                        {
                                            M(a);
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment before the dot of a member access stays behind the code before
    /// it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-DOC-003")]
    public void KeepsStrandedDelimitedDocumentationBeforeMemberAccessDot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 string M(string s)
                                 {
                                     return s.Trim() /** d */ .ToString();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    string M(string s)
                                    {
                                        return s.Trim() /** d */.ToString();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}