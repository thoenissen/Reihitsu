using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the block of an anonymous method or a block lambda in an interpolation hole of a multi-line raw
/// string literal whose anchor stands on a hole line that does not move with the literal, in the column that block
/// indentation gives the block's opening brace — the column of the statement around the literal
/// </summary>
[TestClass]
public partial class HoleAnchorInStatementColumnAlignmentTests : FormatterTestsBase
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

    #endregion // Tests
}