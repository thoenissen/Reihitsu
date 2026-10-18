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
public class RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer, RH5032RegionDirectivesShouldBeFollowedByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that region directives followed by blank lines do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5032-001")]
    [Requirement("SHARED-BLANK-006")]
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
    [Requirement("RH5032-001")]
    [Requirement("SHARED-BLANK-006")]
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
    [Requirement("RH5032-001")]
    [Requirement("RH5032-002")]
    [Requirement("RH5032-003")]
    [Requirement("SHARED-BLANK-006")]
    [Requirement("SHARED-REGION-001")]
    [Requirement("SHARED-REGION-002")]
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
    [Requirement("RH5032-001")]
    [Requirement("RH5032-002")]
    [Requirement("RH5032-003")]
    [Requirement("SHARED-BLANK-006")]
    [Requirement("SHARED-REGION-001")]
    [Requirement("SHARED-REGION-002")]
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
    [Requirement("RH5032-001")]
    [Requirement("SHARED-BLANK-007")]
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
    /// Verifies that an active region, unlike an end-region, requires a blank line before a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionBeforeClosingBraceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Empty|}
                                }

                                #endregion
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Empty

                                 }

                                 #endregion
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat));
    }

    /// <summary>
    /// Verifies that an end-region at end of file remains exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5032-001")]
    [Requirement("SHARED-BLANK-006")]
    public async Task VerifyEndRegionAtEndOfFileIsExempt()
    {
        const string testData = """
                                #region Content

                                internal class TestClass
                                {
                                }

                                #endregion
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies a region or endregion directive is reported on its whole text when the line below is not blank, and an
    /// endregion directive followed by the closing brace is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5032-001")]
    [Requirement("RH5032-002")]
    [Requirement("SHARED-BLANK-006")]
    [Requirement("SHARED-REGION-001")]
    public async Task VerifyRegionDirectiveWithoutBlankLineBelowIsReportedOnItsText()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5032:#region Fields|}
                                    private int _x;

                                    {|RH5032:#endregion // Fields|}
                                    int _a;

                                    {|RH5032:#region Methods|}
                                    void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an endregion directive on the last line of the file is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5032-001")]
    [Requirement("SHARED-BLANK-006")]
    public async Task VerifyEndRegionOnLastLineOfFileIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;

                                    #region R

                                    int _b;
                                }
                                #endregion
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix inserts one empty line directly below each reported directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5032-003")]
    [Requirement("SHARED-REGION-002")]
    public async Task VerifyFixInsertsEmptyLineBelowDirective()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5032:#region Fields|}
                                    private int _x;

                                    {|RH5032:#endregion // Fields|}
                                    int _a;

                                    {|RH5032:#region Methods|}
                                    void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     #region Fields

                                     private int _x;

                                     #endregion // Fields

                                     int _a;

                                     #region Methods

                                     void M()
                                     {
                                     }

                                     #endregion // Methods
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix inserts the empty line between the directive and a comment directly below it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5032-003")]
    [Requirement("SHARED-REGION-002")]
    public async Task VerifyFixKeepsCommentBelowDirectiveInPlace()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5032:#region R|}
                                    // note
                                    int _a;

                                    #endregion
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     #region R

                                     // note
                                     int _a;

                                     #endregion
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the inserted empty line uses the document's CRLF line ending and carries no indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5032-003")]
    [Requirement("SHARED-REGION-002")]
    public async Task VerifyFixInsertsCarriageReturnLineFeedEmptyLineBelowDirective()
    {
        const string testCode = "class C\n{\n    #region R\n    int _a;\n\n    #endregion\n}\n";
        const string fixedCode = "class C\n{\n    #region R\n\n    int _a;\n\n    #endregion\n}\n";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode), actual);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                #region Outer

                                internal class First
                                {
                                    {|#0:#region Empty|}
                                }

                                #endregion

                                internal class Second
                                {
                                    {|#1:#region Members|}
                                    private int _value;

                                    #endregion
                                }

                                #endregion
                                """;
        const string fixedData = """
                                 #region Outer

                                 internal class First
                                 {
                                     #region Empty

                                 }

                                 #endregion

                                 internal class Second
                                 {
                                     #region Members

                                     private int _value;

                                     #endregion
                                 }

                                 #endregion
                                 """;

        // Verifies that Fix All converges for both the newly covered closing-brace boundary and an ordinary region boundary
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5032MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}