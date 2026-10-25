using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.RawStringAlignment;

/// <summary>
/// Tests for raw-string alignment through the whole pipeline, after earlier phases moved the opening quotes
/// </summary>
[TestClass]
public class RawStringAlignmentPipelineTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the spec example: a raw string joined onto the line of its assignment is aligned to the column its opening quotes end up in
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-005")]
    public void RawStringJoinedOntoItsAssignmentLineIsAlignedToItsNewColumn()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a =
                                         """
                                 Hello
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello
                                                """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the raw string of a re-indented statement is aligned to the new column of its opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-005")]
    public void RawStringOfReIndentedStatementFollowsTheStatement()
    {
        const string input = """"
                             class C
                             {
                             void M()
                             {
                             var a = """
                                         Hello
                                         """;
                             }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello
                                                """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a UTF-8 raw string is aligned like a plain one, keeps its relative indentation, and its empty line stays empty
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-002")]
    [Requirement("FMT-RAW-004")]
    public void Utf8RawStringIsAlignedAndKeepsItsEmptyLine()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 Hello

                                   Indented
                                 """u8;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello

                                                  Indented
                                                """u8;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped chain link behind the closing quotes of a raw string, whose opening line is the closing line of an earlier raw string, aligns to its anchor after both literals moved, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-029")]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkBehindSecondRawStringOnASharedLineAlignsAfterBothLiteralsMoved()
    {
        const string input = """"
                             class C
                             {
                                 string M()
                                 {
                                     return """
                                 a
                                 """ + """
                                 b
                                 """.Trim()
                                    .ToString();
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    string M()
                                    {
                                        return """
                                               a
                                               """ + """
                                                     b
                                                     """.Trim()
                                                        .ToString();
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped argument whose opening parenthesis stands behind the closing quotes of a raw string outside any interpolation hole aligns to the column that parenthesis has after the literal moved, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    [Requirement("FMT-INDENT-030")]
    public void WrappedArgumentAnchoredBehindTheClosingQuotesOfARawStringAlignsAfterTheLiteralMoved()
    {
        const string input = """"
                             class C
                             {
                                 string M(string s)
                                 {
                                     return M("""
                                 a
                                 """ + string.Concat("x",
                                                     "y"));
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    string M(string s)
                                    {
                                        return M("""
                                                 a
                                                 """ + string.Concat("x",
                                                                     "y"));
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the chain link anchored behind the closing marker of a raw string that moves 12 columns to the
    /// right aligns to the anchor where the move puts it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkAlignsToAnchorBehindClosingMarkerOfAMovedRawString()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = """
                                 a
                                 """.Trim()
                                 .ToUpper();
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var s = """
                                                a
                                                """.Trim()
                                                   .ToUpper();
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the spec example: when the literal is not moved, the chain link aligns to the same column and
    /// the output is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkBehindClosingMarkerOfAnUnmovedRawStringIsLeftUnchanged()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = """
                                             a
                                             """.Trim()
                                                .ToUpper();
                                 }
                             }
                             """";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: on a line that a comment leads, the literal is aligned to the opening quotes behind the
    /// comment, not to the column the line starts in
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-005")]
    public void RawStringOnCommentLedLineIsAlignedToOpeningQuotesBehindTheComment()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                 /* c */ var s = """
                                 a
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        /* c */ var s = """
                                                        a
                                                        """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: on a line that a comment leads, the literal moves under its opening quotes behind the comment,
    /// and the chain link anchored behind its closing marker aligns to the anchor where that move puts it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkAlignsToAnchorBehindClosingMarkerOfRawStringOnCommentLedLine()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     /* c */ var s = """
                                 a
                                 """.Trim()
                                 .ToUpper();
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        /* c */ var s = """
                                                        a
                                                        """.Trim()
                                                           .ToUpper();
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that on a line that a comment leads, an interpolated raw string with a hole moves under its opening quotes
    /// behind the comment, and the chain link anchored behind its closing marker aligns to the anchor where that move puts it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkAlignsToAnchorBehindClosingMarkerOfInterpolatedRawStringOnCommentLedLine()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     /* c */ var s = $"""
                                 a{x}
                                 """.Trim()
                                 .ToUpper();
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        /* c */ var s = $"""
                                                         a{x}
                                                         """.Trim()
                                                            .ToUpper();
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a raw string passed as an argument on a line that a comment leads moves under its opening quotes behind
    /// the comment, and the chain link anchored behind its closing marker aligns to the anchor where that move puts it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkAlignsToAnchorBehindClosingMarkerOfRawStringArgumentOnCommentLedLine()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = Call(1,
                                         /* c */ """
                                 a
                                 """.Trim()
                                 .ToUpper());
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var s = Call(1,

                                                     /* c */ """
                                                             a
                                                             """.Trim()
                                                                .ToUpper());
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that detached-node formatting moves a literal on a line that a comment leads under its opening quotes behind
    /// the comment, and aligns the chain link behind its closing marker to the anchor where that move puts it, as the
    /// document-level entry point does
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void FormatNodeAlignsChainLinkBehindClosingMarkerOfRawStringOnCommentLedLine()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     /* c */ var s = """
                                 a
                                 """.Trim()
                                 .ToUpper();
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        /* c */ var s = """
                                                        a
                                                        """.Trim()
                                                           .ToUpper();
                                    }
                                }
                                """";

        foreach (var endOfLine in _lineEndings)
        {
            var root = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken)
                                       .GetRoot(TestContext.CancellationToken);

            var actual = ReihitsuFormatter.FormatNode(root, cancellationToken: TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), actual, $"FormatNode output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // Methods
}