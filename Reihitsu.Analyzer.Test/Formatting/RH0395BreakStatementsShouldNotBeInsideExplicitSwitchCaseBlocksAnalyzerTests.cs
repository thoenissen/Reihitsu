using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer"/> and <see cref="RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzerTests : AnalyzerTestsBase<RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer, RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a switch case block ends with a break statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenBreakStatementIsInsideExplicitSwitchCaseBlock()
    {
        const string testCode = """
                                internal class RH0395
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    Consume();
                                                    {|#0:break|};
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0395
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 {
                                                     Consume();
                                                 }
                                                 break;
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH0395MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps comments attached to the moved break statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixPreservesCommentsWhenMovingBreakStatement()
    {
        const string testCode = """
                                internal class RH0395
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    Consume();
                                                    // Exit this case
                                                    {|#0:break|}; // trailing comment
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0395
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 {
                                                     Consume();
                                                 }
                                                 // Exit this case
                                                 break; // trailing comment
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH0395MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix does not reformat unrelated switch section layout
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotReformatNonCompliantSwitchSectionLayout()
    {
        const string testCode = """
                                internal class RH0395
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                        case 1:
                                        {
                                            Consume();
                                            {|#0:break|};
                                        }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0395
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                         case 1:
                                         {
                                             Consume();
                                         }
                                         break;
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH0395MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the break statement already follows the explicit switch case block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenBreakStatementFollowsExplicitSwitchCaseBlock()
    {
        const string testCode = """
                                internal class RH0395
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    Consume();
                                                }
                                                break;
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for break statements inside nested loop blocks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenBreakStatementBelongsToNestedLoop()
    {
        const string testCode = """
                                internal class RH0395
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    while (true) 
                                                    {
                                                        break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered when the break statement is not the last statement in the explicit switch case block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoCodeFixWhenBreakStatementIsNotLastStatementInExplicitSwitchCaseBlock()
    {
        const string testCode = """
                                internal class RH0395
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    break;
                                                    Consume();
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<BreakStatementSyntax>()
                                                               .Single()
                                                               .BreakKeyword
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}