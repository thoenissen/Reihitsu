using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6001KeywordsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6001KeywordsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6001KeywordsMustBeSpacedCorrectlyAnalyzer, RH6001KeywordsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        if (true)
                                        {
                                        }
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
                                        {|#0:if|}(true)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
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

        await Verify(testData, fixedData, Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat));
    }

    /// <summary>
    /// Verifies that operator-like keywords do not require a separating space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTypeofDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    System.Type Method()
                                    {
                                        return typeof(object);
                                    }
                                }
                                """;

        await Verify(testData);
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
                                        {|#0:if|}(true)
                                        {
                                        }
                                        {|#1:while|}(true)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                         }
                                         while (true)
                                         {
                                         }
                                     }
                                 }
                                 """;

        // The two keyword violations sit on adjacent lines with no blank line between them; each fix only
        // inserts a single space at its own keyword's boundary, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}