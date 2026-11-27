using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5203MultiLineArgumentsShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5203MultiLineArgumentsShouldBeAlignedFormatterTests : FormatterTestsBase<RH5203MultiLineArgumentsShouldBeAlignedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns wrapped arguments to a shared anchor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System;

                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Console.WriteLine("first",
                                       "second",
                                             "third");
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Console.WriteLine("first",
                                                           "second",
                                                           "third");
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, 8, 11, 8, 19, MessageFormat),
                              ExpectedDiagnostic(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, 9, 17, 9, 24, MessageFormat));
    }

    /// <summary>
    /// Verifies that an argument behind a comment that starts its line in the alignment column is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5203-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyArgumentBehindLeadingCommentInAlignmentColumnIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    void N()
                                    {
                                        M(1,
                                          /* two */ 2);
                                    }

                                    void M(params int[] values)
                                    {
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that an anonymous method argument whose block aligns under <c>delegate</c>, followed by another argument, is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public async Task VerifyAnonymousMethodArgumentsAlignedUnderDelegateAreNotReportedAndStayAsWritten()
    {
        const string source = """
                              using System;
                              using System.Threading.Tasks;

                              class C
                              {
                                  object M(Func<int> f, int y = 0)
                                  {
                                      var x = M(delegate
                                                {
                                                    return 1;
                                                },
                                                2);
                                      Func<int> s = static delegate
                                                           {
                                                               return 1;
                                                           };
                                      Func<Task<int>> t = async delegate
                                                          {
                                                              return 1;
                                                          };
                                      Func<int, int> d = delegate (int a)
                                                         {
                                                             return a;
                                                         };
                                      Func<int> g;
                                      g = delegate
                                          {
                                              return 1;
                                          };

                                      return M(delegate
                                               {
                                                   return 1;
                                               });
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    #endregion // Tests
}