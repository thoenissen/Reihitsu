using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0455ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0455ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0455ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing extension declaration type parameter comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingTypeParameterDocumentationOnExtensionDeclaration()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  extension<{|#0:T|}>(IEnumerable<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0455ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0455MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for documented extension declaration type parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDocumentedTypeParameterOnExtensionDeclaration()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  /// <typeparam name="T">The element type.</typeparam>
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
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  extension<{|#0:T|}>(IEnumerable<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}