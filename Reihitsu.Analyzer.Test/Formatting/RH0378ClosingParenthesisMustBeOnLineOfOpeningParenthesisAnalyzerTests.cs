using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer"/> and <see cref="RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzerTests : AnalyzerTestsBase<RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer, RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisCodeFixProvider>
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
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
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
                                    void Method(int first,
                                                int second
                                    {|#0:)|}
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer.DiagnosticId, AnalyzerResources.RH0378MessageFormat));
    }

    /// <summary>
    /// Verifies that methods are valid when the closing parenthesis is on the line of the last argument.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenClosingParenthesisIsOnLastArgumentLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first,
                                                int second)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed for constructors.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyConstructorIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass(int first,
                                              int second
                                    {|#0:)|}
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass(int first,
                                               int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer.DiagnosticId, AnalyzerResources.RH0378MessageFormat));
    }
}