using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6021ColonsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6021ColonsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6021ColonsMustBeSpacedCorrectlyAnalyzer, RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider>
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
                                internal class TestClass : System.IDisposable
                                {
                                    public void Dispose()
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
                                internal class TestClass{|#0::|}System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass : System.IDisposable
                                 {
                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6021MessageFormat));
    }

    #endregion // Tests
}