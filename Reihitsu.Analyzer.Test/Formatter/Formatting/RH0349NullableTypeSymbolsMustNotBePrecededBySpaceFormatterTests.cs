using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0349NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer"/>
/// </summary>
[TestClass]
public class RH0349NullableTypeSymbolsMustNotBePrecededBySpaceFormatterTests : FormatterTestsBase<RH0349NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer>
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
                                    void Method(int{|#0: |}? value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int? value)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0349NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH0349MessageFormat));
    }

    #endregion // Tests
}