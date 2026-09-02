using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Verifies issue #699: once an uncommented, separately wrapped non-invoked prefix dot collapses
/// onto the chain root even though a comment sits above the first invoked link, both the raw,
/// as-typed source and the formatter's fixed output stay <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/>-clean,
/// and a second formatter pass is a no-op
/// </summary>
[TestClass]
public class RH5201Issue699Tests : FormatterTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Runs the formatter over the issue's reported input (a wrapped, uncommented non-invoked
    /// <c>.Prop</c> prefix dot, then a comment directly above the first invoked link) and asserts
    /// that both the raw and the fixed source are analyzer-clean, and that the fixed source is
    /// stable on a second formatter pass, under both LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterCollapsesWrappedPrefixDotAndStaysAnalyzerClean()
    {
        const string source = """
                              internal class Example
                              {
                                  internal Example Prop { get; set; }

                                  internal Example Foo()
                                  {
                                      return this;
                                  }

                                  internal Example Bar()
                                  {
                                      return this;
                                  }

                                  internal static Example Run(Example a)
                                  {
                                      var x = a
                                          .Prop
                                          // keep wrapped
                                          .Foo()
                                          .Bar();

                                      return x;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   internal class Example
                                   {
                                       internal Example Prop { get; set; }

                                       internal Example Foo()
                                       {
                                           return this;
                                       }

                                       internal Example Bar()
                                       {
                                           return this;
                                       }

                                       internal static Example Run(Example a)
                                       {
                                           var x = a.Prop

                                                    // keep wrapped
                                                    .Foo()
                                                    .Bar();

                                           return x;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource);
    }

    #endregion // Tests
}