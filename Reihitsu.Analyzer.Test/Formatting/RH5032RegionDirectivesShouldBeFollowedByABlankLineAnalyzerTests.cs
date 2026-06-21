using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer"/> and <see cref="RH5032RegionDirectivesShouldBeFollowedByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer, RH5032RegionDirectivesShouldBeFollowedByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that region directives followed by blank lines do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDirectivesAreFollowedByBlankLines()
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
    /// Verifies that an end-region directive directly before the closing brace does not require a blank line after it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenEndRegionHugsClosingBrace()
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
    /// Verifies that a missing blank line after a region directive is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingBlankLineAfterRegionIsDetectedAndFixed()
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

        await Verify(testData, fixedData, Diagnostics(RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing blank line after an end-region directive is detected and fixed
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

        await Verify(testData, fixedData, Diagnostics(RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat));
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