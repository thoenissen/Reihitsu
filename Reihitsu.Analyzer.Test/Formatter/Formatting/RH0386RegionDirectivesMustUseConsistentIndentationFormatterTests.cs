using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer"/>
/// </summary>
[TestClass]
public class RH0386RegionDirectivesMustUseConsistentIndentationFormatterTests : FormatterTestsBase<RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer>
{
    #region Members

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
                                 ExpectedDiagnostic(RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, 3, 1, 3, 15, AnalyzerResources.RH0386MessageFormat),
                                 ExpectedDiagnostic(RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, 7, 1, 7, 21, AnalyzerResources.RH0386MessageFormat));
    }

    #endregion // Members
}