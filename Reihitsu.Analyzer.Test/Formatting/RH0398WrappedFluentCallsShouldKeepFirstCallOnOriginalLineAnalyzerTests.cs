using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer"/> and <see cref="RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests : AnalyzerTestsBase<RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer, RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider>
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
                     Diagnostics(RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH0398MessageFormat));
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
                     Diagnostics(RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH0398MessageFormat));
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