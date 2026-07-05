using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineFormatterTests : FormatterTestsBase<RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that the formatter keeps the first fluent call on the original line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
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
        const string fixedData = """
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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId, AnalyzerResources.RH5112MessageFormat));
    }

    #endregion // Tests
}