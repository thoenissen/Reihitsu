using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer, RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a throw statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForThrowStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                using System;

                                internal class RH5013
                                {
                                    public void Execute(bool isInvalid)
                                    {
                                        if (isInvalid)
                                        {
                                            var message = "invalid";
                                            {|#0:throw|} new InvalidOperationException(message);
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class RH5013
                                 {
                                     public void Execute(bool isInvalid)
                                     {
                                         if (isInvalid)
                                         {
                                             var message = "invalid";
 
                                             throw new InvalidOperationException(message);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5013MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a throw statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForThrowStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                using System;

                                internal class RH5013
                                {
                                    public void Execute(bool isInvalid)
                                    {
                                        if (isInvalid)
                                        {
                                            var message = "invalid";

                                            throw new InvalidOperationException(message);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a throw statement follows a case label
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForThrowStatementAfterCaseLabel()
    {
        const string testCode = """
                                using System;

                                internal class RH5013
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0:
                                                throw new InvalidOperationException();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a throw statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForThrowStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                using System;

                                internal class RH5013
                                {
                                    public void Execute(bool isInvalid)
                                    {
                                        if (isInvalid)
                                        {
                                            var message = "invalid";
                                            // Comment before throw
                                            throw new InvalidOperationException(message);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}