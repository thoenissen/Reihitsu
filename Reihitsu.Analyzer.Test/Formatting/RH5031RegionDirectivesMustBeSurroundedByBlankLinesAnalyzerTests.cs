using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer"/> and <see cref="RH5031RegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzerTests : AnalyzerTestsBase<RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer, RH5031RegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a region directive surrounded by blank lines does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenRegionIsSurroundedByBlankLines()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a;

                                    #region Helpers

                                    private int _b;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region directive directly after the opening brace does not require a blank line before it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenRegionHugsOpeningBrace()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Helpers

                                    private int _b;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a missing blank line before the region directive is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingBlankLineBeforeRegionIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a;
                                    {|#0:#region Helpers|}

                                    private int _b;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int _a;

                                     #region Helpers

                                     private int _b;

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing blank line after the region directive is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingBlankLineAfterRegionIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a;

                                    {|#0:#region Helpers|}
                                    private int _b;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int _a;

                                     #region Helpers

                                     private int _b;

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    /// <summary>
    /// Verifies that missing blank lines before and after the region directive are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingBlankLinesAroundRegionAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a;
                                    {|#0:#region Helpers|}
                                    private int _b;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int _a;

                                     #region Helpers

                                     private int _b;

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    /// <summary>
    /// Verifies that a region directive that hugs the opening brace still requires a blank line after it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionHuggingOpeningBraceStillRequiresBlankLineAfter()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Helpers|}
                                    private int _b;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Helpers

                                     private int _b;

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    /// <summary>
    /// Verifies that region directives inside disabled code do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDisabledCodeRegionDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if false
                                    private int _a;
                                    #region Helpers
                                    private int _b;
                                    #endregion
                                #endif
                                    private int _c;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}