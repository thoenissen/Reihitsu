using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0454ExtensionDeclarationParametersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0454ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0454ExtensionDeclarationParametersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing extension declaration parameter comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingExtensionDeclarationParameterDocumentation()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text extensions.</summary>
                                  extension(string {|#0:value|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0454ExtensionDeclarationParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0454MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a fully documented generic extension declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDocumentedGenericExtensionDeclaration()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  /// <param name="values">The values to inspect.</param>
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
                                  /// <summary>Provides text extensions.</summary>
                                  extension(string {|#0:value|})
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