using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0355ColonsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0355ColonsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0355ColonsMustBeSpacedCorrectlyAnalyzer>
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
                                internal class TestClass{|#0::|}System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass : System.IDisposable
                                 {
                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0355ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0355MessageFormat));
    }

    #endregion // Members
}