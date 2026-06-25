using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5008ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer, RH5008ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a return statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForReturnStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5008
                                {
                                    public int Execute()
                                    {
                                        var value = 1;
                                        {|#0:return|} value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5008
                                 {
                                     public int Execute()
                                     {
                                         var value = 1;

                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5008MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a return statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5008
                                {
                                    public int Execute()
                                    {
                                        var value = 1;

                                        return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a return statement follows a case label
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnStatementAfterCaseLabel()
    {
        const string testCode = """
                                internal class RH5008
                                {
                                    public int Execute()
                                    {
                                        switch (1)
                                        {
                                            case 1:
                                                return 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly
    /// precedes the return statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForReturnStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5008
                                {
                                    public int Execute()
                                    {
                                        var value = 1;
                                        // Comment before return
                                        {|#0:return|} value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5008
                                 {
                                     public int Execute()
                                     {
                                         var value = 1;

                                         // Comment before return
                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5008MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a whitespace-only blank line directly precedes a comment that
    /// itself precedes the return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnStatementWhenBlankLineThenCommentPrecedesIt()
    {
        const string testCode = """
                                internal class RH5008
                                {
                                    public int Execute()
                                    {
                                        var value = 1;

                                        // Comment before return
                                        return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the line immediately preceding the return statement is a
    /// preprocessor directive, which acts as a transparent boundary rather than ordinary preceding content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnStatementDirectlyAfterPreprocessorDirective()
    {
        const string testCode = """
                                internal class RH5008
                                {
                                    public int Execute()
                                    {
                                        var value = 1;
                                #pragma warning disable CS0219
                                        return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the return statement directly follows an <c>#endif</c> directive,
    /// matching the representative case from issue #350
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnStatementDirectlyAfterEndIfDirective()
    {
        const string testCode = """
                                internal class RH5008
                                {
                                    public int Execute()
                                    {
                                        var value = 1;
                                #if true
                                        value++;
                                #endif
                                        return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}