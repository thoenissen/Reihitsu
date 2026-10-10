using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Regression tests for the blank line in front of a comment that follows a multi-line block comment behind the
/// preceding token. The line breaks inside the block comment are comment text: they neither count as the blank line
/// nor decide whether the following comment starts its own line
/// </summary>
[TestClass]
public class CommentBelowMultiLineTrailingBlockCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a line comment directly below a two-line block comment behind a field gets one blank line
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineAboveLineCommentBelowTwoLineTrailingBlockComment()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d; /* a
                                 b */
                                 // Describes
                                 public string Description { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d; /* a
                                    b */

                                    // Describes
                                    public string Description { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line block comment directly below a two-line block comment behind a field gets one blank line
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineAboveBlockCommentBelowTwoLineTrailingBlockComment()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d; /* a
                                 b */
                                 /* Describes */
                                 public string Description { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d; /* a
                                    b */

                                    /* Describes */
                                    public string Description { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the line break inside the block comment is not counted when the block comment follows another
    /// block comment behind the field
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBelowSeveralTrailingBlockCommentsWithOneInteriorLineBreak()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d; /* x */ /* a
                                 b */
                                 // Describes
                                 public string Description { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d; /* x */ /* a
                                    b */

                                    // Describes
                                    public string Description { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line separator inside the trailing block comment does not count as the blank line either
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineWhenTrailingBlockCommentBreaksWithLineSeparator()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d; /* a<LS>    b */
                                 // Describes
                                 public string Description { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d; /* a<LS>    b */

                                    // Describes
                                    public string Description { get; set; }
                                }
                                """;

        AssertRuleResult(input.Replace("<LS>", "\u2028"), expected.Replace("<LS>", "\u2028"));
    }

    /// <summary>
    /// Verifies that a line comment below a two-line block comment behind an expression statement gets one blank line
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBelowTwoLineTrailingBlockCommentBehindExpressionStatement()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void Method(int x)
                                 {
                                     x++; /* a
                                     b */
                                     // Describes
                                     x++;
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void Method(int x)
                                    {
                                        x++; /* a
                                        b */

                                        // Describes
                                        x++;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that exactly one blank line is inserted below a two-line block comment behind a local declaration, where
    /// the statement spacing also asks for a blank line
    /// </summary>
    [TestMethod]
    public void InsertsSingleBlankLineBelowTwoLineTrailingBlockCommentBehindLocalDeclaration()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void Method()
                                 {
                                     var x = 1; /* a
                                     b */
                                     // Describes
                                     x++;
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void Method()
                                    {
                                        var x = 1; /* a
                                        b */

                                        // Describes
                                        x++;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment below a two-line block comment behind a method's closing brace gets one blank line
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBelowTwoLineTrailingBlockCommentBehindClosingBrace()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void First()
                                 {
                                 } /* a
                                 b */
                                 // Describes
                                 public void Second()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void First()
                                    {
                                    } /* a
                                    b */

                                    // Describes
                                    public void Second()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment below a two-line block comment behind an enum member's comma gets one blank line
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBelowTwoLineTrailingBlockCommentBehindEnumMember()
    {
        const string input = """
                             public enum TestEnum
                             {
                                 First, /* a
                                 b */
                                 // Describes
                                 Second
                             }
                             """;
        const string expected = """
                                public enum TestEnum
                                {
                                    First, /* a
                                    b */

                                    // Describes
                                    Second
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every occurrence in one document is separated in a single pass
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBelowEveryTwoLineTrailingBlockCommentInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _a; /* a
                                 b */
                                 // Describes b
                                 private string _b; /* c
                                 d */
                                 // Describes c
                                 private string _c;
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _a; /* a
                                    b */

                                    // Describes b
                                    private string _b; /* c
                                    d */

                                    // Describes c
                                    private string _c;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that one blank line below a two-line trailing block comment is kept as it is
    /// </summary>
    [TestMethod]
    public void KeepsSingleBlankLineBelowTwoLineTrailingBlockComment()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d; /* a
                                 b */

                                 // Describes
                                 public string Description { get; set; }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that two blank lines below a two-line trailing block comment are collapsed to one
    /// </summary>
    [TestMethod]
    public void CollapsesTwoBlankLinesBelowTwoLineTrailingBlockComment()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d; /* a
                                 b */


                                 // Describes
                                 public string Description { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d; /* a
                                    b */

                                    // Describes
                                    public string Description { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment below a three-line trailing block comment gets one blank line, as it did before the
    /// line breaks inside comment text stopped counting
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBelowThreeLineTrailingBlockComment()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d; /* a
                                 b
                                 c */
                                 // Describes
                                 public string Description { get; set; }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private string _d; /* a
                                    b
                                    c */

                                    // Describes
                                    public string Description { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment directly after an opening brace stays without a blank line even when a two-line
    /// block comment trails the brace
    /// </summary>
    [TestMethod]
    public void KeepsCommentBelowTwoLineBlockCommentBehindOpeningBrace()
    {
        const string input = """
                             public class TestClass
                             { /* a
                                 b */
                                 // Describes
                                 public string Description { get; set; }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a line comment directly below a directive stays without a blank line even when a two-line block
    /// comment trails the preceding field
    /// </summary>
    [TestMethod]
    public void KeepsCommentDirectlyBelowDirectiveAfterTwoLineTrailingBlockComment()
    {
        const string input = """
                             public class TestClass
                             {
                                 private string _d; /* a
                                 b */
                             #if DEBUG
                             #endif
                                 // Describes
                                 public string Description { get; set; }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documenting delimited documentation comment behind a three-line block comment is moved onto its
    /// own line with one blank line above it in one pass
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBelowThreeLineBlockCommentEndingOnItsLineInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /* a
                                                    b
                                                    c */ /** doc */
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x; /* a
                                                       b
                                                       c */

                                    /** doc */
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documenting delimited documentation comment behind a four-line block comment is moved onto its
    /// own line with one blank line above it in one pass
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBelowFourLineBlockCommentEndingOnItsLineInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /* a
                                                    b
                                                    c
                                                    d */ /** doc */
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x; /* a
                                                       b
                                                       c
                                                       d */

                                    /** doc */
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documenting single-line documentation comment behind a two-line block comment is moved onto its
    /// own line with one blank line above it in one pass
    /// </summary>
    [TestMethod]
    public void MovesSingleLineDocumentationBelowTwoLineBlockCommentEndingOnItsLineInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /* a
                                                    b */ /// <summary>Doc.</summary>
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x; /* a
                                                       b */

                                    /// <summary>
                                    /// Doc.
                                    /// </summary>
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line documentation comment that documents nothing stays where the author wrote it behind a
    /// two-line block comment, like the same comment behind code
    /// </summary>
    [TestMethod]
    public void KeepsStrandedSingleLineDocumentationBehindTwoLineBlockComment()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public void Method()
                                 {
                                     var x = 1; /* a
                                     b */ /// stray
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that documents nothing stays where the author wrote it behind a
    /// two-line block comment, like the same comment behind code
    /// </summary>
    [TestMethod]
    public void KeepsStrandedDelimitedDocumentationBehindTwoLineBlockComment()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public void Method()
                                 {
                                     var x = 1; /* a
                                     b */ /** stray */
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documentation comment that documents nothing stays where the author wrote it behind a single-line
    /// block comment
    /// </summary>
    [TestMethod]
    public void KeepsStrandedDocumentationBehindSingleLineBlockComment()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public void Method()
                                 {
                                     var x = 1; /* a */ /// stray
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documentation comment that documents nothing still gets its blank line when the author put it on
    /// its own line below a two-line trailing block comment
    /// </summary>
    [TestMethod]
    public void SeparatesOwnLineStrandedDocumentationBelowTwoLineBlockComment()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public void Method()
                                 {
                                     var x = 1; /* a
                                     b */
                                     /// stray
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public void Method()
                                    {
                                        var x = 1; /* a
                                        b */

                                        /// stray
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}