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
public class RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests : BatchCodeFixTestsBase<RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer, RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider>
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

    /// <summary>
    /// Verifies that RH5112 does not report when the first invoked link is introduced by a
    /// null-forgiving operator whose own receiver is an intermediate member access, matching the
    /// existing exemption for a plain dot in the same position.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenNullForgivingLinkHasIntermediateMemberAccess()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Create(Builder builder)
                                    {
                                        return builder.Inner
                                            !.UseLogging()
                                            .Build();
                                    }

                                    private sealed class Builder
                                    {
                                        public Builder Inner
                                        {
                                            get
                                            {
                                                return this;
                                            }
                                        }

                                        public Builder UseLogging()
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
    /// Verifies the other side of the intermediate-member-access boundary: RH5112 still reports, and
    /// its code fix still converges, when the first invoked link is introduced by a null-forgiving
    /// operator whose own receiver is <em>not</em> an intermediate member access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixWhenNullForgivingLinkHasNoIntermediateMemberAccess()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Create(Builder builder)
                                    {
                                        return builder
                                            {|#0:!|}.UseLogging()
                                            .Build();
                                    }

                                    private sealed class Builder
                                    {
                                        public Builder UseLogging()
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
                                      private static object Create(Builder builder)
                                      {
                                          return builder!.UseLogging()
                                                        .Build();
                                      }

                                      private sealed class Builder
                                      {
                                          public Builder UseLogging()
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
    /// Verifies that a wrapped property access that is the chain's first link is reported and joined onto the root line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedPropertyFirstLinkIsFixedInOneApplication()
    {
        const string testData = """
                                using System.Collections.Generic;
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static object Run(dynamic a)
                                    {
                                        return a
                                            {|#0:.|}Prop
                                            .Foo();
                                    }

                                    private static object Use(object value)
                                    {
                                        return value;
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static object Run(dynamic a)
                                      {
                                          return a.Prop
                                                  .Foo();
                                      }

                                      private static object Use(object value)
                                      {
                                          return value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that the wrapped first link of a chain without calls is reported and joined onto the root line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedFirstLinkOfCallLessChainIsFixedInOneApplication()
    {
        const string testData = """
                                using System.Collections.Generic;
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static object Run(dynamic order)
                                    {
                                        return order
                                            {|#0:.|}Customer
                                            .Address;
                                    }

                                    private static object Use(object value)
                                    {
                                        return value;
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static object Run(dynamic order)
                                      {
                                          return order.Customer
                                                      .Address;
                                      }

                                      private static object Use(object value)
                                      {
                                          return value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped first link written as <c>!.</c> is reported on its exclamation mark and joined onto the root line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedNullForgivingFirstLinkIsFixedInOneApplication()
    {
        const string testData = """
                                using System.Collections.Generic;
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static object Run(dynamic a)
                                    {
                                        return a
                                            {|#0:!|}.B?.C();
                                    }

                                    private static object Use(object value)
                                    {
                                        return value;
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static object Run(dynamic a)
                                      {
                                          return a!.B?.C();
                                      }

                                      private static object Use(object value)
                                      {
                                          return value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix joins a call wrapped behind a multi-line raw string literal and keeps the literal's content and closing delimiter in their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedCallBehindRawStringRootIsFixedInOneApplication()
    {
        const string testData = """"
                                using System.Collections.Generic;
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static string Run()
                                    {
                                        return """
                                               text
                                               """
                                            {|#0:.|}Trim()
                                            .Trim();
                                    }

                                    private static object Use(object value)
                                    {
                                        return value;
                                    }
                                }
                                """";
        const string resultData = """"
                                  using System.Collections.Generic;
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static string Run()
                                      {
                                          return """
                                                 text
                                                 """.Trim()
                                                    .Trim();
                                      }

                                      private static object Use(object value)
                                      {
                                          return value;
                                      }
                                  }
                                  """";

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal sealed class Example
                                {
                                    private static object Create()
                                    {
                                        return new Builder()
                                            {|#0:.|}UseLogging(new Builder()
                                                {|#1:.|}UseValidation()
                                                .Build())
                                            .Build();
                                    }

                                    private sealed class Builder
                                    {
                                        public Builder UseLogging(object value)
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

        const string fixedCode = """
                                 internal sealed class Example
                                 {
                                     private static object Create()
                                     {
                                         return new Builder().UseLogging(new Builder().UseValidation()
                                                                                      .Build())
                                                             .Build();
                                     }

                                     private sealed class Builder
                                     {
                                         public Builder UseLogging(object value)
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

        // The outer diagnostic's fix reformats the whole outer chain, whose span fully contains the inner chain
        // passed as an argument to "UseLogging" and rewritten by the inner diagnostic's fix, so the batch fixer
        // discards the overlapping inner change while the outer rewrite already carries the correctly formatted
        // nested chain
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}