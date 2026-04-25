using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0338PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer"/> and <see cref="RH0338PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0338PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzerTests : AnalyzerTestsBase<RH0338PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer, RH0338PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider>
{
    /// <summary>
    /// Verifies that clean code does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                #pragma warning disable CS0168
                                internal class TestClass
                                {
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
                                {|#0:    |}#pragma warning disable CS0168
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 #pragma warning disable CS0168
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0338PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH0338MessageFormat));
    }

    /// <summary>
    /// Verifies that region directives do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyRegionsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    void Method()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that raw-string content does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyRawStringContentDoesNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    private const string Value = """
                                                                 #pragma warning disable CS0168
                                                                 #endregion
                                                                 """;
                                }
                                """";

        await Verify(testData);
    }
}