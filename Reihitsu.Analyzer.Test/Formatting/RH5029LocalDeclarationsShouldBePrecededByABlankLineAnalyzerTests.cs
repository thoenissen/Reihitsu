using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer, RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a local declaration directly follows an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenLocalDeclarationIsNotPrecededByBlankLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();
                                        {|#0:var|} value = GetValue();
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     public void Execute()
                                     {
                                         Consume();

                                         var value = GetValue();
                                     }

                                     private string GetValue()
                                     {
                                         return string.Empty;
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a local declaration in a switch section directly follows a statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenLocalDeclarationInSwitchSectionIsNotPrecededByBlankLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                Consume();
                                                {|#0:var|} text = GetValue();
                                                break;
                                        }
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5029
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 Consume();

                                                 var text = GetValue();
                                                 break;
                                         }
                                     }

                                     private string GetValue()
                                     {
                                         return string.Empty;
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5029MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a local declaration is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationIsFirstStatementInBlock()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        var value = GetValue();
                                        Consume(value);
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a local declaration is the first statement in a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationIsFirstStatementInSwitchSection()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                var text = GetValue();
                                                Consume(text);
                                                break;
                                        }
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for consecutive local declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConsecutiveLocalDeclarations()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();

                                        var first = GetValue();
                                        var second = GetValue();
                                        Consume(first + second);
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a local declaration already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationAlreadyHasPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5029
                                {
                                    public void Execute()
                                    {
                                        Consume();

                                        var value = GetValue();
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}