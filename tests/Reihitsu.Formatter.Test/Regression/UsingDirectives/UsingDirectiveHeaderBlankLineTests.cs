using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.UsingDirectives;

/// <summary>
/// Full-pipeline tests for the blank line that ends the header above the block's original first using directive when the
/// reorder moves that directive: a line that holds only whitespace ends the header like an empty line, and the comments
/// after it move with the directive. In the fixtures, <c>·</c> stands for a space and <c>→</c> for a tab
/// </summary>
[TestClass]
public class UsingDirectiveHeaderBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: a line of four spaces below the header ends it, the comment below that line
    /// moves with its directive and keeps a blank line above it, and the spaces are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    [Requirement("FMT-USING-010")]
    public void LineOfSpacesEndsTheHeader()
    {
        const string input = """
                             // Header
                             ····
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that a line that holds only a tab ends the header like an empty line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void LineOfTabEndsTheHeader()
    {
        const string input = """
                             // Header
                             →
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that several lines that hold only whitespace end the header, and that they become one blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void SeveralLinesOfSpacesEndTheHeader()
    {
        const string input = """
                             // Header
                             ····
                             ··
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that a line of spaces that is the last blank line among the comments ends the header although an empty line
    /// stands above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void LineOfSpacesBelowEmptyLineEndsTheHeader()
    {
        const string input = """
                             // Header

                             ····
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that an empty line below a line of spaces ends the header, the side of the boundary that already holds
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void EmptyLineBelowLineOfSpacesEndsTheHeader()
    {
        const string input = """
                             // Header
                             ····

                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that a line of spaces between two header comments, below an empty line, is the last blank line among the
    /// comments, so both header comments stay at the top
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void LineOfSpacesBetweenHeaderAndCommentKeepsEarlierHeaderCommentsAtTheTop()
    {
        const string input = """
                             // A

                             // B
                             ····
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // A

                                // B

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that a line of spaces between two header comments, with an empty line above the comment of the directive,
    /// keeps both header comments at the top
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void EmptyLineAboveCommentKeepsHeaderWithLineOfSpacesAtTheTop()
    {
        const string input = """
                             // A
                             ····
                             // B

                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // A

                                // B

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that a line of spaces directly above the original first directive, with no comment below it, keeps the whole
    /// header at the top
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void LineOfSpacesDirectlyAboveDirectiveKeepsTheHeaderAtTheTop()
    {
        const string input = """
                             // Header
                             ····
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that a line that holds only the indentation ends the header inside a namespace with a body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    [Requirement("FMT-USING-011")]
    public void LineOfSpacesEndsTheHeaderInsideNamespaceBody()
    {
        const string input = """
                             namespace N
                             {
                                 // Header
                             ········
                                 // About Linq
                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    // Header

                                    using System;

                                    // About Linq
                                    using System.Linq;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that a line of spaces below a delimited header comment ends the header
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void LineOfSpacesBelowDelimitedCommentEndsTheHeader()
    {
        const string input = """
                             /* Header */
                             ····
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                /* Header */

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected);
    }

    /// <summary>
    /// Verifies that the empty line below a single-line documentation comment, parsed as documentation, ends the header, and
    /// that the blank line below the documentation comment is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    [Requirement("FMT-BLANK-010")]
    public void EmptyLineBelowDocumentationCommentEndsTheHeader()
    {
        const string input = """
                             /// Doc

                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                /// Doc
                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(input, expected, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies that a line of spaces below a single-line documentation comment, parsed as documentation, ends the header
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void LineOfSpacesBelowDocumentationCommentEndsTheHeader()
    {
        const string input = """
                             /// Doc
                             ····
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                /// Doc
                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(ReplacePlaceholders(input), expected, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
    }

    /// <summary>
    /// Replaces the visible placeholders of a fixture with the whitespace they stand for: <c>·</c> with a space and
    /// <c>→</c> with a tab, so a line that holds only whitespace stays visible in the source of the test
    /// </summary>
    /// <param name="text">Fixture text with placeholders</param>
    /// <returns>The fixture text with whitespace</returns>
    private static string ReplacePlaceholders(string text)
    {
        return text.Replace('·', ' ')
                   .Replace('→', '\t');
    }

    #endregion // Methods
}