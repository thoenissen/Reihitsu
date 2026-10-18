using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer"/> and <see cref="RH5027WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer, RH5027WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider>
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
                                        do
                                        {
                                        }
                                        while (true);
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
    [Requirement("RH5027-001")]
    [Requirement("RH5027-003")]
    [Requirement("RH5027-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        do
                                        {
                                        }
                                {|#0:
                                |}        while (true);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         do
                                         {
                                         }
                                         while (true);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5027MessageFormat));
    }

    /// <summary>
    /// Verifies that regular while statements do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWhileStatementsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        while (true)
                                        {

                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the blank line above a do statement's footer is reported when the footer begins its line and when it
    /// follows the body's closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5027-001")]
    [Requirement("RH5027-003")]
    public async Task VerifyBlankLineAboveDoStatementFooterIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        do
                                        {
                                            a++;
                                        }
                                {|RH5027:
                                |}        while (a < 3);

                                        do
                                        {
                                            a++;
                                {|RH5027:
                                |}        } while (a < 4);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the blank line above a while statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5027-002")]
    public async Task VerifyBlankLineAboveWhileStatementIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;

                                        while (a < 5)
                                        {
                                            a++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix deletes the reported blank line and nothing else
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5027-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesBlankLineAboveDoStatementFooter()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        do
                                        {
                                            a++;
                                        }
                                {|RH5027:
                                |}        while (a < 3);
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         do
                                         {
                                             a++;
                                         }
                                         while (a < 3);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a whitespace-only line above a do statement's footer is reported and deleted together with its CRLF line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5027-001")]
    [Requirement("RH5027-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceOnlyLineAboveDoStatementFooterWithCarriageReturnLineFeed()
    {
        const string testCode = "class C\n{\n    void M(int a)\n    {\n        do\n        {\n            a++;\n        }\n        \n        while (a < 3);\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M(int a)\n    {\n        do\n        {\n            a++;\n        }\n        while (a < 3);\n    }\n}\n";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode), actual);
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
                                        do
                                        {
                                        }
                                {|#0:
                                |}        while (true);
                                        do
                                        {
                                        }
                                {|#1:
                                |}        while (false);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         do
                                         {
                                         }
                                         while (true);
                                         do
                                         {
                                         }
                                         while (false);
                                     }
                                 }
                                 """;

        // Two adjacent do statements, each with a blank line directly before its trailing while footer, so the
        // second fix's removal span sits close behind the first fix's own removal
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5027MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}