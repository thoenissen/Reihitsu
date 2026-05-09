using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0382DoNotCombineFieldsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0382DoNotCombineFieldsFormatterTests : FormatterTestsBase<RH0382DoNotCombineFieldsAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter splits combined field declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 private int firstField, {|#0:secondField|};
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0382DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH0382MessageFormat));
    }

    #endregion // Members
}