using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5031RegionDirectivesShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer, RH5031RegionDirectivesShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that region directives preceded by blank lines do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDirectivesArePrecededByBlankLines()
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
    /// Verifies that a missing blank line before a region directive is detected and fixed
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

        await Verify(testData, fixedData, Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing blank line before an end-region directive is detected and fixed
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

        await Verify(testData, fixedData, Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat));
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

    /// <summary>
    /// Verifies that active regions adjacent to formattable literal and comment lines are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormattableLiteralAndCommentAdjacencyIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private string _text = "value";
                                    {|#0:#region Text|}

                                    #endregion

                                    /* comment */
                                    {|#1:#region Comment|}

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private string _text = "value";

                                     #region Text

                                     #endregion

                                     /* comment */

                                     #region Comment

                                     #endregion
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a syntax tree without region directives remains analyzer-clean
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDirectiveTreeDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private string _value = "#region is literal content";
                                    /* #endregion is comment content */
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private string _ordinary = "value";
                                    {|#0:#region Ordinary|}

                                    #endregion

                                    private System.ReadOnlySpan<byte> Utf8 => "value"u8;
                                    {|#1:#region Utf8|}

                                    #endregion

                                    private string _interpolated = $"{1}";
                                    {|#2:#region Interpolated|}

                                    #endregion

                                    /** <summary>Documentation.</summary> */
                                    {|#3:#region Documentation|}

                                    #endregion

                                #if false
                                    disabled text
                                #endif
                                    {|#4:#region DisabledText|}

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private string _ordinary = "value";

                                     #region Ordinary

                                     #endregion

                                     private System.ReadOnlySpan<byte> Utf8 => "value"u8;

                                     #region Utf8

                                     #endregion

                                     private string _interpolated = $"{1}";

                                     #region Interpolated

                                     #endregion

                                     /** <summary>Documentation.</summary> */

                                     #region Documentation

                                     #endregion

                                 #if false
                                     disabled text
                                 #endif

                                     #region DisabledText

                                     #endregion
                                 }
                                 """;

        // Verifies literal variants, delimited documentation, and disabled-text boundaries do not suppress active regions
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5031MessageFormat, 5),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}