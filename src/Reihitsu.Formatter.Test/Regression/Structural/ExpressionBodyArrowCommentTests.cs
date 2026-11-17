using System;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests for comments in front of the arrow of an expression body the formatter converts: comments on lines
/// of their own lead the created statement inside the block, and the expression joins the created <c>return</c> keyword; and
/// for comments behind the arrow, which the formatter refuses as unsupported
/// </summary>
[TestClass]
public class ExpressionBodyArrowCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the specified example: a comment on a line of its own in front of the arrow leads the created statement, and a
    /// comment behind the token in front of the arrow does not stop the move
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentsInFrontOfTheArrowLeadTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Other()
                                 // before arrow
                                 => 1;

                                 void Run() // run
                                     // first
                                     => Foo();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Other()
                                    {
                                        // before arrow
                                        return 1;
                                    }

                                    void Run() // run
                                    {
                                        // first
                                        Foo();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the inputs whose comments FMT-STRUCT-006 placed between the arrow and the expression of a member the formatter
    /// converts are refused as unsupported: the input is returned unchanged, and the first comment behind the arrow is reported
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int Get() =>\n        // after arrow\n        42;", "// after arrow", DisplayName = "Comment line behind the arrow of a returning method")]
    [DataRow("    int Get() =>\n    // c\n    42;", "// c", DisplayName = "Comment line behind the arrow in the member's column")]
    [DataRow("    void Run() =>\n        // run\n        Foo();", "// run", DisplayName = "Comment line behind the arrow of a void method")]
    [DataRow("    int Fail() =>\n        // never\n        throw new System.InvalidOperationException();", "// never", DisplayName = "Comment line in front of a throw expression")]
    [DataRow("    int Value() =>\n        /* c */ 42;", "/* c */", DisplayName = "Block comment in front of the expression on its line")]
    [DataRow("    int Both()\n        // before\n        =>\n        // after\n        42;", "// after", DisplayName = "Comment lines in front of and behind the arrow")]
    [DataRow("    int Two() =>\n        // a\n\n        // b\n        42;", "// a", DisplayName = "Two comment lines with a blank line between them behind the arrow")]
    [DataRow("    void M()\n    {\n        int Local() =>\n            // note\n            1;\n    }", "// note", DisplayName = "Comment line behind the arrow of a local function")]
    [DataRow("    int A2()\n    // a\n    => // b\n    42;", "// b", DisplayName = "Line comment behind the arrow below a comment line in front of it")]
    [DataRow("    int Get3()\n    // own\n    => /* glued */ 3;", "/* glued */", DisplayName = "Block comment glued behind the arrow below a comment line in front of it")]
    [DataRow("    int M() =>\n        // a\n        42 /* t */;", "// a", DisplayName = "Comment line behind the arrow and a comment in front of the semicolon")]
    [DataRow("    int N() =>\n        // b\n        42; // t", "// b", DisplayName = "Comment line behind the arrow and a comment behind the semicolon")]
    [DataRow("    int M() /* c */ =>\n        // a\n        42;", "// a", DisplayName = "Comment line behind the arrow and a comment behind the token in front of it")]
    public void CommentsBehindTheArrowOfAConvertedMemberAreUnsupported(string members, string comment)
    {
        // Act & Assert
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, $"class C\n{{\n{members}\n}}", comment);
    }

    /// <summary>
    /// Verifies the reported shape: a comment on its own line before the arrow is carried into the block
    /// instead of standing between the signature and the opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowLeadsTheCreatedReturnStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Other()
                                 // before arrow
                                 => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Other()
                                    {
                                        // before arrow
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between two comments on lines of their own in front of the arrow is kept in front of the
    /// created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    [Requirement("FMT-PIPE-013")]
    public void BlankLineBetweenTwoCommentsBeforeArrowIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Two()
                                     // a

                                     // b
                                     => 42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Two()
                                    {
                                        // a

                                        // b
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment spanning several lines before the arrow leads the created
    /// statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void MultiLineDelimitedCommentBeforeArrowLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Multi()
                                     /* first
                                        second */
                                     => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Multi()
                                    {
                                        /* first
                                           second */
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the arrow of a local function leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfLocalFunctionLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int Local()
                                         // note
                                         => 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int Local()
                                        {
                                            // note
                                            return 1;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the arrow of a finalizer leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfFinalizerLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 ~C()
                                     // note
                                     => Release();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    ~C()
                                    {
                                        // note
                                        Release();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the arrow of an operator leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfOperatorLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static C operator +(C a, C b)
                                     // note
                                     => a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public static C operator +(C a, C b)
                                    {
                                        // note
                                        return a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the arrow of a conversion operator leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfConversionOperatorLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static implicit operator int(C c)
                                     // note
                                     => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public static implicit operator int(C c)
                                    {
                                        // note
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between a constraint clause and the arrow leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfMethodWithConstraintClauseLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 T Get<T>()
                                     where T : new()
                                     // note
                                     => new T();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    T Get<T>()
                                        where T : new()
                                    {
                                        // note
                                        return new T();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment before the arrow is not moved in front of the created statement
    /// when a comment stands in front of the arrow on the arrow's line, and both comments keep their order
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void OwnLineCommentStaysOutOfTheBlockWhenACommentStandsInFrontOfTheArrow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A4()
                                 // a
                                 /* c */ => 42;
                             }
                             """;

        // Act & Assert
        AssertCommentsKeepOrderOutsideTheBlock(input, "// a", "/* c */", "42;");
    }

    /// <summary>
    /// Verifies that a comment between the expression and the semicolon on the arrow's line does not stand at the
    /// arrow, so the own-line comment before the arrow still leads the created statement, and the comment stays in
    /// front of the statement's semicolon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBehindTheExpressionOnTheArrowLineDoesNotStopTheMove()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 // a
                                 => 42 /* t */;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        // a
                                        return 42 /* t */;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment behind the semicolon on the arrow's line does not stand at the arrow, so the own-line
    /// comment before the arrow still leads the created statement. Where the comment behind the semicolon goes is not
    /// specified, so only that it survives behind the statement is asserted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBehindTheSemicolonOnTheArrowLineDoesNotStopTheMove()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int N()
                                 // b
                                 => 42; // t
                             }
                             """;

        // Act & Assert
        AssertOwnLineCommentLeadsStatementAndTrailingCommentFollows(input, "// b", "return 42;", "// t");
    }

    /// <summary>
    /// Verifies that a comment behind the token in front of the arrow does not stop the move of an own-line comment
    /// before the arrow either
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBehindTheTokenInFrontOfTheArrowDoesNotStopTheMoveOfACommentBeforeTheArrow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() /* c */
                                 // a
                                 => 42;
                             }
                             """;

        // Act & Assert
        AssertOwnLineCommentLeadsStatementAndOtherCommentSurvives(input, "// a", "return 42;", "/* c */");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Formats the input under LF and CRLF and asserts what FMT-STRUCT-006 specifies for a member whose arrow
    /// line carries a comment: the member is converted, every comment survives once, the comments keep the
    /// order in which they were written, the own-line comment is not moved between the block's opening brace
    /// and the created statement, and a second pass changes nothing. The placement of the comments is left
    /// unspecified, so the exact layout is not asserted
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="ownLineComment">The comment written on a line of its own before the arrow</param>
    /// <param name="arrowLineComment">The comment written on the arrow's line</param>
    /// <param name="statementText">Text of the created statement that follows both comments</param>
    private static void AssertCommentsKeepOrderOutsideTheBlock(string input, string ownLineComment, string arrowLineComment, string statementText)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var formatted = Format(NormalizeLineEndings(input, endOfLine));
            var secondPass = Format(formatted);

            Assert.AreEqual(formatted, secondPass, $"Formatter is not idempotent under {endingName} line endings.");
            AssertUsesLineEnding(formatted, endOfLine);

            var ownLineIndex = formatted.IndexOf(ownLineComment, StringComparison.Ordinal);
            var arrowLineIndex = formatted.IndexOf(arrowLineComment, StringComparison.Ordinal);
            var statementIndex = formatted.IndexOf(statementText, StringComparison.Ordinal);

            Assert.DoesNotContain("=>", formatted, $"The member must be converted to a block body under {endingName} line endings.");
            Assert.IsGreaterThanOrEqualTo(0, ownLineIndex, $"The own-line comment must survive under {endingName} line endings.");
            Assert.AreEqual(ownLineIndex, formatted.LastIndexOf(ownLineComment, StringComparison.Ordinal), $"The own-line comment must appear once under {endingName} line endings.");
            Assert.AreEqual(arrowLineIndex, formatted.LastIndexOf(arrowLineComment, StringComparison.Ordinal), $"The arrow-line comment must appear once under {endingName} line endings.");
            Assert.IsLessThan(arrowLineIndex, ownLineIndex, $"The comments must keep their source order under {endingName} line endings.");
            Assert.IsLessThan(statementIndex, arrowLineIndex, $"The arrow-line comment must stay in front of the statement under {endingName} line endings.");

            var openBraceIndex = formatted.LastIndexOf('{', statementIndex);

            Assert.IsLessThanOrEqualTo(openBraceIndex,
                                       ownLineIndex,
                                       $"The own-line comment must not be moved between the opening brace and the created statement under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Formats the input under LF and CRLF and asserts that the own-line comment leads the created statement inside
    /// the block, on the line directly after the opening brace and directly above the statement, that the comment
    /// behind the semicolon survives once behind the statement, and that a second pass changes nothing
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="ownLineComment">The comment written on a line of its own next to the arrow</param>
    /// <param name="statementText">Text of the created statement</param>
    /// <param name="trailingComment">The comment written behind the semicolon</param>
    private static void AssertOwnLineCommentLeadsStatementAndTrailingCommentFollows(string input, string ownLineComment, string statementText, string trailingComment)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var formatted = Format(NormalizeLineEndings(input, endOfLine));

            Assert.AreEqual(formatted, Format(formatted), $"Formatter is not idempotent under {endingName} line endings.");
            AssertUsesLineEnding(formatted, endOfLine);
            Assert.MatchesRegex(new Regex($@"\{{\r?\n[ ]*{Regex.Escape(ownLineComment)}\r?\n[ ]*{Regex.Escape(statementText)}", RegexOptions.None, Regex.InfiniteMatchTimeout),
                                formatted,
                                $"The own-line comment must lead the created statement inside the block under {endingName} line endings.");

            var statementIndex = formatted.IndexOf(statementText, StringComparison.Ordinal);
            var trailingIndex = formatted.IndexOf(trailingComment, StringComparison.Ordinal);

            Assert.AreEqual(trailingIndex, formatted.LastIndexOf(trailingComment, StringComparison.Ordinal), $"The comment behind the semicolon must appear once under {endingName} line endings.");
            Assert.IsGreaterThan(statementIndex, trailingIndex, $"The comment behind the semicolon must stay behind the statement under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Formats the input under LF and CRLF and asserts that the own-line comment leads the created statement inside
    /// the block, on the line directly after the opening brace and directly above the statement, that the other
    /// comment survives exactly once, and that a second pass changes nothing
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="ownLineComment">The comment written on a line of its own next to the arrow</param>
    /// <param name="statementText">Text of the created statement</param>
    /// <param name="otherComment">A further comment of the member whose placement is not specified</param>
    private static void AssertOwnLineCommentLeadsStatementAndOtherCommentSurvives(string input, string ownLineComment, string statementText, string otherComment)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var formatted = Format(NormalizeLineEndings(input, endOfLine));

            Assert.AreEqual(formatted, Format(formatted), $"Formatter is not idempotent under {endingName} line endings.");
            AssertUsesLineEnding(formatted, endOfLine);
            Assert.MatchesRegex(new Regex($@"\{{\r?\n[ ]*{Regex.Escape(ownLineComment)}\r?\n[ ]*{Regex.Escape(statementText)}", RegexOptions.None, Regex.InfiniteMatchTimeout),
                                formatted,
                                $"The own-line comment must lead the created statement inside the block under {endingName} line endings.");

            var otherIndex = formatted.IndexOf(otherComment, StringComparison.Ordinal);

            Assert.IsGreaterThanOrEqualTo(0, otherIndex, $"The other comment must survive under {endingName} line endings.");
            Assert.AreEqual(otherIndex, formatted.LastIndexOf(otherComment, StringComparison.Ordinal), $"The other comment must appear once under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Formats the source text with the whole formatter
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: CancellationToken.None);

        return ReihitsuFormatter.FormatSyntaxTree(tree, CancellationToken.None).Output.GetRoot(CancellationToken.None).ToFullString();
    }

    #endregion // Methods
}