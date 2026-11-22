using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider"/> on a literal whose closing
/// quotes have a tab in the whitespace in front of them: the fix writes the stripped indentation as spaces. In the fixtures,
/// <c>→</c> stands for one tab, <c>·</c> for one space at the end of a line, and <c>¤</c> for one no-break space (U+00A0)
/// </summary>
[TestClass]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyTabIndentationTests : BatchCodeFixTestsBase<RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer, RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Methods

    /// <summary>
    /// Replaces the placeholders of a fixture with the whitespace characters they stand for
    /// </summary>
    /// <param name="text">The fixture text</param>
    /// <returns>The text with the whitespace characters</returns>
    private static string Expand(string text)
    {
        return ExpandWhitespaceMarkers(text).Replace('¤', ' ');
    }

    /// <summary>
    /// Verifies the diagnostic marked in the source and the result of the code fix, under LF and CRLF line endings
    /// </summary>
    /// <param name="source">The source text with placeholders and markup for the expected diagnostic</param>
    /// <param name="fixedSource">The expected fixed source text with placeholders</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFixUnderBothLineEndings(string source, string fixedSource)
    {
        await VerifyCodeFixWithEndOfLine(Expand(source), Expand(fixedSource), "\n");
        await VerifyCodeFixWithEndOfLine(Expand(source), Expand(fixedSource), "\r\n");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the specification example: the literal moves 14 columns to the right, the two tabs in front of the closing quotes
    /// and of <c>Hello</c> become spaces, and the third tab in front of <c>World</c> is behind the width and stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixWritesTheStrippedTabsAsSpacesAndKeepsTheTabBehindTheWidth()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                →→Hello
                                →→→World
                                →→"""|};
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                                  Hello
                                                  →World
                                                  """;
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes only the shift when the whitespace in front of the closing quotes holds no tab, so the tab
    /// behind the width on the content line stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixKeepsTheTabOfTheContentLineWhenTheClosingWhitespaceHoldsNoTab()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                    →x
                                    """|};
                                    }
                                }
                                """";
        const string resultData = """"
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

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a literal whose closing quotes stand in the column of the opening quotes behind 12 spaces and four tabs,
    /// counting each tab as one column, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyLiteralAlignedInCharactersBehindTabsIsNotReported()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                            →→→→x
                                            →→→→""";
                                    }
                                }
                                """";

        await Verify(Expand(testData));
        await Verify(NormalizeToCarriageReturnLineFeed(Expand(testData)));
    }

    /// <summary>
    /// Verifies that the fix moves a literal behind twenty tabs 4 columns to the left, which removes four tabs, and writes the
    /// sixteen left as spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixShiftsLeftOverTabsAndWritesTheRestAsSpaces()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                →→→→→→→→→→→→→→→→→→→→x
                                →→→→→→→→→→→→→→→→→→→→"""|};
                                    }
                                }
                                """";
        const string resultData = """"
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

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix moves a literal behind two spaces and eighteen tabs 4 columns to the left, which removes both spaces
    /// and two tabs, and writes the sixteen tabs left as spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixShiftsLeftOverSpacesAndTabsAndWritesTheRestAsSpaces()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                  →→→→→→→→→→→→→→→→→→x
                                  →→→→→→→→→→→→→→→→→→"""|};
                                    }
                                }
                                """";
        const string resultData = """"
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

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes a whitespace-only content line holding one tab, a start of the closing line's two, as one
    /// space per character after the shift: 15 spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixWritesAWhitespaceOnlyLineShorterThanTheWidthAsOneSpacePerCharacter()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                →→Hello
                                →
                                →→"""|};
                                    }
                                }
                                """";
        const string resultData = """"
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

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that once the closing whitespace holds a tab, the fix writes a no-break space in it as a space as well
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixWritesANoBreakSpaceNextToATabAsASpace()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                →¤x
                                →¤"""|};
                                    }
                                }
                                """";
        const string resultData = """"
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

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the stripped tabs of an interpolated literal as spaces on a content line that starts with a
    /// hole behind the start token, on one that starts with a hole behind the closing brace of a hole on the line above, and in
    /// front of the closing quotes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixWritesTheStrippedTabsOfAnInterpolatedLiteralAsSpaces()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int n)
                                    {
                                        var a = {|RH5202:$"""
                                →→{n}
                                →→{n}
                                →→"""|};
                                    }
                                }
                                """";
        const string resultData = """"
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

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the stripped tabs of a UTF-8 literal as spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixWritesTheStrippedTabsOfAUtf8LiteralAsSpaces()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                →→x
                                →→"""u8|};
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                                  x
                                                  """u8;
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix's result for a tab-indented literal is the formatter's layout of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    public async Task VerifyFixMatchesFormatterForATabIndentedLiteral()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                →→Hello
                                →→→World
                                →→"""|};
                                    }
                                }
                                """";

        await VerifyCodeFixMatchesFormatter(Expand(testData));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                →→Hello
                                →→"""|};
                                        var b = {|#1:"""
                                →→→→→→→→→→→→→→→→→→→→World
                                →→→→→→→→→→→→→→→→→→→→"""|};
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                                  Hello
                                                  """;
                                          var b = """
                                                  World
                                                  """;
                                      }
                                  }
                                  """";

        return new FixAllScenario(Expand(testCode),
                                  Expand(resultData),
                                  Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}