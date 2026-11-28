using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer"/>
/// </summary>
[TestClass]
public class RH5204IndentationMustUseFourSpacesPerScopeLevelFormatterTests : FormatterTestsBase<RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter normalizes indentation to four spaces per level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                               internal bool Value { get; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Value { get; }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, 3, 3, 3, 11, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter indents a misindented delimited documentation comment on its own line below an opening brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5204-005")]
    public async Task VerifyFormatterFixesDelimitedDocumentationCommentOnOwnLine()
    {
        const string input = """
                             internal class Example
                             {
                             /** Doc */
                                 internal bool Value { get; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /** Doc */
                                     internal bool Value { get; }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, 3, 1, 3, 11, MessageFormat));
    }

    /// <summary>
    /// Verifies that an opening brace followed by a delimited documentation comment on the same line is analyzer-clean and
    /// formatter-stable, because the comment does not start its line and so does not decide that line's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOpeningBraceWithDelimitedDocumentationBehindItIsStable()
    {
        const string source = """
                              internal class Example
                              { /** Doc */
                                  internal bool Value { get; }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that a switch label followed by a delimited documentation comment on the same line is analyzer-clean and
    /// formatter-stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCaseLabelWithDelimitedDocumentationBehindItIsStable()
    {
        const string source = """
                              internal class Example
                              {
                                  internal int Method(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1: /** Doc */
                                              return 2;
                                      }

                                      return 0;
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter leaves region directives inside a branch the compiler excluded exactly where the
    /// author wrote them. Together with the analyzer's matching carve-out this pins the parity that was missing while
    /// the analyzer re-indented what the formatter declined to touch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesRegionDirectivesInsideInactiveBranchUntouched()
    {
        const string source = """
                              internal class Example
                              {
                              #if false
                              #region Disabled
                              #endregion // Disabled
                              #endif

                                  internal bool Value => true;
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter moves a closing brace that starts its line behind a documentation comment written behind
    /// code to the column of its type, which clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public async Task VerifyFormatterFixesClosingBraceBehindStrayDocumentationComment()
    {
        const string input = """
                             internal class C
                             {
                                 internal int A; /// <summary>end</summary>
                                       }
                             """;
        const string fixedData = """
                                 internal class C
                                 {
                                     internal int A; /// <summary>end</summary>
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, 4, 11, 4, 12, MessageFormat));
    }

    /// <summary>
    /// Verifies that anonymous method blocks aligned under their anchor in a field, an object initializer, a conditional branch, a switch-expression arm, a collection expression, behind <c>+=</c>, in a chain, and nested are not reported and stay as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public async Task VerifyAnonymousMethodBlocksAlignedUnderTheirAnchorAreNotReportedAndStayAsWritten()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;
                              using System.Linq;

                              class D
                              {
                                  public Func<int> F { get; set; }
                              }

                              class C
                              {
                                  private Func<int> _f = delegate
                                                         {
                                                             return 1;
                                                         };

                                  event EventHandler E;

                                  int M(Func<int> f, int k = 0)
                                  {
                                      var d1 = new D
                                               {
                                                   F = delegate
                                                       {
                                                           return 1;
                                                       }
                                               };
                                      Func<int> c1 = k > 0
                                                         ? delegate
                                                           {
                                                               return 1;
                                                           }
                                                         : null;
                                      var a = k switch
                                              {
                                                  0 => M(delegate
                                                         {
                                                             return 1;
                                                         }),
                                                  _ => 2,
                                              };
                                      Func<int>[] arr = [
                                                            delegate
                                                            {
                                                                return 1;
                                                            },
                                                            null
                                                        ];
                                      E += delegate
                                           {
                                               Console.WriteLine();
                                           };

                                      var q = new List<int>().Select(delegate (int b)
                                                                     {
                                                                         return b;
                                                                     }).Where(delegate (int b)
                                                                              {
                                                                                  return b > 0;
                                                                              });

                                      return M(delegate
                                               {
                                                   return M(delegate
                                                            {
                                                                return 2;
                                                            });
                                               });
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that comments in the column of the closing brace of an interpolation of a regular, a verbatim, or a single-line raw interpolated string, also one nested in a hole of a multi-line raw string literal, are not reported and stay as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public async Task VerifyCommentsInTheBraceColumnOfInterpolationsAreNotReportedAndStayAsWritten()
    {
        const string source = """"
                              class C
                              {
                                  string M(int a, int b)
                                  {
                                      var t = $"{
                                          // e
                                          a

                                          // f
                                          }";
                                      var u = $"{a
                                                 + b

                                          // f
                                          }";
                                      var v = $@"{
                                          a

                                          // f
                                          }";
                                      var s = $"""{
                                          a

                                          // f
                                          }""";
                                      var n = $"""
                                               {$"{
                                                   a

                                                   // f
                                                   }"}
                                               """;

                                      return t + u + v + s + n;
                                  }
                              }
                              """";

        await VerifyFormatter(source);
    }

    #endregion // Tests
}