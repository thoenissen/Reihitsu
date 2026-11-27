using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5014SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer, RH5014SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a switch statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5014-001")]
    [Requirement("RH5014-002")]
    public async Task VerifyDiagnosticForSwitchStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5014
                                {
                                    public int Execute(int number)
                                    {
                                        var offset = 1;
                                        {|#0:switch|} (number)
                                        {
                                            case 0:
                                                return offset;
                                            default:
                                                return number;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5014
                                 {
                                     public int Execute(int number)
                                     {
                                         var offset = 1;

                                         switch (number)
                                         {
                                             case 0:
                                                 return offset;
                                             default:
                                                 return number;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a switch statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5014-001")]
    public async Task VerifyNoDiagnosticForSwitchStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5014
                                {
                                    public int Execute(int number)
                                    {
                                        var offset = 1;

                                        switch (number)
                                        {
                                            case 0:
                                                return offset;
                                            default:
                                                return number;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a switch statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5014-001")]
    public async Task VerifyNoDiagnosticForSwitchStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5014
                                {
                                    public int Execute(int number)
                                    {
                                        switch (number)
                                        {
                                            case 0:
                                                return 0;
                                            default:
                                                return number;
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
    [Requirement("RH5014-001")]
    public async Task VerifyDiagnosticForSwitchStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5014
                                {
                                    public int Execute(int number)
                                    {
                                        var offset = 1;
                                        // Comment before switch
                                        {|#0:switch|} (number)
                                        {
                                            case 0:
                                                return offset;
                                            default:
                                                return number;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5014
                                 {
                                     public int Execute(int number)
                                     {
                                         var offset = 1;

                                         // Comment before switch
                                         switch (number)
                                         {
                                             case 0:
                                                 return offset;
                                             default:
                                                 return number;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies a switch statement after a statement is reported on its switch keyword while a switch expression is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5014-001")]
    [Requirement("RH5014-002")]
    public async Task VerifyDiagnosticForSwitchStatementButNotForSwitchExpression()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        {|#0:switch|} (a)
                                        {
                                            case 1:
                                                break;
                                        }

                                        a++;
                                        a = a switch
                                        {
                                            1 => 2,
                                            _ => 3
                                        };
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

                                         a++;
                                         a = a switch
                                         {
                                             1 => 2,
                                             _ => 3
                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a switch expression inside a statement that directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5014-001")]
    public async Task VerifyNoDiagnosticForSwitchExpressionAfterStatement()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        a++;
                                        var b = a switch
                                        {
                                            1 => 2,
                                            _ => 3
                                        };

                                        return b;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a <c>switch</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while a <c>switch</c> statement that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task SwitchStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; switch (a) { default: a++; break; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; switch (a) { default: a++; break; } return a; })} b"} y";

                                    int O()
                                    {
                                        var a = 1;
                                        {|RH5014:switch|} ($"{a}".Length)
                                        {
                                            default:
                                                a++;
                                                break;
                                        }

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies a <c>switch</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task SwitchStatementInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; {|RH5014:switch|} (a) { default: a++; break; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; {|RH5014:switch|} (a) { default: a++; break; } return a; })} b"} y";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies a <c>switch</c> statement inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task SwitchStatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $@"a {F(() => { var a = 1; {|RH5014:switch|} (a) { default: a++; break; } return a; })} b";

                                    string P() => @$"a {F(() => { var a = 1; {|RH5014:switch|} (a) { default: a++; break; } return a; })} b";
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
                                internal class RH5014
                                {
                                    public int Execute(int number)
                                    {
                                        var offset = 1;
                                        {|#0:switch|} (number)
                                        {
                                            case 0:
                                                return offset;
                                        }
                                        {|#1:switch|} (number)
                                        {
                                            case 0:
                                                return offset;
                                            default:
                                                return number;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5014
                                 {
                                     public int Execute(int number)
                                     {
                                         var offset = 1;

                                         switch (number)
                                         {
                                             case 0:
                                                 return offset;
                                         }

                                         switch (number)
                                         {
                                             case 0:
                                                 return offset;
                                             default:
                                                 return number;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent switch statements, each missing its preceding blank line, so the second fix's insertion
        // point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}