using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5415EmptyRecordsShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that the formatter converts an empty record to semicolon form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyRecord()
    {
        const string input = """
                             internal record {|#0:Example|}
                             {
                             }
                             """;
        const string fixedData = """
                                 internal record Example;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 null,
                                 Diagnostics(RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5415MessageFormat));
    }

    #endregion // Tests
}