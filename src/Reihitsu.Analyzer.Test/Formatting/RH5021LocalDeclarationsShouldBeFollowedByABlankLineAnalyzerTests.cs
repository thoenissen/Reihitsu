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
public class RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer, RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a local declaration with initializer is directly followed by an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5021-001")]
    [Requirement("RH5021-003")]
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
    [Requirement("RH5021-001")]
    [Requirement("RH5021-003")]
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
    [Requirement("RH5021-001")]
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
    [Requirement("RH5021-001")]
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
    [Requirement("RH5021-001")]
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
    [Requirement("RH5021-001")]
    [Requirement("RH5021-002")]
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

    /// <summary>
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly
    /// follows the local declaration, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5021-001")]
    public async Task VerifyDiagnosticWhenLocalDeclarationIsFollowedByCommentLineThenExpressionStatement()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        var value = GetValue();
                                        // Comment after declaration
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

                                         // Comment after declaration
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
    /// Verifies an expression statement is reported on its first token after a local declaration and after a using declaration, but not after another expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5021-001")]
    [Requirement("RH5021-003")]
    public async Task VerifyDiagnosticForExpressionStatementAfterLocalOrUsingDeclarationButNotAfterExpressionStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a, System.IDisposable resource)
                                    {
                                        var b = a;
                                        {|#0:M|}(b, resource);
                                        M(a, resource);

                                        using var other = resource;
                                        {|#1:other|}.Dispose();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a, System.IDisposable resource)
                                     {
                                         var b = a;

                                         M(b, resource);
                                         M(a, resource);

                                         using var other = resource;

                                         other.Dispose();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5021MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a diagnostic is reported on the first token of an expression statement that directly follows a using declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5021-001")]
    [Requirement("RH5021-003")]
    public async Task VerifyDiagnosticForExpressionStatementAfterUsingDeclaration()
    {
        const string testCode = """
                                class C
                                {
                                    void M(System.IDisposable resource)
                                    {
                                        using var other = resource;
                                        {|#0:other|}.Dispose();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(System.IDisposable resource)
                                     {
                                         using var other = resource;

                                         other.Dispose();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5021MessageFormat));
    }

    /// <summary>
    /// Verifies simple and compound assignments after a local declaration are exempt while an increment after a local declaration is reported on its first token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5021-002")]
    [Requirement("RH5021-001")]
    [Requirement("RH5021-003")]
    public async Task VerifyNoDiagnosticForAssignmentsAfterLocalDeclarationButDiagnosticForIncrement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var b = a;
                                        b = 2;

                                        var c = a;
                                        c += 3;

                                        var d = a;
                                        {|#0:d|}++;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         var b = a;
                                         b = 2;

                                         var c = a;
                                         c += 3;

                                         var d = a;

                                         d++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5021MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a compound assignment that directly follows a local declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5021-002")]
    public async Task VerifyNoDiagnosticForCompoundAssignmentAfterLocalDeclaration()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        var c = a;
                                        c += 3;

                                        return c;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a null-coalescing assignment that directly follows a local declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5021-002")]
    public async Task VerifyNoDiagnosticForNullCoalescingAssignmentAfterLocalDeclaration()
    {
        const string testCode = """
                                class C
                                {
                                    string M(string value)
                                    {
                                        var text = value;
                                        text ??= string.Empty;

                                        return text;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5021
                                {
                                    public void Execute()
                                    {
                                        var first = GetValue();
                                        {|#0:Consume|}(first);
                                        var second = GetValue();
                                        {|#1:Consume|}(second);
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
                                         var first = GetValue();

                                         Consume(first);
                                         var second = GetValue();

                                         Consume(second);
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

        // Two independent declaration/consumption pairs, each missing its separating blank line, so the second
        // fix's insertion point is unaffected by the first occurrence's own blank-line insertion
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5021MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}