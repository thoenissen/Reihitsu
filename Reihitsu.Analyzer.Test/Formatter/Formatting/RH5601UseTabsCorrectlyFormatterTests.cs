using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5601UseTabsCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5601UseTabsCorrectlyFormatterTests : FormatterTestsBase<RH5601UseTabsCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        var testData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}{{|#0:\t|}}void Method(){Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}}}";
        var fixedData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}    void Method(){Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}}}";

        await VerifyFormatterFix(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat));
    }

    #endregion // Tests
}