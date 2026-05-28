using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer"/>
/// </summary>
[TestClass]
public class RH7302RegionsShouldStartWithAUpperCaseLetterFormatterTests : FormatterTestsBase<RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer>
{
    #region Tests

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
                                 ExpectedDiagnostic(RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, 3, 5, 3, 19, AnalyzerResources.RH7302MessageFormat));
    }

    #endregion // Tests
}