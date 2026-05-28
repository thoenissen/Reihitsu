using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineFormatterTests : FormatterTestsBase<RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter moves generic constraints onto their own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example<T> where T : class
                             {
                                 public int Bar { get; set; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example<T>
                                     where T : class
                                 {
                                     public int Bar { get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, 1, 27, 1, 32, AnalyzerResources.RH5110MessageFormat));
    }

    #endregion // Tests
}