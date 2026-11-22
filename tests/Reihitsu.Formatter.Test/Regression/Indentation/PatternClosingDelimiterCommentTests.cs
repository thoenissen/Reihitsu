using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for a comment on its own line in front of the closing delimiter of a list pattern, a positional pattern,
/// or a parenthesized pattern, which is indented into the column of the content the delimiter closes, while a comment in front
/// of the closing parenthesis of an argument list keeps the column of that parenthesis
/// </summary>
[TestClass]
public class PatternClosingDelimiterCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: a comment in front of the closing delimiter of a list pattern, a positional pattern,
    /// or a parenthesized pattern is indented into the column of the content it closes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void SpecExampleCommentBeforeClosingDelimiterOfAPatternIsIndentedIntoItsContent()
    {
        // Arrange
        const string input = """
                             class P
                             {
                                 public void Deconstruct(out int a, out int b)
                                 {
                                     a = 0;
                                     b = 0;
                                 }
                             }

                             class C
                             {
                                 bool M(int[] a, object o)
                                 {
                                     var b = a is [
                                                      1,

                                                  // last
                                                  ];
                                     var c = o is P(
                                                       1,
                                                       2

                                                   // last
                                                   );
                                     var d = o is
                                               (
                                                   1
                                                   or 2

                                               // last
                                               );

                                     return b && c && d;
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public void Deconstruct(out int a, out int b)
                                    {
                                        a = 0;
                                        b = 0;
                                    }
                                }

                                class C
                                {
                                    bool M(int[] a, object o)
                                    {
                                        var b = a is [
                                                         1,

                                                         // last
                                                     ];
                                        var c = o is P(
                                                          1,
                                                          2

                                                          // last
                                                      );
                                        var d = o is
                                                  (
                                                      1
                                                      or 2

                                                      // last
                                                  );

                                        return b && c && d;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change's example input as written by an author: the comment in front of the <c>]</c> of a list pattern and
    /// in front of the <c>)</c> of a parenthesized pattern goes into the content's column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingBracketOfListPatternAndClosingParenthesisOfParenthesizedPatternIsIndented()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int[] a, object o)
                                 {
                                     var b = a is [
                                                       1,

                                                   // last
                                                   ];
                                     var c = o is (1 or
                                         2

                                         // last
                                         );

                                     return b && c;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int[] a, object o)
                                    {
                                        var b = a is [
                                                         1,

                                                         // last
                                                     ];
                                        var c = o is
                                                  (
                                                      1
                                                      or 2

                                                      // last
                                                  );

                                        return b && c;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written directly behind the last element of a list pattern, without a blank line, and a
    /// delimited comment both go into the elements' column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void LineAndDelimitedCommentBeforeClosingBracketOfListPatternAreIndented()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int[] a)
                                 {
                                     var b = a is [
                                         1,
                                         // last
                                         ];
                                     var d = a is [
                                         1,
                                         /* last */
                                         ];

                                     return b && d;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int[] a)
                                    {
                                        var b = a is [
                                                         1,

                                                         // last
                                                     ];
                                        var d = a is [
                                                         1,

                                                         /* last */
                                                     ];

                                        return b && d;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of the <c>]</c> of a list pattern nested in a list pattern goes into the inner
    /// pattern's elements' column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingBracketOfNestedListPatternIsIndented()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int[][] a)
                                 {
                                     return a is [
                                         [
                                             1,

                                             // inner
                                             ],
                                         ];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int[][] a)
                                    {
                                        return a is [
                                                        [
                                                            1,

                                                            // inner
                                                        ],
                                                    ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of the <c>]</c> of a list pattern in a <c>case</c> label and in a switch-expression arm
    /// goes into the elements' column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingBracketOfListPatternInCaseLabelAndSwitchArmIsIndented()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int[] a)
                                 {
                                     switch (a)
                                     {
                                         case [
                                             1,

                                             // last
                                             ]:
                                             break;
                                     }

                                     return a switch
                                     {
                                         [
                                             1,

                                             // last
                                             ] => 1,
                                         _ => 0
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int[] a)
                                    {
                                        switch (a)
                                        {
                                            case [
                                                     1,

                                                     // last
                                                 ]:
                                                break;
                                        }

                                        return a switch
                                               {
                                                   [
                                                       1,

                                                       // last
                                                   ] => 1,
                                                   _ => 0
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the closing delimiter: a comment in front of a slice that ends a list pattern is a comment in
    /// front of an element and stays in the elements' column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeSliceThatEndsListPatternStaysInTheElementsColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int[] a)
                                 {
                                     return a is [
                                                     1,

                                                     // trailing
                                                     ..
                                                 ];
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the other side of the cause: a comment in front of the closing parenthesis of an argument list, whose content is
    /// not indented deeper than the parenthesis, keeps the column of that parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingParenthesisOfArgumentListKeepsTheParenthesisColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1

                                         // last
                                         );
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}