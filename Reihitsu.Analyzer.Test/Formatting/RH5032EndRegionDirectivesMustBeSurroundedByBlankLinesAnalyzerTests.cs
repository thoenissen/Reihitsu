using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer"/> and <see cref="RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzerTests : AnalyzerTestsBase<RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer, RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that an end-region directive surrounded by blank lines does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenEndRegionIsSurroundedByBlankLines()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Helpers

                                    private int _b;

                                    #endregion

                                    private int _c;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an end-region directive directly before the closing brace does not require a blank line after it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenEndRegionHugsClosingBrace()
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
    /// Verifies that a missing blank line before the end-region directive is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingBlankLineBeforeEndRegionIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Helpers

                                    private int _b;
                                    {|#0:#endregion|}

                                    private int _c;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Helpers

                                     private int _b;

                                     #endregion

                                     private int _c;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing blank line after the end-region directive is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingBlankLineAfterEndRegionIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Helpers

                                    private int _b;

                                    {|#0:#endregion|}
                                    private int _c;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Helpers

                                     private int _b;

                                     #endregion

                                     private int _c;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat));
    }

    /// <summary>
    /// Verifies that missing blank lines before and after the end-region directive are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingBlankLinesAroundEndRegionAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Helpers

                                    private int _b;
                                    {|#0:#endregion|}
                                    private int _c;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Helpers

                                     private int _b;

                                     #endregion

                                     private int _c;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat));
    }

    /// <summary>
    /// Verifies that an end-region directive that hugs the closing brace still requires a blank line before it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEndRegionHuggingClosingBraceStillRequiresBlankLineBefore()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Helpers

                                    private int _b;
                                    {|#0:#endregion|}
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

        await Verify(testData, fixedData, Diagnostics(RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat));
    }

    /// <summary>
    /// Verifies that end-region directives inside disabled code do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDisabledCodeEndRegionDoesNotProduceDiagnostics()
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