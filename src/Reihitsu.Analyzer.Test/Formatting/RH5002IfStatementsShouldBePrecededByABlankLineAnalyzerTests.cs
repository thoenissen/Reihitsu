using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer, RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when an if statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    [Requirement("RH5002-004")]
    [Requirement("SHARED-STMT-004")]
    [Requirement("SHARED-STMT-001")]
    public async Task VerifyDiagnosticForIfStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:if|} (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         if (value > 0)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    public async Task VerifyNoDiagnosticForIfStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        var value = 1;

                                        if (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    public async Task VerifyNoDiagnosticForIfStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        if (true)
                                        {
                                        }
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
    [Requirement("RH5002-001")]
    [Requirement("SHARED-STMT-004")]
    [Requirement("SHARED-STMT-001")]
    public async Task VerifyDiagnosticForIfStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(bool isReady)
                                    {
                                        var value = 1;
                                        // Comment before if
                                        {|#0:if|} (isReady)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute(bool isReady)
                                     {
                                         var value = 1;

                                         // Comment before if
                                         if (isReady)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement is the unbraced embedded body of a while statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-002")]
    public async Task VerifyNoDiagnosticForIfStatementAsEmbeddedWhileBody()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public int Execute(bool outer, bool inner)
                                    {
                                        while (outer)
                                            if (inner)
                                                return 1;

                                        return 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for an else-if statement, since its parent is an else clause rather than a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-002")]
    public async Task VerifyNoDiagnosticForElseIfStatement()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(bool first, bool second)
                                    {
                                        if (first)
                                        {
                                            DoFirst();
                                        }
                                        else if (second)
                                        {
                                            DoSecond();
                                        }
                                    }

                                    private static void DoFirst()
                                    {
                                    }

                                    private static void DoSecond()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostic is reported when a preprocessor directive is the last trivia directly above the if statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-003")]
    public async Task VerifyNoDiagnosticForIfStatementDirectlyBelowDirective()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(int value)
                                    {
                                        value++;
                                #if DEBUG
                                        value--;
                                #endif
                                        if (value > 6)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix moves an if statement that shares its line with the previous statement onto its own line after a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    [Requirement("SHARED-STMT-004")]
    [Requirement("SHARED-STMT-001")]
    public async Task VerifyIfStatementOnSameLineAsPreviousStatementIsMovedToItsOwnLine()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(int value)
                                    {
                                        value++; {|#0:if|} (value > 5) { }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute(int value)
                                     {
                                         value++;

                                         if (value > 5) { }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies one application of the fix corrects the line in front of the reported <c>if</c>, so the rule does not
    /// report it again
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-001")]
    public async Task OneApplicationCorrectsTheLineInFrontOfTheReportedIf()
    {
        const string testCode = "class C\n{\n    void M(int a)\n    {\n        a++;\n        {|RH5002:if|} (a > 1)\n        {\n        }\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M(int a)\n    {\n        a++;\n\n        if (a > 1)\n        {\n        }\n    }\n}\n";

        await Verify(testCode, fixedCode, test => test.NumberOfIncrementalIterations = 1);
    }

    /// <summary>
    /// Verifies one application of the fix corrects the line in front of the reported <c>if</c> under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-001")]
    public async Task OneApplicationCorrectsTheLineInFrontOfTheReportedIfUnderCarriageReturnLineFeed()
    {
        const string testCode = "class C\r\n{\r\n    void M(int a)\r\n    {\r\n        a++;\r\n        {|RH5002:if|} (a > 1)\r\n        {\r\n        }\r\n    }\r\n}\r\n";
        const string fixedCode = "class C\r\n{\r\n    void M(int a)\r\n    {\r\n        a++;\r\n\r\n        if (a > 1)\r\n        {\r\n        }\r\n    }\r\n}\r\n";

        await Verify(testCode, fixedCode, test => test.NumberOfIncrementalIterations = 1);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:if|} (value > 0)
                                        {
                                            value++;
                                        }
                                        {|#1:if|} (value > 1)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         if (value > 0)
                                         {
                                             value++;
                                         }

                                         if (value > 1)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent if statements, each missing its preceding blank line, so the second fix's insertion point
        // sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}