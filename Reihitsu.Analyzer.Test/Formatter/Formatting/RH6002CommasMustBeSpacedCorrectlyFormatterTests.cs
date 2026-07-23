using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6002CommasMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6002CommasMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6002CommasMustBeSpacedCorrectlyAnalyzer>
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
                                    void Method(int x{|#0:,|}int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes a space before a comma and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesSpaceBeforeComma()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x {|#0:,|} int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses multiple spaces after a comma and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultipleSpacesAfterComma()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x{|#0:,|}  int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    #endregion // Tests
}