using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the block of an anonymous method or a block lambda in an interpolation hole of a multi-line raw
/// string literal whose anchor stands on a hole line that does not move with the literal — the continuation line of a
/// nested verbatim string, or the line on which a multi-line comment ends — measured after the literal moved
/// </summary>
[TestClass]
public class HoleAnchorOnUnmovedLineAlignmentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 12 on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, is aligned under <c>delegate</c> when the literal moves 13 columns to the right, in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnVerbatimContinuationLineOneColumnLeftOfShiftIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             continued{M(delegate { return 1; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                continued{M(delegate
                                            {
                                                return 1;
                                            })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 13 on the continuation line of a <c>$@</c> string nested in a hole, the column of the literal's 13-column shift, is aligned under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnVerbatimContinuationLineInShiftColumnIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             continuing{M(delegate { return 1; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                continuing{M(delegate
                                             {
                                                 return 1;
                                             })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 12 on the continuation line of a <c>$@</c> string nested in a hole of an already aligned literal is aligned under <c>delegate</c>, as when the literal moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnVerbatimContinuationLineInAlignedLiteralIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {$@"first
                             continued{M(delegate { return 1; })} end"}
                                              """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                continued{M(delegate
                                            {
                                                return 1;
                                            })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 12 behind the end of a multi-line comment in a hole, on a line that does not move, is aligned under <c>delegate</c> when the literal moves 13 columns to the right, in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineOneColumnLeftOfShiftIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             ends here */delegate { return 1; })}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                ends here */delegate
                                            {
                                                return 1;
                                            })}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 13 behind the end of a multi-line comment in a hole, the column of the literal's 13-column shift, is aligned under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineInShiftColumnIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             ends there */delegate { return 1; })}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                ends there */delegate
                                             {
                                                 return 1;
                                             })}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 12 behind the end of a multi-line comment in a hole of an already aligned literal is aligned under <c>delegate</c>, as when the literal moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineInAlignedLiteralIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             ends here */delegate { return 1; })}
                                              """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                ends here */delegate
                                            {
                                                return 1;
                                            })}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block lambda whose anchor stands in column 12 on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, is aligned under its <c>(</c> when the literal moves 13 columns to the right, in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void BlockLambdaOnVerbatimContinuationLineOneColumnLeftOfShiftIsAlignedAtItsParenthesis()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             continued{M(() => { return 1; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                continued{M(() =>
                                            {
                                                return 1;
                                            })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block lambda whose anchor stands in column 13 on the continuation line of a <c>$@</c> string nested in a hole, the column of the literal's 13-column shift, is aligned under its <c>(</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void BlockLambdaOnVerbatimContinuationLineInShiftColumnIsAlignedAtItsParenthesis()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             continuing{M(() => { return 1; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                continuing{M(() =>
                                             {
                                                 return 1;
                                             })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block lambda whose anchor stands in column 12 on the continuation line of a <c>$@</c> string nested in a hole of an already aligned literal is aligned under its <c>(</c>, as when the literal moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void BlockLambdaOnVerbatimContinuationLineInAlignedLiteralIsAlignedAtItsParenthesis()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {$@"first
                             continued{M(() => { return 1; })} end"}
                                              """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                continued{M(() =>
                                            {
                                                return 1;
                                            })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block lambda whose anchor stands in column 12 behind the end of a multi-line comment in a hole, on a line that does not move, is aligned under its <c>(</c> when the literal moves 13 columns to the right, in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void BlockLambdaBehindCommentEndLineOneColumnLeftOfShiftIsAlignedAtItsParenthesis()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             ends here */() => { return 1; })}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                ends here */() =>
                                            {
                                                return 1;
                                            })}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block lambda whose anchor stands in column 13 behind the end of a multi-line comment in a hole, the column of the literal's 13-column shift, is aligned under its <c>(</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void BlockLambdaBehindCommentEndLineInShiftColumnIsAlignedAtItsParenthesis()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             ends there */() => { return 1; })}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                ends there */() =>
                                             {
                                                 return 1;
                                             })}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block lambda whose anchor stands in column 12 behind the end of a multi-line comment in a hole of an already aligned literal is aligned under its <c>(</c>, as when the literal moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void BlockLambdaBehindCommentEndLineInAlignedLiteralIsAlignedAtItsParenthesis()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             ends here */() => { return 1; })}
                                              """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                ends here */() =>
                                            {
                                                return 1;
                                            })}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 4 on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, is aligned under <c>delegate</c> when the literal moves 13 columns to the right, in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnInterpolatedVerbatimContinuationLineLeftOfShiftIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {@$"first
                             s{M(delegate { return 1; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {@$"first
                                s{M(delegate
                                    {
                                        return 1;
                                    })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method behind the end of a multi-line comment in a hole loses the space behind the comment and is aligned under <c>delegate</c> in column 6, measured where the spacing leaves it, when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-SPACE-013")]
    public void AnonymousMethodBehindSpacedCommentEndLineLeftOfShiftIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             two */ delegate { return 1; })}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                two */delegate
                                      {
                                          return 1;
                                      })}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 22 on the continuation line of a <c>$@</c> string nested in a hole, right of the literal's 13-column shift, is aligned under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnVerbatimContinuationLineRightOfShiftIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             continued with more{M(delegate { return 1; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                continued with more{M(delegate
                                                      {
                                                          return 1;
                                                      })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method whose anchor stands in column 4 on the continuation line of a <c>$@</c> string nested in a hole is aligned under <c>delegate</c> when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnVerbatimContinuationLineOfLiteralMovedLeftIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {$@"first
                             s{M(delegate { return 1; })} end"}
                                                      """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                s{M(delegate
                                    {
                                        return 1;
                                    })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every line of a <c>///</c> comment inside the block of an anonymous method whose anchor stands in column 4 on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, takes the column of the statement it documents, when the literal moves 13 columns to the right and the block's lines stand below column 0 before the move
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-INDENT-008")]
    [Requirement("FMT-RAW-007")]
    public void DocumentationCommentInBlockOfAnonymousMethodLeftOfShiftKeepsEveryLineInTheStatementColumn()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {@$"first
                             s{M(delegate {
                                 /// a
                                     /// b
                               /// c
                             return 1; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {@$"first
                                s{M(delegate
                                    {
                                        /// a
                                        /// b
                                        /// c
                                        return 1;
                                    })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every line of a <c>///</c> comment inside the block of a block lambda whose anchor stands in column 4 on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, takes the column of the statement it documents, when the literal moves 13 columns to the right and the block's lines stand below column 0 before the move
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-INDENT-008")]
    [Requirement("FMT-RAW-007")]
    public void DocumentationCommentInBlockOfBlockLambdaLeftOfShiftKeepsEveryLineInTheStatementColumn()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {@$"first
                             s{M(() => {
                                 /// a
                                     /// b
                               /// c
                             return 1; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 V {@$"first
                                s{M(() =>
                                    {
                                        /// a
                                        /// b
                                        /// c
                                        return 1;
                                    })} end"}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}