using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Tests for <see cref="RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
public partial class RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests
{
    #region Tests

    /// <summary>
    /// Verifies that the fix lays out a literal with a multi-line comment in a hole as the formatter lays it out
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixMatchesFormatterForCommentInHole()
    {
        await VerifyCodeFixMatchesFormatter(""""
                                            class C
                                            {
                                                void M()
                                                {
                                                    var name = "x";
                                                    var a = {|RH5202:$"""
                                                Hello {name /* a
                                              b */
                                                }
                                                """|};
                                                }
                                            }
                                            """");
    }

    /// <summary>
    /// Verifies that the fix lays out a literal with an empty line inside a verbatim string in a hole as the formatter
    /// lays it out
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixMatchesFormatterForEmptyLineInsideVerbatimStringInHole()
    {
        await VerifyCodeFixMatchesFormatter(""""
                                            class C
                                            {
                                                void M()
                                                {
                                                    var a = {|RH5202:$"""
                                                Hello {@"a

                                            b"}
                                                """|};
                                                }
                                            }
                                            """");
    }

    /// <summary>
    /// Verifies that the fix lays out a literal with an empty content line as the formatter lays it out, for a plain
    /// and an interpolated literal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-005")]
    public async Task VerifyFixMatchesFormatterForEmptyContentLine()
    {
        await VerifyCodeFixMatchesFormatter(""""
                                            class C
                                            {
                                                void M()
                                                {
                                                    var a = {|RH5202:"""
                                                one

                                                two
                                                """|};
                                                }
                                            }
                                            """");

        await VerifyCodeFixMatchesFormatter(""""
                                            class C
                                            {
                                                void M(int name)
                                                {
                                                    var a = {|RH5202:$"""
                                                one

                                                two {name}
                                                """|};
                                                }
                                            }
                                            """");
    }

    /// <summary>
    /// Verifies that the fix shifts a literal to the right by the full width of the whitespace in front of its closing quotes when that whitespace starts with a no-break space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsRightWhenClosingWhitespaceStartsWithNoBreakSpace()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                ¤   x
                                ¤   """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                              ¤   x
                                              ¤   """;
                                      }
                                  }
                                  """";

        await Verify(testData.Replace('¤', '\u00A0'), resultData.Replace('¤', '\u00A0'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a literal to the right by the full width of the whitespace in front of its closing quotes when that whitespace starts with a no-break space, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsRightWhenClosingWhitespaceStartsWithNoBreakSpaceWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                ¤   x
                                ¤   """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                              ¤   x
                                              ¤   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData.Replace('¤', '\u00A0')), NormalizeToCarriageReturnLineFeed(resultData.Replace('¤', '\u00A0')), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a literal to the right by the full width of the whitespace in front of its closing quotes when that whitespace starts with a form feed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsRightWhenClosingWhitespaceStartsWithFormFeed()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                ¤   x
                                ¤   """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                              ¤   x
                                              ¤   """;
                                      }
                                  }
                                  """";

        await Verify(testData.Replace('¤', '\f'), resultData.Replace('¤', '\f'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a literal to the right by the full width of the whitespace in front of its closing quotes when that whitespace starts with a vertical tab
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsRightWhenClosingWhitespaceStartsWithVerticalTab()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                ¤   x
                                ¤   """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                              ¤   x
                                              ¤   """;
                                      }
                                  }
                                  """";

        await Verify(testData.Replace('¤', '\v'), resultData.Replace('¤', '\v'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a UTF-8 literal to the right by the full width of the whitespace in front of its closing quotes when that whitespace starts with an em space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsUtf8LiteralRightWhenClosingWhitespaceStartsWithEmSpace()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                ¤   x
                                ¤   """u8|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                              ¤   x
                                              ¤   """u8;
                                      }
                                  }
                                  """";

        await Verify(testData.Replace('¤', '\u2003'), resultData.Replace('¤', '\u2003'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts an interpolated literal to the right by the full width of the whitespace in front of its closing quotes when that whitespace starts with a no-break space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsInterpolatedLiteralRightWhenClosingWhitespaceStartsWithNoBreakSpace()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int name)
                                    {
                                        var a = {|#0:$"""
                                ¤   {name}
                                ¤   """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(int name)
                                      {
                                          var a = $"""
                                               ¤   {name}
                                               ¤   """;
                                      }
                                  }
                                  """";

