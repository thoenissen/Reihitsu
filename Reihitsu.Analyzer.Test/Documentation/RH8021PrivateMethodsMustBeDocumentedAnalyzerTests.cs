using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8021PrivateMethodsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8021PrivateMethodsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8021PrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateMethodWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  private void {|#0:Execute|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an implicitly private method without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitlyPrivateMethodWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  void {|#0:Execute|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { private void Execute() { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an implicitly private extension block member without documentation, which defaults to private accessibility
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitlyPrivateExtensionMemberMethod()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text helpers.</summary>
                                  /// <param name="value">The source text.</param>
                                  extension(string value)
                                  {
                                      int {|#0:WordCount|}() => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8021PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8021MessageFormat));
    }

    #endregion // Tests
}