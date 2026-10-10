using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Analyzer and formatter parity tests for the first link of a chain in <see cref="RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5112FluentChainLinkFormatterTests : FormatterTestsBase<RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a first link that is a non-invoked member access is reported when it is wrapped, and that the formatter joins it onto the root line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedPrefixFirstLinkIsReportedAndJoined()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          {|#0:.|}Prop
                                          .Foo();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a.Prop
                                                   .Foo();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped property in front of the first call is reported as the chain's first link
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedPrefixFirstLinkBeforeCallsIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x
                                          {|#0:.|}Items.Where()
                                          .First();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return x.Items.Where()
                                                         .First();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a chain without any call is reported when its first member access is wrapped
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedFirstLinkOfCallLessChainIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return order
                                          {|#0:.|}Customer
                                          .Address;
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return order.Customer
                                                       .Address;
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped first link in front of a conditional access is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedFirstLinkInFrontOfConditionalAccessIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          {|#0:.|}B?.C;
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a.B?.C;
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a first link whose operator starts with a null-forgiving operator is reported on that operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedNullForgivingFirstLinkIsReportedOnExclamationMark()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          {|#0:!|}.B?.C();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a!.B?.C();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped first link written as <c>!?.</c> is reported on its exclamation mark
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedNullForgivingConditionalFirstLinkIsReportedOnExclamationMark()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          {|#0:!|}?.B();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a!?.B();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped first link written as <c>?.</c> is reported on its question mark
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedConditionalFirstLinkIsReportedOnQuestionMark()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return Get()
                                          {|#0:?|}.Bar;
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return Get()?.Bar;
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that the first call may stay wrapped when the root ends in a property access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedCallAfterPropertyPrefixIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.Items
                                          .Where();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return x.Items
                                                   .Where();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the first call may stay wrapped when the root ends in a property access followed by an index access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedCallAfterIndexedPropertyPrefixIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.Items[0]
                                          .ToString();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return x.Items[0]
                                                   .ToString();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the first call may stay wrapped when the root ends in a property access in front of a link written as <c>!.</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedNullForgivingCallAfterPropertyPrefixIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.B
                                              !.C();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the first call may stay wrapped when the root ends in a property access followed by a conditional index access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedCallAfterConditionallyIndexedPropertyPrefixIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.P?[0]
                                          .B();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a.P?[0]
                                                   .B();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the first call may stay wrapped when the root ends in a conditional property access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedCallAfterConditionalPropertyPrefixIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a?.B
                                          .C();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a?.B
                                                   .C();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a wrapped conditional element access is not reported, since it is not a member access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedConditionalElementAccessWithoutLinkIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          ?[0];
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a?[0];
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the first member access behind a conditional element access is reported when it is wrapped
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedLinkBehindConditionalElementAccessIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          ?[0]
                                          {|#0:.|}B();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a?[0].B();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a line break between the two characters of the first <c>?.</c> is not reported as a wrapped first link
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineBreakInsideFirstConditionalOperatorIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a?
                                          .B();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return a?.B();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a call directly behind the closing delimiter of a multi-line raw string literal is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCallDirectlyBehindMultiLineRawStringIsNotReported()
    {
        const string source = """"
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return """
                                             text
                                             """?.Trim();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """";

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that a chain whose first call follows a multi-line raw string literal directly is not reported when only a later call is wrapped
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCallBehindMultiLineRawStringWithWrappedSecondCallIsNotReported()
    {
        const string source = """"
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return """
                                             text
                                             """.Trim()
                                                .Trim();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """";

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that a call wrapped onto the line after the closing delimiter of a multi-line raw string literal is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedCallBehindMultiLineRawStringIsReported()
    {
        const string source = """"
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return """
                                             text
                                             """
                                          {|#0:.|}Trim()
                                          .Trim();
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """";
        const string fixedSource = """"
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return """
                                                  text
                                                  """.Trim()
                                                     .Trim();
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """";

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped first link of a chain inside an argument is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedFirstLinkInsideArgumentIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return Use(a
                                          {|#0:.|}Prop
                                          .Foo());
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal sealed class Example
                                   {
                                       private static object Run(dynamic a, dynamic x, dynamic order)
                                       {
                                           return Use(a.Prop
                                                       .Foo());
                                       }

                                       private static dynamic Get()
                                       {
                                           return null;
                                       }

                                       private static object Use(object value)
                                       {
                                           return value;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped member access on the left side of a null-conditional assignment is not reported when a conditional property access comes first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedLinkOfNullConditionalAssignmentAfterConditionalPrefixIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      a?.B
                                       .C = 5;

                                      return a;
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that a wrapped member access on the left side of a compound null-conditional assignment is not reported when the chain starts with a property access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedLinkOfCompoundNullConditionalAssignmentAfterPrefixIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      x.Y?.B
                                       .C += 1;

                                      return x;
                                  }

                                  private static dynamic Get()
                                  {
                                      return null;
                                  }

                                  private static object Use(object value)
                                  {
                                      return value;
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    #endregion // Tests
}