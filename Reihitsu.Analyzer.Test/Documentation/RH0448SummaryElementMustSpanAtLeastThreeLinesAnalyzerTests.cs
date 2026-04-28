using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer"/> and <see cref="RH0448SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzerTests : AnalyzerTestsBase<RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer, RH0448SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider>
{
    /// <summary>
    /// Verifies that a correctly formatted three-line summary does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForCorrectThreeLineSummary()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Summary text
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-line summary is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineSummaryIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>Summary text</summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Summary text
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0448MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line summary with inline XML content is detected and fixed without breaking the content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineSummaryWithInlineXmlIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>Uses <see cref="string"/> values</summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Uses <see cref="string"/> values
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0448MessageFormat));
    }

    /// <summary>
    /// Verifies that a multiline summary with multiple content lines does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForMultilineSummaryWithMultipleContentLines()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// First line
                                    /// Second line
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a two-line summary with content on start-tag line produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForTwoLineSummaryWithContentOnStartTagLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>Summary text
                                    /// </summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Summary text
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0448MessageFormat));
    }

    /// <summary>
    /// Verifies that an empty single-line summary is detected and fixed to the three-line form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptySingleLineSummaryIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary></summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// 
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH0448MessageFormat));
    }
}