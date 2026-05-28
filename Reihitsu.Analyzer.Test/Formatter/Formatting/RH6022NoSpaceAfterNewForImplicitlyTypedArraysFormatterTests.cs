using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer"/>
/// </summary>
[TestClass]
public class RH6022NoSpaceAfterNewForImplicitlyTypedArraysFormatterTests : FormatterTestsBase<RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer>
{
    #region Tests

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
                                 Diagnostics(RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer.DiagnosticId, AnalyzerResources.RH6022MessageFormat));
    }

    #endregion // Tests
}