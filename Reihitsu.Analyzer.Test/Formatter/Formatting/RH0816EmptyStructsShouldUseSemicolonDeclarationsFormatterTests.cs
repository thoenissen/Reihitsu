using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0816EmptyStructsShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that the formatter converts an empty struct to semicolon form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyStruct()
    {
        const string input = """
                             internal struct {|#0:Example|}
                             {
                             }
                             """;
        const string fixedData = """
                                 internal struct Example;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 null,
                                 Diagnostics(RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH0816MessageFormat));
    }

    #endregion // Tests
}