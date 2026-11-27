using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer, RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a while statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5003-001")]
    [Requirement("RH5003-002")]
    public async Task VerifyDiagnosticForWhileStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5003
                                {
                                    public void Execute()
                                    {
                                        var index = 0;
                                        {|#0:while|} (index < 3)
                                        {
                                            index++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5003
                                 {
                                     public void Execute()
                                     {
                                         var index = 0;

                                         while (index < 3)
                                         {
                                             index++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a while statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5003-001")]
    public async Task VerifyNoDiagnosticForWhileStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5003
                                {
                                    public void Execute()
                                    {
                                        var index = 0;

                                        while (index < 3)
                                        {
                                            index++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a while statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5003-001")]
    public async Task VerifyNoDiagnosticForWhileStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5003
                                {
                                    public void Execute()
                                    {
                                        while (false)
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
    [Requirement("RH5003-001")]
    public async Task VerifyDiagnosticForWhileStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5003
                                {
                                    public void Execute(bool keepRunning)
                                    {
                                        var value = 1;
                                        // Comment before while
                                        {|#0:while|} (keepRunning)
                                        {
                                            value++;
                                            break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5003
                                 {
                                     public void Execute(bool keepRunning)
                                     {
                                         var value = 1;

                                         // Comment before while
                                         while (keepRunning)
                                         {
                                             value++;
                                             break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies a <c>while</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while a <c>while</c> statement that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task WhileStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; while (a < 3) { a++; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; while (a < 3) { a++; } return a; })} b"} y";

                                    int O()
                                    {
                                        var a = 1;
                                        {|RH5003:while|} ($"{a}".Length < 3)
                                        {
                                            a++;
                                        }

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies a <c>while</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task WhileStatementInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; {|RH5003:while|} (a < 3) { a++; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; {|RH5003:while|} (a < 3) { a++; } return a; })} b"} y";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies a <c>while</c> statement inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task WhileStatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $@"a {F(() => { var a = 1; {|RH5003:while|} (a < 3) { a++; } return a; })} b";

                                    string P() => @$"a {F(() => { var a = 1; {|RH5003:while|} (a < 3) { a++; } return a; })} b";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5003
                                {
                                    public void Execute()
                                    {
                                        var index = 0;
                                        {|#0:while|} (index < 1)
                                        {
                                            index++;
                                        }
                                        {|#1:while|} (index < 3)
                                        {
                                            index++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5003
                                 {
                                     public void Execute()
                                     {
                                         var index = 0;

                                         while (index < 1)
                                         {
                                             index++;
                                         }

                                         while (index < 3)
                                         {
                                             index++;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent while statements, each missing its preceding blank line, so the second fix's insertion
        // point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}