using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer>
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
                                 Diagnostics(RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6020MessageFormat));
    }

    #endregion // Tests
}