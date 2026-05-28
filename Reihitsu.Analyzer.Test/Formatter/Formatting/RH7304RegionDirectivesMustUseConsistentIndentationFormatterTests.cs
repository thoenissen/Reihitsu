using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer"/>
/// </summary>
[TestClass]
public class RH7304RegionDirectivesMustUseConsistentIndentationFormatterTests : FormatterTestsBase<RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns region directives with the containing scope
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                             #region Fields

                                 private string _name;

                             #endregion // Fields
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Fields

                                     private string _name;

                                     #endregion // Fields
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, 3, 1, 3, 15, AnalyzerResources.RH7304MessageFormat),
                                 ExpectedDiagnostic(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, 7, 1, 7, 21, AnalyzerResources.RH7304MessageFormat));
    }

    #endregion // Tests
}