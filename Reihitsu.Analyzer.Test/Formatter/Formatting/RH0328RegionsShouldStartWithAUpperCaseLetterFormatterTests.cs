using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer"/>
/// </summary>
[TestClass]
public class RH0328RegionsShouldStartWithAUpperCaseLetterFormatterTests : FormatterTestsBase<RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter capitalizes lowercase region names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 #region fields
                                 private readonly int _value;
                                 #endregion // fields
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Fields
                                     private readonly int _value;

                                     #endregion // Fields
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, 3, 5, 3, 19, AnalyzerResources.RH0328MessageFormat));
    }

    #endregion // Members
}