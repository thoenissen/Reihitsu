using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3102CodeMustNotContainEmptyStatementsAnalyzer"/> and <see cref="RH3102CodeMustNotContainEmptyStatementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3102CodeMustNotContainEmptyStatementsAnalyzerTests : BatchCodeFixTestsBase<RH3102CodeMustNotContainEmptyStatementsAnalyzer, RH3102CodeMustNotContainEmptyStatementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying empty statement in method body is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-001")]
    [Requirement("RH3102-002")]
    [Requirement("RH3102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement after valid statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-001")]
    [Requirement("RH3102-002")]
    [Requirement("RH3102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in if block is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-001")]
    [Requirement("RH3102-002")]
    [Requirement("RH3102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in while loop is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-001")]
    [Requirement("RH3102-002")]
    [Requirement("RH3102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in for loop is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-001")]
    [Requirement("RH3102-002")]
    [Requirement("RH3102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in switch case is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-001")]
    [Requirement("RH3102-002")]
    [Requirement("RH3102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying empty statement in try block is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-001")]
    [Requirement("RH3102-002")]
    [Requirement("RH3102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements."));
    }

    /// <summary>
    /// Verifying the fix is not offered for an embedded empty statement that cannot be removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-004")]
    public async Task FixIsNotOfferedForEmbeddedEmptyStatement()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run(bool condition)
                                    {
                                        if (condition)
                                            ;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<EmptyStatementSyntax>()
                                                               .Single()
                                                               .SemicolonToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying the fix is not offered when a comment is attached to the empty statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-004")]
    public async Task FixIsNotOfferedWhenCommentIsAttachedToEmptyStatement()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        ; // why
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<EmptyStatementSyntax>()
                                                               .Single()
                                                               .SemicolonToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that no code fix is offered when a documentation comment is attached to the empty statement,
    /// because removing the statement would discard it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-004")]
    public async Task VerifyDocumentationCommentedEmptyStatementIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       private static void Method()
                                       {
                                           /// <summary>Deliberate no-op placeholder.</summary>
                                           ;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<EmptyStatementSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies empty statements in a block, as the body of a loop, and after a label are reported on the semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-001")]
    [Requirement("RH3102-002")]
    public async Task EmptyStatementsInBlockLoopBodyAndLabelAreReportedOnTheSemicolon()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a, bool b)
                                    {
                                        {|RH3102:;|}
                                        while (b) {|RH3102:;|}
                                        L: {|RH3102:;|}
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix removes empty statements of a block and of a switch section together with their lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-003")]
    public async Task FixRemovesTheLinesOfEmptyStatementsInBlockAndSwitchSection()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        {|RH3102:;|}
                                        a++;
                                        switch (a)
                                        {
                                            case 1:
                                                {|RH3102:;|}
                                                break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;
                                         switch (a)
                                         {
                                             case 1:
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies embedded and labeled empty statements, and an empty statement with a trailing comment, are reported
    /// without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3102-004")]
    public async Task EmbeddedLabeledAndCommentedEmptyStatementsAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        while (b) {|RH3102:;|}
                                        for (;;) {|RH3102:;|}
                                    }

                                    void N()
                                    {
                                        L: {|RH3102:;|}
                                        {|RH3102:;|} // done
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that removing an empty statement behind another statement on the same line joins the next line
    /// to that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfEmptyStatementBehindAStatementJoinsTheNextLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++; ;
                                        a++;
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int a)
                                        {
                                            a++;         a++;
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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

        // Verifying multiple consecutive empty statements are reported and fixed
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId, "Code must not contain empty statements.", 2),
                                  config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}