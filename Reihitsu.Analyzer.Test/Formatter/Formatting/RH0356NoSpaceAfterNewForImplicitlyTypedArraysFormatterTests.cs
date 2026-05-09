using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0356NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer"/>
/// </summary>
[TestClass]
public class RH0356NoSpaceAfterNewForImplicitlyTypedArraysFormatterTests : FormatterTestsBase<RH0356NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = new{|#0: |}[] { 1 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] values = new[] { 1 };
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0356NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer.DiagnosticId, AnalyzerResources.RH0356MessageFormat));
    }

    #endregion // Members
}