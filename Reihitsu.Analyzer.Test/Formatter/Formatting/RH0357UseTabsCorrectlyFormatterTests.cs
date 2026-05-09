using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0357UseTabsCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0357UseTabsCorrectlyFormatterTests : FormatterTestsBase<RH0357UseTabsCorrectlyAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        var testData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}{{|#0:\t|}}void Method(){Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}}}";
        var fixedData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}    void Method(){Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}}}";

        await VerifyFormatterFix(testData, fixedData, Diagnostics(RH0357UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0357MessageFormat));
    }

    #endregion // Members
}