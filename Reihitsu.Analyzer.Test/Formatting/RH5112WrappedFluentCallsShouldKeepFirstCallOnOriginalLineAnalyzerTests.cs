using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer"/> and <see cref="RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests : AnalyzerTestsBase<RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer, RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a multiline fluent chain reports when the first call starts on the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixWhenFirstCallStartsOnNextLine()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Create()
                                    {
                                        return new Builder()
                                            {|#0:.|}UseLogging()
                                            .UseValidation()
                                            .Build();
                                    }

                                    private sealed class Builder
                                    {
                                        public Builder UseLogging()
                                        {
                                            return this;
                                        }

                                        public Builder UseValidation()
                                        {
                                            return this;
                                        }

                                        public object Build()
                                        {
                                            return new object();
                                        }
                                    }
                                }
                                """;
        const string resultData = """
                                  internal sealed class Example
                                  {
                                      private static object Create()
                                      {
                                          return new Builder().UseLogging()
                                                              .UseValidation()
                                                              .Build();
                                      }

                                      private sealed class Builder
                                      {
                                          public Builder UseLogging()
                                          {
                                              return this;
                                          }

                                          public Builder UseValidation()
                                          {
                                              return this;
                                          }

                                          public object Build()
                                          {
                                              return new object();
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a compliant multiline fluent chain does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenFirstCallStaysOnOriginalLine()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Create()
                                    {
                                        return new Builder().UseLogging()
                                                            .UseValidation()
                                                            .Build();
                                    }

                                    private sealed class Builder
                                    {
                                        public Builder UseLogging()
                                        {
                                            return this;
                                        }

                                        public Builder UseValidation()
                                        {
                                            return this;
                                        }

                                        public object Build()
                                        {
                                            return new object();
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a wrapped conditional-access chain is reported, fixed once, and clean afterwards
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConditionalAccessChainFixConvergesInOneApplication()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values
                                            ?.Where(value => value > 0)
                                            ?.ToArray();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static int[] Convert(int[] values)
                                      {
                                          return values?.Where(value => value > 0)
                                                       ?.ToArray();
                                      }
                                  }
                                  """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(resultData, fixedSource);
        await Verify(fixedSource);
    }

    /// <summary>
    /// Verifies that RH5112 does not report when an active directive separates the root from the first call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAcrossActiveDirectiveGap()
    {
        const string testData = """
                                #define FEATURE

                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values
                                #if FEATURE
                                            ?.Where(value => value > 0)
                                #endif
                                            ?.ToArray();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that RH5112 does not report when disabled text separates the root from the first active call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAcrossDisabledTextGap()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values
                                #if FEATURE
                                            ?.Where(value => value > 0)
                                #endif
                                            ?.ToArray();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a wrapped single fluent call reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForSingleWrappedCall()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Create()
                                    {
                                        return new Builder()
                                            {|#0:.|}Build();
                                    }

                                    private sealed class Builder
                                    {
                                        public object Build()
                                        {
                                            return new object();
                                        }
                                    }
                                }
                                """;
        const string resultData = """
                                  internal sealed class Example
                                  {
                                      private static object Create()
                                      {
                                          return new Builder().Build();
                                      }

                                      private sealed class Builder
                                      {
                                          public object Build()
                                          {
                                              return new object();
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that chains with a comment directly above the first wrapped call are exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenCommentIsDirectlyAboveFirstWrappedCall()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Create()
                                    {
                                        return new Builder()

                                            // Keep this step separate.
                                            .UseLogging()
                                            .UseValidation()
                                            .Build();
                                    }

                                    private sealed class Builder
                                    {
                                        public Builder UseLogging()
                                        {
                                            return this;
                                        }

                                        public Builder UseValidation()
                                        {
                                            return this;
                                        }

                                        public object Build()
                                        {
                                            return new object();
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}