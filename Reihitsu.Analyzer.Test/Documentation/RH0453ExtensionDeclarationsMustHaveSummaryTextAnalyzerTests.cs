using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0453ExtensionDeclarationsMustHaveSummaryTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH0453ExtensionDeclarationsMustHaveSummaryTextAnalyzerTests : AnalyzerTestsBase<RH0453ExtensionDeclarationsMustHaveSummaryTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for an extension declaration without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingDocumentationOnExtensionDeclaration()
    {
        const string source = """
                              public static class Extensions
                              {
                                  {|#0:extension|}(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0453ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH0453MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an extension declaration with an empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEmptySummaryOnExtensionDeclaration()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// {|#0:<summary></summary>|}
                                  extension(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0453ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH0453MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a documented generic extension declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForGenericExtensionDeclarationWithSummary()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  extension<T>(IEnumerable<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              public static class Extensions
                              {
                                  {|#0:extension|}(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject);
                     });
    }

    #endregion // Tests
}