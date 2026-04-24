using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer"/> and <see cref="RH0370ElementDocumentationHeaderMustBePrecededByBlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzerTests : AnalyzerTestsBase<RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer, RH0370ElementDocumentationHeaderMustBePrecededByBlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies that clean code does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First()
                                    {
                                    }

                                    /// <summary>
                                    /// Summary.
                                    /// </summary>
                                    void Second()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First()
                                    {
                                    }
                                    {|#0:///|} <summary>
                                    /// Summary.
                                    /// </summary>
                                    void Second()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void First()
                                     {
                                     }
                                 
                                     /// <summary>
                                     /// Summary.
                                     /// </summary>
                                     void Second()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0370MessageFormat));
    }
}