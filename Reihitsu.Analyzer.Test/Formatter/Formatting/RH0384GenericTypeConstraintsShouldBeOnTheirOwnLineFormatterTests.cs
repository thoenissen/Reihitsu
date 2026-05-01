using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineFormatterTests : FormatterTestsBase<RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer>
{
    #region Members

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
                             }
                             """;
        const string fixedData = """
                                 internal class Example<T>
                                     where T : class
                                 {
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, 1, 27, 1, 32, AnalyzerResources.RH0384MessageFormat));
    }

    #endregion // Members
}