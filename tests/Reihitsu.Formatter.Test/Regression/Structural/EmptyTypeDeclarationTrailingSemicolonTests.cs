using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests for an empty type declaration followed by a semicolon behind its closing brace: the braces and that
/// semicolon become one semicolon behind the header, unless a comment, directive, or disabled text between the closing brace
/// and the semicolon keeps the braces
/// </summary>
[TestClass]
public class EmptyTypeDeclarationTrailingSemicolonTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that an empty struct, interface, record class, record struct, and positional record followed by a semicolon
    /// each become one semicolon behind the header
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void EveryEmptyTypeKindFollowedByASemicolonBecomesOneSemicolon()
    {
        const string input = """
                             struct S
                             {
                             };

                             interface I
                             {
                             };

                             record class Q { };

                             record R(int Value)
                             {
                             };

                             record struct T(int Value)
                             {
                             };
                             """;
        const string expected = """
                                struct S;

                                interface I;

                                record class Q;

                                record R(int Value);

                                record struct T(int Value);
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace and a blank line between the closing brace and the semicolon go with the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void WhitespaceAndBlankLineBetweenClosingBraceAndSemicolonAreRemoved()
    {
        const string input = """
                             class A
                             {
                             } ;

                             class B
                             {
                             }

                             ;
                             """;
        const string expected = """
                                class A;

                                class B;
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment behind the semicolon that follows the closing brace follows the new semicolon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void BlockCommentBehindTheSemicolonFollowsTheNewSemicolon()
    {
        const string input = """
                             class A
                             {
                             }; /* note */
                             """;
        const string expected = """
                                class A; /* note */
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty class followed by a semicolon and nested in another class becomes one semicolon behind its
    /// header, while the outer class keeps its braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void NestedEmptyClassFollowedByASemicolonBecomesOneSemicolon()
    {
        const string input = """
                             class H
                             {
                                 class I
                                 {
                                 };
                             }
                             """;
        const string expected = """
                                class H
                                {
                                    class I;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two one-line empty classes with a semicolon behind their braces on one line become one semicolon
    /// declaration each, every one on a line of its own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void TwoEmptyClassesFollowedBySemicolonsOnOneLineBecomeOneSemicolonEach()
    {
        const string input = """
                             class A { }; class B { };
                             """;
        const string expected = """
                                class A;
                                class B;
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that under C# 11, below the version of semicolon class declarations, an empty class followed by a semicolon
    /// keeps its braces and its semicolon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void EmptyClassFollowedByASemicolonKeepsItsBracesUnderCSharp11()
    {
        const string input = """
                             class A
                             {
                             };
                             """;

        AssertRuleResult(input, null, new CSharpParseOptions(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that under C# 11, at or above the version of semicolon record declarations, an empty record followed by a
    /// semicolon becomes one semicolon behind the header
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void EmptyRecordFollowedByASemicolonBecomesOneSemicolonUnderCSharp11()
    {
        const string input = """
                             record R
                             {
                             };
                             """;
        const string expected = """
                                record R;
                                """;

        AssertRuleResult(input, expected, new CSharpParseOptions(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that a line comment behind the closing brace, with the semicolon on the next line, keeps the braces, the
    /// comment, and the semicolon in their order, and that the output is stable
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    public void LineCommentBetweenClosingBraceAndSemicolonKeepsTheBraces()
    {
        const string input = """
                             class A
                             {
                             } // note
                             ;
                             """;

        AssertBracesAndTriviaAreKept(input, "classA{}//note;");
    }

    /// <summary>
    /// Verifies that a comment on a line of its own between the closing brace and the semicolon keeps the braces, the
    /// comment, and the semicolon in their order, and that the output is stable
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    public void CommentOnItsOwnLineBetweenClosingBraceAndSemicolonKeepsTheBraces()
    {
        const string input = """
                             class A
                             {
                             }
                             // note
                             ;
                             """;

        AssertBracesAndTriviaAreKept(input, "classA{}//note;");
    }

    /// <summary>
    /// Verifies that an empty <c>#if</c> block between the closing brace and the semicolon keeps the braces, the directives,
    /// and the semicolon in their order, and that the output is stable
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    public void DirectiveBetweenClosingBraceAndSemicolonKeepsTheBraces()
    {
        const string input = """
                             class A
                             {
                             }
                             #if X
                             #endif
                             ;
                             """;

        AssertBracesAndTriviaAreKept(input, "classA{}#ifX#endif;");
    }

    /// <summary>
    /// Verifies that code a conditional directive excludes between the closing brace and the semicolon keeps the braces, the
    /// disabled text, and the semicolon in their order, and that the output is stable
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    public void DisabledTextBetweenClosingBraceAndSemicolonKeepsTheBraces()
    {
        const string input = """
                             class A
                             {
                             }
                             #if X
                             class B;
                             #endif
                             ;
                             """;

        AssertBracesAndTriviaAreKept(input, "classA{}#ifXclassB;#endif;");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment between the closing brace and the semicolon keeps the braces, the
    /// comment, and the semicolon, whether the documentation mode parses it as documentation or as a comment
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    [DataRow(DocumentationMode.None)]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.Diagnose)]
    public void DocumentationCommentBetweenClosingBraceAndSemicolonKeepsTheBraces(DocumentationMode documentationMode)
    {
        const string input = """
                             class A
                             {
                             } /** note */;
                             """;

        AssertRuleResult(input, null, CSharpParseOptions.Default.WithDocumentationMode(documentationMode));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Formats the input under LF and CRLF line endings and verifies that its text without whitespace and line breaks is the
    /// given text, so the braces, the trivia between them and the semicolon, and the semicolon stay in their order, and that a
    /// second pass leaves the output unchanged. The columns of the kept semicolon are not decided by FMT-STRUCT-018
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expectedWithoutWhitespace">The expected output with every whitespace character removed</param>
    private static void AssertBracesAndTriviaAreKept(string input, string expectedWithoutWhitespace)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = FormatWithPipeline(input, endOfLine);
            var withoutWhitespace = string.Concat(output.Split([' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries));

            Assert.AreEqual(expectedWithoutWhitespace, withoutWhitespace, $"The braces, the trivia, and the semicolon must stay under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output}");
        }

        AssertSecondPassIsStable(input);
    }

    #endregion // Methods
}