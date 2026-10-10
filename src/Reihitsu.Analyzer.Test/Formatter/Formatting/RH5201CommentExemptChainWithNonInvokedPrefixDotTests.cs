using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Verifies that a chain whose first link is kept on its own line by a comment aligns every wrapped link to the
/// chain root token, and that <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/> measures links against the
/// same column, also when a non-invoked prefix link shares the line below the comment with the first call
/// </summary>
[TestClass]
public class RH5201CommentExemptChainWithNonInvokedPrefixDotTests : FormatterTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; } = null;

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Runs the actual formatter over an input built from (a chain root token, a wrapping
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
        var formattedRoot = await ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken)
                                                   .GetRootAsync(TestContext.CancellationToken);
        var formatted = formattedRoot.ToFullString();

        await Verify(formatted);
    }

    /// <summary>
    /// Verifies the full round trip: the raw, as-typed source reports <c>RH5201</c> on the two
    /// wrapped links that are not in the chain root's column and on the link that shares a line with
    /// a later call, the formatter's fixed output clears the diagnostics by starting every link on its
    /// own line in the chain root's column, and a second formatter pass is a no-op under both LF and CRLF
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
                                          {|#0:.|}Prop.Foo()
                                          {|#1:.|}Bar(){|#2:.|}Baz();

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
                                                   .Prop
                                                   .Foo()
                                                   .Bar()
                                                   .Baz();

                                           return x;
                                       }
                                   }
                                   """;

        await VerifyFormatter(source, fixedSource, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, "Method chains should be aligned.", 3));
    }

    #endregion // Tests
}