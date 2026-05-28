using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer"/> and <see cref="RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzerTests : AnalyzerTestsBase<RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer, RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider>
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
                                    void Method()
                                    {
                                        int[] values = new[] { 1 };
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
                                    void Method()
                                    {
                                        int[] values = new{|#0: |}[] { 1 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] values = new[] { 1 };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer.DiagnosticId, AnalyzerResources.RH6022MessageFormat));
    }

    #endregion // Tests
}