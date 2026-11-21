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
/// Test methods for <see cref="RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider"/> on the lines inside the holes of an interpolated literal: strings, comments, and raw strings nested in a hole
/// </summary>
[TestClass]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests : BatchCodeFixTestsBase<RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer, RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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
                                                                                                       .WithMessage(MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
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

        await Verify(testData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));

        TestFileMarkupParser.GetSpans(testData, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
        TestFileMarkupParser.GetSpans(resultData, out var plainResult, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await ApplyCodeFixAsync(plainSource);

        Assert.AreEqual(plainResult, fixedSource);

        await Verify(resultData, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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
    /// Verifies the specification example: a raw string nested in a hole stands aligned in characters behind nine spaces and two tabs, so it is not reported, and no code fix is offered for the reported outer literal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-008")]
    public async Task VerifyFixIsWithheldWhenUnreportedRawStringNestedInHoleIsIndentedWithTabs()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:$"""
                                    Outer {"""
                                         →→inner
                                         →→"""}
                                    """|};
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies the specification example: a raw string nested in a hole stands behind two tabs and is reported; no code fix is offered for the outer literal, the nested literal's fix moves its lines nine columns to the right and writes their indentation as spaces, and the outer literal's fix is then offered and moves every line 13 columns to the right
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-008")]
    public async Task VerifyFixIsWithheldWhenReportedRawStringNestedInHoleIsIndentedWithTabsAndOfferedAfterTheNestedFix()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Outer {{|#1:"""
                                →→inner
                                →→"""|}}
                                    """|};
                                    }
                                }
                                """";
        const string intermediateData = """"
                                        class C
                                        {
                                            void M()
                                            {
                                                var a = {|#0:$"""
                                            Outer {"""
                                                   inner
                                                   """}
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
                                                   Outer {"""
                                                          inner
                                                          """}
                                                   """;
                                      }
                                  }
                                  """";

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var source = ConvertEndOfLine(ExpandWhitespaceMarkers(testData), endOfLine);
            var intermediate = ConvertEndOfLine(intermediateData, endOfLine);
            var result = ConvertEndOfLine(resultData, endOfLine);

            await VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(source, 2);

            TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);
            TestFileMarkupParser.GetSpans(intermediate, out var plainIntermediate, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

            var nestedFixed = await TryApplyCodeFixAtAsync(plainSource, RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, root => GetRawStringLiterals(root).Last().GetLocation());

            Assert.AreEqual(plainIntermediate, nestedFixed, "The nested literal's fix must write its indentation as spaces.");

            await Verify(intermediate, result, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
        }
    }

    /// <summary>
    /// Verifies that the incremental and the Fix All application of the code fix both end at the formatter's layout when the outer literal's fix is withheld until the fix of a tab-indented raw string nested in its hole has run
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-008")]
    public async Task VerifyEveryFixApplicationConvergesWhenRawStringNestedInHoleIsIndentedWithTabs()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|#0:$"""
                                    Outer {{|#1:"""
                                →→inner
                                →→"""|}}
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
                                                   Outer {"""
                                                          inner
                                                          """}
                                                   """;
                                      }
                                  }
                                  """";

        // The outer literal's fix is withheld until the nested literal's fix has run, so Fix All needs one pass for the nested literal and one for the outer literal
        await Verify(ExpandWhitespaceMarkers(testData),
                     resultData,
                     static config => config.NumberOfFixAllIterations = 2,
                     Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when a raw string nested in a hole of an interpolated literal nested in its hole, two levels deep, is indented with tabs, while neither nested literal is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-008")]
    public async Task VerifyFixIsWithheldWhenRawStringNestedTwoLevelsDeepIsIndentedWithTabs()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:$"""
                                    Outer {$"""
                                            Mid {"""
                                               →→inner
                                               →→"""}
                                            """}
                                    """|};
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when an interpolated raw string nested in its hole is indented with tabs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-008")]
    public async Task VerifyFixIsWithheldWhenInterpolatedRawStringNestedInHoleIsIndentedWithTabs()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:$"""
                                    Outer {$"""
                                          →→{1}
                                          →→"""}
                                    """|};
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that no code fix is offered for the outer literal when a UTF-8 raw string nested in its hole is indented with tabs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-008")]
    public async Task VerifyFixIsWithheldWhenUtf8RawStringNestedInHoleIsIndentedWithTabs()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:$"""
                                    Outer {"""
                                         →→x
                                         →→"""u8.Length}
                                    """|};
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that no code fix is offered for an outer literal that would move to the left when a raw string nested in its hole is indented with tabs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-008")]
    public async Task VerifyFixIsWithheldWhenShiftingLeftAndRawStringNestedInHoleIsIndentedWithTabs()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:$"""
                                                      Outer {"""
                                                           →→inner
                                                           →→"""}
                                                      """|};
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that the code fix is offered and shifts the lines of a raw string nested in a hole when the whitespace in front of its closing quotes holds no tab, and that the tab behind that width on its content line stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-007")]
    [Requirement("RH5202-008")]
    public async Task VerifyFixIsOfferedAndKeepsTheTabOfRawStringNestedInHoleWithoutATabInFrontOfItsClosingQuotes()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5202:$"""
                                    Outer {"""
                                           →inner
                                           """}
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
                                                   Outer {"""
                                                          →inner
                                                          """}
                                                   """;
                                      }
                                  }
                                  """";

        await VerifyCodeFixWithEndOfLine(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(resultData), "\n");
        await VerifyCodeFixWithEndOfLine(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(resultData), "\r\n");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Verifies that the analyzer reports exactly the diagnostics marked in the source, that the code fix registers no
    /// action for the outer literal, the first raw string literal of the document, and that it registers one action
    /// for the raw string literal nested in it, the last raw string literal of the document
    /// </summary>
    /// <param name="source">Source text with markup for every expected diagnostic</param>
    /// <param name="diagnosticCount">The number of expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered(string source, int diagnosticCount)
    {
        await Verify(source, Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, diagnosticCount));

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var outerActions = await GetCodeFixActionsAsync(plainSource, RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, root => GetRawStringLiterals(root).First().GetLocation());

        Assert.IsEmpty(outerActions, "No code fix may be offered for the outer literal.");

        var nestedActions = await GetCodeFixActionsAsync(plainSource, RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, root => GetRawStringLiterals(root).Last().GetLocation());

        Assert.HasCount(1, nestedActions, "The code fix must be offered for the nested literal.");
    }

    /// <summary>
    /// Gets the multi-line raw string literal expressions of a syntax tree in document order
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The raw string literal expressions</returns>
    private static IEnumerable<ExpressionSyntax> GetRawStringLiterals(SyntaxNode root)
    {
        foreach (var node in root.DescendantNodes())
        {
            if (node is LiteralExpressionSyntax literal
                && (literal.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
                    || literal.Token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken)))
            {
                yield return literal;
            }
            else if (node is InterpolatedStringExpressionSyntax interpolated
                     && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken))
            {
                yield return interpolated;
            }
        }
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