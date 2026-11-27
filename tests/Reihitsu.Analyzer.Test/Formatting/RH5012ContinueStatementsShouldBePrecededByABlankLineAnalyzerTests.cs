using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer, RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a continue statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    [Requirement("RH5012-002")]
    public async Task VerifyDiagnosticForContinueStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;
                                            {|#0:continue|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5012
                                 {
                                     public void Iterate()
                                     {
                                         while (true)
                                         {
                                             var shouldContinue = true;

                                             continue;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a continue statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    public async Task VerifyNoDiagnosticForContinueStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;

                                            continue;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a continue statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    public async Task VerifyNoDiagnosticForContinueStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            continue;
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
    [Requirement("RH5012-001")]
    public async Task VerifyDiagnosticForContinueStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;
                                            // Comment before continue
                                            {|#0:continue|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5012
                                 {
                                     public void Iterate()
                                     {
                                         while (true)
                                         {
                                             var shouldContinue = true;

                                             // Comment before continue
                                             continue;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a labeled <c>continue</c> that follows a statement is reported on the <c>continue</c> keyword and
    /// fixed with a blank line in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    [Requirement("RH5012-002")]
    public async Task VerifyLabeledContinueFollowingAStatementIsReportedAndFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] values, int a)
                                    {
                                        outer: foreach (var v in values)
                                        {
                                            foreach (var w in values)
                                            {
                                                a++;
                                                {|RH5012:continue|} outer;
                                            }
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[] values, int a)
                                     {
                                         outer: foreach (var v in values)
                                         {
                                             foreach (var w in values)
                                             {
                                                 a++;

                                                 continue outer;
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a labeled <c>continue</c> that is the first statement of its block, and one with a blank line in
    /// front of it, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    public async Task VerifyLabeledContinueFirstInBlockOrAfterBlankLineIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] values, int a)
                                    {
                                        outer: foreach (var v in values)
                                        {
                                            foreach (var w in values)
                                            {
                                                if (v == w)
                                                {
                                                    continue outer;
                                                }

                                                a++;

                                                continue outer;
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a <c>continue</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while a <c>continue</c> statement that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task ContinueStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 0; for (var i = 0; i < 2; i++) { a++; continue; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 0; for (var i = 0; i < 2; i++) { a++; continue; } return a; })} b"} y";

                                    int O()
                                    {
                                        var a = 0;

                                        for (var i = 0; i < 2; i++)
                                        {
                                            a = $"{a}".Length;
                                            {|RH5012:continue|};
                                        }

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies a <c>continue</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task ContinueStatementInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { var a = 0; for (var i = 0; i < 2; i++) { a++; {|RH5012:continue|}; } return a; })} b";

                                    string N() => $@"x {$"a {F(() => { var a = 0; for (var i = 0; i < 2; i++) { a++; {|RH5012:continue|}; } return a; })} b"} y";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies a <c>continue</c> statement inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task ContinueStatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $@"a {F(() => { var a = 0; for (var i = 0; i < 2; i++) { a++; {|RH5012:continue|}; } return a; })} b";

                                    string P() => @$"a {F(() => { var a = 0; for (var i = 0; i < 2; i++) { a++; {|RH5012:continue|}; } return a; })} b";
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
                                #pragma warning disable CS0162

                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;
                                            {|#0:continue|};
                                            {|#1:continue|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 #pragma warning disable CS0162

                                 internal class RH5012
                                 {
                                     public void Iterate()
                                     {
                                         while (true)
                                         {
                                             var shouldContinue = true;

                                             continue;

                                             continue;
                                         }
                                     }
                                 }
                                 """;

        // The second continue is unreachable code, kept alive only to exercise Fix All on two adjacent
        // occurrences in the same block; the pragma at file top suppresses CS0162 without touching either
        // occurrence
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}