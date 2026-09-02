using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5201MethodChainsShouldBeAlignedFormatterTests : FormatterTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter wraps and aligns method chains consistently
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System.Linq;
                             
                             internal class Example
                             {
                                 internal int[] Filter(int[] values)
                                 {
                                     return values.Where(obj => obj > 0){|#0:.|}Select(obj => obj)
                                                  .ToArray();
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Linq;
                                 
                                 internal class Example
                                 {
                                     internal int[] Filter(int[] values)
                                     {
                                         return values.Where(obj => obj > 0)
                                                      .Select(obj => obj)
                                                      .ToArray();
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's own output for a wrapped chain whose first invoked link is
    /// introduced by <c>?.</c> after a plain (non-invoked) property access satisfies RH5201 and stays
    /// stable on a second pass, closing the format-fix-format oscillation reported in issue #680
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesConditionalAccessAfterPlainPropertyAccessViolation()
    {
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             internal sealed class Example
                             {
                                 internal sealed class Choice
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Option
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Request
                                 {
                                     public IEnumerable<Choice> Choices { get; set; }
                                 }

                                 internal static List<Option> Convert(Request request)
                                 {
                                     var options = request.Choices?.Select(choice => new Option
                                                                                     {
                                                                                         Name = choice.Name
                                                                                     })
                                                          {|#0:.|}ToList();

                                     return options;
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 internal sealed class Example
                                 {
                                     internal sealed class Choice
                                     {
                                         public string Name { get; set; }
                                     }

                                     internal sealed class Option
                                     {
                                         public string Name { get; set; }
                                     }

                                     internal sealed class Request
                                     {
                                         public IEnumerable<Choice> Choices { get; set; }
                                     }

                                     internal static List<Option> Convert(Request request)
                                     {
                                         var options = request.Choices?.Select(choice => new Option
                                                                                         {
                                                                                             Name = choice.Name
                                                                                         })
                                                                      .ToList();

                                         return options;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's own output for a wrapped chain whose first invoked link is
    /// introduced by <c>!.</c> after a plain (non-invoked) property access satisfies RH5201 and stays
    /// stable on a second pass. <c>!.</c> selects a different anchor token than <c>?.</c>
    /// (<see cref="Reihitsu.Core.FluentChainUtilities.GetInvokedLinkOperator"/> returns the <c>!</c>,
    /// not the following <c>.</c>), so this is the arm most able to drift from the analyzer (issue #680)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesNullForgivingAfterPlainPropertyAccessViolation()
    {
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             internal sealed class Example
                             {
                                 internal sealed class Choice
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Option
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Request
                                 {
                                     public IEnumerable<Choice> Choices { get; set; }
                                 }

                                 internal static List<Option> Convert(Request request)
                                 {
                                     var options = request.Choices!.Select(choice => new Option
                                                                                     {
                                                                                         Name = choice.Name
                                                                                     })
                                                          {|#0:.|}ToList();

                                     return options;
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 internal sealed class Example
                                 {
                                     internal sealed class Choice
                                     {
                                         public string Name { get; set; }
                                     }

                                     internal sealed class Option
                                     {
                                         public string Name { get; set; }
                                     }

                                     internal sealed class Request
                                     {
                                         public IEnumerable<Choice> Choices { get; set; }
                                     }

                                     internal static List<Option> Convert(Request request)
                                     {
                                         var options = request.Choices!.Select(choice => new Option
                                                                                         {
                                                                                             Name = choice.Name
                                                                                         })
                                                                      .ToList();

                                         return options;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's output for a chain whose own first dot is wrapped is
    /// RH5201-clean, so the formatter and the analyzer no longer disagree on this shape (issue #683)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterOutputForWrappedFirstChainDotIsClean()
    {
        const string source = """
                              internal class Example
                              {
                                  internal string Prop { get; set; }

                                  internal static string Run(Example a)
                                  {
                                      return a.Prop?.ToString()
                                                   .Trim();
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter's output for a chain rooted in a single-line array initializer is
    /// RH5201-clean, so the code fix no longer moves a column the formatter puts back (issue #684)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterOutputForChainRootedInSingleLineInitializerIsClean()
    {
        const string source = """
                              using System.Linq;

                              internal class Example
                              {
                                  internal static int[] Run()
                                  {
                                      return new[] { 1, 2, 3 }.Select(item => item)
                                                              .ToArray();
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter's output for a chain whose member name was split from its own dot
    /// is RH5201-clean once the name is rejoined (issue #685)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterOutputForRejoinedSplitMemberNameIsClean()
    {
        const string source = """
                              internal class Example
                              {
                                  internal Example Prop { get; set; }

                                  internal Example Call()
                                  {
                                      return this;
                                  }

                                  internal Example Then()
                                  {
                                      return this;
                                  }

                                  internal static Example Run(Example a)
                                  {
                                      return a?.Prop
                                              .Call()
                                              .Then();
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    #endregion // Tests
}