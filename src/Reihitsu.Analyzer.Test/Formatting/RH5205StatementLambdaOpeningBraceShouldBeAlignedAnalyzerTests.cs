using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer"/> and <see cref="RH5205StatementLambdaOpeningBraceShouldBeAlignedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzerTests : BatchCodeFixTestsBase<RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer, RH5205StatementLambdaOpeningBraceShouldBeAlignedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that an aligned simple statement lambda does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenSimpleStatementLambdaBraceIsAligned()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, object> projector = obj =>
                                                                         {
                                                                             return obj;
                                                                         };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an aligned parenthesized statement lambda does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenParenthesizedStatementLambdaBraceIsAligned()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, int, object> projector = (obj, index) =>
                                                                              {
                                                                                  return obj;
                                                                              };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that expression-bodied lambdas are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenLambdaIsExpressionBodied()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, object> projector = obj => obj;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a misaligned simple statement lambda brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySimpleStatementLambdaBraceMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, object> projector = obj =>
                                                                                 {|#0:{|}
                                                                                     return obj;
                                                                                 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, object> projector = obj =>
                                                                          {
                                                                              return obj;
                                                                          };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5205MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned parenthesized statement lambda brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyParenthesizedStatementLambdaBraceMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, int, object> projector = (obj, index) =>
                                                                                              {|#0:{|}
                                                                                                  return obj;
                                                                                              };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, int, object> projector = (obj, index) =>
                                                                               {
                                                                                   return obj;
                                                                               };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5205MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned async simple statement lambda brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAsyncSimpleStatementLambdaBraceMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, Task<object>> projector = async obj =>
                                                                                           {|#0:{|}
                                                                                               return await Task.FromResult(obj);
                                                                                           };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.Threading.Tasks;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, Task<object>> projector = async obj =>
                                                                                {
                                                                                    return await Task.FromResult(obj);
                                                                                };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5205MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned async parenthesized statement lambda brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAsyncParenthesizedStatementLambdaBraceMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Func<object, int, Task<object>> projector = async (obj, index) =>
                                                                                                         {|#0:{|}
                                                                                                             return await Task.FromResult(obj);
                                                                                                         };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.Threading.Tasks;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, int, Task<object>> projector = async (obj, index) =>
                                                                                     {
                                                                                         return await Task.FromResult(obj);
                                                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5205MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        Action outer = () =>
                                        {|#0:{|}
                                            Func<object, object> inner = obj =>
                                            {|#1:{|}
                                                return obj;
                                            };
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Action outer = () =>
                                                        {
                                                            Func<object, object> inner = obj =>
                                                                                         {
                                                                                             return obj;
                                                                                         };
                                                        };
                                     }
                                 }
                                 """;

        // The outer lambda's misaligned brace is fixed by reformatting the whole outer lambda expression, whose
        // span fully contains the inner lambda flagged by the second diagnostic, so the batch fixer discards the
        // overlapping inner change while the outer rewrite already re-aligns the nested lambda's brace too
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5205MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}