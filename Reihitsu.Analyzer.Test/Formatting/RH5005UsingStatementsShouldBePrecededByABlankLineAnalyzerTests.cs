using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer, RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a using statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForUsingStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";
                                        {|#0:using|} (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH5005
                                 {
                                     public void Execute()
                                     {
                                         var fileName = "test.txt";

                                         using (var stream = File.OpenRead(fileName))
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5005MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a using statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUsingStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";

                                        using (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a using statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUsingStatementAtStartOfBlock()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        using (var stream = File.OpenRead("test.txt"))
                                        {
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
    public async Task VerifyDiagnosticForUsingStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";
                                        // Comment before using
                                        {|#0:using|} (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH5005
                                 {
                                     public void Execute()
                                     {
                                         var fileName = "test.txt";

                                         // Comment before using
                                         using (var stream = File.OpenRead(fileName))
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5005MessageFormat));
    }

    #endregion // Tests
}