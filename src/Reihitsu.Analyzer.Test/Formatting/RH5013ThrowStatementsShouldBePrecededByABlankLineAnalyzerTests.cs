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
public class RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer, RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a throw statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5013-001")]
    [Requirement("RH5013-002")]
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
    [Requirement("RH5013-001")]
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
    [Requirement("RH5013-001")]
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
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5013-001")]
    public async Task VerifyDiagnosticForThrowStatementWhenCommentLineDirectlyPrecedesIt()
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

                                             // Comment before throw
                                             throw new InvalidOperationException(message);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5013MessageFormat));
    }

    /// <summary>
    /// Verifies a throw statement after a statement is reported on its throw keyword while a throw expression is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5013-001")]
    [Requirement("RH5013-002")]
    public async Task VerifyDiagnosticForThrowStatementButNotForThrowExpression()
    {
        const string testCode = """
                                class C
                                {
                                    void M(object value)
                                    {
                                        var text = value.ToString();
                                        {|#0:throw|} new System.InvalidOperationException(text);
                                    }

                                    string N(string value)
                                    {
                                        var text = value ?? throw new System.ArgumentNullException(nameof(value));

                                        return text;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(object value)
                                     {
                                         var text = value.ToString();

                                         throw new System.InvalidOperationException(text);
                                     }

                                     string N(string value)
                                     {
                                         var text = value ?? throw new System.ArgumentNullException(nameof(value));

                                         return text;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5013MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a throw expression inside a statement that directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5013-001")]
    public async Task VerifyNoDiagnosticForThrowExpressionAfterStatement()
    {
        const string testCode = """
                                class C
                                {
                                    string M(string value, int count)
                                    {
                                        count++;
                                        var text = value ?? throw new System.ArgumentNullException(nameof(value));

                                        return text + count;
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
                                #pragma warning disable CS0162

                                using System;

                                internal class RH5013
                                {
                                    public void Execute(bool isInvalid)
                                    {
                                        if (isInvalid)
                                        {
                                            var message = "invalid";
                                            {|#0:throw|} new InvalidOperationException(message);
                                            {|#1:throw|} new InvalidOperationException(message);
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 #pragma warning disable CS0162

                                 using System;

                                 internal class RH5013
                                 {
                                     public void Execute(bool isInvalid)
                                     {
                                         if (isInvalid)
                                         {
                                             var message = "invalid";

                                             throw new InvalidOperationException(message);

                                             throw new InvalidOperationException(message);
                                         }
                                     }
                                 }
                                 """;

        // The second throw is unreachable code, kept alive only to exercise Fix All on two adjacent occurrences
        // in the same block; the pragma at file top suppresses CS0162 without touching either occurrence
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5013MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}