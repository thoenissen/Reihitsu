using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer"/>
/// </summary>
[TestClass]
public class RH8301DocumentationLinesMustBeginWithSingleSpaceFormatterTests : FormatterTestsBase<RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
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

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that formatter-preserved nested-list indentation remains analyzer-clean
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterPreservesNestedListIndentation()
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

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that the formatter replaces a non-breaking-space separator and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesNonBreakingSpaceViolation()
    {
        const string testDataWithNonBreakingSpaceMarker = """
                                                          internal class TestClass
                                                          {
                                                              {|#0:///|}{NBSP}Summary.
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
        var testData = testDataWithNonBreakingSpaceMarker.Replace("{NBSP}", "\u00A0");

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes a whitespace-only non-breaking-space suffix and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesWhitespaceOnlyNonBreakingSpaceViolation()
    {
        const string testDataWithNonBreakingSpaceMarker = """
                                                          internal class TestClass
                                                          {
                                                              {|#0:///|} {NBSP}
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
        var testData = testDataWithNonBreakingSpaceMarker.Replace("{NBSP}", "\u00A0");

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter fixes content and whitespace-only non-breaking space on indented continuation lines across supported line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesIndentedContinuationsAcrossSupportedLineEndings()
    {
        const string testDataWithNonBreakingSpaceMarker = """
                                                          internal class TestClass
                                                          {
                                                              /// <summary>
                                                              {|#0:///|}{NBSP}Summary.
                                                              {|#1:///|} {NBSP}
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
                                     /// Summary.
                                     ///
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;
        var testData = testDataWithNonBreakingSpaceMarker.Replace("{NBSP}", "\u00A0");

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat, 2));
    }

    #endregion // Tests
}