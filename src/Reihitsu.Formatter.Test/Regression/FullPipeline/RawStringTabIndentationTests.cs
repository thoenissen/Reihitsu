using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests for a multi-line raw string literal whose closing marker has a tab in the whitespace in front of it: after
/// the move, every whitespace character in front of the closing marker, and every whitespace character at the start of a content
/// line within that width, becomes one space (FMT-RAW-010). In the fixtures, <c>→</c> stands for one tab, <c>·</c> for one
/// space at the end of a line, <c>¤</c> for one no-break space (U+00A0), <c>¶</c> for one vertical tab, and <c>§</c> for one form
/// feed
/// </summary>
[TestClass]
public class RawStringTabIndentationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Replaces the placeholders of a fixture with the whitespace characters they stand for
    /// </summary>
    /// <param name="text">The fixture text</param>
    /// <returns>The text with the whitespace characters</returns>
    private static string Expand(string text)
    {
        return text.Replace("→", "\t")
                   .Replace("·", " ")
                   .Replace("¤", ((char)0x00A0).ToString())
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

    /// <summary>
    /// Formats the input and asserts the expected output under LF and CRLF, with a second pass that changes nothing
    /// </summary>
    /// <param name="input">The input with placeholders</param>
    /// <param name="expected">The expected output with placeholders</param>
    private static void AssertExpanded(string input, string expected)
    {
        AssertRuleResult(Expand(input), Expand(expected));
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the specification example: the literal moves 14 columns to the right, and the two tabs in front of the content
    /// line and the closing marker become spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void RightShiftWritesTheStrippedTabsAsSpaces()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                             →→Hello
                             →→""";
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

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a content line indented further than the closing marker keeps the tab behind the
    /// width of the closing whitespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void TabBehindTheStrippedWidthStays()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                             →→→Hello
                             →→""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                →Hello
                                                """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the whitespace in front of the closing marker holds no tab, so the tab on the content
    /// line stays and the literal only moves 12 columns to the right
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void ClosingWhitespaceWithoutATabKeepsTheTabOfTheContentLine()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 →x
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
                                                →x
                                                """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the closing marker stands behind twenty tabs, the literal moves 4 columns to the
    /// left, which removes four tabs, and the sixteen left become spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void LeftShiftRemovesTabsAndWritesTheRestAsSpaces()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                             →→→→→→→→→→→→→→→→→→→→x
                             →→→→→→→→→→→→→→→→→→→→""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                x
                                                """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that a left shift smaller than the spaces in front of the tabs removes only spaces, and the tabs become spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void LeftShiftSmallerThanTheSpacesInFrontOfTheTabsWritesTheTabsAsSpaces()
    {
        AssertRuleResult(InMethod(Literal("          →→→→→→→→")), InMethod(Literal("                ")));
    }

    /// <summary>
    /// Verifies that a left shift as large as the spaces in front of the tabs removes every space, and the tabs become spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void LeftShiftEqualToTheSpacesInFrontOfTheTabsWritesTheTabsAsSpaces()
    {
        AssertRuleResult(InMethod(Literal("    →→→→→→→→→→→→→→→→")), InMethod(Literal("                ")));
    }

    /// <summary>
    /// Verifies that a left shift larger than the spaces in front of the tabs also removes tabs, and the tabs left become spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void LeftShiftLargerThanTheSpacesInFrontOfTheTabsWritesTheRemainingTabsAsSpaces()
    {
        AssertRuleResult(InMethod(Literal("  →→→→→→→→→→→→→→→→→→")), InMethod(Literal("                ")));
    }

    /// <summary>
    /// Verifies the specification example: a whitespace-only content line holding one tab, a start of the closing line's two,
    /// moves 14 columns with the other lines and becomes 15 spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void WhitespaceOnlyLineShorterThanTheWidthBecomesOneSpacePerCharacter()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                             →→Hello
                             →
                             →→""";
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
                                ···············
                                                """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that a whitespace-only content line longer than the width of the closing whitespace gets spaces within that
    /// width and keeps the tab behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void WhitespaceOnlyLineLongerThanTheWidthKeepsTheTabBehindIt()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                             →→Hello
                             →→→
                             →→""";
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
                                                →
                                                """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that once the closing whitespace holds a tab, a no-break space in it becomes a space as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void NoBreakSpaceNextToATabBecomesASpace()
    {
        AssertRuleResult(InMethod(Literal("→¤")), InMethod(Literal("                ")));
    }

    /// <summary>
    /// Verifies that once the closing whitespace holds a tab, a vertical tab and a form feed in it become spaces as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void VerticalTabAndFormFeedNextToATabBecomeSpaces()
    {
        AssertRuleResult(InMethod(Literal("¶→§")), InMethod(Literal("                ")));
    }

    /// <summary>
    /// Verifies that the stripped tabs of a UTF-8 literal become spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void Utf8LiteralWritesTheStrippedTabsAsSpaces()
    {
        var input = InMethod("        var a = \"\"\"\n→→x\n→→\"\"\"u8;");
        var expected = InMethod("        var a = \"\"\"\n                x\n                \"\"\"u8;");

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the stripped tabs of an interpolated literal become spaces on a content line that starts with a hole
    /// behind the start token, on one that starts with a hole behind the closing brace of a hole on the line above, and in
    /// front of the closing marker
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void InterpolatedLiteralWritesTheStrippedTabsInFrontOfHolesAsSpaces()
    {
        const string input = """"
                             class C
                             {
                                 void M(int n)
                                 {
                                     var a = $"""
                             →→{n}
                             →→{n}
                             →→""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M(int n)
                                    {
                                        var a = $"""
                                                 {n}
                                                 {n}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that the stripped tabs of a literal with two <c>$</c> become spaces, aligned under its quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void DoubleDollarLiteralWritesTheStrippedTabsAsSpaces()
    {
        const string input = """"
                             class C
                             {
                                 void M(int n)
                                 {
                                     var a = $$"""
                             →→{"a": {{n}}}
                             →→""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M(int n)
                                    {
                                        var a = $$"""
                                                  {"a": {{n}}}
                                                  """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the wrapped argument in the hole stays in column 38, and the tab in front of the
    /// content line it stands under becomes a space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void HoleLineAlignedToATokenOnATabIndentedContentLineKeepsItsColumn()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                             →Hello {string.Concat("a",
                                 "b")}
                             →""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {string.Concat("a",
                                                                      "b")}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that in a literal already aligned in characters behind a tab, the tabs become spaces while the hole line aligned
    /// to a token on a content line keeps its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-006")]
    [Requirement("FMT-RAW-010")]
    public void AlignedLiteralWithAnAnchoredHoleLineWritesTheTabsAsSpacesWithoutMovingALine()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                             →Hello {string.Concat("a",
                                                                   "b")}
                                             →""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {string.Concat("a",
                                                                      "b")}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that the body of a block lambda in a hole on a tab-indented content line keeps the columns it is laid out in,
    /// counted in characters, while the tab in front of the content line and of the closing marker becomes a space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void BlockLambdaInAHoleOnATabIndentedContentLineKeepsItsColumns()
    {
        const string input = """"
                             class C
                             {
                                 int Run(System.Func<int> f)
                                 {
                                     return f();
                                 }

                                 void M()
                                 {
                                     var a = $"""
                             →Value {Run(() =>
                             {
                                 return 1;
                             })}
                             →""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int Run(System.Func<int> f)
                                    {
                                        return f();
                                    }

                                    void M()
                                    {
                                        var a = $"""
                                                 Value {Run(() =>
                                                            {
                                                                return 1;
                                                            })}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a raw string nested in a hole is decided by its own closing marker, so its two tabs
    /// become spaces while the outer literal, without a tab in front of its closing marker, only moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void NestedRawStringIsDecidedByItsOwnClosingMarker()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Outer {"""
                             →→inner
                             →→"""}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {"""
                                                        inner
                                                        """}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that a raw string nested in a hole and already aligned in characters behind spaces and tabs gets its tabs written
    /// as spaces when the outer literal moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void NestedRawStringAlignedInCharactersBehindTabsIsWrittenWithSpaces()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Outer {"""
                                      →→inner
                                      →→"""}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {"""
                                                        inner
                                                        """}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that when both the outer literal and the raw string nested in its hole hold tabs in front of their closing
    /// markers, the tabs of both become spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void OuterAndNestedRawStringsIndentedWithTabsAreBothWrittenWithSpaces()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                             →Outer {"""
                             →→inner
                             →→"""}
                             →""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {"""
                                                        inner
                                                        """}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that the outer literal's tab does not rewrite the lines of a raw string nested in its hole whose own closing
    /// marker holds no tab, so the tab on the nested content line stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void NestedRawStringWithoutATabInFrontOfItsClosingMarkerKeepsItsTabsInsideATabIndentedLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                             →Outer {"""
                               →inner
                               """}
                             →""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {"""
                                                        →inner
                                                        """}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that a tab-indented raw string in a hole of a regular interpolated string, the outermost raw string there, gets
    /// its stripped tabs written as spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void RawStringInAHoleOfARegularInterpolatedStringWritesTheTabsAsSpaces()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"a{"""
                             →→x
                             →→"""}b";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"a{"""
                                                    x
                                                    """}b";
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the continuation line of a verbatim string in a hole starts inside that string's text
    /// and keeps its tab, while the content lines and the closing marker get spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void ContinuationLineOfAVerbatimStringInAHoleKeepsItsTab()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                             →Hello {@"a
                             →b"}
                             →""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {@"a
                                →b"}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    /// <summary>
    /// Verifies that the continuation line of a multi-line comment in a hole starts inside the comment and keeps its tab, while
    /// the content lines and the closing marker get spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-010")]
    public void ContinuationLineOfACommentInAHoleKeepsItsTab()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                             →Hello {/* a
                             →b */ 1}
                             →""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {/* a
                                →b */ 1}
                                                 """;
                                    }
                                }
                                """";

        AssertExpanded(input, expected);
    }

    #endregion // Tests
}