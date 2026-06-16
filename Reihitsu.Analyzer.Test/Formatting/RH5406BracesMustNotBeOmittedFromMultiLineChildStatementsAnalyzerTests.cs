using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer"/> and <see cref="RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests : AnalyzerTestsBase<RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer, RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider>
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
                                        if (true)
                                            {|#0:Other(1,
                                                  2);|}
                                    }
                                
                                    void Other(int value1, int value2)
                                    {
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
                                             Other(1,
                                                   2);
                                         }
                                     }
                                 
                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH5406MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is fixed without deleting the header when the multi-line child starts on the parent's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsFixedWhenChildStartsOnParentLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|#0:Other(1,
                                                  2);|}
                                    }

                                    void Other(int value1, int value2)
                                    {
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
                                             Other(1,
                                                   2);
                                         }
                                     }

                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH5406MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line brace-less child statement is not flagged by RH5406 (it is reported by RH5405),
    /// so the two rules never report the same statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineChildStatementIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multi-line brace-less else child statement is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineElseChildStatementIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x)
                                    {
                                        if (x)
                                        {
                                        }
                                        else
                                            {|#0:Other(1,
                                                  2);|}
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(bool x)
                                     {
                                         if (x)
                                         {
                                         }
                                         else
                                         {
                                             Other(1,
                                                   2);
                                         }
                                     }

                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH5406MessageFormat));
    }

    #endregion // Tests
}