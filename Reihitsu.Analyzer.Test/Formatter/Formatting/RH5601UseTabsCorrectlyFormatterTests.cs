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

    /// <summary>
    /// Verifies that the formatter converts a tab used as the gap before a trailing comment, a position no
    /// other phase normalizes, and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTabBeforeTrailingComment()
    {
        var testData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}    void Method(){Environment.NewLine}    {{{Environment.NewLine}        var x = 1;{{|#0:\t|}}// comment{Environment.NewLine}    }}{Environment.NewLine}}}";
        var fixedData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}    void Method(){Environment.NewLine}    {{{Environment.NewLine}        var x = 1;    // comment{Environment.NewLine}    }}{Environment.NewLine}}}";

        await VerifyFormatterFix(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter converts a tab used as the keyword gap inside a #pragma directive, a
    /// position the token-level cleanup pass cannot reach directly since directive trivia is structured,
    /// and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTabInsidePragmaDirective()
    {
        var testData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}#pragma{{|#0:\t|}}warning disable CS0168{Environment.NewLine}    void Method(){Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}#pragma warning restore CS0168{Environment.NewLine}}}";
        var fixedData = $"internal class TestClass{Environment.NewLine}{{{Environment.NewLine}#pragma    warning disable CS0168{Environment.NewLine}    void Method(){Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}#pragma warning restore CS0168{Environment.NewLine}}}";

        await VerifyFormatterFix(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat));
    }

    #endregion // Tests
}