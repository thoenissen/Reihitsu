using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer, RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a fixed statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5017-001")]
    [Requirement("RH5017-002")]
    public async Task VerifyDiagnosticForFixedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];
                                        {|#0:fixed|} (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5017
                                 {
                                     private readonly byte[] data = new byte[1];

                                     public unsafe void Pin()
                                     {
                                         var buffer = new byte[4];

                                         fixed (byte* pointer = buffer)
                                         {
                                             *pointer = 1;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a fixed statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5017-001")]
    public async Task VerifyNoDiagnosticForFixedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];

                                        fixed (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a fixed statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5017-001")]
    public async Task VerifyNoDiagnosticForFixedStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        fixed (byte* value = data)
                                        {
                                            *value = 42;
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
    [Requirement("RH5017-001")]
    public async Task VerifyDiagnosticForFixedStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];
                                        // Comment before fixed
                                        {|#0:fixed|} (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5017
                                 {
                                     public unsafe void Pin()
                                     {
                                         var buffer = new byte[4];

                                         // Comment before fixed
                                         fixed (byte* pointer = buffer)
                                         {
                                             *pointer = 1;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies a <c>fixed</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while a <c>fixed</c> statement that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task FixedStatementInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    unsafe string M() => $"a {F(() => { var b = new int[1]; fixed (int* p = b) { *p = 1; } return b[0]; })} b";

                                    unsafe string N() => $@"x {$"a {F(() => { var b = new int[1]; fixed (int* p = b) { *p = 1; } return b[0]; })} b"} y";

                                    unsafe int O()
                                    {
                                        var a = 1;
                                        {|RH5017:fixed|} (char* p = $"{a}")
                                        {
                                            a = *p;
                                        }

                                        return a;
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
                         test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10));
                     });
    }

    /// <summary>
    /// Verifies a <c>fixed</c> statement inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task FixedStatementInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    unsafe string M() => $"a {F(() => { var b = new int[1]; {|RH5017:fixed|} (int* p = b) { *p = 1; } return b[0]; })} b";

                                    unsafe string N() => $@"x {$"a {F(() => { var b = new int[1]; {|RH5017:fixed|} (int* p = b) { *p = 1; } return b[0]; })} b"} y";
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
                         test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11));
                     });
    }

    /// <summary>
    /// Verifies a <c>fixed</c> statement inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-005")]
    public async Task FixedStatementInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    unsafe string M() => $@"a {F(() => { var b = new int[1]; {|RH5017:fixed|} (int* p = b) { *p = 1; } return b[0]; })} b";

                                    unsafe string P() => @$"a {F(() => { var b = new int[1]; {|RH5017:fixed|} (int* p = b) { *p = 1; } return b[0]; })} b";
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
                         test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10));
                     });
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5017
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];
                                        {|#0:fixed|} (byte* first = buffer)
                                        {
                                            *first = 1;
                                        }
                                        {|#1:fixed|} (byte* second = data)
                                        {
                                            *second = 2;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5017
                                 {
                                     private readonly byte[] data = new byte[1];

                                     public unsafe void Pin()
                                     {
                                         var buffer = new byte[4];

                                         fixed (byte* first = buffer)
                                         {
                                             *first = 1;
                                         }

                                         fixed (byte* second = data)
                                         {
                                             *second = 2;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent fixed statements, each missing its preceding blank line, so the second fix's insertion
        // point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}