using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6002CommasMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6002CommasMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6002CommasMustBeSpacedCorrectlyAnalyzer>
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
                                    void Method(int x{|#0:,|}int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes a space before a comma and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesSpaceBeforeComma()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x {|#0:,|} int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses multiple spaces after a comma and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultipleSpacesAfterComma()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x{|#0:,|}  int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in front of an argument comma leaves the formatter output analyzer-clean. The
    /// horizontal spacing rewriter exempts the gap in front of such a comment from normalization, so the exemption
    /// must not be wide enough to leave a space this analyzer reports (issues #591, #625)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeCommaStaysAnalyzerClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value = System.Math.Max(1 /* c */, 2);
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that a compact interpolation-alignment comma stays unchanged and analyzer-clean (issue #696)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCompactInterpolationAlignmentCommaStaysAnalyzerClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(string value)
                                    {
                                        return $"{value,-10}";
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that the formatter removes a space after an interpolation-alignment comma, clears the
    /// analyzer diagnostic, and remains stable on a second pass (issue #696)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesSpaceAfterInterpolationAlignmentCommaAndIsIdempotent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(string value)
                                    {
                                        return $"{value{|#0:,|} -10}";
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     string Method(string value)
                                     {
                                         return $"{value,-10}";
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    #endregion // Tests
}