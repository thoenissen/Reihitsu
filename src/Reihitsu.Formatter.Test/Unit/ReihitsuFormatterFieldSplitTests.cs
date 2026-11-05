using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> writes back every field the field split makes
/// of a combined field declaration given as its target, with the declaring type as context, as the RH7101 code fix passes them
/// </summary>
[TestClass]
public class ReihitsuFormatterFieldSplitTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: the comment in front of the later declarator gets the blank line above it that the blank-line
    /// phase writes, and the other combined field of the class and the combined field of a nested type stay as they are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task SplitTargetIsWrittenBackWithTheLayoutOfThePhasesAndNothingElseOfTheContext()
    {
        const string input = """
                             class C
                             {
                                 private int _x, // horizontal
                                             /* vertical */ _y;
                                 private int _a, _b;

                                 class D
                                 {
                                     private int _c, _d;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x; // horizontal

                                    /* vertical */
                                    private int _y;
                                    private int _a, _b;

                                    class D
                                    {
                                        private int _c, _d;
                                    }
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example: a target off its member column keeps the column of its first token, and the later field stands
    /// where the class's depth places a member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task LaterFieldOfATargetOffItsMemberColumnStandsAtTheMemberColumn()
    {
        const string input = """
                             class C
                             {
                                   private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                      private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example: a target behind another member on its line has its first field start the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-018")]
    [Requirement("FMT-PIPE-010")]
    public async Task FirstFieldOfATargetBehindAMemberOnItsLineStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p;
                                    private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example: a member behind the target on its line starts the next line, and a comment in each gap stays
    /// behind the token in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-010")]
    public async Task CommentsInTheGapsStayBehindTheTokenInFrontOfThem()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* c */ private int _x, _y; /* d */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* c */
                                    private int _x;
                                    private int _y; /* d */
                                    private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that a member behind the target on its line, with only whitespace in the gap, starts the next line with one line
    /// break and no blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-010")]
    public async Task MemberBehindTheTargetOnItsLineStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _x, _y; private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;
                                    private int _y;
                                    private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example: a documentation comment in each gap goes with the member behind it onto the next line, and no
    /// blank line is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-018")]
    public async Task DocumentationCommentsInTheGapsGoWithTheMemberBehindThem()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /** X. */ private int _x, _y; /** Q. */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p;
                                    /** X. */ private int _x;
                                    private int _y;
                                    /** Q. */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example: a target on two lines with a member behind its last line has that member start the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MemberBehindTheLastLineOfATwoLineTargetStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _x,
                                             _y; private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;
                                    private int _y;
                                    private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example: a tab in front of the target stays in front of the first field, and the later field stands at
    /// the member column, indented with spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task TabInFrontOfTheTargetStaysAndTheLaterFieldIsIndentedWithSpaces()
    {
        const string input = "class C\n{\n\tprivate int _x, _y;\n}";
        const string expected = "class C\n{\n\tprivate int _x;\n    private int _y;\n}";

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the tab in front of a tab-indented context counts as one column, so the later field of a target in it stands
    /// one column plus the depth of a member from the line start
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task TabInFrontOfTheContextCountsAsOneColumn()
    {
        const string input = "namespace N\n{\n\tclass C\n\t{\n\t\tprivate int _x, _y;\n\t}\n}";
        const string expected = "namespace N\n{\n\tclass C\n\t{\n\t\tprivate int _x;\n     private int _y;\n\t}\n}";

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that a target behind the opening brace and in front of the closing brace of its type keeps its line, because
    /// neither brace is a member, and the later field stands at the member column. The input holds no line break, so the formatter
    /// writes <see cref="Environment.NewLine"/> (FMT-PIPE-007), and the test runs under that line ending only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task TargetBetweenTheBracesOfItsTypeKeepsItsLine()
    {
        const string input = "class C { private int _x, _y; }";
        const string expected = "class C { private int _x;\n    private int _y; }";

        await AssertSplitsTarget(input, expected, "_x", "C", [Environment.NewLine]);
    }

    /// <summary>
    /// Verifies that a method in front of the target on its line is not formatted, and the first field starts the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-010")]
    public async Task MethodInFrontOfTheTargetOnItsLineIsNotFormatted()
    {
        const string input = """
                             class C
                             {
                                 void M() { } private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M() { }
                                    private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the second of two combined declarations on one line has its first field start the next line, while the
    /// first declaration stays combined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task SecondOfTwoCombinedDeclarationsOnOneLineStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _a, _b; private int _c, _d;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _a, _b;
                                    private int _c;
                                    private int _d;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_c", "C");
    }

    /// <summary>
    /// Verifies that the first of two combined declarations on one line has the other declaration start the next line, while
    /// that declaration stays combined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task FirstOfTwoCombinedDeclarationsOnOneLineStartsTheOtherOnTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _a, _b; private int _c, _d;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _a;
                                    private int _b;
                                    private int _c, _d;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_a", "C");
    }

    /// <summary>
    /// Verifies that the entry point writes back the split fields whatever the target's first token is: a modifier, the type, or
    /// the opening bracket of an attribute list that is or is not split
    /// </summary>
    /// <param name="declaration">The combined declaration</param>
    /// <param name="fields">The fields the split makes of it, one per line, without indentation</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [DataRow("int _x, _y;", "int _x;\nint _y;")]
    [DataRow("private int _x, _y;", "private int _x;\nprivate int _y;")]
    [DataRow("[A]\n    private int _x, _y;", "[A]\nprivate int _x;\n[A]\nprivate int _y;")]
    [DataRow("[A] private int _x, _y;", "[A]\nprivate int _x;\n[A]\nprivate int _y;")]
    [DataRow("[A, B] private int _x, _y;", "[A]\n[B]\nprivate int _x;\n[A]\n[B]\nprivate int _y;")]
    public async Task SplitFieldsAreWrittenBackWhateverTheFirstTokenOfTheTarget(string declaration, string fields)
    {
        var input = $"class C\n{{\n    {declaration}\n}}";
        var expected = $"class C\n{{\n{string.Join("\n", fields.Split('\n').Select(line => "    " + line))}\n}}";

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the entry point writes back the split fields of a target in every type kind the field split splits in
    /// </summary>
    /// <param name="keyword">The keyword that declares the context</param>
    /// <param name="modifiers">The modifiers of the target</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [DataRow("class", "private")]
    [DataRow("struct", "private")]
    [DataRow("record", "private")]
    [DataRow("record struct", "private")]
    [DataRow("interface", "private static")]
    public async Task SplitFieldsAreWrittenBackInEveryTypeKind(string keyword, string modifiers)
    {
        var input = $"{keyword} T\n{{\n    {modifiers} int _x, _y;\n}}";
        var expected = $"{keyword} T\n{{\n    {modifiers} int _x;\n    {modifiers} int _y;\n}}";

        await AssertSplitsTarget(input, expected, "_x", "T");
    }

    /// <summary>
    /// Verifies that the blank lines and comments above the target stay as they stand in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task LeadingTriviaOfTheTargetStaysAsInTheDocument()
    {
        const string input = """
                             class C
                             {
                                 private int _p;


                               // about the fields
                                 private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p;


                                  // about the fields
                                    private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example with a comment that spans lines in each gap: document-level formatting leaves the first field
    /// behind the closing <c>*/</c> in front and the member behind the closing <c>*/</c> behind, so both keep their place and
    /// only the later field starts a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-010")]
    public async Task MembersBehindACommentThatSpansLinesInTheGapsKeepTheirPlace()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* a
                                    b */ private int _x, _y; /* c
                                    d */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* a
                                       b */ private int _x;
                                    private int _y; /* c
                                       d */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the first field keeps its place behind the closing <c>*/</c> of a comment that spans lines in the gap in
    /// front of the target, and that the member in front keeps its trailing trivia
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-010")]
    public async Task FirstFieldBehindACommentThatSpansLinesInTheGapInFrontKeepsItsPlace()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* a
                                    b */ private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* a
                                       b */ private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that a member behind the closing <c>*/</c> of a comment that spans lines behind the target keeps its place,
    /// because document-level formatting does not start it on a new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-010")]
    public async Task MemberBehindACommentThatSpansLinesInTheGapBehindKeepsItsPlace()
    {
        const string input = """
                             class C
                             {
                                 private int _x, _y; /* a
                                    b */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;
                                    private int _y; /* a
                                       b */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example with a member in front of the target on its first line and a comment that spans lines behind
    /// the separating comma: the first field starts the next line at the member column, the comment ends its line, and the later
    /// field starts a line of its own at the member column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task LaterFieldBehindACommentThatSpansLinesBehindTheSeparatorStartsItsOwnLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; private int _x, /* a
                                    b */ _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p;
                                    private int _x; /* a
                                       b */
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the later field behind a comment that spans lines behind the separating comma starts its own line at the
    /// member column when no other member stands on the target's lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task LaterFieldBehindACommentThatSpansLinesStandsAtTheMemberColumn()
    {
        const string input = """
                             class C
                             {
                                 private int _x, /* a
                                    b */ _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x; /* a
                                       b */
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example with a documentation comment that spans lines in each gap: the documentation comment starts
    /// the next line with its member, in front of the first field and in front of the member behind the target, and no blank
    /// line is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MembersBehindADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /** a
                                    b */ private int _x, _y; /** c
                                    d */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p;
                                    /** a
                                       b */ private int _x;
                                    private int _y;
                                    /** c
                                       d */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the first field starts the next line with the documentation comment in front of it when the gap in
    /// front of the target holds a documentation comment that spans lines, and that no blank line is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task FirstFieldBehindADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /** a
                                    b */ private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p;
                                    /** a
                                       b */ private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the member behind the target starts the next line with its documentation comment when the gap behind
    /// the target holds a documentation comment that spans lines, and that no blank line is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MemberBehindADocumentationCommentThatSpansLinesInTheGapBehindStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _x, _y; /** c
                                    d */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;
                                    private int _y;
                                    /** c
                                       d */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example with a comment that spans lines and then a one-line documentation comment in each gap: the
    /// documentation comment starts the next line with its member, in front of the first field and in front of the member
    /// behind the target, a comment in front of it stays behind the code in front of it, and no blank line is written above
    /// it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MembersBehindACommentThatSpansLinesAndADocumentationCommentInTheGapsStartTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* a
                                    b */ /** c */ private int _x, _y; /* d
                                    e */ /** f */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* a
                                       b */
                                    /** c */ private int _x;
                                    private int _y; /* d
                                       e */
                                    /** f */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the first field starts the next line with the documentation comment in front of it when the gap in
    /// front of the target holds a comment that spans lines and then a one-line documentation comment, and that no blank
    /// line is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task FirstFieldBehindACommentThatSpansLinesAndADocumentationCommentInTheGapInFrontStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* a
                                    b */ /** c */ private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* a
                                       b */
                                    /** c */ private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the member behind the target starts the next line with its documentation comment when the gap behind
    /// the target holds a comment that spans lines and then a one-line documentation comment, and that no blank line is
    /// written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MemberBehindACommentThatSpansLinesAndADocumentationCommentInTheGapBehindStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _x, _y; /* d
                                    e */ /** f */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;
                                    private int _y; /* d
                                       e */
                                    /** f */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example with a one-line comment and then a documentation comment that spans lines in each gap: the
    /// documentation comment starts the next line with its member, in front of the first field and in front of the member
    /// behind the target, a comment in front of it stays behind the code in front of it, and no blank line is written above
    /// it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MembersBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* a */ /** b
                                    c */ private int _x, _y; /* d */ /** e
                                    f */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* a */
                                    /** b
                                       c */ private int _x;
                                    private int _y; /* d */
                                    /** e
                                       f */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the first field starts the next line with the documentation comment in front of it when the gap in
    /// front of the target holds a one-line comment and then a documentation comment that spans lines, and that no blank
    /// line is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task FirstFieldBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* a */ /** b
                                    c */ private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* a */
                                    /** b
                                       c */ private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the member behind the target starts the next line with its documentation comment when the gap behind
    /// the target holds a one-line comment and then a documentation comment that spans lines, and that no blank line is
    /// written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MemberBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapBehindStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _x, _y; /* d */ /** e
                                    f */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;
                                    private int _y; /* d */
                                    /** e
                                       f */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example with a comment that spans lines and then a documentation comment that spans lines in each
    /// gap: the documentation comment starts the next line with its member, in front of the first field and in front of the
    /// member behind the target, a comment in front of it stays behind the code in front of it, and no blank line is written
    /// above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MembersBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* a
                                    b */ /** c
                                    d */ private int _x, _y; /* e
                                    f */ /** g
                                    h */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* a
                                       b */
                                    /** c
                                       d */ private int _x;
                                    private int _y; /* e
                                       f */
                                    /** g
                                       h */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the first field starts the next line with the documentation comment in front of it when the gap in
    /// front of the target holds a comment that spans lines and then a documentation comment that spans lines, and that no
    /// blank line is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task FirstFieldBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /* a
                                    b */ /** c
                                    d */ private int _x, _y;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /* a
                                       b */
                                    /** c
                                       d */ private int _x;
                                    private int _y;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies that the member behind the target starts the next line with its documentation comment when the gap behind
    /// the target holds a comment that spans lines and then a documentation comment that spans lines, and that no blank line
    /// is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    public async Task MemberBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapBehindStartsTheNextLine()
    {
        const string input = """
                             class C
                             {
                                 private int _x, _y; /* e
                                    f */ /** g
                                    h */ private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;
                                    private int _y; /* e
                                       f */
                                    /** g
                                       h */ private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the spec example on the other side of the condition: the target's first token and the first token of the
    /// member behind it already start their lines, so the documentation comments that span lines behind the code in front
    /// stay where they stand, although document-level formatting moves each onto a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task DocumentationCommentsThatSpanLinesInFrontOfMembersThatStartTheirLinesStayWhereTheyStand()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /** a
                                    b */
                                 private int _x, _y; /** c
                                    d */
                                 private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /** a
                                       b */
                                    private int _x;
                                    private int _y; /** c
                                       d */
                                    private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Verifies the other side of the condition with a <c>///</c> comment behind the code in front of the target and behind
    /// the target: the first token behind each comment already starts its line, so the comments stay where they stand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-029")]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task SingleLineDocumentationCommentsInFrontOfMembersThatStartTheirLinesStayWhereTheyStand()
    {
        const string input = """
                             class C
                             {
                                 private int _p; /// a
                                 private int _x, _y; /// c
                                 private int _q;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _p; /// a
                                    private int _x;
                                    private int _y; /// c
                                    private int _q;
                                }
                                """;

        await AssertSplitsTarget(input, expected, "_x", "C");
    }

    /// <summary>
    /// Finds the field declaration that declares the variable with the given name
    /// </summary>
    /// <param name="root">The root to search</param>
    /// <param name="variableName">The declared variable name</param>
    /// <returns>The field declaration</returns>
    private static FieldDeclarationSyntax FindField(SyntaxNode root, string variableName)
    {
        return root.DescendantNodes()
                   .OfType<FieldDeclarationSyntax>()
                   .Single(field => field.Declaration.Variables.Any(variable => variable.Identifier.ValueText == variableName));
    }

    /// <summary>
    /// Finds the type declaration with the given name
    /// </summary>
    /// <param name="root">The root to search</param>
    /// <param name="typeName">The declared type name</param>
    /// <returns>The type declaration</returns>
    private static TypeDeclarationSyntax FindType(SyntaxNode root, string typeName)
    {
        return root.DescendantNodes()
                   .OfType<TypeDeclarationSyntax>()
                   .Single(type => type.Identifier.ValueText == typeName);
    }

    /// <summary>
    /// Formats the field that declares the named variable with the named type as context through
    /// <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> under LF and CRLF, asserts the document's text after one
    /// pass, and that a second pass with the field that now declares the variable as target leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="variableName">A variable the target declares</param>
    /// <param name="typeName">The name of the context type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private Task AssertSplitsTarget(string inputWithLf, string expectedWithLf, string variableName, string typeName)
    {
        return AssertSplitsTarget(inputWithLf, expectedWithLf, variableName, typeName, _lineEndings);
    }

    /// <summary>
    /// Formats the field that declares the named variable with the named type as context through
    /// <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> under the given line endings, asserts the document's text
    /// after one pass, and that a second pass with the field that now declares the variable as target leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="variableName">A variable the target declares</param>
    /// <param name="typeName">The name of the context type</param>
    /// <param name="lineEndings">The line endings to run under</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertSplitsTarget(string inputWithLf, string expectedWithLf, string variableName, string typeName, IEnumerable<string> lineEndings)
    {
        foreach (var endOfLine in lineEndings)
        {
            var expected = NormalizeLineEndings(expectedWithLf, endOfLine);
            var firstPass = await Format(NormalizeLineEndings(inputWithLf, endOfLine), variableName, typeName);

            Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {DescribeLineEnding(endOfLine)} line endings.");

            var secondPass = await Format(firstPass, variableName, typeName);

            Assert.AreEqual(expected, secondPass, $"The second pass must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Formats the field that declares the named variable with the named type as context
    /// </summary>
    /// <param name="text">Document text</param>
    /// <param name="variableName">A variable the target declares</param>
    /// <param name="typeName">The name of the context type</param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> Format(string text, string variableName, string typeName)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(text));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            if (root == null)
            {
                throw new InvalidOperationException("Expected a syntax root.");
            }

            var result = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, FindField(root, variableName), FindType(root, typeName), TestContext.CancellationToken);

            return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    #endregion // Methods
}