using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the block of an anonymous method or a block lambda in an interpolation hole of a multi-line raw
/// string literal whose anchor stands on a hole line that does not move with the literal, in the column that block
/// indentation gives the block's opening brace — the column of the statement around the literal
/// </summary>
[TestClass]
public class HoleAnchorInStatementColumnAlignmentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaaa */delegate { return 1; })}
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
                                aaaaa */delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */delegate { return 1; })}
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
                                aaaaa */delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */delegate { return 1; })}
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
                                aaaaa */delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {q/** one
                             aa */.N(delegate { return 1; })}
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
                                                 K {q/** one
                                aa */.N(delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindDocumentationCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {q/** one
                             aa */.N(delegate { return 1; })}
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
                                                 K {q/** one
                                aa */.N(delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {q/** one
                             aa */.N(delegate { return 1; })}
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
                                                 K {q/** one
                                aa */.N(delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             aaaaa{M(delegate { return 1; })} end"}
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
                                aaaaa{M(delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {$@"first
                             aaaaa{M(delegate { return 1; })} end"}
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
                                aaaaa{M(delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {$@"first
                             aaaaa{M(delegate { return 1; })} end"}
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
                                aaaaa{M(delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {@$"first
                             aaaaa{M(delegate { return 1; })} end"}
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
                                aaaaa{M(delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnInterpolatedVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {@$"first
                             aaaaa{M(delegate { return 1; })} end"}
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
                                aaaaa{M(delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {@$"first
                             aaaaa{M(delegate { return 1; })} end"}
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
                                aaaaa{M(delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodBehindCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaaa */async delegate { return 1; })}
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
                                aaaaa */async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodBehindCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */async delegate { return 1; })}
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
                                aaaaa */async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */async delegate { return 1; })}
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
                                aaaaa */async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {q/** one
                             aa */.N(async delegate { return 1; })}
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
                                                 K {q/** one
                                aa */.N(async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodBehindDocumentationCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {q/** one
                             aa */.N(async delegate { return 1; })}
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
                                                 K {q/** one
                                aa */.N(async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {q/** one
                             aa */.N(async delegate { return 1; })}
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
                                                 K {q/** one
                                aa */.N(async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodOnVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             aaaaa{M(async delegate { return 1; })} end"}
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
                                aaaaa{M(async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodOnVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {$@"first
                             aaaaa{M(async delegate { return 1; })} end"}
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
                                aaaaa{M(async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodOnVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {$@"first
                             aaaaa{M(async delegate { return 1; })} end"}
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
                                aaaaa{M(async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {@$"first
                             aaaaa{M(async delegate { return 1; })} end"}
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
                                aaaaa{M(async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodOnInterpolatedVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {@$"first
                             aaaaa{M(async delegate { return 1; })} end"}
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
                                aaaaa{M(async delegate
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
    /// Verifies that the block of an <c>async</c> anonymous method whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncAnonymousMethodOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {@$"first
                             aaaaa{M(async delegate { return 1; })} end"}
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
                                aaaaa{M(async delegate
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaaa */() => { return 1; })}
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
                                aaaaa */() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */() => { return 1; })}
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
                                aaaaa */() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */() => { return 1; })}
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
                                aaaaa */() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {q/** one
                             aa */.N(() => { return 1; })}
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
                                                 K {q/** one
                                aa */.N(() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindDocumentationCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {q/** one
                             aa */.N(() => { return 1; })}
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
                                                 K {q/** one
                                aa */.N(() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {q/** one
                             aa */.N(() => { return 1; })}
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
                                                 K {q/** one
                                aa */.N(() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaOnVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             aaaaa{M(() => { return 1; })} end"}
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
                                aaaaa{M(() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaOnVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {$@"first
                             aaaaa{M(() => { return 1; })} end"}
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
                                aaaaa{M(() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaOnVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {$@"first
                             aaaaa{M(() => { return 1; })} end"}
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
                                aaaaa{M(() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {@$"first
                             aaaaa{M(() => { return 1; })} end"}
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
                                aaaaa{M(() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {@$"first
                             aaaaa{M(() => { return 1; })} end"}
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
                                aaaaa{M(() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its <c>(</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {@$"first
                             aaaaa{M(() => { return 1; })} end"}
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
                                aaaaa{M(() =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaBehindCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaaa */async () => { return 1; })}
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
                                aaaaa */async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaBehindCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */async () => { return 1; })}
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
                                aaaaa */async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */async () => { return 1; })}
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
                                aaaaa */async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {q/** one
                             aa */.N(async () => { return 1; })}
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
                                                 K {q/** one
                                aa */.N(async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaBehindDocumentationCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {q/** one
                             aa */.N(async () => { return 1; })}
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
                                                 K {q/** one
                                aa */.N(async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {q/** one
                             aa */.N(async () => { return 1; })}
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
                                                 K {q/** one
                                aa */.N(async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaOnVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             aaaaa{M(async () => { return 1; })} end"}
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
                                aaaaa{M(async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaOnVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {$@"first
                             aaaaa{M(async () => { return 1; })} end"}
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
                                aaaaa{M(async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaOnVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {$@"first
                             aaaaa{M(async () => { return 1; })} end"}
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
                                aaaaa{M(async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 V {@$"first
                             aaaaa{M(async () => { return 1; })} end"}
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
                                aaaaa{M(async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              V {@$"first
                             aaaaa{M(async () => { return 1; })} end"}
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
                                aaaaa{M(async () =>
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
    /// Verifies that the block of an <c>async</c> block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>async</c> with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AsyncLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      V {@$"first
                             aaaaa{M(async () => { return 1; })} end"}
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
                                aaaaa{M(async () =>
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
    /// Verifies that the block of a simple block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaBehindCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaaa */x => { return x; })}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                aaaaa */x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaBehindCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */x => { return x; })}
                                              """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                aaaaa */x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */x => { return x; })}
                                                      """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 K {M(/* one
                                aaaaa */x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                 K {q/** one
                             aa */.N(x => { return x; })}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 K {q/** one
                                aa */.N(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaBehindDocumentationCommentEndLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                              K {q/** one
                             aa */.N(x => { return x; })}
                                              """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 K {q/** one
                                aa */.N(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands behind the end of a multi-line <c>/** */</c> comment that a member access follows in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaBehindDocumentationCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                                      K {q/** one
                             aa */.N(x => { return x; })}
                                                      """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 K {q/** one
                                aa */.N(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaOnVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                 V {$@"first
                             aaaaa{M(x => { return x; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                aaaaa{M(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaOnVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                              V {$@"first
                             aaaaa{M(x => { return x; })} end"}
                                              """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                aaaaa{M(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands on the continuation line of a <c>$@</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaOnVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                                      V {$@"first
                             aaaaa{M(x => { return x; })} end"}
                                                      """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 V {$@"first
                                aaaaa{M(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                 V {@$"first
                             aaaaa{M(x => { return x; })} end"}
                                 """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 V {@$"first
                                aaaaa{M(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                              V {@$"first
                             aaaaa{M(x => { return x; })} end"}
                                              """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 V {@$"first
                                aaaaa{M(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of a simple block lambda whose anchor stands on the continuation line of an <c>@$</c> string nested in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under its parameter with its body one level deeper when the literal moves 8 columns to the left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void SimpleLambdaOnInterpolatedVerbatimContinuationLineInStatementColumnInLiteralMovedLeftIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int, int> f)
                                 {
                                     var a = $"""
                                                      V {@$"first
                             aaaaa{M(x => { return x; })} end"}
                                                      """;

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int, int> f)
                                    {
                                        var a = $"""
                                                 V {@$"first
                                aaaaa{M(x =>
                                        {
                                            return x;
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 7, next to the column block indentation gives its opening brace, starts under <c>delegate</c> when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineOneColumnLeftOfStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaa */delegate { return 1; })}
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
                                aaaa */delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 7, next to the column block indentation gives its opening brace, starts under <c>delegate</c> in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineOneColumnLeftOfStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaa */delegate { return 1; })}
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
                                aaaa */delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 9, next to the column block indentation gives its opening brace, starts under <c>delegate</c> when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineOneColumnRightOfStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaaaa */delegate { return 1; })}
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
                                aaaaaa */delegate
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 9, next to the column block indentation gives its opening brace, starts under <c>delegate</c> in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineOneColumnRightOfStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaaa */delegate { return 1; })}
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
                                aaaaaa */delegate
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 7, next to the column block indentation gives its opening brace, starts under its <c>(</c> when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineOneColumnLeftOfStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaa */() => { return 1; })}
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
                                aaaa */() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 7, next to the column block indentation gives its opening brace, starts under its <c>(</c> in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineOneColumnLeftOfStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaa */() => { return 1; })}
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
                                aaaa */() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 9, next to the column block indentation gives its opening brace, starts under its <c>(</c> when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineOneColumnRightOfStatementColumnInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 K {M(/* one
                             aaaaaa */() => { return 1; })}
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
                                aaaaaa */() =>
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 9, next to the column block indentation gives its opening brace, starts under its <c>(</c> in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineOneColumnRightOfStatementColumnInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaaa */() => { return 1; })}
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
                                aaaaaa */() =>
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 12, the column block indentation gives its opening brace when the statement stands one block deeper, starts under <c>delegate</c> when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineInColumnOfStatementOneBlockDeeperInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     if (f != null)
                                     {
                                         var a = $"""
                                     K {M(/* one
                             aaaaaaaaa */delegate { return 1; })}
                                     """;
                                     }

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        if (f != null)
                                        {
                                            var a = $"""
                                                     K {M(/* one
                                aaaaaaaaa */delegate
                                            {
                                                return 1;
                                            })}
                                                     """;
                                        }

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 12, the column block indentation gives its opening brace when the statement stands one block deeper, starts under <c>delegate</c> in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodBehindCommentEndLineInColumnOfStatementOneBlockDeeperInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     if (f != null)
                                     {
                                         var a = $"""
                                                  K {M(/* one
                             aaaaaaaaa */delegate { return 1; })}
                                                  """;
                                     }

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        if (f != null)
                                        {
                                            var a = $"""
                                                     K {M(/* one
                                aaaaaaaaa */delegate
                                            {
                                                return 1;
                                            })}
                                                     """;
                                        }

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 12, the column block indentation gives its opening brace when the statement stands one block deeper, starts under its <c>(</c> when the literal moves 13 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineInColumnOfStatementOneBlockDeeperInLiteralMovedRightIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     if (f != null)
                                     {
                                         var a = $"""
                                     K {M(/* one
                             aaaaaaaaa */() => { return 1; })}
                                     """;
                                     }

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        if (f != null)
                                        {
                                            var a = $"""
                                                     K {M(/* one
                                aaaaaaaaa */() =>
                                            {
                                                return 1;
                                            })}
                                                     """;
                                        }

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 12, the column block indentation gives its opening brace when the statement stands one block deeper, starts under its <c>(</c> in an already aligned literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaBehindCommentEndLineInColumnOfStatementOneBlockDeeperInAlignedLiteralIsAlignedUnderItsAnchor()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     if (f != null)
                                     {
                                         var a = $"""
                                                  K {M(/* one
                             aaaaaaaaa */() => { return 1; })}
                                                  """;
                                     }

                                     return 0;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        if (f != null)
                                        {
                                            var a = $"""
                                                     K {M(/* one
                                aaaaaaaaa */() =>
                                            {
                                                return 1;
                                            })}
                                                     """;
                                        }

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, starts under <c>delegate</c> when the literal moves 8 columns to the left, and that a wrapped member-access chain inside the block aligns to its own anchor in the block, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-029")]
    public void AnonymousMethodWithInnerChainBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */delegate { return q.A()
                             .B()
                             .C(); })}
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
                                aaaaa */delegate
                                        {
                                            return q.A()
                                                    .B()
                                                    .C();
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, starts under <c>delegate</c> when the literal moves 8 columns to the left, and that a wrapped argument list inside the block aligns to its own anchor in the block, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-012")]
    public void AnonymousMethodWithInnerWrappedArgumentsBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */delegate { return N(1,
                             2); })}
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
                                aaaaa */delegate
                                        {
                                            return N(1,
                                                     2);
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, starts under <c>delegate</c> when the literal moves 8 columns to the left, and that a nested multi-line raw string literal inside the block aligns to its own anchor in the block, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-RAW-001")]
    public void AnonymousMethodWithInnerNestedRawLiteralBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */delegate { return """
                             x
                             """.Length; })}
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
                                aaaaa */delegate
                                        {
                                            return """
                                                   x
                                                   """.Length;
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, starts under <c>delegate</c> when the literal moves 8 columns to the left, and that a nested block lambda inside the block aligns to its own anchor in the block, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-028")]
    public void AnonymousMethodWithInnerNestedBlockLambdaBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */delegate { return N(() => { return 1; }); })}
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
                                aaaaa */delegate
                                        {
                                            return N(() =>
                                                     {
                                                         return 1;
                                                     });
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, starts under its <c>(</c> when the literal moves 8 columns to the left, and that a wrapped member-access chain inside the block aligns to its own anchor in the block, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-029")]
    public void ParenthesizedLambdaWithInnerChainBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */() => { return q.A()
                             .B()
                             .C(); })}
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
                                aaaaa */() =>
                                        {
                                            return q.A()
                                                    .B()
                                                    .C();
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, starts under its <c>(</c> when the literal moves 8 columns to the left, and that a wrapped argument list inside the block aligns to its own anchor in the block, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-012")]
    public void ParenthesizedLambdaWithInnerWrappedArgumentsBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */() => { return N(1,
                             2); })}
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
                                aaaaa */() =>
                                        {
                                            return N(1,
                                                     2);
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, starts under its <c>(</c> when the literal moves 8 columns to the left, and that a nested multi-line raw string literal inside the block aligns to its own anchor in the block, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-RAW-001")]
    public void ParenthesizedLambdaWithInnerNestedRawLiteralBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */() => { return """
                             x
                             """.Length; })}
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
                                aaaaa */() =>
                                        {
                                            return """
                                                   x
                                                   """.Length;
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
    /// Verifies that the block of a parenthesized block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, starts under its <c>(</c> when the literal moves 8 columns to the left, and that a nested block lambda inside the block aligns to its own anchor in the block, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-028")]
    public void ParenthesizedLambdaWithInnerNestedBlockLambdaBehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */() => { return N(() => { return 1; }); })}
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
                                aaaaa */() =>
                                        {
                                            return N(() =>
                                                     {
                                                         return 1;
                                                     });
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
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper in an already aligned literal, and that a member-access chain around the block whose anchor link <c>.A</c> stands on the block's closing line, written in column 6, aligns its next link under <c>.A</c> as measured after the block is placed, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodClosingLineInInputColumn6WithOuterChainBehindCommentEndLineInStatementColumnInAlignedLiteralAlignsTheChainUnderItsAnchorLink()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */delegate { return 1;
                                   }).A()
                             .B()}
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
                                aaaaa */delegate
                                        {
                                            return 1;
                                        }).A()
                                          .B()}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper in an already aligned literal, and that a member-access chain around the block whose anchor link <c>.A</c> stands on the block's closing line, written in column 8, aligns its next link under <c>.A</c> as measured after the block is placed, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodClosingLineInInputColumn8WithOuterChainBehindCommentEndLineInStatementColumnInAlignedLiteralAlignsTheChainUnderItsAnchorLink()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */delegate { return 1;
                                     }).A()
                             .B()}
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
                                aaaaa */delegate
                                        {
                                            return 1;
                                        }).A()
                                          .B()}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 8 columns to the left, and that a member-access chain around the block whose anchor link <c>.A</c> stands on the block's closing line, written in column 14, aligns its next link under <c>.A</c> as measured after the block is placed, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodClosingLineInInputColumn14WithOuterChainBehindCommentEndLineInStatementColumnInLiteralMovedLeftAlignsTheChainUnderItsAnchorLink()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */delegate { return 1;
                                           }).A()
                             .B()}
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
                                aaaaa */delegate
                                        {
                                            return 1;
                                        }).A()
                                          .B()}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under <c>delegate</c> with its body one level deeper when the literal moves 8 columns to the left, and that a member-access chain around the block whose anchor link <c>.A</c> stands on the block's closing line, written in column 16, aligns its next link under <c>.A</c> as measured after the block is placed, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void AnonymousMethodClosingLineInInputColumn16WithOuterChainBehindCommentEndLineInStatementColumnInLiteralMovedLeftAlignsTheChainUnderItsAnchorLink()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */delegate { return 1;
                                             }).A()
                             .B()}
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
                                aaaaa */delegate
                                        {
                                            return 1;
                                        }).A()
                                          .B()}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under the <c>(</c> of its parameter list with its body one level deeper in an already aligned literal, and that a member-access chain around the block whose anchor link <c>.A</c> stands on the block's closing line, written in column 6, aligns its next link under <c>.A</c> as measured after the block is placed, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaClosingLineInInputColumn6WithOuterChainBehindCommentEndLineInStatementColumnInAlignedLiteralAlignsTheChainUnderItsAnchorLink()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */() => { return 1;
                                   }).A()
                             .B()}
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
                                aaaaa */() =>
                                        {
                                            return 1;
                                        }).A()
                                          .B()}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under the <c>(</c> of its parameter list with its body one level deeper in an already aligned literal, and that a member-access chain around the block whose anchor link <c>.A</c> stands on the block's closing line, written in column 8, aligns its next link under <c>.A</c> as measured after the block is placed, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaClosingLineInInputColumn8WithOuterChainBehindCommentEndLineInStatementColumnInAlignedLiteralAlignsTheChainUnderItsAnchorLink()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              K {M(/* one
                             aaaaa */() => { return 1;
                                     }).A()
                             .B()}
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
                                aaaaa */() =>
                                        {
                                            return 1;
                                        }).A()
                                          .B()}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under the <c>(</c> of its parameter list with its body one level deeper when the literal moves 8 columns to the left, and that a member-access chain around the block whose anchor link <c>.A</c> stands on the block's closing line, written in column 14, aligns its next link under <c>.A</c> as measured after the block is placed, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaClosingLineInInputColumn14WithOuterChainBehindCommentEndLineInStatementColumnInLiteralMovedLeftAlignsTheChainUnderItsAnchorLink()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */() => { return 1;
                                           }).A()
                             .B()}
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
                                aaaaa */() =>
                                        {
                                            return 1;
                                        }).A()
                                          .B()}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a block lambda whose anchor stands behind the end of a multi-line <c>/* */</c> comment in a hole, a line that does not move, in column 8, the column block indentation gives its opening brace, starts under the <c>(</c> of its parameter list with its body one level deeper when the literal moves 8 columns to the left, and that a member-access chain around the block whose anchor link <c>.A</c> stands on the block's closing line, written in column 16, aligns its next link under <c>.A</c> as measured after the block is placed, stable on a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    [Requirement("FMT-RAW-007")]
    public void ParenthesizedLambdaClosingLineInInputColumn16WithOuterChainBehindCommentEndLineInStatementColumnInLiteralMovedLeftAlignsTheChainUnderItsAnchorLink()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                                      K {M(/* one
                             aaaaa */() => { return 1;
                                             }).A()
                             .B()}
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
                                aaaaa */() =>
                                        {
                                            return 1;
                                        }).A()
                                          .B()}
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