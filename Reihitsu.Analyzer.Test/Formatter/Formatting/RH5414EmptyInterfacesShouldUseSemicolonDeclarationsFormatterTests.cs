using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5414EmptyInterfacesShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that the formatter converts an empty interface to semicolon form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyInterface()
    {
        const string input = """
                             internal interface {|#0:IExample|}
                             {
                             }
                             """;
        const string fixedData = """
                                 internal interface IExample;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 null,
                                 Diagnostics(RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5414MessageFormat));
    }

    #endregion // Tests
}