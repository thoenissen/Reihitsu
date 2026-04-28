using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0003CodeMustNotContainEmptyStatementsAnalyzer"/> and <see cref="RH0003CodeMustNotContainEmptyStatementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0003CodeMustNotContainEmptyStatementsAnalyzerTests : AnalyzerTestsBase<RH0003CodeMustNotContainEmptyStatementsAnalyzer, RH0003CodeMustNotContainEmptyStatementsCodeFixProvider>
{
    /// <summary>
    /// Verifying empty statement in method body is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStatementInMethodBodyIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:;|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement after valid statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStatementAfterValidStatementIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        int x = 5;
                                        {|#0:;|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         int x = 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in if block is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStatementInIfBlockIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run(bool condition)
                                    {
                                        if (condition)
                                        {
                                            {|#0:;|}
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run(bool condition)
                                     {
                                         if (condition)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in while loop is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStatementInWhileLoopIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        while (true)
                                        {
                                            {|#0:;|}
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         while (true)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in for loop is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStatementInForLoopIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        for (int i = 0; i < 10; i++)
                                        {
                                            {|#0:;|}
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         for (int i = 0; i < 10; i++)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying multiple consecutive empty statements are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MultipleEmptyStatementsAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:;|}
                                        {|#1:;|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     onConfigure: config => config.NumberOfFixAllIterations = 2,
                     Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements.", 2));
    }

    /// <summary>
    /// Verifying empty statement in switch case is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStatementInSwitchCaseIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {|#0:;|}
                                                break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in try block is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStatementInTryBlockIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public void Run()
                                    {
                                        try
                                        {
                                            {|#0:;|}
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         try
                                         {
                                         }
                                         catch (Exception)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0003CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }
}