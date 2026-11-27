using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5018LockStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer, RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a lock statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5018-001")]
    [Requirement("RH5018-002")]
    public async Task VerifyDiagnosticForLockStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:lock|} (gate)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5018
                                 {
                                     private readonly object gate = new();

                                     public void Execute()
                                     {
                                         var value = 1;

                                         lock (gate)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a lock statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5018-001")]
    public async Task VerifyNoDiagnosticForLockStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        var value = 1;

                                        lock (gate)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a lock statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5018-001")]
    public async Task VerifyNoDiagnosticForLockStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        lock (gate)
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
    [Requirement("RH5018-001")]
    public async Task VerifyDiagnosticForLockStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        var value = 1;
                                        // Comment before lock
                                        {|#0:lock|} (gate)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5018
                                 {
                                     private readonly object gate = new();

                                     public void Execute()
                                     {
                                         var value = 1;

                                         // Comment before lock
                                         lock (gate)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies a <c>lock</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while a <c>lock</c> statement that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task LockStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; lock (this) { a++; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; lock (this) { a++; } return a; })} b"} y";

                                    int O()
                                    {
                                        var a = 1;
                                        {|RH5018:lock|} ($"{a}")
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
    /// Verifies a <c>lock</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task LockStatementInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; {|RH5018:lock|} (this) { a++; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; {|RH5018:lock|} (this) { a++; } return a; })} b"} y";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies a <c>lock</c> statement inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task LockStatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $@"a {F(() => { var a = 1; {|RH5018:lock|} (this) { a++; } return a; })} b";

                                    string P() => @$"a {F(() => { var a = 1; {|RH5018:lock|} (this) { a++; } return a; })} b";
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
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:lock|} (gate)
                                        {
                                            value++;
                                        }
                                        {|#1:lock|} (gate)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5018
                                 {
                                     private readonly object gate = new();

                                     public void Execute()
                                     {
                                         var value = 1;

                                         lock (gate)
                                         {
                                             value++;
                                         }

                                         lock (gate)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent lock statements, each missing its preceding blank line, so the second fix's insertion
        // point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}