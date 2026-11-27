using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the column of a comment on its own line in front of the closing brace of an interpolation: the
/// column of the brace for a regular, a verbatim, or a single-line raw interpolated string, and one level deeper than the brace
/// for a multi-line raw string literal
/// </summary>
[TestClass]
public class InterpolationClosingBraceCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: a comment in front of the closing brace of an interpolation of a regular, a verbatim, or a single-line raw interpolated string, also one nested in a hole of a multi-line raw string literal, takes the column of the brace, also when the hole's content stands in another column, while one in front of the closing brace of a multi-line raw string literal's own interpolation stays one level deeper; a blank line stands above each comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void SpecExampleCommentInFrontOfClosingBraceOfNonRawInterpolationTakesTheBraceColumn()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"{
                                         // e
                                         a
                                         // f
                                         }";
                                     var u = $"{a +
                                         b
                                         // f
                                         }";
                                     var v = $@"{
                                         a
                                     // f
                                         }";
                                     var s = $"""{
                                         a
                                         // f
                                         }""";
                                     var n = $"""
                                         {$"{
                                             a
                                             // f
                                             }"}
                                         """;
                                     var r = $$"""
                                               {{
                                                   a
                                               // f
                                               }}
                                               """;

                                     return t + u + v + s + n + r;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        var t = $"{
                                            // e
                                            a

                                            // f
                                            }";
                                        var u = $"{a
                                                   + b

                                            // f
                                            }";
                                        var v = $@"{
                                            a

                                            // f
                                            }";
                                        var s = $"""{
                                            a

                                            // f
                                            }""";
                                        var n = $"""
                                                 {$"{
                                                     a

                                                     // f
                                                     }"}
                                                 """;
                                        var r = $$"""
                                                  {{
                                                      a

                                                      // f
                                                  }}
                                                  """;

                                        return t + u + v + s + n + r;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment in front of the closing brace of an interpolation of a regular interpolated string takes the column of the brace, with a blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentInFrontOfClosingBraceOfRegularInterpolationTakesTheBraceColumn()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"{
                                         a
                                         // f
                                         }";

                                     return string.Empty;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        var t = $"{
                                            a

                                            // f
                                            }";

                                        return string.Empty;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written one level deeper than the closing brace of an interpolation of a regular interpolated string moves left to the column of the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentWrittenOneLevelDeeperThanClosingBraceOfRegularInterpolationMovesToTheBraceColumn()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"{
                                         a
                                                 // f
                                         }";

                                     return string.Empty;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        var t = $"{
                                            a

                                            // f
                                            }";

                                        return string.Empty;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment in front of the closing brace of an interpolation of a regular interpolated string takes the column of the brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void DelimitedCommentInFrontOfClosingBraceOfRegularInterpolationTakesTheBraceColumn()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"{
                                         a
                                         /* f */
                                         }";

                                     return string.Empty;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        var t = $"{
                                            a

                                            /* f */
                                            }";

                                        return string.Empty;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two comments in front of the closing brace of an interpolation take the column of that brace, and a comment in front of the closing brace of a second interpolation of the same string takes the column of the second brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void TwoCommentsInFrontOfClosingBracesOfTwoInterpolationsTakeTheirBracesColumns()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"{
                                         a
                                         // f
                                         // g
                                         }x{
                                         a
                                         // h
                                         }";

                                     return string.Empty;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        var t = $"{
                                            a

                                            // f
                                            // g
                                            }x{
                                                a

                                                // h
                                                }";

                                        return string.Empty;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the comments in front of the closing braces of a regular interpolated string nested in a hole of another one each take the column of the brace they precede
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentsInFrontOfClosingBracesOfNestedRegularInterpolationsTakeTheirBracesColumns()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"{
                                         $"{
                                             a
                                             // f
                                             }"
                                         // g
                                         }";

                                     return string.Empty;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        var t = $"{
                                            $"{
                                                a

                                                // f
                                                }"

                                            // g
                                            }";

                                        return string.Empty;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies both sides of the boundary in one string: a comment in front of the closing brace of a multi-line raw string literal's own interpolation, nested in a hole of a regular interpolated string, stays one level deeper than that brace, while a comment in front of the closing brace of the regular string's interpolation takes the column of that brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentInFrontOfRegularBraceTakesItsColumnWhileCommentInNestedRawHoleStaysOneLevelDeeper()
    {
        // Arrange
        const string input = """"
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"{
                                         $$"""
                                           {{
                                               a
                                               // f
                                           }}
                                           """
                                         // g
                                         }";

                                     return string.Empty;
                                 }
                             }
                             """";
        const string expected = """"
                                using System;

                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        var t = $"{
                                            $$"""
                                              {{
                                                  a

                                                  // f
                                              }}
                                              """

                                            // g
                                            }";

                                        return string.Empty;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of the closing brace of an interpolation of a regular interpolated string takes the column of the brace, not the column of the wrapped argument above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBehindWrappedArgumentInRegularInterpolationTakesTheBraceColumn()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"x{M(a,
                                         b)
                                         // f
                                         }";

                                     return string.Empty;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        var t = $"x{M(a,
                                                      b)

                                            // f
                                            }";

                                        return string.Empty;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of the closing brace of an interpolation of a regular interpolated string inside the block body of a lambda takes the column of the brace, measured after the lambda's block moved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentInFrontOfClosingBraceOfRegularInterpolationInBlockLambdaTakesTheBraceColumn()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     Func<string> l = () =>
                                     {
                                         return $"{
                                             a
                                             // f
                                             }";
                                     };

                                     return string.Empty;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    string M(int a, int b)
                                    {
                                        Func<string> l = () =>
                                                         {
                                                             return $"{
                                                                 a

                                                                 // f
                                                                 }";
                                                         };

                                        return string.Empty;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment that already stands in the column of the closing brace of an interpolation of a regular interpolated string, with the blank line above it, is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentAlreadyInTheBraceColumnOfRegularInterpolationIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 string M(int a, int b)
                                 {
                                     var t = $"{
                                         a

                                         // f
                                         }";

                                     return string.Empty;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Tests
}