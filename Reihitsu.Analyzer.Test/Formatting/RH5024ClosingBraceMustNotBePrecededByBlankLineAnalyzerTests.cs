using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer"/> and <see cref="RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzerTests : AnalyzerTestsBase<RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer, RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider>
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
                                        int value = 0;
                                {|#0:
                                |}    }
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

        await Verify(testData, fixedData, Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that raw strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       value

                                                       }
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a closing brace with a trailing semicolon is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyClosingBraceWithTrailingSemicolonIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int[] Create()
                                    {
                                        return new[]
                                        {
                                            1,
                                {|#0:
                                |}        };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int[] Create()
                                     {
                                         return new[]
                                         {
                                             1,
                                         };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that a closing brace preceded by a blank line inside a multi-line comment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineCommentClosingBraceDoesNotProduceDiagnostics()
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
    /// Verifies that a closing brace preceded by a blank line inside disabled code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDisabledCodeClosingBraceDoesNotProduceDiagnostics()
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
}