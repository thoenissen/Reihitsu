using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests for a multi-line raw string literal whose closing line starts with whitespace other than spaces:
/// every whitespace character in front of the closing marker counts as one column (FMT-RAW-001), a shift to the right adds
/// spaces, and a shift to the left removes leading whitespace characters, whatever they are (FMT-RAW-002). In the fixtures,
/// <c>·</c> stands for one no-break space (U+00A0), <c>→</c> for one tab, <c>‡</c> for one em space (U+2003), <c>¶</c> for
/// one vertical tab, and <c>§</c> for one form feed
/// </summary>
[TestClass]
public class RawStringClosingWhitespaceMeasureTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Replaces the placeholders of a fixture with the whitespace characters they stand for
    /// </summary>
    /// <param name="text">The fixture text</param>
    /// <returns>The text with the whitespace characters</returns>
    private static string Expand(string text)
    {
        return text.Replace("·", ((char)0x00A0).ToString())
                   .Replace("→", "\t")
                   .Replace("‡", ((char)0x2003).ToString())
                   .Replace("¶", ((char)0x000B).ToString())
                   .Replace("§", ((char)0x000C).ToString());
    }

    /// <summary>
    /// Wraps the given statements in a method body
    /// </summary>
    /// <param name="statements">The statements</param>
    /// <returns>The source text</returns>
    private static string InMethod(string statements)
    {
        return Expand($"class C\n{{\n    void M()\n    {{\n{statements}\n    }}\n}}");
    }

    /// <summary>
    /// Builds a statement that declares a plain raw string with one content line
    /// </summary>
    /// <param name="prefix">The whitespace in front of the content line and the closing marker</param>
    /// <returns>The statement</returns>
    private static string Literal(string prefix)
    {
        return $"        var a = \"\"\"\n{prefix}x\n{prefix}\"\"\";";
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the specification example: a literal behind four no-break spaces moves 12 columns to the right, and one
    /// behind twenty moves 4 columns to the left, removing four of them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-RAW-002")]
    public void NoBreakSpacesCountAsOneColumnEach()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                             ····x
                             ····""";
                                     var b = """
                             ····················y
                             ····················""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                            ····x
                                            ····""";
                                        var b = """
                                ················y
                                ················""";
                                    }
                                }
                                """";

        AssertRuleResult(Expand(input), Expand(expected));
    }

    /// <summary>
    /// Verifies the specification example and the maintainer's decision: a literal behind four tabs counts each tab as one
    /// column, gains 12 spaces, and keeps its tabs
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void TabsCountAsOneColumnEachAndStay()
    {
        var input = InMethod(Literal("→→→→"));
        var expected = InMethod(Literal("            →→→→"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported example: four no-break spaces in front of the closing marker give 12 spaces in front of them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-RAW-002")]
    public void LiteralBehindNoBreakSpacesMovesRight()
    {
        var input = InMethod(Literal("····"));
        var expected = InMethod(Literal("            ····"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a literal that is aligned by the shared measure, 12 spaces and four no-break spaces, stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void LiteralAlignedBySharedMeasureStays()
    {
        AssertRuleResult(InMethod(Literal("            ····")));
    }

    /// <summary>
    /// Verifies that a literal aligned by the shared measure with tabs, 12 spaces and four tabs, stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void LiteralAlignedBySharedMeasureWithTabsStays()
    {
        AssertRuleResult(InMethod(Literal("            →→→→")));
    }

    /// <summary>
    /// Verifies that a shift to the left smaller than the spaces in front of the no-break spaces removes only spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-002")]
    public void LeftShiftSmallerThanLeadingSpacesRemovesOnlySpaces()
    {
        var input = InMethod(Literal("                  ····"));
        var expected = InMethod(Literal("            ····"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a shift to the left as large as the spaces in front of the no-break spaces removes every space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-002")]
    public void LeftShiftEqualToLeadingSpacesRemovesEverySpace()
    {
        var input = InMethod(Literal("    ················"));
        var expected = InMethod(Literal("················"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a shift to the left larger than the spaces in front of the no-break spaces also removes no-break spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-002")]
    public void LeftShiftLargerThanLeadingSpacesRemovesNoBreakSpaces()
    {
        var input = InMethod(Literal("  ····················"));
        var expected = InMethod(Literal("················"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a literal behind four em spaces moves 12 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void LiteralBehindEmSpacesMovesRight()
    {
        var input = InMethod(Literal("‡‡‡‡"));
        var expected = InMethod(Literal("            ‡‡‡‡"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a literal behind four vertical tabs moves 12 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void LiteralBehindVerticalTabsMovesRight()
    {
        var input = InMethod(Literal("¶¶¶¶"));
        var expected = InMethod(Literal("            ¶¶¶¶"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a literal behind four form feeds moves 12 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void LiteralBehindFormFeedsMovesRight()
    {
        var input = InMethod(Literal("§§§§"));
        var expected = InMethod(Literal("            §§§§"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that spaces in front of no-break spaces count with them, and the added spaces go in front of both
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-RAW-002")]
    public void SpacesInFrontOfNoBreakSpacesCountWithThem()
    {
        var input = InMethod(Literal("  ··"));
        var expected = InMethod(Literal("              ··"));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that spaces behind no-break spaces count with them, and the added spaces go in front of both
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-RAW-002")]
    public void SpacesBehindNoBreakSpacesCountWithThem()
    {
        var input = InMethod(Literal("··  "));
        var expected = InMethod(Literal("            ··  "));

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an interpolated literal behind no-break spaces moves under its quotes, not under the <c>$</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-RAW-003")]
    public void InterpolatedLiteralBehindNoBreakSpacesMovesRight()
    {
        var input = InMethod("        var n = 1;\n        var a = $\"\"\"\n····{n}\n····\"\"\";");
        var expected = InMethod("        var n = 1;\n        var a = $\"\"\"\n             ····{n}\n             ····\"\"\";");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a UTF-8 literal behind no-break spaces moves 12 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void Utf8LiteralBehindNoBreakSpacesMovesRight()
    {
        var input = InMethod("        var a = \"\"\"\n····x\n····\"\"\"u8;");
        var expected = InMethod("        var a = \"\"\"\n            ····x\n            ····\"\"\"u8;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a content line indented deeper than the closing marker keeps its relative indentation when the literal
    /// moves left by more than its leading spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-002")]
    public void DeeperContentLineKeepsRelativeIndentationOnLeftShift()
    {
        var input = InMethod("        var a = \"\"\"\n  ······················x\n  ····················\"\"\";");
        var expected = InMethod("        var a = \"\"\"\n··················x\n················\"\"\";");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a member access chained to the closing marker stays aligned with it, and a wrapped interpolation hole
    /// stays aligned after its opening parenthesis, once the literal moves by the shared measure
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-INDENT-030")]
    public void AnchoredLinesFollowTheMovedLiteral()
    {
        const string input = """"
                             class C
                             {
                                 int M2(int a, int b)
                                 {
                                     return a;
                                 }

                                 void M()
                                 {
                                     var a = """
                             ····x
                             ····""".Trim()
                                 .ToString();
                                     var b = $"""
                             ····{M2(1,
                             2)}
                             ····""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M2(int a, int b)
                                    {
                                        return a;
                                    }

                                    void M()
                                    {
                                        var a = """
                                            ····x
                                            ····""".Trim()
                                                   .ToString();
                                        var b = $"""
                                             ····{M2(1,
                                                     2)}
                                             ····""";
                                    }
                                }
                                """";

        AssertRuleResult(Expand(input), Expand(expected));
    }

    #endregion // Tests
}