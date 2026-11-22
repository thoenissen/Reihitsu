using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the block of an anonymous method in an interpolation hole of a multi-line raw string literal whose anchor stands on a hole line that does not move with the literal, in the column that block indentation gives the block's opening brace — the column of the statement around the literal
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

    #endregion // Tests
}