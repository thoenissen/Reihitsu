using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer, RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a checked statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5015-001")]
    [Requirement("RH5015-002")]
    public async Task VerifyDiagnosticForCheckedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5015
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        {|#0:checked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5015
                                 {
                                     public int Execute(int value)
                                     {
                                         var factor = 2;

                                         checked
                                         {
                                             return value * factor;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5015MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a checked statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5015-001")]
    public async Task VerifyNoDiagnosticForCheckedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5015
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;

                                        checked
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a checked statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5015-001")]
    public async Task VerifyNoDiagnosticForCheckedStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5015
                                {
                                    public int Execute(int value)
                                    {
                                        checked
                                        {
                                            return value + 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5015-001")]
    public async Task VerifyDiagnosticForCheckedStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5015
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        // Comment before checked
                                        {|#0:checked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5015
                                 {
                                     public int Execute(int value)
                                     {
                                         var factor = 2;

                                         // Comment before checked
                                         checked
                                         {
                                             return value * factor;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5015MessageFormat));
    }

    /// <summary>
    /// Verifies a checked statement after a statement is reported on its checked keyword while an unchecked statement and a checked expression are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5015-001")]
    [Requirement("RH5015-002")]
    public async Task VerifyDiagnosticForCheckedStatementButNotForUncheckedStatementOrCheckedExpression()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        {|#0:checked|}
                                        {
                                            a++;
                                        }

                                        a++;
                                        unchecked
                                        {
                                            a--;
                                        }

                                        a++;
                                        a = checked(a + 1);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         checked
                                         {
                                             a++;
                                         }

                                         a++;
                                         unchecked
                                         {
                                             a--;
                                         }

                                         a++;
                                         a = checked(a + 1);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5015MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for an unchecked statement that directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5015-001")]
    public async Task VerifyNoDiagnosticForUncheckedStatementAfterStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        unchecked
                                        {
                                            a--;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a checked expression inside a statement that directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5015-001")]
    public async Task VerifyNoDiagnosticForCheckedExpressionAfterStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        a = checked(a + 1);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5015
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        {|#0:checked|}
                                        {
                                            value *= factor;
                                        }
                                        {|#1:checked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5015
                                 {
                                     public int Execute(int value)
                                     {
                                         var factor = 2;

                                         checked
                                         {
                                             value *= factor;
                                         }

                                         checked
                                         {
                                             return value * factor;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent checked statements, each missing its preceding blank line, so the second fix's insertion
        // point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5015MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}