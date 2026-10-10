using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Analyzer and formatter parity tests for the links of a wrapped chain in <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5201FluentChainLinkFormatterTests : FormatterTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a property access after a call in a wrapped chain is reported when it does not start its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyAfterCallSharingLineIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.Where()
                                              .First(){|#0:.|}Name;
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
                                           return x.Where()
                                                   .First()
                                                   .Name;
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a conditional property access after a call in a wrapped chain is reported on its question mark when it does not start its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConditionalPropertyAfterCallSharingLineIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.Where()
                                              .FirstOrDefault(){|#0:?|}.Name;
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
                                           return x.Where()
                                                   .FirstOrDefault()
                                                   ?.Name;
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a property access after a call that follows a prefix is reported when it does not start its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyAfterWrappedCallBehindPrefixIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.Items
                                              .Where(){|#0:.|}Count;
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
                                                   .Where()
                                                   .Count;
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that every member access after the first call is reported when it shares the root line of a wrapped chain
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMemberAccessesAfterFirstCallSharingRootLineAreReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.B(){|#0:.|}C{|#1:.|}D()
                                              .E();
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
                                           return a.B()
                                                   .C
                                                   .D()
                                                   .E();
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a wrapped first link makes the whole chain wrapped, so a later call sharing a line is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCallsBehindWrappedPrefixFirstLinkAreReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          .Prop.Foo(){|#0:.|}Bar();
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
                                           return a.Prop.Foo()
                                                        .Bar();
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a property access sharing the line of a wrapped first call is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyBehindWrappedFirstCallIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          .B(){|#0:.|}C;
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
                                           return a.B()
                                                   .C;
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a call sharing the line of a wrapped first call is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCallBehindWrappedFirstCallIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          .B(){|#0:.|}C();
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
                                           return a.B()
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped call after a property prefix is reported when it is not aligned with the first link
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedCallAfterPropertyPrefixIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.Items.Count
                                          {|#0:.|}ToString();
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
                                           return x.Items.Count
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that wrapped member accesses of a chain without calls are reported when they are not aligned with the first link
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedLinksOfCallLessChainAreReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return order.Customer
                                          {|#0:.|}Address
                                                {|#1:.|}City;
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
                                                       .Address
                                                       .City;
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a wrapped member access in front of the first call is reported when it is not aligned with the first link
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedPrefixLinkIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.B
                                          {|#0:.|}C.D;
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
                                           return a.B
                                                   .C.D;
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a chain whose calls are wrapped behind a conditionally indexed property is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyChainAfterConditionallyIndexedPrefixIsClean()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.P?[0]
                                              .B()
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

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that a wrapped call aligned with the first call behind a conditional property is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCallAlignedWithFirstCallBehindConditionalPrefixIsClean()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a?.B.C()
                                                 .D();
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
    /// Verifies that wrapped links written as <c>!?.</c> and <c>.</c> aligned with the first link are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNullForgivingConditionalLinksAlignedWithPrefixAreClean()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.X
                                              !?.B()
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

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that a wrapped call aligned with the exclamation mark of a <c>!?.</c> first link is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCallAlignedWithNullForgivingConditionalFirstLinkIsClean()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a!?.B()
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

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that a conditional element access sharing the line of a call is not reported, since it belongs to the call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConditionalElementAccessBehindCallIsNotALink()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.B()?[0]
                                              ?.C();
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
    /// Verifies that an element access on its own line is not reported, since it belongs to the element in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedElementAccessIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.B()
                                          [0]
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
                                           return x.B()[0]
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
    /// Verifies that an argument list on its own line is not reported, since it belongs to the element in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedArgumentListIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.B
                                          (1)
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
                                           return x.B(1)
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
    /// Verifies that a call indented deeper than the wrapped first call is reported independently of the wrapped first call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIndentedCallBehindWrappedFirstCallIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          .B()
                                            {|#0:.|}C();
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
                                           return a.B()
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a call aligned with a wrapped first call is not reported, since only the first link is misplaced
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCallAlignedWithWrappedFirstCallIsClean()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          .B()
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
                                           return a.B()
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
    /// Verifies that a line break between the null-forgiving operator and the dot of a later link is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineBreakInsideNullForgivingLinkIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.Y(){|#0:!|}
                                          .Z();
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
                                           return x.Y()
                                                   !.Z();
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a line break between the question mark and the dot of a later link is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineBreakInsideConditionalLinkIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.B(){|#0:?|}
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
                                           return a.B()
                                                   ?.C();
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a line break between the two characters of the first <c>?.</c> is reported and makes the chain wrapped
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineBreakInsideFirstConditionalLinkIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a{|#0:?|}
                                          .B(){|#1:.|}C();
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
                                           return a?.B()
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a call directly behind the closing delimiter of a multi-line raw string literal does not count as starting a line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCallDirectlyBehindMultiLineRawStringDoesNotStartALine()
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
    /// Verifies that a chain whose first link is kept on its own line by a comment aligns every wrapped link with the root, and that only a link sharing a line is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentKeptFirstLinkAlignsChainToRootColumn()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                             // Comment
                                             .Prop.Foo()
                                             .Bar(){|#0:.|}Baz();
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
                                           return a

                                                  // Comment
                                                  .Prop
                                                  .Foo()
                                                  .Bar()
                                                  .Baz();
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped conditional link on the left side of a null-conditional assignment is not reported when it is aligned with the first call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConditionalLinkOfNullConditionalAssignmentAlignedWithCallIsClean()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      a.B()
                                       ?.C = 5;

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
    /// Verifies that wrapped links on the left side of a null-conditional assignment are not reported when each starts its own line aligned with the first call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLinksOfNullConditionalAssignmentAlignedWithCallAreClean()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      a.B()
                                       ?.C()
                                       .D = 5;

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
    /// Verifies that a link whose line starts with a block comment is measured from the start of its line, so the formatted output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockCommentInFrontOfWrappedFirstLinkAlignsTheLineStart()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a
                                          /* block */ {|#0:.|}B()
                                          {|#1:.|}C();
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
                                           return a

                                                  /* block */ .B()
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a later link whose line starts with a block comment is measured from the start of its line, so the formatted output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockCommentInFrontOfWrappedLaterLinkAlignsTheLineStart()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a.B() /* first */
                                          /* second */ {|#0:?|}.C()
                                          {|#1:.|}D();
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
                                           return a.B() /* first */

                                                   /* second */ ?.C()
                                                   .D();
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

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat, 2));
    }

    #endregion // Tests
}