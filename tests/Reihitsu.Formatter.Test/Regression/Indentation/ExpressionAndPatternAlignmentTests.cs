using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for the alignment of wrapped pattern expressions, combinators, collection expressions, and list patterns
/// </summary>
[TestClass]
public class ExpressionAndPatternAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: an <c>or</c> combinator of an <c>is</c> expression is indented to the column of the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-019")]
    public void OrCombinatorOfAnIsPatternAlignsToTheIsKeyword()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is int
                                         or long;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        return o is int
                                                 or long;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped <c>is</c> keyword is indented to the column of the tested expression
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-019")]
    public void WrappedIsKeywordAlignsToTheTestedExpression()
    {
        const string input = """
                             class C
                             {
                                 bool M(object value)
                                 {
                                     return value // tested
                                         is string;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object value)
                                    {
                                        return value // tested
                                               is string;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>or</c> combinator of a switch-expression arm pattern is indented to the column of the combined pattern's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-019")]
    public void CombinatorOutsideAnIsExpressionAlignsToThePatternStart()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     return x switch
                                            {
                                                1 // one
                                                  or 2 => 10,
                                                _ => 0
                                            };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        return x switch
                                               {
                                                   1 // one
                                                   or 2 => 10,
                                                   _ => 0
                                               };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the elements of a multi-line list pattern are indented one level deeper than the opening bracket and the closing bracket aligns with it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-025")]
    public void ListPatternElementsIndentFromTheOpeningBracket()
    {
        const string input = """
                             class C
                             {
                                 bool M(int[] a)
                                 {
                                     return a is [
                                     1,
                                             2
                                         ];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int[] a)
                                    {
                                        return a is [
                                                        1,
                                                        2
                                                    ];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the elements of a multi-line collection expression are indented one level deeper than the opening bracket and the closing bracket aligns with it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-025")]
    public void CollectionExpressionElementsIndentFromTheOpeningBracket()
    {
        const string input = """
                             class C
                             {
                                 int[] M()
                                 {
                                     int[] a = [
                                     1,
                                             2
                                         ];

                                     return a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] M()
                                    {
                                        int[] a = [
                                                      1,
                                                      2
                                                  ];

                                        return a;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}