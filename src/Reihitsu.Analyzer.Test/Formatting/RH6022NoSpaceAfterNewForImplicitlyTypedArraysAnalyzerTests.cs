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
public class RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzerTests : BatchCodeFixTestsBase<RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer, RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider>
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

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] first = new{|#0: |}[] { 1 };
                                        int[] second = new{|#1: |}[] { 2 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] first = new[] { 1 };
                                         int[] second = new[] { 2 };
                                     }
                                 }
                                 """;

        // Two implicitly typed array creations on adjacent lines each carry their own unwanted whitespace run
        // after the new keyword; the fixes only remove their own run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer.DiagnosticId, AnalyzerResources.RH6022MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}