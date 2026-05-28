using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7101DoNotCombineFieldsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7101DoNotCombineFieldsFormatterTests : FormatterTestsBase<RH7101DoNotCombineFieldsAnalyzer>
{
    #region Tests

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
                                 Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    #endregion // Tests
}