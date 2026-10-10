using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer"/> and <see cref="RH8301DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzerTests : BatchCodeFixTestsBase<RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer, RH8301DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// Summary.
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:///|}Summary.
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// Summary.
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that indentation used for nested list content does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedListIndentationDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Supported modes:
                                    /// - Standard
                                    ///   - Fast
                                    ///   - Safe
                                    /// - Advanced
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that raw strings containing documentation-like text do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       ///Not documentation
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab after the required space is treated as content indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabContentIndentationDoesNotProduceDiagnostics()
    {
        const string testDataWithTabMarker = """
                                             internal class TestClass
                                             {
                                                 /// <summary>
                                                 /// {TAB}Summary.
                                                 /// </summary>
                                                 void Method()
                                                 {
                                                 }
                                             }
                                             """;
        var testData = testDataWithTabMarker.Replace("{TAB}", "\t");

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that fixing an invalid separator preserves the indentation which follows it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInvalidSeparatorFixPreservesContentIndentation()
    {
        const string testDataWithTabMarker = """
                                             internal class TestClass
                                             {
                                                 /// <summary>
                                                 {|#0:///|}{TAB}  - Fast
                                                 /// </summary>
                                                 void Method()
                                                 {
                                                 }
                                             }
                                             """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     ///   - Fast
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;
        var testData = testDataWithTabMarker.Replace("{TAB}", "\t");

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that extra whitespace without content is detected and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWhitespaceOnlySuffixIsDetectedAndFixed()
    {
        const string testDataWithWhitespaceMarker = """
                                                    internal class TestClass
                                                    {
                                                        {|#0:///|}{WHITESPACE}
                                                        void Method()
                                                        {
                                                        }
                                                    }
                                                    """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     ///
                                     void Method()
                                     {
                                     }
                                 }
                                 """;
        var testData = testDataWithWhitespaceMarker.Replace("{WHITESPACE}", "  ");

        await Verify(testData, fixedData, Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that documentation-like text in multi-line comments does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineCommentsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /*
                                    ///Not documentation
                                    */
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that documentation-like text in disabled regions does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDisabledTextDoesNotProduceDiagnostics()
    {
        const string testData = """
                                #if false
                                ///Not documentation
                                #endif
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that documentation lines are ignored when documentation mode is disabled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    ///Summary.
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    {|#0:///|}Summary.
                                    {|#1:///|}</summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Summary.
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        // Verifies that continuation lines are detected and fixed together
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}