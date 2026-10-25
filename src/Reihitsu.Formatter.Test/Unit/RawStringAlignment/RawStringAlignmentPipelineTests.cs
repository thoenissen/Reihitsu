using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.RawStringAlignment;

/// <summary>
/// Tests for raw-string alignment through the whole pipeline, after earlier phases moved the opening quotes
/// </summary>
[TestClass]
public class RawStringAlignmentPipelineTests : FormatterTestsBase
{
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

    #endregion // Methods
}