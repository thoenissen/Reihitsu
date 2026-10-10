using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer, RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a fixed statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForFixedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];
                                        {|#0:fixed|} (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5017
                                 {
                                     private readonly byte[] data = new byte[1];

                                     public unsafe void Pin()
                                     {
                                         var buffer = new byte[4];

                                         fixed (byte* pointer = buffer)
                                         {
                                             *pointer = 1;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5017MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a fixed statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFixedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];

                                        fixed (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a fixed statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFixedStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        fixed (byte* value = data)
                                        {
                                            *value = 42;
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
    public async Task VerifyDiagnosticForFixedStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];
                                        // Comment before fixed
                                        {|#0:fixed|} (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5017
                                 {
                                     public unsafe void Pin()
                                     {
                                         var buffer = new byte[4];

                                         // Comment before fixed
                                         fixed (byte* pointer = buffer)
                                         {
                                             *pointer = 1;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5017MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];
                                        {|#0:fixed|} (byte* first = buffer)
                                        {
                                            *first = 1;
                                        }
                                        {|#1:fixed|} (byte* second = data)
                                        {
                                            *second = 2;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5017
                                 {
                                     private readonly byte[] data = new byte[1];

                                     public unsafe void Pin()
                                     {
                                         var buffer = new byte[4];

                                         fixed (byte* first = buffer)
                                         {
                                             *first = 1;
                                         }

                                         fixed (byte* second = data)
                                         {
                                             *second = 2;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent fixed statements, each missing its preceding blank line, so the second fix's insertion
        // point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5017MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}