using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7301RegionsShouldMatchAnalyzer"/>
/// </summary>
[TestClass]
public class RH7301RegionsShouldMatchFormatterTests : FormatterTestsBase<RH7301RegionsShouldMatchAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter synchronizes mismatched region comments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 #region Fields
                                 private readonly int _value;
                                 #endregion // Properties
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
                                 ExpectedDiagnostic(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, 5, 5, 5, 29, AnalyzerResources.RH7301MessageFormat));
    }

    #endregion // Tests
}