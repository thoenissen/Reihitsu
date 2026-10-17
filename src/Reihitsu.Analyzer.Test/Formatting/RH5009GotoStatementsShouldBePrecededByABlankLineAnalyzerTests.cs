using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer, RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a goto statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5009-001")]
    [Requirement("RH5009-002")]
    public async Task VerifyDiagnosticForGotoStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:goto|} Done;

                                        Done:
                                        value++;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5009
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         goto Done;

                                         Done:
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a goto case statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5009-001")]
    [Requirement("RH5009-002")]
    public async Task VerifyDiagnosticForGotoCaseStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0:
                                                value++;
                                                {|#0:goto|} case 1;
                                            case 1:
                                                break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5009
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 0:
                                                 value++;

                                                 goto case 1;
                                             case 1:
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a goto default statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5009-001")]
    [Requirement("RH5009-002")]
    public async Task VerifyDiagnosticForGotoDefaultStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0:
                                                value++;
                                                {|#0:goto|} default;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5009
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 0:
                                                 value++;

                                                 goto default;
                                             default:
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a goto statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5009-001")]
    public async Task VerifyNoDiagnosticForGotoStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        var value = 1;

                                        goto Done;

                                        Done:
                                        value++;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a goto statement follows a label
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-002")]
    public async Task VerifyNoDiagnosticForGotoStatementAfterLabel()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        Start:
                                        goto Done;

                                        Done:
                                        return;
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
    [Requirement("RH5009-001")]
    public async Task VerifyDiagnosticForGotoStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        // Comment before goto
                                        {|#0:goto|} Done;

                                        Done:
                                        value++;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5009
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         // Comment before goto
                                         goto Done;

                                         Done:
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat));
    }

    /// <summary>
    /// Verifies a goto to a label, a goto case, and a goto default that each end a switch section after another statement are reported on their goto keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5009-001")]
    [Requirement("RH5009-002")]
    public async Task VerifyDiagnosticForGotoLabelGotoCaseAndGotoDefaultEndingSwitchSections()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                a++;
                                                {|#0:goto|} case 2;
                                            case 2:
                                                a++;
                                                {|#1:goto|} default;
                                            default:
                                                a++;
                                                {|#2:goto|} End;
                                        }

                                        End:
                                        a--;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         switch (a)
                                         {
                                             case 1:
                                                 a++;

                                                 goto case 2;
                                             case 2:
                                                 a++;

                                                 goto default;
                                             default:
                                                 a++;

                                                 goto End;
                                         }

                                         End:
                                         a--;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat, 3));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:goto|} First;

                                        First:
                                        value++;
                                        {|#1:goto|} Second;

                                        Second:
                                        value++;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5009
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         goto First;

                                         First:
                                         value++;

                                         goto Second;

                                         Second:
                                         value++;
                                     }
                                 }
                                 """;

        // Two independent goto statements, each directly following an unrelated statement, so the second fix's
        // insertion point is unaffected by the first occurrence's own blank-line insertion
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}