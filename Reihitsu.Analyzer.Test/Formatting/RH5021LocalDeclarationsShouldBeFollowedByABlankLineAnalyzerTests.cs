using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer"/> and <see cref="RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer, RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a local declaration with initializer is directly followed by an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenLocalDeclarationIsFollowedByExpressionStatementWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        var value = GetValue();
                                        {|#0:Consume|}(value);
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

        const string fixedCode = """
                                 internal class RH5021
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

        await Verify(testCode, fixedCode, Diagnostics(RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5021MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a local declaration without initializer is directly followed by an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenLocalDeclarationWithoutInitializerIsFollowedByExpressionStatementWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        string text;
                                        {|#0:Consume|}();
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5021
                                 {
                                     public void Execute()
                                     {
                                         string text;

                                         Consume();
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5021MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an expression statement is followed by a local declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenExpressionStatementIsFollowedByLocalDeclarationWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        Consume();
                                        var next = GetValue();
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

    /// <summary>
    /// Verifies no diagnostics are reported for consecutive local declarations with initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConsecutiveLocalDeclarationsWithInitializer()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        var first = GetValue();
                                        var second = GetValue();

                                        Consume(first + second);
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
    /// Verifies no diagnostics are reported for consecutive local declarations without initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConsecutiveLocalDeclarationsWithoutInitializer()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        string first;
                                        string second;

                                        Consume();
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported inside a mixed local declaration block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMixedLocalDeclarationBlockWithAndWithoutInitializer()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        string text;
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
    /// Verifies no diagnostics are reported for consecutive expression statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConsecutiveExpressionStatements()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        Consume();
                                        Consume();
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a local declaration and expression statement are already separated by a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenLocalDeclarationAndExpressionStatementAreAlreadySeparatedByBlankLine()
    {
        const string testCode = """
                                internal class RH5021
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
    /// Verifies no diagnostics are reported when a local declaration is followed by a control flow statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForLocalDeclarationAdjacentToControlFlowStatement()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public string Execute()
                                    {
                                        var value = GetValue();
                                        return value;
                                    }

                                    private string GetValue()
                                    {
                                        return string.Empty;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a plain assignment is followed by an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForAssignmentStatementAdjacentToExpressionStatement()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        var value = string.Empty;
                                        value = GetValue();
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

    #endregion // Tests
}