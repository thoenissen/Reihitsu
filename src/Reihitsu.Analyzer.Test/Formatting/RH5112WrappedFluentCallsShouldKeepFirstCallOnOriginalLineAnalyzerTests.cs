using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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
    #region Constants

    /// <summary>
    /// Source in a file indented by two spaces whose chain is the expression of a statement behind two blank lines, with its
    /// first link wrapped, an anonymous method argument whose block the depth places, and a space in front of the statement's
    /// semicolon
    /// </summary>
    private const string ChainOfStatementInTwoSpaceIndentedFileTestData = """
                                                                          using System;

                                                                          internal class Builder
                                                                          {
                                                                            internal Builder A(Action action) => this;
                                                                          }

                                                                          internal class Example
                                                                          {
                                                                            private static void M(Builder b)
                                                                            {
                                                                            }

                                                                            private static void Run(Builder b)
                                                                            {
                                                                              M(b);


                                                                              b
                                                                                {|#0:.|}A(delegate
                                                                                   {
                                                                                     M(b);
                                                                                   }) ;
                                                                            }
                                                                          }
                                                                          """;

    /// <summary>
    /// Expected result of fixing <see cref="ChainOfStatementInTwoSpaceIndentedFileTestData"/>: only the chain changes, its
    /// block keeps its place relative to the statement's first token, and the blank lines and the space in front of the
    /// semicolon stay
    /// </summary>
    private const string ChainOfStatementInTwoSpaceIndentedFileResultData = """
                                                                            using System;

                                                                            internal class Builder
                                                                            {
                                                                              internal Builder A(Action action) => this;
                                                                            }

                                                                            internal class Example
                                                                            {
                                                                              private static void M(Builder b)
                                                                              {
                                                                              }

                                                                              private static void Run(Builder b)
                                                                              {
                                                                                M(b);


                                                                                b.A(delegate
                                                                                {
                                                                                    M(b);
                                                                                }) ;
                                                                              }
                                                                            }
                                                                            """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that a multiline fluent chain reports when the first call starts on the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("RH5112-002")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-003")]
    [Requirement("SHARED-CHAIN-008")]
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
                     Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a compliant multiline fluent chain does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("SHARED-CHAIN-003")]
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
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-008")]
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
    [Requirement("RH5112-001")]
    [Requirement("SHARED-CHAIN-004")]
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
    [Requirement("RH5112-001")]
    [Requirement("SHARED-CHAIN-004")]
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
    [Requirement("RH5112-001")]
    [Requirement("RH5112-002")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-003")]
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
                     Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that chains with a comment directly above the first wrapped call are exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("SHARED-CHAIN-004")]
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
    [Requirement("RH5112-001")]
    [Requirement("SHARED-CHAIN-001")]
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
    [Requirement("RH5112-001")]
    [Requirement("RH5112-002")]
    [Requirement("RH5112-003")]
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
                     Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped property access that is the chain's first link is reported and joined onto the root line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-003")]
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

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the example of RH5112-003 for a prefix of property reads: the fix joins the first link to the root's line and
    /// aligns the later wrapped link under the last link in front of it, in one application under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-007")]
    public async Task VerifyFixAlignsLaterLinkUnderTheLastPrefixLink(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    int[] Items { get; } = [];

                                    void M(C x)
                                    {
                                        var s = x
                                            {|RH5112:.|}Items.Length
                                            .ToString();
                                    }
                                }
                                """;
        const string resultData = """
                                  class C
                                  {
                                      int[] Items { get; } = [];

                                      void M(C x)
                                      {
                                          var s = x.Items.Length
                                                         .ToString();
                                      }
                                  }
                                  """;

        await VerifyCodeFixWithEndOfLine(testData, resultData, endOfLine);
    }

    /// <summary>
    /// Verifies that the wrapped first link of a chain without calls is reported and joined onto the root line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-001")]
    [Requirement("SHARED-CHAIN-003")]
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

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped first link written as <c>!.</c> is reported on its exclamation mark and joined onto the root line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("RH5112-002")]
    [Requirement("RH5112-003")]
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

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix joins a call wrapped behind a multi-line raw string literal and keeps the literal's content and closing delimiter in their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("RH5112-003")]
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

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: the fix joins the first call and aligns the remaining links from the column the
    /// chain's first token keeps, while the misspaced declaration in front of the chain stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-009")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixJoinsFirstCallBehindMisspacedDeclaration()
    {
        const string testData = """
                                using System.Collections.Generic;
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static object Run(dynamic a, IEnumerable<Node> source, bool flag, dynamic d)
                                    {
                                        var r=a
                                            {|#0:.|}B()
                                            .C();

                                        return r;
                                    }

                                    private static object Use(object value)
                                    {
                                        return value;
                                    }

                                    private sealed class Node
                                    {
                                        public List<Node> Children { get; } = new List<Node>();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static object Run(dynamic a, IEnumerable<Node> source, bool flag, dynamic d)
                                      {
                                          var r=a.B()
                                                 .C();

                                          return r;
                                      }

                                      private static object Use(object value)
                                      {
                                          return value;
                                      }

                                      private sealed class Node
                                      {
                                          public List<Node> Children { get; } = new List<Node>();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a chain in a branch of a conditional expression changes only the chain: the chain keeps
    /// its first token in its line and column and aligns its remaining links from there, while the conditional around
    /// it stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-009")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixJoinsFirstCallOfChainInConditionalExpression()
    {
        const string testData = """
                                using System.Collections.Generic;
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static object Run(dynamic a, IEnumerable<Node> source, bool flag, dynamic d)
                                    {
                                        var r = flag ? a
                                            {|#0:.|}B()
                                            .C() : d;

                                        return r;
                                    }

                                    private static object Use(object value)
                                    {
                                        return value;
                                    }

                                    private sealed class Node
                                    {
                                        public List<Node> Children { get; } = new List<Node>();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static object Run(dynamic a, IEnumerable<Node> source, bool flag, dynamic d)
                                      {
                                          var r = flag ? a.B()
                                                          .C() : d;

                                          return r;
                                      }

                                      private static object Use(object value)
                                      {
                                          return value;
                                      }

                                      private sealed class Node
                                      {
                                          public List<Node> Children { get; } = new List<Node>();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped conditional first link is reported on the question mark of its operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("RH5112-002")]
    [Requirement("SHARED-CHAIN-003")]
    public async Task VerifyWrappedConditionalFirstLinkIsReportedOnQuestionMark()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private Example Self() => this;

                                    private static object Run(Example c)
                                    {
                                        return c
                                            {|#0:?|}.Self();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped first link written as <c>!?.</c> is reported on its exclamation mark
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("RH5112-002")]
    [Requirement("SHARED-CHAIN-001")]
    public async Task VerifyWrappedNullForgivingConditionalFirstLinkIsReportedOnExclamationMark()
    {
        const string testData = """
                                #nullable enable

                                internal sealed class Example
                                {
                                    private Example Self() => this;

                                    private static object? Run(Example? c)
                                    {
                                        return c
                                            {|#0:!|}?.Self();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment ending the root's line keeps the wrapped first link from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("SHARED-CHAIN-004")]
    public async Task VerifyNoDiagnosticWhenCommentFollowsRootOnItsLine()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] items)
                                    {
                                        return items // only positive
                                            .Where(value => value > 0)
                                            .ToArray();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside the argument list of the root does not keep the wrapped first link from being
    /// reported and fixed, because it is not in the gap in front of the first link's operator. The fix gives the chain
    /// its formatted layout, which also removes the space between the comment and the argument behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-001")]
    public async Task VerifyCommentInsideRootArgumentListDoesNotKeepFirstLinkInPlace()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert()
                                    {
                                        return Get(/* value */ 1)
                                            {|#0:.|}Where(value => value > 0)
                                            .ToArray();
                                    }

                                    private static int[] Get(int value) => [value];
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static int[] Convert()
                                      {
                                          return Get(/* value */1).Where(value => value > 0)
                                                                  .ToArray();
                                      }

                                      private static int[] Get(int value) => [value];
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped first link behind an element access attached to the root is reported, although a
    /// comment sits in front of the element access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("SHARED-CHAIN-001")]
    public async Task VerifyWrappedFirstLinkBehindAttachedElementAccessIsReported()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static string Run(int[] items)
                                    {
                                        return items // element
                                            ?[0]
                                            {|#0:.|}ToString();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a chain written on one line is not reported, however long it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("SHARED-CHAIN-002")]
    public async Task VerifyNoDiagnosticForLongChainOnOneLine()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static object Run(int[] items)
                                    {
                                        return items.Where(value => value > 0).Select(value => value * 2).OrderBy(value => value).ThenBy(value => value).ToList().AsReadOnly();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix joins the first link and aligns the later links under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-008")]
    public async Task VerifyWrappedFirstLinkIsFixedWithCrLf()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static object Run(int[] items)
                                    {
                                        return items
                                            {|#0:.|}Where(value => value > 0)
                                            .ToList();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static object Run(int[] items)
                                      {
                                          return items.Where(value => value > 0)
                                                      .ToList();
                                      }
                                  }
                                  """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix changes only the chain when formatting the statement would give the chain the same text
    /// and start column, so unformatted code behind the chain stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-009")]
    public async Task VerifyFixLeavesCodeBehindTheChainAsWritten()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static int Run(dynamic a, int d)
                                    {
                                        var r = a
                                            {|#0:.|}B()
                                            .C() + d+1;

                                        return r;
                                    }
                                }
                                """;
        const string resultData = """
                                  internal sealed class Example
                                  {
                                      private static int Run(dynamic a, int d)
                                      {
                                          var r = a.B()
                                                   .C() + d+1;

                                          return r;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a chain whose first link stands behind a block comment which starts the link's line is not
    /// reported, because the comment between the root and the first link keeps the rule from reporting the chain
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CHAIN-004")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForFirstLinkBehindBlockCommentStartingTheLine()
    {
        const string testData = """
                                class C
                                {
                                    string M(string s)
                                    {
                                        var x = s
                                /* c */ .Trim();

                                        return x;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a chain whose first link stands behind a block comment which starts the link's line is not
    /// reported under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CHAIN-004")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForFirstLinkBehindBlockCommentStartingTheLineWithCrLf()
    {
        const string testData = """
                                class C
                                {
                                    string M(string s)
                                    {
                                        var x = s
                                /* c */ .Trim();

                                        return x;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that a first link that starts its line behind a documentation comment written behind the root keeps its
    /// line, as behind any other comment, and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    public async Task VerifyFirstLinkBehindStrayDocumentationCommentIsNotReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class C
                                {
                                    internal object M(int[] items)
                                    {
                                        return items /// x
                                               .Where(x => x > 0)
                                               .ToList();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix of a chain that is the expression of a statement in a file indented by two spaces changes only the
    /// chain: the anonymous method's block keeps its place relative to the statement's first token, and the blank lines in
    /// front of the statement and the space in front of its semicolon stay as written, under LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-CHAIN-009")]
    public async Task VerifyFixOfChainOfStatementInTwoSpaceIndentedFileChangesOnlyTheChain()
    {
        await Verify(ChainOfStatementInTwoSpaceIndentedFileTestData.Replace("\r\n", "\n"),
                     ChainOfStatementInTwoSpaceIndentedFileResultData.Replace("\r\n", "\n"),
                     Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a chain that is the expression of a statement in a file indented by two spaces changes only the
    /// chain: the anonymous method's block keeps its place relative to the statement's first token, and the blank lines in
    /// front of the statement and the space in front of its semicolon stay as written, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-CHAIN-009")]
    public async Task VerifyFixOfChainOfStatementInTwoSpaceIndentedFileChangesOnlyTheChainWithCrLf()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(ChainOfStatementInTwoSpaceIndentedFileTestData),
                     NormalizeToCarriageReturnLineFeed(ChainOfStatementInTwoSpaceIndentedFileResultData),
                     Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a chain that is the expression of a statement indented off its depth changes only the chain:
    /// the anonymous method's block keeps its place relative to the statement's first token, and the blank lines in front of
    /// the statement and the space in front of its semicolon stay as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-CHAIN-009")]
    public async Task VerifyFixOfChainOfStatementOffItsDepthChangesOnlyTheChain()
    {
        const string testData = """
                                using System;

                                internal class Builder
                                {
                                    internal Builder A(Action action) => this;
                                }

                                internal class Example
                                {
                                    private static void M(Builder b)
                                    {
                                    }

                                    private static void Run(Builder b)
                                    {
                                        M(b);


                                          b
                                            {|#0:.|}A(delegate
                                               {
                                                   M(b);
                                               }) ;
                                    }
                                }
                                """;
        const string resultData = """
                                  using System;

                                  internal class Builder
                                  {
                                      internal Builder A(Action action) => this;
                                  }

                                  internal class Example
                                  {
                                      private static void M(Builder b)
                                      {
                                      }

                                      private static void Run(Builder b)
                                      {
                                          M(b);


                                            b.A(delegate
                                            {
                                                M(b);
                                            }) ;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the layout the RH5111 fix gives an assignment behind a conditional access with wrapped links, which is the
    /// layout formatting the document gives it, is not reported by RH5112
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyNoDiagnosticForAssignmentsBehindAConditionalAccessLaidOutAsTheDocument()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?.B
                                         .C = 5;
                                        a?[0].C = 5;
                                        a?.B?.B
                                            .C = 5;
                                        a?.B
                                         .B
                                         .C = 5;
                                        a.B?.B
                                           .C = 5;
                                        a?[0].B
                                             .C = 5;
                                    }
                                }
                                """;

        await Verify(testData);
        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Create()
                                  {
                                      return new Builder()
                                          .UseLogging(new Builder()
                                              .UseValidation()
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

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies the spec example: the fix of a chain in the condition of an if statement joins only the chain, while
    /// the rest of the condition and the body stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-009")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfChainInIfConditionLeavesTheRestOfTheConditionAndTheBodyAsWritten(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int x, dynamic a)
                                    {
                                        if (x==1 && a
                                            {|RH5112:.|}B())
                                        {
                                            int  y = 1;
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int x, dynamic a)
                                     {
                                         if (x==1 && a.B())
                                         {
                                             int  y = 1;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a chain that starts its line keeps the comment above it and inserts no blank line
    /// between that comment and the statement in front of it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-CHAIN-009")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfChainThatStartsItsLineInsertsNoBlankLineAboveTheCommentThatLeadsIt(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        int b = 0;
                                        // c
                                        a
                                            {|RH5112:.|}B()
                                            .C();
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(dynamic a)
                                     {
                                         int b = 0;
                                         // c
                                         a.B()
                                          .C();
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a chain in a statement indented with two tabs keeps the tabs and aligns the remaining
    /// link from the column the chain's first token keeps, each tab counted as one column
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-CHAIN-009")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixOfChainBehindTabsCountsEachTabAsOneColumn(string endOfLine)
    {
        const string testData = "class C\n{\n    object M(dynamic a)\n    {\n\t\tvar r = a\n            {|RH5112:.|}B()\n            .C();\n\n        return r;\n    }\n}";
        const string fixedData = "class C\n{\n    object M(dynamic a)\n    {\n\t\tvar r = a.B()\n           .C();\n\n        return r;\n    }\n}";

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a chain in the embedded statement of an if statement, which starts in the middle of the
    /// if statement's line, joins the first link onto the root's line, aligns the next link under it, and keeps the code
    /// in front of the chain as written, its double space included, in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-009")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfChainInEmbeddedStatementThatStartsMidLineKeepsTheCodeInFrontAsWritten(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    object M(dynamic a, bool k)
                                    {
                                        if(k)return  a
                                            {|RH5112:.|}B()
                                            .C();

                                        return null;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     object M(dynamic a, bool k)
                                     {
                                         if(k)return  a.B()
                                                       .C();

                                         return null;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a chain in a statement behind a case label on the label's line joins the first link onto
    /// the root's line, aligns the next link under it, and keeps the code in front of the chain as written, in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-009")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfChainInStatementBehindCaseLabelKeepsTheCodeInFrontAsWritten(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    object M(dynamic a, int k)
                                    {
                                        switch (k)
                                        {
                                            case 1:return  a
                                                {|RH5112:.|}B()
                                                .C();
                                        }

                                        return null;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     object M(dynamic a, int k)
                                     {
                                         switch (k)
                                         {
                                             case 1:return  a.B()
                                                             .C();
                                         }

                                         return null;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a chain in a statement of a lambda block that opens in the middle of a line joins the first
    /// link onto the root's line, aligns the next link under it, and keeps the code in front of the chain and behind it as
    /// written, in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5112-003")]
    [Requirement("SHARED-CHAIN-009")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfChainInLambdaBlockOpenedMidLineKeepsTheCodeAroundAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void M(dynamic a)
                                    {
                                        Run(()=>{ var r=a
                                            {|RH5112:.|}B()
                                            .C(); });
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void M(dynamic a)
                                     {
                                         Run(()=>{ var r=a.B()
                                                          .C(); });
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a chain whose root stands behind a comment that leads its line joins the first link onto
    /// the root's line and gives the chain the layout the formatter gives it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5112-003")]
    public async Task VerifyFixOfChainBehindLineLeadingCommentMatchesTheFormatter(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    string M(string s)
                                    {
                                        return
                                        /* c */ s
                                            {|RH5112:.|}Trim()
                                            .ToUpper();
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string M(string s)
                                     {
                                         return
                                         /* c */ s.Trim()
                                                  .ToUpper();
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);

        Assert.AreEqual(ConvertEndOfLine(fixedData, endOfLine), Format(ConvertEndOfLine(RemoveMarkup(testData), endOfLine)));
    }

    /// <summary>
    /// Verifies the change example: the chain the fix formats holds a comment in front of its conditional element access, which the
    /// formatter refuses, so no fix is offered and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5112-001")]
    [Requirement("FMT-UNSUP-014")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedElementAccess(string endOfLine)
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static string Run(int[] items)
                                    {
                                        return items // element
                                            ?[0]
                                            {|RH5112:.|}ToString();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the change example: a comment between the <c>?</c> and the <c>.</c> of a link withholds the fix, because the
    /// formatter refuses the statement the fix formats, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-015")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentInsideALinkOperator(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a
                                            {|RH5112:.|}B()? // note
                                            .C();

                                        return r;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the change example: a comment between the <c>.</c> of a link and its member name withholds the fix, because the
    /// formatter refuses the statement the fix formats, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-016")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBetweenTheDotAndTheMemberName(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a
                                            {|RH5112:.|}B(). // note
                                            C();

                                        return r;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that the rule's title names the first member access of a wrapped chain, not a call
    /// </summary>
    [TestMethod]
    public void VerifyTitleNamesFirstMemberAccess()
    {
        var descriptor = new RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer().SupportedDiagnostics.Single();

        Assert.AreEqual("Wrapped chains should keep the first member access on the original line.", descriptor.Title.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Verifies that a wrapped property read as the first link of a chain without any call is reported with the message
    /// that names the first member access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedPropertyFirstLinkIsReportedWithMemberAccessMessage()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Run(dynamic order)
                                    {
                                        var city = order
                                            {|#0:.|}Customer
                                            .Address;

                                        return city;
                                    }
                                }
                                """;
        const string resultData = """
                                  internal sealed class Example
                                  {
                                      private static object Run(dynamic order)
                                      {
                                          var city = order.Customer
                                                          .Address;

                                          return city;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, "Wrapped chains should keep the first member access on the original line."));
    }

    /// <summary>
    /// Verifies that the code fix offered for a wrapped property read as the first link is titled with the first wrapped
    /// member access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixTitleNamesFirstWrappedMemberAccess()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Run(dynamic order)
                                    {
                                        var city = order
                                            .Customer
                                            .Address;

                                        return city;
                                    }
                                }
                                """;
        var firstLinkDot = testData.IndexOf(".Customer", StringComparison.Ordinal);

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(firstLinkDot, 1)));

        Assert.HasCount(1, actions);
        Assert.AreEqual("Keep first wrapped member access on original line", actions[0].Title);
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
                                  Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}