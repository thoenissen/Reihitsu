using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5205StatementLambdaOpeningBraceShouldBeAlignedFormatterTests : FormatterTestsBase<RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns statement lambda opening braces with the lambda anchor
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
                                     Func<object, object> projector = obj =>
                                                                                       {
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

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, 8, 59, 8, 60, MessageFormat));
    }

    /// <summary>
    /// Verifies that a lambda block brace behind a comment that starts its line in the column of the lambda is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5205-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyBraceBehindLeadingCommentInLambdaColumnIsNotReported()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = x =>
                                                           /* body */ {
                                                               return x;
                                                           };
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that a block lambda whose body holds an anonymous method aligned under <c>delegate</c>, in a raw-string hole and outside one, is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public async Task VerifyAnonymousMethodInLambdaBodyAlignedUnderDelegateIsNotReportedAndStaysAsWritten()
    {
        const string source = """"
                              class C
                              {
                                  int M(System.Func<int> f)
                                  {
                                      var a = $"""
                                               G {M(() =>
                                                    {
                                                        return M(delegate
                                                                 {
                                                                     return 1;
                                                                 });
                                                    })}
                                               """;

                                      return M(() =>
                                               {
                                                   return M(delegate
                                                            {
                                                                return 1;
                                                            });
                                               });
                                  }
                              }
                              """";

        await VerifyFormatter(source);
    }

    #endregion // Tests
}