        await Verify(testData.Replace('¤', '\u00A0'), resultData.Replace('¤', '\u00A0'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts an interpolated literal to the right by the full width of the whitespace in front of its closing quotes when that whitespace starts with a no-break space, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsInterpolatedLiteralRightWhenClosingWhitespaceStartsWithNoBreakSpaceWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int name)
                                    {
                                        var a = {|#0:$"""
                                ¤   {name}
                                ¤   """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(int name)
                                      {
                                          var a = $"""
                                               ¤   {name}
                                               ¤   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData.Replace('¤', '\u00A0')), NormalizeToCarriageReturnLineFeed(resultData.Replace('¤', '\u00A0')), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a literal to the left by the full width of the whitespace in front of its closing quotes when a no-break space follows the spaces of that whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsLeftWhenClosingWhitespaceEndsWithNoBreakSpace()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                                    ¤x
                                                    ¤"""|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                                 ¤x
                                                 ¤""";
                                      }
                                  }
                                  """";

        await Verify(testData.Replace('¤', '\u00A0'), resultData.Replace('¤', '\u00A0'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a literal to the left by the full width of the whitespace in front of its closing quotes when a no-break space follows the spaces of that whitespace, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsLeftWhenClosingWhitespaceEndsWithNoBreakSpaceWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                                    ¤x
                                                    ¤"""|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = """
                                                 ¤x
                                                 ¤""";
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData.Replace('¤', '\u00A0')), NormalizeToCarriageReturnLineFeed(resultData.Replace('¤', '\u00A0')), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a literal to the left by removing leading whitespace that starts with a no-break space, so that its closing quotes start in the column of its opening quotes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsLeftWhenClosingWhitespaceStartsWithNoBreakSpace()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                ¤                    x
                                ¤                    """|};
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

        await Verify(testData.Replace('¤', '\u00A0'), resultData.Replace('¤', '\u00A0'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty line of hole code, outside every literal's text, empty when it shifts the hole's other lines to the right
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingRight()
    {
        const string testData = """"
                                class C
                                {
                                    void M(string name)
                                    {
                                        var a = {|#0:$"""
                                    Hello {name

                                    }
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(string name)
                                      {
                                          var a = $"""
                                                   Hello {name

                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty line of hole code, outside every literal's text, empty when it shifts the hole's other lines to the right, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingRightWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M(string name)
                                    {
                                        var a = {|#0:$"""
                                    Hello {name

                                    }
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(string name)
                                      {
                                          var a = $"""
                                                   Hello {name

                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty line inside an interpolation hole empty when it shifts the literal to the left, because the line has no leading whitespace to remove
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingLeft()
    {
        const string testData = """"
                                class C
                                {
                                    void M(string name)
                                    {
                                        var a = {|#0:$"""
                                                         Hello {name

                                                         }
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(string name)
                                      {
                                          var a = $"""
                                                   Hello {name

                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty line of code in a multi-line hole of a regular interpolated string nested in a hole empty when it shifts the other lines to the right
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyLineInsideHoleOfNestedInterpolatedStringEmptyWhenShiftingRight()
    {
        const string testData = """"
                                class C
                                {
                                    void M(string name)
                                    {
                                        var a = {|#0:$"""
                                    Hello {$"x{
                                    name

                                    }"}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(string name)
                                      {
                                          var a = $"""
                                                   Hello {$"x{
                                                   name

                                                   }"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty line of code in a multi-line hole of a regular interpolated string nested in a hole empty when it shifts the other lines to the right, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyLineInsideHoleOfNestedInterpolatedStringEmptyWhenShiftingRightWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M(string name)
                                    {
                                        var a = {|#0:$"""
                                    Hello {$"x{
                                    name

                                    }"}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(string name)
                                      {
                                          var a = $"""
                                                   Hello {$"x{
                                                   name

                                                   }"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a line inside an interpolation hole that holds only whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyFixShiftsWhitespaceOnlyLineInsideHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M(string name)
                                    {
                                        var a = {|#0:$"""
                                    Hello {name
                                ··
                                    }
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(string name)
                                      {
                                          var a = $"""
                                                   Hello {name
                                  ···············
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the outer literal leaves an empty content line of a raw string literal nested in one of its holes empty when it shifts the literal to the right
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-005")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingRight()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {{|#1:"""
                                        x

                                        y
                                        """|}}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                                       x

                                                       y
                                                       """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = testData;
        var result = resultData;

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the outer literal leaves an empty content line of a raw string literal nested in one of its holes empty when it shifts the literal to the right, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-005")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingRightWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {{|#1:"""
                                        x

                                        y
                                        """|}}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                                       x

                                                       y
                                                       """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = NormalizeToCarriageReturnLineFeed(testData);
        var result = NormalizeToCarriageReturnLineFeed(resultData);

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the outer literal leaves an empty content line of a raw string literal nested in one of its holes empty when it shifts the literal to the left
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-005")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingLeft()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                             x

                                                             y
                                                             """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                                       x

                                                       y
                                                       """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = testData;
        var result = resultData;

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the outer literal leaves an empty content line of a raw string literal nested in one of its holes empty when it shifts the literal to the left, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-005")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmptyWhenShiftingLeftWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                             x

                                                             y
                                                             """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                                       x

                                                       y
                                                       """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = NormalizeToCarriageReturnLineFeed(testData);
        var result = NormalizeToCarriageReturnLineFeed(resultData);

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when shifting its lines to the left would remove fewer characters from the closing line of a raw string literal nested in one of its holes than from a content line of it, and that the nested literal keeps its own fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfRawStringNestedInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(testData, 2);
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when shifting its lines to the left would remove fewer characters from the closing line of a raw string literal nested in one of its holes than from a content line of it, and that the nested literal keeps its own fix, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfRawStringNestedInHoleWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(NormalizeToCarriageReturnLineFeed(testData), 2);
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when shifting its lines to the left would change the value of an interpolated raw string literal nested in one of its holes, and that the nested literal keeps its own fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfInterpolatedRawStringNestedInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int n)
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:$"""
                                                                  x{n}
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(testData, 2);
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when shifting its lines to the left would change the value of an interpolated raw string literal nested in one of its holes, and that the nested literal keeps its own fix, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfInterpolatedRawStringNestedInHoleWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int n)
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:$"""
                                                                  x{n}
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(NormalizeToCarriageReturnLineFeed(testData), 2);
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when it shifts its lines to the left and the closing quotes of a raw string literal nested in one of its holes start in column 0 while a content line of it is indented, and that the nested literal keeps its own fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftAndClosingQuotesOfRawStringNestedInHoleStartInColumnZero()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                """|}}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(testData, 2);
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when it shifts its lines to the left and the closing quotes of a raw string literal nested in one of its holes start in column 0 while a content line of it is indented, and that the nested literal keeps its own fix, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftAndClosingQuotesOfRawStringNestedInHoleStartInColumnZeroWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                """|}}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(NormalizeToCarriageReturnLineFeed(testData), 2);
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when shifting its lines to the left would change the value of a UTF-8 raw string literal nested in one of its holes, and that the nested literal keeps its own fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfUtf8RawStringNestedInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                    """u8|}.Length}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(testData, 2);
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when shifting its lines to the left would change the value of a raw string literal nested in a hole of a regular interpolated string in one of its holes, and that the nested raw string literal keeps its own fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftWouldChangeValueOfRawStringNestedInHoleOfNestedInterpolatedString()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {$"a{{|#1:"""
                                                                  x
                                    """|}}b"}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(testData, 2);
    }

    /// <summary>
    /// Verifies that, where the outer literal's fix is withheld, applying the fixes repeatedly or as Fix All first aligns the raw string literal nested in its hole and then the outer literal, and that the nested literal keeps the whitespace in front of its content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllFixesRawStringNestedInHoleFirstAndThenOuterLiteralKeepingNestedValue()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {"""
                                                                                        x
                                                          """}
                                                   """;
                                      }
                                  }
                                  """";

        // The second pass is guard-dependent: the fix withholds the outer literal's fix on the starting document while the
        // raw string literal nested in its hole is misaligned (RH5202-006), and offers it only once the nested literal is
        // aligned
        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 2;
                         test.NumberOfFixAllIterations = 2;
                         test.NumberOfFixAllInDocumentIterations = 2;
                         test.NumberOfFixAllInProjectIterations = 2;
                     },
                     Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that, where the outer literal's fix is withheld, applying the fixes repeatedly or as Fix All first aligns the raw string literal nested in its hole and then the outer literal, and that the nested literal keeps the whitespace in front of its content, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllFixesRawStringNestedInHoleFirstAndThenOuterLiteralKeepingNestedValueWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {"""
                                                                                        x
                                                          """}
                                                   """;
                                      }
                                  }
                                  """";

        // The second pass is guard-dependent, as in the test above (RH5202-006)
        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(resultData),
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 2;
                         test.NumberOfFixAllIterations = 2;
                         test.NumberOfFixAllInDocumentIterations = 2;
                         test.NumberOfFixAllInProjectIterations = 2;
                     },
                     Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix is offered when it shifts the lines to the left by as many columns as the whitespace in front of the closing quotes of a raw string literal nested in one of its holes is wide, and that the nested literal is still reported after the outer literal's fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsLeftWhenClosingWhitespaceOfRawStringNestedInHoleIsAsWideAsShift()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                        """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                                            x
                                  """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = testData;
        var result = resultData;

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered when it shifts the lines to the left by as many columns as the whitespace in front of the closing quotes of a raw string literal nested in one of its holes is wide, and that the nested literal is still reported after the outer literal's fix, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsLeftWhenClosingWhitespaceOfRawStringNestedInHoleIsAsWideAsShiftWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                                                  x
                                        """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                                            x
                                  """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = NormalizeToCarriageReturnLineFeed(testData);
        var result = NormalizeToCarriageReturnLineFeed(resultData);

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered when it shifts the lines to the left by more columns than the closing whitespace of a raw string literal nested in one of its holes is wide but no content line of it starts with more whitespace than its closing line, and that each of those lines loses all of its whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsLeftWhenNoContentLineOfRawStringNestedInHoleIsWiderThanItsClosingLine()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                    x
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                  x
                                  """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = testData;
        var result = resultData;

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered when it shifts the lines to the left by more columns than the closing whitespace of a raw string literal nested in one of its holes is wide but no content line of it starts with more whitespace than its closing line, and that each of those lines loses all of its whitespace, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsLeftWhenNoContentLineOfRawStringNestedInHoleIsWiderThanItsClosingLineWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                    x
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                  x
                                  """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = NormalizeToCarriageReturnLineFeed(testData);
        var result = NormalizeToCarriageReturnLineFeed(resultData);

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when shifting its lines to the left would remove whitespace that a whitespace-only content line of a raw string literal nested in one of its holes contributes to its value, and that the nested literal keeps its own fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftWouldShortenWhitespaceOnlyContentLineOfRawStringNestedInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                    x
                                ······
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(ExpandWhitespaceMarkers(testData), 2);
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when shifting its lines to the left would remove whitespace that a whitespace-only content line of a raw string literal nested in one of its holes contributes to its value, and that the nested literal keeps its own fix, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-006")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftWouldShortenWhitespaceOnlyContentLineOfRawStringNestedInHoleWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                    x
                                ······
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), 2);
    }

    /// <summary>
    /// Verifies that the fix is offered when a whitespace-only content line of a raw string literal nested in one of its holes is no wider than its closing line, and that the line loses all of its whitespace when the outer literal shifts to the left
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsLeftWhenWhitespaceOnlyContentLineOfRawStringNestedInHoleIsNoWiderThanItsClosingLine()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                    x
                                ····
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                  x

                                  """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = ExpandWhitespaceMarkers(testData);
        var result = ExpandWhitespaceMarkers(resultData);

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered when a whitespace-only content line of a raw string literal nested in one of its holes is no wider than its closing line, and that the line loses all of its whitespace when the outer literal shifts to the left, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsLeftWhenWhitespaceOnlyContentLineOfRawStringNestedInHoleIsNoWiderThanItsClosingLineWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:"""
                                    x
                                ····
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                  x

                                  """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData));
        var result = NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(resultData));

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered when it shifts the lines to the right although a content line of a raw string literal nested in one of its holes starts with more whitespace than its closing line, and that every line of the nested literal moves by the same amount
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsRightWhenContentLineOfRawStringNestedInHoleIsWiderThanItsClosingLine()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {{|#1:"""
                                                                  x
                                    """|}}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                                                                 x
                                                   """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = testData;
        var result = resultData;

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered when it shifts the lines to the right although a content line of a raw string literal nested in one of its holes starts with more whitespace than its closing line, and that every line of the nested literal moves by the same amount, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsRightWhenContentLineOfRawStringNestedInHoleIsWiderThanItsClosingLineWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {{|#1:"""
                                                                  x
                                    """|}}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $"""
                                                   Hello {{|#0:"""
                                                                                 x
                                                   """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = NormalizeToCarriageReturnLineFeed(testData);
        var result = NormalizeToCarriageReturnLineFeed(resultData);

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered when it shifts the lines to the left by more columns than the closing whitespace of an interpolated raw string literal nested in one of its holes is wide, and a line of the nested literal that starts with more whitespace than its closing line starts in one of the nested literal's own holes rather than in its text, and that the nested literal is still reported after the outer literal's fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsLeftWhenWiderLineOfInterpolatedRawStringNestedInHoleStartsInItsOwnHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int n)
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:$"""
                                    {n
                                              + 1}
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(int n)
                                      {
                                          var a = $"""
                                                   Hello {{|#0:$"""
                                  {n
                                        + 1}
                                  """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = testData;
        var result = resultData;

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered when it shifts the lines to the left by more columns than the closing whitespace of an interpolated raw string literal nested in one of its holes is wide, and a line of the nested literal that starts with more whitespace than its closing line starts in one of the nested literal's own holes rather than in its text, and that the nested literal is still reported after the outer literal's fix, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    [Requirement("RH5202-006")]
    public async Task VerifyFixShiftsLeftWhenWiderLineOfInterpolatedRawStringNestedInHoleStartsInItsOwnHoleWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int n)
                                    {
                                        var a = {|#0:$"""
                                                         Hello {{|#1:$"""
                                    {n
                                              + 1}
                                    """|}}
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(int n)
                                      {
                                          var a = $"""
                                                   Hello {{|#0:$"""
                                  {n
                                        + 1}
                                  """|}}
                                                   """;
                                      }
                                  }
                                  """";

        var source = NormalizeToCarriageReturnLineFeed(testData);
        var result = NormalizeToCarriageReturnLineFeed(resultData);

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the requirement's example in a document whose line breaks are all a lone carriage return: the aligned
    /// literal is not reported and the misaligned interpolated literal is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyAlignedLiteralIsNotReportedAndMisalignedLiteralIsReportedWithLoneCarriageReturns()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello
                                                """;
                                        var b = {|#0:$"""
                                            Hello {1}
                                            """|};
                                    }
                                }
                                """";

        await Verify(ConvertEndOfLine(testData, "\r"), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that aligned plain, UTF-8, and interpolated literals in a document whose line breaks are all a lone
    /// carriage return are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyAlignedLiteralsAreNotReportedWithLoneCarriageReturns()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello
                                                """;
                                        var b = """
                                                Hello
                                                """u8;
                                        var c = $"""
                                                 Hello {1}
                                                 """;
                                    }
                                }
                                """";

        await Verify(ConvertEndOfLine(testData, "\r"));
    }

    /// <summary>
    /// Verifies the requirement's fix example in a document whose line breaks are all a lone carriage return: one
    /// application aligns the literal, the analyzer no longer reports it, and the literal's lone carriage returns stay
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedInterpolatedLiteralIsFixedInOneApplicationWithLoneCarriageReturns()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var b = {|RH5202:$"""
                                            Hello {1}
                                            """|};
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M()
                                     {
                                         var b = $"""
                                                  Hello {1}
                                                  """;
                                     }
                                 }
                                 """";

        await VerifyCodeFixWithEndOfLine(testData, fixedData, "\r");
    }

    /// <summary>
    /// Verifies that a misaligned plain literal in a document whose line breaks are all a lone carriage return is aligned
    /// in one application, and that the analyzer no longer reports it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedPlainLiteralIsFixedInOneApplicationWithLoneCarriageReturns()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:"""
                                            Hello
                                            """|};
                                    }
                                }
                                """";
        const string fixedData = """"
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

        await VerifyCodeFixWithEndOfLine(testData, fixedData, "\r");
    }

    /// <summary>
    /// Verifies that the formatter's output for a document whose line breaks are all a lone carriage return, which keeps
    /// the lone carriage returns inside the literals, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyFormatterOutputForLoneCarriageReturnDocumentIsNotReported()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var a = """
                                          Hello
                                          """;
                                      var b = $"""
                                          Hello {1}
                                          """;
                                  }
                              }
                              """";

        await Verify(Format(ConvertEndOfLine(source, "\r")));
    }

    /// <summary>
    /// Verifies that aligned literals whose line breaks are U+0085 in an otherwise LF document are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyAlignedLiteralsAreNotReportedWithNextLineBreaksInTheLiteral()
    {
        await Verify(CreateAlignedLiteralsWithLiteralLineBreak("\u0085"));
    }

    /// <summary>
    /// Verifies that aligned literals whose line breaks are U+2028 in an otherwise LF document are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyAlignedLiteralsAreNotReportedWithLineSeparatorsInTheLiteral()
    {
        await Verify(CreateAlignedLiteralsWithLiteralLineBreak("\u2028"));
    }

    /// <summary>
    /// Verifies that aligned literals whose line breaks are U+2029 in an otherwise LF document are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyAlignedLiteralsAreNotReportedWithParagraphSeparatorsInTheLiteral()
    {
        await Verify(CreateAlignedLiteralsWithLiteralLineBreak("\u2029"));
    }

    /// <summary>
    /// Verifies that misaligned plain and interpolated literals whose line breaks are U+0085 are aligned in one
    /// application, keep their line breaks, and are no longer reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedLiteralsAreFixedWithNextLineBreaksInTheLiteral()
    {
        await VerifyMisalignedLiteralsWithLiteralLineBreak("\u0085");
    }

    /// <summary>
    /// Verifies that misaligned plain and interpolated literals whose line breaks are U+2028 are aligned in one
    /// application, keep their line breaks, and are no longer reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedLiteralsAreFixedWithLineSeparatorsInTheLiteral()
    {
        await VerifyMisalignedLiteralsWithLiteralLineBreak("\u2028");
    }

    /// <summary>
    /// Verifies that misaligned plain and interpolated literals whose line breaks are U+2029 are aligned in one
    /// application, keep their line breaks, and are no longer reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedLiteralsAreFixedWithParagraphSeparatorsInTheLiteral()
    {
        await VerifyMisalignedLiteralsWithLiteralLineBreak("\u2029");
    }

    #endregion // Tests
}