using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Regression tests for the whitespace in front of a token that follows a stray documentation comment written behind
/// the previous token. The documentation comment ends its line, so that whitespace is the token's indentation and not
/// a same-line gap.
/// </summary>
[TestClass]
public class StrandedDocumentationTokenSpacingTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a statement terminator below a stray documentation comment behind a return expression keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsTerminatorIndentationBelowStrayDocumentationCommentBehindReturnExpression()
    {
        const string input = """
                             public class C
                             {
                                 public int M()
                                 {
                                     return 1 /// stray
                                     ;
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a closing bracket below a stray documentation comment behind an attribute argument list keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsClosingBracketIndentationBelowStrayDocumentationCommentBehindAttributeArguments()
    {
        const string input = """
                             public class C
                             {
                                 [System.Obsolete("x") /// stray
                                 ]
                                 public int P { get; set; }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the column the author wrote is kept even when it is deeper than the construct's indentation, the
    /// same way a closing brace in that position keeps it
    /// </summary>
    [TestMethod]
    public void KeepsAuthoredColumnOfOverIndentedTerminatorBelowStrayDocumentationComment()
    {
        const string input = """
                             public class C
                             {
                                 public int M()
                                 {
                                     return 1 /// stray
                                             ;
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a field terminator below a stray documentation comment keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsFieldTerminatorIndentationBelowStrayDocumentationComment()
    {
        const string input = """
                             public class C
                             {
                                 private int _value = 1 /// stray
                                 ;
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an argument separator below a stray documentation comment keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsCommaIndentationBelowStrayDocumentationComment()
    {
        const string input = """
                             public class C
                             {
                                 public void M()
                                 {
                                     System.Math.Max(1 /// stray
                                         , 2);
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a closing parenthesis below a stray documentation comment keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsClosingParenthesisIndentationBelowStrayDocumentationComment()
    {
        const string input = """
                             public class C
                             {
                                 public void M()
                                 {
                                     System.Math.Abs(1 /// stray
                                         );
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a member access dot below a stray documentation comment keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsMemberAccessDotIndentationBelowStrayDocumentationComment()
    {
        const string input = """
                             public class C
                             {
                                 public string M()
                                 {
                                     return this /// stray
                                         .ToString();
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a terminator below a stray documentation comment that follows a two-line block comment keeps its
    /// indentation
    /// </summary>
    [TestMethod]
    public void KeepsTerminatorIndentationBelowStrayDocumentationCommentBehindTwoLineBlockComment()
    {
        const string input = """
                             public class C
                             {
                                 public int M()
                                 {
                                     return 1 /* a
                                     b */ /// stray
                                     ;
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a terminator below a two-line stray documentation comment keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsTerminatorIndentationBelowTwoLineStrayDocumentationComment()
    {
        const string input = """
                             public class C
                             {
                                 public int M()
                                 {
                                     return 1 /// a
                                     /// b
                                     ;
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a tab-indented terminator below a stray documentation comment keeps its indentation, with the tabs
    /// expanded to spaces as everywhere else
    /// </summary>
    [TestMethod]
    public void KeepsTabIndentedTerminatorBelowStrayDocumentationCommentAsSpaces()
    {
        const string input = "public class C\n{\n    public int M()\n    {\n        return 1 /// stray\n\t\t;\n    }\n}";
        const string expected = """
                                public class C
                                {
                                    public int M()
                                    {
                                        return 1 /// stray
                                        ;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a terminator separated from a stray documentation comment by a blank line keeps its indentation once
    /// the blank line is removed
    /// </summary>
    [TestMethod]
    public void KeepsTerminatorIndentationWhenBlankLineFollowsStrayDocumentationComment()
    {
        const string input = """
                             public class C
                             {
                                 public int M()
                                 {
                                     return 1 /// stray

                                     ;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M()
                                    {
                                        return 1 /// stray
                                        ;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the gap between a block comment and a terminator on the line below a stray documentation comment is
    /// still removed, because it lies on one line
    /// </summary>
    [TestMethod]
    public void RemovesGapBetweenBlockCommentAndTerminatorBelowStrayDocumentationComment()
    {
        const string input = """
                             public class C
                             {
                                 public int M()
                                 {
                                     return 1 /// stray
                                     /* b */ ;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M()
                                    {
                                        return 1 /// stray
                                        /* b */;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the gap between a delimited documentation comment and a terminator on the same line is still removed
    /// </summary>
    [TestMethod]
    public void RemovesGapBetweenDelimitedDocumentationCommentAndTerminatorOnSameLine()
    {
        const string input = """
                             public class C
                             {
                                 public int M()
                                 {
                                     return 1 /** a */ ;
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public int M()
                                    {
                                        return 1 /** a */;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}