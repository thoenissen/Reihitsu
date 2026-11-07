using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for a comment on its own line in front of the closing bracket of a collection expression, which is
/// indented one level deeper than the bracket, as one in front of the closing brace of an initializer (FMT-INDENT-006,
/// FMT-PIPE-014)
/// </summary>
[TestClass]
public class CollectionExpressionClosingBracketCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Wraps the given statements in a method body
    /// </summary>
    /// <param name="statements">The statements, indented for the method body</param>
    /// <returns>The source text</returns>
    private static string InMethod(string statements)
    {
        return $"using System.Collections.Generic;\n\nclass C\n{{\n    int[] N(int[] v)\n    {{\n{statements}\n    }}\n}}";
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the specification example: the comment in front of the <c>]</c> of a collection expression moves to the
    /// elements' column, as the comment in front of the <c>}</c> of a collection initializer does
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    [Requirement("FMT-PIPE-014")]
    public void CommentBeforeClosingBracketIsIndentedLikeCommentBeforeClosingBrace()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     int[] a = [
                                                   1,

                                               // last
                                               ];
                                     var l = new List<int>
                                             {
                                                 1

                                             // last
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        int[] a = [
                                                      1,

                                                      // last
                                                  ];
                                        var l = new List<int>
                                                {
                                                    1

                                                    // last
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the comment in front of the <c>]</c> of a field initializer's collection expression moves to the
    /// elements' column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingBracketOfFieldInitializerIsIndented()
    {
        const string input = """
                             class C
                             {
                                 private int[] _a = [
                                                        1,

                                                    // last
                                                    ];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int[] _a = [
                                                           1,

                                                           // last
                                                       ];
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment in front of the <c>]</c> moves to the elements' column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void DelimitedCommentBeforeClosingBracketIsIndented()
    {
        var input = InMethod("        int[] a = [\n                      1,\n\n                  /* last */\n                  ];\n\n        return a;");
        var expected = InMethod("        int[] a = [\n                      1,\n\n                      /* last */\n                  ];\n\n        return a;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the comment in front of the <c>]</c> of a nested collection expression moves to the nested elements'
    /// column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingBracketOfNestedCollectionExpressionIsIndented()
    {
        var input = InMethod("        int[][] n = [\n                        [\n                            1,\n\n                        // inner\n                        ],\n                    ];\n\n        return v;");
        var expected = InMethod("        int[][] n = [\n                        [\n                            1,\n\n                            // inner\n                        ],\n                    ];\n\n        return v;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the comment in front of the <c>]</c> of an empty collection expression is indented one level deeper
    /// than the bracket
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingBracketOfEmptyCollectionExpressionIsIndented()
    {
        var input = InMethod("        int[] e = [\n\n                  // none\n                  ];\n\n        return e;");
        var expected = InMethod("        int[] e = [\n\n                      // none\n                  ];\n\n        return e;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the comment in front of the <c>]</c> of a collection expression passed as an argument moves to the
    /// elements' column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingBracketOfArgumentIsIndented()
    {
        var input = InMethod("        return N([\n                     1,\n\n                 // last\n                 ]);");
        var expected = InMethod("        return N([\n                     1,\n\n                     // last\n                 ]);");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment already in the elements' column in front of the <c>]</c> stays there
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentInElementsColumnBeforeClosingBracketStays()
    {
        var input = InMethod("        int[] a = [\n                      1,\n\n                      // last\n                  ];\n\n        return a;");

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment in front of the <c>]</c> of a list pattern, which FMT-INDENT-006 does not name, stays in the
    /// bracket's column
    /// </summary>
    [TestMethod]
    public void CommentBeforeClosingBracketOfListPatternStaysInBracketColumn()
    {
        var input = InMethod("        if (v is [\n                     1,\n\n                 // last\n                 ])\n        {\n        }\n\n        return v;");

        AssertRuleResult(input);
    }

    #endregion // Tests
}