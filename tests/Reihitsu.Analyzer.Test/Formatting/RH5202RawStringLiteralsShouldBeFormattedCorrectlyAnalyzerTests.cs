using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider"/>: what is reported, the shift the fix writes, the whitespace of the closing line, and the line breaks of the document and the literal
/// </summary>
[TestClass]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer, RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that correctly aligned raw string literals do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyValidRawStringLiteralsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void ValidSingleLine()
                                    {
                                        var a = """Test""";
                                    }

                                    void ValidMultiLineAligned()
                                    {
                                        var a = """
                                                Test
                                                """;
                                    }

                                    void ValidInterpolatedAligned()
                                    {
                                        var x = 1;

                                        var a = $"""
                                                 {x} Test
                                                 """;
                                    }

                                    void ValidDoubleInterpolatedAligned()
                                    {
                                        var x = 1;

                                        var a = $$"""
                                                  {{x}} Test
                                                  """;
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that correctly aligned raw string literals with four quotes do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyValidFourQuoteRawStringLiteralsDoNotProduceDiagnostics()
    {
        const string testData = """""
                                using System;

                                internal class RH5202
                                {
                                    void ValidFourQuotes()
                                    {
                                        var a = """"
                                                Test """
                                                """";
                                    }
                                }
                                """"";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that misaligned raw string literals are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-002")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedRawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedClosingLess()
                                    {
                                        var a = {|#0:"""
                                    Test
                                    """|};
                                    }

                                    void MisalignedClosingMore()
                                    {
                                        var b = {|#1:"""
                                                        Test
                                                        """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedClosingLess()
                                      {
                                          var a = """
                                                  Test
                                                  """;
                                      }

                                      void MisalignedClosingMore()
                                      {
                                          var b = """
                                                  Test
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that misaligned raw string literals with four quotes are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-002")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedFourQuoteRawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """""
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedFourQuotes()
                                    {
                                        var a = {|#0:""""
                                    Test """
                                    """"|};
                                    }
                                }
                                """"";

        const string resultData = """""
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedFourQuotes()
                                      {
                                          var a = """"
                                                  Test """
                                                  """";
                                      }
                                  }
                                  """"";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that misaligned interpolated raw string literals are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-002")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedInterpolatedRawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedInterpolated()
                                    {
                                        var x = 1;

                                        var a = {|#0:$"""
                                    {x} Test
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedInterpolated()
                                      {
                                          var x = 1;

                                          var a = $"""
                                                   {x} Test
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that misaligned double-dollar interpolated raw string literals are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-002")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedDoubleInterpolatedRawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedDoubleInterpolated()
                                    {
                                        var x = 1;

                                        var a = $$"""
                                    {{x}} Test
                                    """;
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedDoubleInterpolated()
                                      {
                                          var x = 1;

                                          var a = $$"""
                                                    {{x}} Test
                                                    """;
                                      }
                                  }
                                  """";

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.TestState.MarkupHandling = MarkupMode.None;
                         test.FixedState.MarkupHandling = MarkupMode.None;
                     },
                     Diagnostic(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId).WithSpan(9, 17, 11, 8)
                                                                                                       .WithMessage(MessageFormat));
    }

    /// <summary>
    /// Verifies that a raw string literal with multiple content lines is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedMultiLineContentRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MultiLineContent()
                                    {
                                        var a = {|#0:"""
                                    Line 1
                                    Line 2
                                    Line 3
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MultiLineContent()
                                      {
                                          var a = """
                                                  Line 1
                                                  Line 2
                                                  Line 3
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a correctly aligned UTF-8 (<c>u8</c>) raw string literal does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    public async Task VerifyValidUtf8RawStringLiteralsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void ValidUtf8MultiLineAligned()
                                    {
                                        var a = """
                                                Test
                                                """u8;
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a misaligned UTF-8 (<c>u8</c>) raw string literal is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-002")]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedUtf8RawStringLiteralsAreDetectedAndFixed()
    {
        const string testData = """"
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedUtf8()
                                    {
                                        var a = {|#0:"""
                                    Test
                                    """u8|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedUtf8()
                                      {
                                          var a = """
                                                  Test
                                                  """u8;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies the rule whose subject is a raw string literal checks how the literal spells its value and reports the
    /// literal whose closing quotes are not in the column of its opening quotes, while an aligned literal is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyMisalignedRawStringIsReportedBecauseTheLiteralIsTheRuleSubject()
    {
        const string testData = """"
                                class C
                                {
                                    string s = {|#0:"""
                                        a
                                        """|};

                                    string t = """
                                               a
                                               """;
                                }
                                """";

        await Verify(testData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies a raw string literal whose closing quotes are not in the column of its opening quotes is reported and an
    /// aligned one is not, when the document uses CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyMisalignedRawStringIsReportedBecauseTheLiteralIsTheRuleSubjectWithCarriageReturnLineFeed()
    {
        const string testData = """"
                                class C
                                {
                                    string s = {|#0:"""
                                        a
                                        """|};

                                    string t = """
                                               a
                                               """;
                                }
                                """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts every line after the first by the same amount, so the relative indentation of the
    /// content lines is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-001")]
    [Requirement("RH5202-003")]
    public async Task VerifyRelativeIndentationOfContentLinesIsKept()
    {
        const string testData = """"
                                internal class RH5202
                                {
                                    void RelativeIndentation()
                                    {
                                        var a = {|#0:"""
                                            Test
                                              indented
                                            """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  internal class RH5202
                                  {
                                      void RelativeIndentation()
                                      {
                                          var a = """
                                                  Test
                                                    indented
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix aligns the literal under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedRawStringLiteralIsFixedWithCrLf()
    {
        const string testData = """"
                                internal class RH5202
                                {
                                    void Misaligned()
                                    {
                                        var a = {|#0:"""
                                            Test
                                            """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  internal class RH5202
                                  {
                                      void Misaligned()
                                      {
                                          var a = """
                                                  Test
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty content line empty when it shifts the other lines to the right
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyContentLineEmpty()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                    one

                                    two
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
                                                  one

                                                  two
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty content line empty when it shifts the other lines to the right, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyContentLineEmptyWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                    one

                                    two
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
                                                  one

                                                  two
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty content line of an interpolated literal empty
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyContentLineOfInterpolatedLiteralEmpty()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int name)
                                    {
                                        var a = {|#0:$"""
                                    one

                                    two {name}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(int name)
                                      {
                                          var a = $"""
                                                   one

                                                   two {name}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty content line of an interpolated literal empty, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyContentLineOfInterpolatedLiteralEmptyWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int name)
                                    {
                                        var a = {|#0:$"""
                                    one

                                    two {name}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M(int name)
                                      {
                                          var a = $"""
                                                   one

                                                   two {name}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves an empty content line empty when it shifts the other lines to the left
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-005")]
    public async Task VerifyFixLeavesEmptyContentLineEmptyWhenShiftingLeft()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                                        one

                                                        two
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
                                                  one

                                                  two
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a content line that holds only whitespace, because such a line is not empty
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-005")]
    public async Task VerifyFixShiftsWhitespaceOnlyContentLine()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""
                                    one
                                ······
                                    two
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
                                                  one
                                  ··················
                                                  two
                                                  """;
                                      }
                                  }
                                  """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out a literal with a verbatim string in a hole as the formatter lays it out, in both
    /// directions of the shift
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixMatchesFormatterForVerbatimStringInHole()
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

    #region Methods

    /// <summary>
    /// Creates an LF document holding aligned plain, UTF-8, and interpolated multi-line raw string literals whose own line
    /// breaks are <paramref name="literalLineBreak"/>
    /// </summary>
    /// <param name="literalLineBreak">Line break inside the literals</param>
    /// <returns>The document text</returns>
    private static string CreateAlignedLiteralsWithLiteralLineBreak(string literalLineBreak)
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var a = """¶
                                              Hello¶
                                              """;
                                      var b = """¶
                                              Hello¶
                                              """u8;
                                      var c = $"""¶
                                               Hello {1}¶
                                               """;
                                  }
                              }
                              """";

        return ConvertEndOfLine(source, "\n").Replace("¶\n", literalLineBreak);
    }

    /// <summary>
    /// Verifies that misaligned plain and interpolated multi-line raw string literals whose own line breaks are
    /// <paramref name="literalLineBreak"/>, in an LF document, are reported, are aligned in one application that keeps
    /// those line breaks, and are not reported afterwards
    /// </summary>
    /// <param name="literalLineBreak">Line break inside the literals</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyMisalignedLiteralsWithLiteralLineBreak(string literalLineBreak)
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:"""¶
                                            Hello¶
                                            """|};
                                        var b = {|#1:$"""¶
                                            Hello {1}¶
                                            """|};
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M()
                                     {
                                         var a = """¶
                                                 Hello¶
                                                 """;
                                         var b = $"""¶
                                                  Hello {1}¶
                                                  """;
                                     }
                                 }
                                 """";

        await Verify(ConvertEndOfLine(testData, "\n").Replace("¶\n", literalLineBreak),
                     ConvertEndOfLine(fixedData, "\n").Replace("¶\n", literalLineBreak),
                     Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """"
                                using System;

                                internal class RH5202
                                {
                                    void MisalignedClosingLess()
                                    {
                                        var a = {|#0:"""
                                    Test
                                    """|};
                                    }

                                    void MisalignedClosingMore()
                                    {
                                        var b = {|#1:"""
                                                        Test
                                                        """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  using System;

                                  internal class RH5202
                                  {
                                      void MisalignedClosingLess()
                                      {
                                          var a = """
                                                  Test
                                                  """;
                                      }

                                      void MisalignedClosingMore()
                                      {
                                          var b = """
                                                  Test
                                                  """;
                                      }
                                  }
                                  """";

        // Each fix rewrites the exact text span of one raw string literal expression, and one raw string literal
        // can never nest inside another (its content is atomic character data, not a syntax tree), so the two
        // literals here can never share or abut a text span; interference is structurally unreachable for this
        // rule's fix, so this proves both occurrences are corrected independently
        return new FixAllScenario(testCode,
                                  resultData,
                                  Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}