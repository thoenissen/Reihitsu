using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer"/> and <see cref="RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzerTests : AnalyzerTestsBase<RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer, RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider>
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
                                internal class RH7501
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
                                 internal class RH7501
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

        await Verify(testCode, fixedCode, Diagnostics(RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH7501MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps comments attached to the moved break statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixPreservesCommentsWhenMovingBreakStatement()
    {
        const string testCode = """
                                internal class RH7501
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
                                 internal class RH7501
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

        await Verify(testCode, fixedCode, Diagnostics(RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH7501MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix does not reformat unrelated switch section layout
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotReformatNonCompliantSwitchSectionLayout()
    {
        const string testCode = """
                                internal class RH7501
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
                                 internal class RH7501
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

        await Verify(testCode, fixedCode, Diagnostics(RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH7501MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the break statement already follows the explicit switch case block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenBreakStatementFollowsExplicitSwitchCaseBlock()
    {
        const string testCode = """
                                internal class RH7501
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
                                internal class RH7501
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
                                internal class RH7501
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
                                                   RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<BreakStatementSyntax>()
                                                               .Single()
                                                               .BreakKeyword
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}