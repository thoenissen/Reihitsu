using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer"/>
/// </summary>
[TestClass]
public class RH6004PreprocessorKeywordsMustNotBePrecededBySpaceFormatterTests : FormatterTestsBase<RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer>
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
                                {|#0:    |}#pragma warning disable CS0168
                                internal class TestClass;
                                """;
        const string fixedData = """
                                 #pragma warning disable CS0168
                                 internal class TestClass;
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6004MessageFormat));
    }

    #endregion // Tests
}