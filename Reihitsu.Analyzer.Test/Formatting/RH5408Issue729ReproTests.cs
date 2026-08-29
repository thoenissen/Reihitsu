using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Reproduction-gate regression tests for issue #729 (RH5408 code fix loops without progress when an individual
/// accessor carries its own attribute in combination with a property-level attribute)
/// </summary>
[TestClass]
public class RH5408Issue729ReproTests : AnalyzerTestsBase<RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer, RH5408SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider>
{
    #region Fields

    /// <summary>
    /// The issue's minimal reproducible example, without analyzer-test markup
    /// </summary>
    private const string IssueInput = """
                                      sealed class ExampleAttribute : System.Attribute;

                                      internal class Example
                                      {
                                          [Example]
                                          public int Count
                                          {
                                              [Example]
                                              get;
                                              [Example]
                                              set;
                                          }
                                      }
                                      """;

    #endregion // Fields

    #region Tests

    /// <summary>
    /// Verifies that applying the registered code fix once to the issue's minimal reproducible example produces
    /// text matching the issue's reported "Actual Behavior" (still multi-line, not the single-line form the
    /// declaration's own accessor list started from)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleCodeFixApplicationMatchesIssueReportedActualBehavior()
    {
        const string expectedAfterOneApplication = """
                                                   sealed class ExampleAttribute : System.Attribute;

                                                   internal class Example
                                                   {
                                                       [Example]
                                                       public int Count { [Example]
                                                           get; [Example]
                                                           set; }
                                                   }
                                                   """;

        var afterOneApplication = await ApplyCodeFixAsync(IssueInput);

        Assert.AreEqual(expectedAfterOneApplication, afterOneApplication, "Single code fix application should match the issue's reported broken output.");
    }

    /// <summary>
    /// Verifies that the RH5408 diagnostic is still reported at the same property declaration after one code fix
    /// application, so the fix has not silenced the diagnostic it was invoked for
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticStillReportedAfterOneCodeFixApplication()
    {
        const string sourceAfterOneApplication = """
                                                 sealed class ExampleAttribute : System.Attribute;

                                                 internal class Example
                                                 {
                                                     [Example]
                                                     {|#0:public int Count { [Example]
                                                         get; [Example]
                                                         set; }|}
                                                 }
                                                 """;

        await Verify(sourceAfterOneApplication,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that applying the code fix a second time, to its own first-application output, produces
    /// byte-for-byte identical text, so the fix makes no further progress once applied (LF line endings)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySecondCodeFixApplicationMakesNoProgressLf()
    {
        var afterOneApplication = await ApplyCodeFixAsync(IssueInput);
        var afterTwoApplications = await ApplyCodeFixAsync(afterOneApplication);

        Assert.AreEqual(afterOneApplication, afterTwoApplications, "A second code fix application should not change the text produced by the first.");
    }

    /// <summary>
    /// Verifies that applying the code fix a second time, to its own first-application output, produces
    /// byte-for-byte identical text under CRLF line endings, so the non-convergence is not an LF-only artifact
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySecondCodeFixApplicationMakesNoProgressCrlf()
    {
        var crlfInput = NormalizeToCarriageReturnLineFeed(IssueInput);
        var afterOneApplication = await ApplyCodeFixAsync(crlfInput);
        var afterTwoApplications = await ApplyCodeFixAsync(afterOneApplication);

        Assert.AreEqual(afterOneApplication, afterTwoApplications, "A second code fix application should not change the text produced by the first, under CRLF line endings.");
    }

    #endregion // Tests
}