using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5405BracesMustNotBeOmittedAnalyzer"/> and <see cref="RH5405BracesMustNotBeOmittedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5405BracesMustNotBeOmittedAnalyzerTests : BatchCodeFixTestsBase<RH5405BracesMustNotBeOmittedAnalyzer, RH5405BracesMustNotBeOmittedCodeFixProvider>
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
                                            {|#0:return;|}
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
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is fixed without deleting the header when the child shares the parent's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsFixedWhenChildSharesParentLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x)
                                    {
                                        if (x) {|#0:Foo();|}
                                    }

                                    void Foo()
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
                                             Foo();
                                         }
                                     }

                                     void Foo()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line brace-less else child statement is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyElseChildStatementIsDetectedAndFixed()
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
                                            {|#0:return;|}
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
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that a multi-line brace-less child statement is not flagged by RH5405 (it is reported by RH5406),
    /// so the two rules never report the same statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineChildStatementIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                            Other(1,
                                                  2);
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an "else if" does not produce a diagnostic for the nested if-statement header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyElseIfDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x, bool y)
                                    {
                                        if (x)
                                        {
                                        }
                                        else if (y)
                                        {
                                        }
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
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method1(bool x)
                                    {
                                        if (x)
                                            {|#0:Foo();|}
                                    }

                                    void Method2(bool y)
                                    {
                                        if (y)
                                            {|#1:Bar();|}
                                    }

                                    void Foo()
                                    {
                                    }

                                    void Bar()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method1(bool x)
                                     {
                                         if (x)
                                         {
                                             Foo();
                                         }
                                     }

                                     void Method2(bool y)
                                     {
                                         if (y)
                                         {
                                             Bar();
                                         }
                                     }

                                     void Foo()
                                     {
                                     }

                                     void Bar()
                                     {
                                     }
                                 }
                                 """;

        // RH5405 only reports single-line embedded child statements; nesting a second violation inside the first
        // would make the outer child multi-line, which RH5406 owns instead, so the only way two diagnostics could
        // share an owning statement is an if/else pair - and there both branches resolve to the identical
        // if-statement node, collapsing to what the batch fixer treats as a single reformat rather than two
        // interfering ones. Two independent if-statements in different methods are used instead, because
        // interference beyond that identical-owner collapse is unreachable for this rule
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}