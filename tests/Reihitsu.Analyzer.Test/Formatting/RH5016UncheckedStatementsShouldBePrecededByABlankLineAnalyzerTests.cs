using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer, RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when an unchecked statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5016-001")]
    [Requirement("RH5016-002")]
    public async Task VerifyDiagnosticForUncheckedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        {|#0:unchecked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5016
                                 {
                                     public int Execute(int value)
                                     {
                                         var factor = 2;

                                         unchecked
                                         {
                                             return value * factor;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an unchecked statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5016-001")]
    public async Task VerifyNoDiagnosticForUncheckedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;

                                        unchecked
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an unchecked statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5016-001")]
    public async Task VerifyNoDiagnosticForUncheckedStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        unchecked
                                        {
                                            return value + 1;
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
    [Requirement("RH5016-001")]
    public async Task VerifyDiagnosticForUncheckedStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        // Comment before unchecked
                                        {|#0:unchecked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5016
                                 {
                                     public int Execute(int value)
                                     {
                                         var factor = 2;

                                         // Comment before unchecked
                                         unchecked
                                         {
                                             return value * factor;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies an unchecked statement after a statement is reported on its unchecked keyword while a checked statement and an unchecked expression are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5016-001")]
    [Requirement("RH5016-002")]
    public async Task VerifyDiagnosticForUncheckedStatementButNotForCheckedStatementOrUncheckedExpression()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        {|#0:unchecked|}
                                        {
                                            a++;
                                        }

                                        a++;
                                        checked
                                        {
                                            a--;
                                        }

                                        a++;
                                        a = unchecked(a + 1);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         unchecked
                                         {
                                             a++;
                                         }

                                         a++;
                                         checked
                                         {
                                             a--;
                                         }

                                         a++;
                                         a = unchecked(a + 1);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a checked statement that directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5016-001")]
    public async Task VerifyNoDiagnosticForCheckedStatementAfterStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        checked
                                        {
                                            a--;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for an unchecked expression inside a statement that directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5016-001")]
    public async Task VerifyNoDiagnosticForUncheckedExpressionAfterStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        a = unchecked(a + 1);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an <c>unchecked</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while an <c>unchecked</c> statement that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task UncheckedStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; unchecked { a++; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; unchecked { a++; } return a; })} b"} y";

                                    int O()
                                    {
                                        var a = 1;
                                        {|RH5016:unchecked|}
                                        {
                                            a = $"{a}".Length;
                                        }

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies an <c>unchecked</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task UncheckedStatementInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; {|RH5016:unchecked|} { a++; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; {|RH5016:unchecked|} { a++; } return a; })} b"} y";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies an <c>unchecked</c> statement inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task UncheckedStatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $@"a {F(() => { var a = 1; {|RH5016:unchecked|} { a++; } return a; })} b";

                                    string P() => @$"a {F(() => { var a = 1; {|RH5016:unchecked|} { a++; } return a; })} b";
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
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        {|#0:unchecked|}
                                        {
                                            value *= factor;
                                        }
                                        {|#1:unchecked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5016
                                 {
                                     public int Execute(int value)
                                     {
                                         var factor = 2;

                                         unchecked
                                         {
                                             value *= factor;
                                         }

                                         unchecked
                                         {
                                             return value * factor;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent unchecked statements, each missing its preceding blank line, so the second fix's
        // insertion point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}