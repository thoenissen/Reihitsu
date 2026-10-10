using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Tests for the first link of a chain in <see cref="RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer"/>
/// </summary>
[TestClass]
public class RH5048FluentChainLinkAnalyzerTests : AnalyzerTestsBase<RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment in front of a wrapped property that is the chain's first link is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeWrappedPropertyFirstLinkIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x {|#0:// Comment|}
                                          .Items
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

        await Verify(source, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in front of the wrapped first link of a chain without calls is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeWrappedFirstLinkOfCallLessChainIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return order {|#0:// Comment|}
                                          .Customer
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

        await Verify(source, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in front of a wrapped first link in front of the first call is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeWrappedPrefixFirstLinkIsReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a {|#0:// Comment|}
                                          .Prop
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

        await Verify(source, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in front of a wrapped call is not reported when a property access in front of it is the chain's first link
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeWrappedCallAfterPropertyPrefixIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return x.Items // Comment
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

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a comment between the two characters of the first <c>?.</c> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInsideConditionalOperatorIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a? // Comment
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

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a comment in front of a wrapped conditional element access is not reported, since it is not a member access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeWrappedConditionalElementAccessIsNotReported()
    {
        const string source = """
                              internal sealed class Example
                              {
                                  private static object Run(dynamic a, dynamic x, dynamic order)
                                  {
                                      return a // Comment
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

        await Verify(source);
    }

    #endregion // Tests
}