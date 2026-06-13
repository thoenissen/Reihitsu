using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8020NonPrivateMethodsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8020NonPrivateMethodsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8020NonPrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMethodWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  internal void {|#0:Execute|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { internal void Execute() { } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an implicitly private method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForImplicitlyPrivateMethod()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies extension block members are still validated by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForUndocumentedExtensionMemberMethod()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text helpers.</summary>
                                  /// <param name="value">The source text.</param>
                                  extension(string value)
                                  {
                                      public int {|#0:WordCount|}() => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8020MessageFormat));
    }

    #endregion // Tests
}