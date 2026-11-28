using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5027WhileDoFooterMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines before do-while footers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                                 {
                                     do
                                     {
                                     }
                             {|#0:
                             |}        while (true);
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         do
                                         {
                                         }
                                         while (true);
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between an own-line comment and a do statement's <c>while</c>, so
    /// its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindCommentBeforeDoStatementWhile()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0)
                                     {
                                         a++;
                                     }

                                     // c

                                     else
                                     {
                                         a--;
                                     }

                                     try
                                     {
                                         a++;
                                     }

                                     // t

                                     catch
                                     {
                                     }

                                     // u

                                     finally
                                     {
                                     }

                                     do
                                     {
                                         a++;
                                     }

                                     // f
                             {|#0:
                             |}        while (a > 0);
                                 }
                             }
                             """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         if (a > 0)
                                         {
                                             a++;
                                         }

                                         // c
                                         else
                                         {
                                             a--;
                                         }

                                         try
                                         {
                                             a++;
                                         }

                                         // t
                                         catch
                                         {
                                         }

                                         // u
                                         finally
                                         {
                                         }

                                         do
                                         {
                                             a++;
                                         }

                                         // f
                                         while (a > 0);
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    #endregion // Tests
}