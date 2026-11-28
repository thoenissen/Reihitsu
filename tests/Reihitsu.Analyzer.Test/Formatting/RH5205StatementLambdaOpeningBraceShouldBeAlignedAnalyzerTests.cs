using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer"/> and <see cref="RH5205StatementLambdaOpeningBraceShouldBeAlignedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzerTests : BatchCodeFixTestsBase<RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer, RH5205StatementLambdaOpeningBraceShouldBeAlignedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that an aligned simple statement lambda does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    public async Task VerifyNoDiagnosticsWhenSimpleStatementLambdaBraceIsAligned()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, object> projector = obj =>
                                                                         {
                                                                             return obj;
                                                                         };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an aligned parenthesized statement lambda does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    public async Task VerifyNoDiagnosticsWhenParenthesizedStatementLambdaBraceIsAligned()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, int, object> projector = (obj, index) =>
                                                                              {
                                                                                  return obj;
                                                                              };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that expression-bodied lambdas are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    public async Task VerifyNoDiagnosticsWhenLambdaIsExpressionBodied()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, object> projector = obj => obj;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a misaligned simple statement lambda brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    [Requirement("RH5205-002")]
    [Requirement("RH5205-003")]
    public async Task VerifySimpleStatementLambdaBraceMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, object> projector = obj =>
                                                                                 {|#0:{|}
                                                                                     return obj;
                                                                                 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, object> projector = obj =>
                                                                          {
                                                                              return obj;
                                                                          };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned parenthesized statement lambda brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    [Requirement("RH5205-002")]
    [Requirement("RH5205-003")]
    public async Task VerifyParenthesizedStatementLambdaBraceMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, int, object> projector = (obj, index) =>
                                                                                              {|#0:{|}
                                                                                                  return obj;
                                                                                              };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, int, object> projector = (obj, index) =>
                                                                               {
                                                                                   return obj;
                                                                               };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned async simple statement lambda brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    [Requirement("RH5205-002")]
    [Requirement("RH5205-003")]
    public async Task VerifyAsyncSimpleStatementLambdaBraceMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, Task<object>> projector = async obj =>
                                                                                           {|#0:{|}
                                                                                               return await Task.FromResult(obj);
                                                                                           };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.Threading.Tasks;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, Task<object>> projector = async obj =>
                                                                                {
                                                                                    return await Task.FromResult(obj);
                                                                                };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned async parenthesized statement lambda brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    [Requirement("RH5205-002")]
    [Requirement("RH5205-003")]
    public async Task VerifyAsyncParenthesizedStatementLambdaBraceMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, int, Task<object>> projector = async (obj, index) =>
                                                                                                         {|#0:{|}
                                                                                                             return await Task.FromResult(obj);
                                                                                                         };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.Threading.Tasks;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, int, Task<object>> projector = async (obj, index) =>
                                                                                     {
                                                                                         return await Task.FromResult(obj);
                                                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that an opening brace on the line of the arrow is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    public async Task VerifyNoDiagnosticsWhenBraceIsOnArrowLine()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    internal void Method()
                                    {
                                        Func<int, int> value = x => {
                                            return x;
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix aligns the lambda body under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    [Requirement("RH5205-002")]
    [Requirement("RH5205-003")]
    public async Task VerifyParenthesizedLambdaBraceIsFixedWithCrLf()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    internal void Method()
                                    {
                                        Func<int, int> value = (x) =>
                                        {|#0:{|}
                                            return x;
                                        };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class TestClass
                                 {
                                     internal void Method()
                                     {
                                         Func<int, int> value = (x) =>
                                                                {
                                                                    return x;
                                                                };
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a lambda that carries an attribute list of its own is reported, and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-004")]
    public async Task NoFixIsOfferedForALambdaWithAnAttributeList()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> c = [Obsolete] (x) =>
                                        {|RH5205:{|}
                                            return x;
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              using System;

                              internal class Example
                              {
                                  internal void Method()
                                  {
                                      Action outer = () =>
                                      {
                                          Func<object, object> inner = obj =>
                                          {
                                              return obj;
                                          };
                                      };
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that a lambda block brace behind a comment that starts its line outside the column of the lambda is reported, although the brace behind the comment reaches that column, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    [Requirement("SHARED-COMMENT-011")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyBraceBehindMisplacedLeadingCommentIsReported(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = x =>
                                                   /* b */ {|RH5205:{|}
                                                               return x;
                                                           };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<int, int> f = x =>
                                                            /* b */ {
                                                                return x;
                                                            };
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix, which formats the misaligned lambda, aligns the block of an anonymous method inside the lambda's body under <c>delegate</c>, measured after the lambda's block moved
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-INDENT-031")]
    public async Task VerifyFixAlignsTheBlockOfAnAnonymousMethodInTheLambdaBodyUnderDelegate(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    object M(System.Func<object> f)
                                    {
                                        return M(() =>
                                            {|RH5205:{|}
                                                return M(delegate { return 1; });
                                            });
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     object M(System.Func<object> f)
                                     {
                                         return M(() =>
                                                  {
                                                      return M(delegate
                                                               {
                                                                   return 1;
                                                               });
                                                  });
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix, which formats the misaligned lambda, puts a comment in front of the closing brace of an interpolation of a regular interpolated string in the lambda's body in the column of that brace
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-INDENT-006")]
    public async Task VerifyFixPutsACommentInFrontOfTheClosingBraceOfARegularInterpolationInTheBraceColumn(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string N(System.Func<string> f, int a)
                                    {
                                        return N(() =>
                                            {|RH5205:{|}
                                                return $"{
                                                    a
                                                    // f
                                                    }";
                                            }, a);
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     string N(System.Func<string> f, int a)
                                     {
                                         return N(() =>
                                                  {
                                                      return $"{
                                                          a

                                                          // f
                                                          }";
                                                  }, a);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Action outer = () =>
                                        {|#0:{|}
                                            Func<object, object> inner = obj =>
                                            {|#1:{|}
                                                return obj;
                                            };
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Action outer = () =>
                                                        {
                                                            Func<object, object> inner = obj =>
                                                                                         {
                                                                                             return obj;
                                                                                         };
                                                        };
                                     }
                                 }
                                 """;

        // The outer lambda's misaligned brace is fixed by reformatting the whole outer lambda expression, whose
        // span fully contains the inner lambda flagged by the second diagnostic, so the batch fixer discards the
        // overlapping inner change while the outer rewrite already re-aligns the nested lambda's brace too
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}