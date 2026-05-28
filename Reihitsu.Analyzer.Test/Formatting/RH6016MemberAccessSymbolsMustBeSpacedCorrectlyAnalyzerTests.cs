using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer, RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        _ = string.Empty;
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
                                        _ = string {|#0:.|}Empty;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that wrapped method chains do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMethodChainsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass Foo1()
                                    {
                                        return this;
                                    }

                                    TestClass Foo2()
                                    {
                                        return this;
                                    }

                                    void Method(TestClass value)
                                    {
                                        _ = value.Foo1()
                                                 .Foo2();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}