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
                              ExpectedDiagnostic(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, 8, 11, 8, 19, AnalyzerResources.RH5203MessageFormat),
                              ExpectedDiagnostic(RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, 9, 17, 9, 24, AnalyzerResources.RH5203MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's layout for a leading comma kept by a comment is stable and not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLeadingCommaLayoutIsStableAndNotReported()
    {
        const string source = """
                              using System;

                              internal class Example
                              {
                                  internal void Method()
                                  {
                                      Console.WriteLine("first" // keep
                                                        , "second");
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    #endregion // Tests
}