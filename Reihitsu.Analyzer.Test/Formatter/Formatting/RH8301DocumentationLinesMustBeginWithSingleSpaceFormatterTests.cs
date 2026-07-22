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

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes non-breaking space from a documentation line and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesNonBreakingSpaceViolation()
    {
        const string testDataWithNonBreakingSpaceMarker = """
                                                          internal class TestClass
                                                          {
                                                              {|#0:///|} {NBSP}Summary.
                                                              void Method()
                                                              {
                                                              }
                                                          }
                                                          """;
        const string fixedDataWithSourceLineEndings = """
                                                      internal class TestClass
                                                      {
                                                          /// Summary.
                                                          void Method()
                                                          {
                                                          }
                                                      }
                                                      """;
        var testDataWithLineFeeds = testDataWithNonBreakingSpaceMarker.Replace("\r\n", "\n");
        var testDataWithPlatformLineEndings = testDataWithLineFeeds.Replace("\n", System.Environment.NewLine);
        var testData = testDataWithPlatformLineEndings.Replace("{NBSP}", "\u00A0");
        var fixedDataWithLineFeeds = fixedDataWithSourceLineEndings.Replace("\r\n", "\n");
        var fixedData = fixedDataWithLineFeeds.Replace("\n", System.Environment.NewLine);

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH8301MessageFormat));
    }

    #endregion // Tests
}