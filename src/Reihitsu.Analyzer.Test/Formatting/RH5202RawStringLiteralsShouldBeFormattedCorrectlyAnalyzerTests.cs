using System.Collections.Immutable;
using System.Threading.Tasks;

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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 1));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 1));
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
                                                                                                       .WithMessage(AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 1));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 1));
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

        await Verify(testData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of a verbatim string in an interpolation hole as written when it shifts the literal to the right
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfVerbatimStringInHoleWhenShiftingRight()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {@"a
                                b"}
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
                                                   Hello {@"a
                                  b"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of a verbatim string in an interpolation hole as written when it shifts the literal to the right, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfVerbatimStringInHoleWhenShiftingRightWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {@"a
                                b"}
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
                                                   Hello {@"a
                                  b"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of a verbatim string in an interpolation hole as written when it shifts the literal to the left
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfVerbatimStringInHoleWhenShiftingLeft()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {@"a
                                                  b"}
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
                                                   Hello {@"a
                                                    b"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of a verbatim string in an interpolation hole as written when it shifts the literal to the left, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfVerbatimStringInHoleWhenShiftingLeftWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                                         Hello {@"a
                                                  b"}
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
                                                   Hello {@"a
                                                    b"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the following line of a multi-line comment behind a token in a hole as written, and shifts the line of the hole's closing brace that follows the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesFollowingLineOfTrailingCommentInHoleAndShiftsClosingBraceLine()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = {|#0:$"""
                                    Hello {name /* a
                                  b */
                                    }
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var name = "x";
                                          var a = $"""
                                                   Hello {name /* a
                                    b */
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the following line of a multi-line comment behind a token in a hole as written, and shifts the line of the hole's closing brace that follows the comment, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesFollowingLineOfTrailingCommentInHoleAndShiftsClosingBraceLineWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = {|#0:$"""
                                    Hello {name /* a
                                  b */
                                    }
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var name = "x";
                                          var a = $"""
                                                   Hello {name /* a
                                    b */
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the following line of a multi-line comment in a hole as written when it shifts the literal to the left
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesFollowingLineOfCommentInHoleWhenShiftingLeft()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = {|#0:$"""
                                                         Hello {name /* a
                                                  b */
                                                         }
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var name = "x";
                                          var a = $"""
                                                   Hello {name /* a
                                                    b */
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix writes no whitespace onto an empty line inside a verbatim string in a hole
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesEmptyLineInsideVerbatimStringInHoleEmpty()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {@"a

                                b"}
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
                                                   Hello {@"a

                                  b"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix writes no whitespace onto an empty line inside a verbatim string in a hole, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesEmptyLineInsideVerbatimStringInHoleEmptyWithCrLf()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {@"a

                                b"}
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
                                                   Hello {@"a

                                  b"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of an interpolated verbatim string written <c>$@</c> in a hole as written, also when the line holds a hole of that string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfInterpolatedVerbatimStringInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int name)
                                    {
                                        var a = {|#0:$"""
                                    Hello {$@"a
                                b{name}"}
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
                                                   Hello {$@"a
                                  b{name}"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of an interpolated verbatim string written <c>@$</c> in a hole as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfInterpolatedVerbatimStringWrittenAtDollarInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int name)
                                    {
                                        var a = {|#0:$"""
                                    Hello {@$"a
                                b{name}"}
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
                                                   Hello {@$"a
                                  b{name}"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of a UTF-8 verbatim string in a hole as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfUtf8VerbatimStringInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {@"a
                                b"u8.Length}
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
                                                   Hello {@"a
                                  b"u8.Length}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves as written both the continuation line of a verbatim string in a hole of a nested interpolated verbatim string and the continuation line of that nested string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesLinesOfVerbatimStringInHoleOfNestedInterpolatedVerbatimString()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {$@"x{@"a
                                b"}
                                y"}
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
                                                   Hello {$@"x{@"a
                                  b"}
                                  y"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of a verbatim string in a multi-line hole of a nested regular interpolated string as written, and shifts the lines of that hole on which the verbatim string begins and after it has ended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfVerbatimStringInMultiLineHoleOfNestedRegularInterpolatedString()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {$"x{
                                    @"a
                                b"
                                    }"}
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
                                                   Hello {$"x{
                                                   @"a
                                  b"
                                                   }"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of a verbatim string passed as an argument in a hole as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfVerbatimStringArgumentInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {string.Concat("x", @"a
                                b")}
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
                                                   Hello {string.Concat("x", @"a
                                  b")}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the following line of a multi-line comment in front of a token in a hole as written, and shifts the line on which the comment begins
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesFollowingLineOfLeadingCommentInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = {|#0:$"""
                                    Hello {
                                    /* a
                                  b */ name}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var name = "x";
                                          var a = $"""
                                                   Hello {
                                                   /* a
                                    b */ name}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the following line of a <c>/** */</c> comment in a hole as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesFollowingLineOfDocumentationStyleCommentInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = {|#0:$"""
                                    Hello {name /** a
                                  b */
                                    }
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var name = "x";
                                          var a = $"""
                                                   Hello {name /** a
                                    b */
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves as written the line that starts inside a comment in a hole and holds the comment's end and the hole's closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesLineHoldingEndOfCommentAndClosingBraceOfHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = {|#0:$"""
                                    Hello {name /* a
                                  b */}
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var name = "x";
                                          var a = $"""
                                                   Hello {name /* a
                                    b */}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix leaves the continuation line of a verbatim string in a hole as written when the literal starts with two <c>$</c> characters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesContinuationLineOfVerbatimStringInHoleOfDoubleDollarLiteral()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $$"""
                                    Hello {{@"a
                                b"}}
                                    """;
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var a = $$"""
                                                    Hello {{@"a
                                  b"}}
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
                     Diagnostic(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId).WithSpan(5, 17, 8, 8)
                                                                                                       .WithMessage(AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a content line that follows the line on which a verbatim string in a hole ends
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixShiftsContentLineAfterVerbatimStringInHoleHasEnded()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {@"a
                                b"} tail
                                    next
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
                                                   Hello {@"a
                                  b"} tail
                                                   next
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts the line that follows a single-line comment in a hole, because it does not start inside the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixShiftsLineAfterSingleLineCommentInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = {|#0:$"""
                                    Hello {name // a
                                    }
                                    """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var name = "x";
                                          var a = $"""
                                                   Hello {name // a
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a line of a hole that follows a verbatim string ending on the line before
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixShiftsLineAfterSingleLineVerbatimStringInHole()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Hello {@"a" +
                                    @"b"}
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
                                                   Hello {@"a" +
                                                   @"b"}
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix, also applied to every diagnostic of the document at once, leaves the lines of text nested in the holes of several literals as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixLeavesNestedTextAsWrittenForSeveralLiteralsInOneDocument()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = {|#0:$"""
                                    Hello {@"a
                                b"}
                                    """|};
                                        var b = {|#1:$"""
                                                         Hello {name /* a
                                                  b */
                                                         }
                                                         """|};
                                    }
                                }
                                """";

        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var name = "x";
                                          var a = $"""
                                                   Hello {@"a
                                  b"}
                                                   """;
                                          var b = $"""
                                                   Hello {name /* a
                                                    b */
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts a content line that holds only whitespace, because such a line is not empty
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts the lines of a raw string literal nested in a hole with the outer literal, and that
    /// the nested literal is still reported after the outer literal's fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
    public async Task VerifyFixShiftsLinesOfRawStringNestedInHoleAndNestedLiteralStaysReported()
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

        await Verify(testData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));

        TestFileMarkupParser.GetSpans(testData, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(resultData, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData.Replace('¤', '\u00A0'), resultData.Replace('¤', '\u00A0'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData.Replace('¤', '\u00A0')), NormalizeToCarriageReturnLineFeed(resultData.Replace('¤', '\u00A0')), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData.Replace('¤', '\f'), resultData.Replace('¤', '\f'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData.Replace('¤', '\v'), resultData.Replace('¤', '\v'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData.Replace('¤', '\u2003'), resultData.Replace('¤', '\u2003'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData.Replace('¤', '\u00A0'), resultData.Replace('¤', '\u00A0'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData.Replace('¤', '\u00A0')), NormalizeToCarriageReturnLineFeed(resultData.Replace('¤', '\u00A0')), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData.Replace('¤', '\u00A0'), resultData.Replace('¤', '\u00A0'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData.Replace('¤', '\u00A0')), NormalizeToCarriageReturnLineFeed(resultData.Replace('¤', '\u00A0')), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData.Replace('¤', '\u00A0'), resultData.Replace('¤', '\u00A0'), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts an empty line inside an interpolation hole, outside any nested text, like every other line of the hole, because only an empty content line stays empty
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-005")]
    public async Task VerifyFixShiftsEmptyLineInsideHole()
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
                                  ·············
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix shifts an empty line inside an interpolation hole, outside any nested text, like every other line of the hole, because only an empty content line stays empty, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    [Requirement("RH5202-005")]
    public async Task VerifyFixShiftsEmptyLineInsideHoleWithCrLf()
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
                                  ·············
                                                   }
                                                   """;
                                      }
                                  }
                                  """";

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(resultData)), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
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

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the outer literal leaves an empty content line of a raw string literal nested in one of its holes empty when it shifts the literal to the right
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
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

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the outer literal leaves an empty content line of a raw string literal nested in one of its holes empty when it shifts the literal to the right, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
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

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the outer literal leaves an empty content line of a raw string literal nested in one of its holes empty when it shifts the literal to the left
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
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

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the outer literal leaves an empty content line of a raw string literal nested in one of its holes empty when it shifts the literal to the left, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-004")]
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

        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(result, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat));
    }

    #endregion // Tests

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
                                  Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5202MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}