using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer"/> and <see cref="RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer, RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider>
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
                                {|#0:
                                |}        int value = 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5022MessageFormat));
    }

    /// <summary>
    /// Verifies that blank lines inside raw strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       {

                                                       }
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an opening brace followed by a blank line inside a multi-line comment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineCommentOpeningBraceDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /*
                                    void Disabled()
                                    {

                                        int value = 0;
                                    }
                                    */
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an opening brace followed by a blank line inside disabled code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDisabledCodeOpeningBraceDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if false
                                    void Disabled()
                                    {

                                        int value = 0;
                                    }
                                #endif
                                    void Method()
                                    {
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
                                        if (true)
                                        {
                                {|#0:
                                |}            int first = 0;
                                        }
                                        if (true)
                                        {
                                {|#1:
                                |}            int second = 0;
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
                                             int first = 0;
                                         }
                                         if (true)
                                         {
                                             int second = 0;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent if blocks, each with a blank line directly after its opening brace, so the second fix's
        // removal span sits close behind the first fix's own removal
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5022MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}