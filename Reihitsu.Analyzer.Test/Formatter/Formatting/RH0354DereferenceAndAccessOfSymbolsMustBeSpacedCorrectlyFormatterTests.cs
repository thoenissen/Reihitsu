using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    unsafe void Method()
                                    {
                                        int value = 0;
                                        int* pointer = &{|#0: |}value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     unsafe void Method()
                                     {
                                         int value = 0;
                                         int* pointer = &value;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0354DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0354MessageFormat));
    }

    #endregion // Tests
}