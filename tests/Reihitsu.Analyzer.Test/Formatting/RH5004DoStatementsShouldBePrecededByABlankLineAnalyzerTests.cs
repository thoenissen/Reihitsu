using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5004DoStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer, RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a do statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5004-001")]
    [Requirement("RH5004-002")]
    public async Task VerifyDiagnosticForDoStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5004
                                {
                                    public void Execute()
                                    {
                                        var index = 0;
                                        {|#0:do|}
                                        {
                                            index++;
                                        }
                                        while (index < 3);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5004
                                 {
                                     public void Execute()
                                     {
                                         var index = 0;

                                         do
                                         {
                                             index++;
                                         }
                                         while (index < 3);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a do statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5004-001")]
    public async Task VerifyNoDiagnosticForDoStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5004
                                {
                                    public void Execute()
                                    {
                                        var index = 0;

                                        do
                                        {
                                            index++;
                                        }
                                        while (index < 3);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a do statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5004-001")]
    public async Task VerifyNoDiagnosticForDoStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5004
                                {
                                    public void Execute()
                                    {
                                        do
                                        {
                                        }
                                        while (false);
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
    [Requirement("RH5004-001")]
    public async Task VerifyDiagnosticForDoStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5004
                                {
                                    public void Execute()
                                    {
                                        var index = 0;
                                        // Comment before do
                                        {|#0:do|}
                                        {
                                            index++;
                                        }
                                        while (index < 1);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5004
                                 {
                                     public void Execute()
                                     {
                                         var index = 0;

                                         // Comment before do
                                         do
                                         {
                                             index++;
                                         }
                                         while (index < 1);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies a <c>do</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while a <c>do</c> statement that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task DoStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; do { a++; } while (a < 3); return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; do { a++; } while (a < 3); return a; })} b"} y";

                                    int O()
                                    {
                                        var a = 1;
                                        {|RH5004:do|}
                                        {
                                            a++;
                                        }
                                        while ($"{a}".Length < 3);

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies a <c>do</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task DoStatementInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 1; {|RH5004:do|} { a++; } while (a < 3); return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 1; {|RH5004:do|} { a++; } while (a < 3); return a; })} b"} y";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies a <c>do</c> statement inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task DoStatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $@"a {F(() => { var a = 1; {|RH5004:do|} { a++; } while (a < 3); return a; })} b";

                                    string P() => @$"a {F(() => { var a = 1; {|RH5004:do|} { a++; } while (a < 3); return a; })} b";
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
                                internal class RH5004
                                {
                                    public void Execute()
                                    {
                                        var index = 0;
                                        {|#0:do|}
                                        {
                                            index++;
                                        }
                                        while (index < 1);
                                        {|#1:do|}
                                        {
                                            index++;
                                        }
                                        while (index < 3);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5004
                                 {
                                     public void Execute()
                                     {
                                         var index = 0;

                                         do
                                         {
                                             index++;
                                         }
                                         while (index < 1);

                                         do
                                         {
                                             index++;
                                         }
                                         while (index < 3);
                                     }
                                 }
                                 """;

        // Two adjacent do statements, each missing its preceding blank line, so the second fix's insertion point
        // sits directly against the first fix's trailing while clause
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}