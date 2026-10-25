using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5301ObjectInitializerShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns object initializer braces and members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var value = new Example
                                     {
                                     Name = "Test"
                                     };
                                 }

                                 internal string Name { get; set; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Name = "Test"
                                                     };
                                     }

                                     internal string Name { get; set; }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 5, 21, 8, 10, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns target-typed object initializer braces and members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTargetTypedViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Example value = new()
                                     {
                                     Name = "Test"
                                     };
                                 }

                                 internal string Name { get; set; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Example value = new()
                                                         {
                                                             Name = "Test"
                                                         };
                                     }

                                     internal string Name { get; set; }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 5, 25, 8, 10, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifies that comment-prefixed object initializer anchors remain analyzer-clean and formatter-stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsCommentPrefixedInitializerStable()
    {
        const string source = """
                              internal class Example
                              {
                                  internal int Value { get; set; }

                                  private static void Method()
                                  {
                                      var value = new Example
                                                  /* Keep open. */ {
                                                      /* Keep value. */ Value = 1
                                                  /* Keep close. */ };
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter aligns the braces of an object creation whose <c>new</c> stands behind a comment that leads its
    /// line to <c>new</c>, where the analyzer measures it, and clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5301-001")]
    [Requirement("FMT-INDENT-022")]
    [Requirement("FMT-INDENT-030")]
    public async Task VerifyFormatterAlignsObjectCreationToNewBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             internal class P
                             {
                                 public int X { get; set; }
                             }

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var p =

                                     /* c */ {|#0:new P
                                     {
                                         X = 2
                                     }|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class P
                                 {
                                     public int X { get; set; }
                                 }

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var p =

                                         /* c */ new P
                                                 {
                                                     X = 2
                                                 };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5301MessageFormat));
    }

    #endregion // Tests
}