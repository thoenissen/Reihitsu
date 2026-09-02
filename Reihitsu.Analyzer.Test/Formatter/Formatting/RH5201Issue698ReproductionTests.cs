using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Reproduction test for issue #698: a comment-exempt chain aligns its continuation dots to the
/// chain root token, while <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/> measures its
/// reference column from the first invoked link. For a chain that carries a non-invoked prefix dot
/// on the commented line, the two columns can differ
/// </summary>
[TestClass]
public class RH5201Issue698ReproductionTests : FormatterTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Runs the actual formatter over the issue's reported input (a chain root token, a wrapping
    /// comment, a non-invoked <c>.Prop</c> prefix on the commented line, then the invoked
    /// <c>.Foo()</c>/<c>.Bar()</c>/<c>.Baz()</c> links) and asserts that the formatter's own output is
    /// reported clean by <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterOutputForCommentExemptChainWithNonInvokedPrefixIsRH5201Clean()
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

                                  internal Example Baz()
                                  {
                                      return this;
                                  }

                                  internal static Example Run(Example a)
                                  {
                                      var x = a
                                          // keep wrapped
                                          .Prop.Foo()
                                          .Bar().Baz();

                                      return x;
                                  }
                              }
                              """;

        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
        var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken)
                                         .GetRoot(TestContext.CancellationToken)
                                         .ToFullString();

        await Verify(formatted);
    }

    /// <summary>
    /// Verifies the full round trip: the raw, as-typed source reports <c>RH5201</c> on the two
    /// invoked links that do not match the first invoked link's column, the formatter's fixed
    /// output clears the diagnostic by aligning every continuation dot to that first invoked
    /// link's own rendered column (not the chain-root column), and a second formatter pass is a
    /// no-op under both LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesCommentExemptChainWithNonInvokedPrefixAndStaysIdempotent()
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

                                  internal Example Baz()
                                  {
                                      return this;
                                  }

                                  internal static Example Run(Example a)
                                  {
                                      var x = a
                                          // keep wrapped
                                          .Prop.Foo()
                                          {|#0:.|}Bar(){|#1:.|}Baz();

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

                                       internal Example Baz()
                                       {
                                           return this;
                                       }

                                       internal static Example Run(Example a)
                                       {
                                           var x = a

                                                   // keep wrapped
                                                   .Prop.Foo()
                                                        .Bar()
                                                        .Baz();

                                           return x;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, "Method chains should be aligned.", 2));
    }

    #endregion // Tests
}