using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8101ElementParametersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8101ElementParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8101ElementParametersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing parameter comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <param name="secondValue">Second value.</param>
                                  internal void TestMethod(int {|#0:firstValue|}, int secondValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <param name="secondValue">Second value.</param>
                                  internal void TestMethod(int {|#0:firstValue|}, int secondValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies extension member method parameters are still validated by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExtensionMemberParameterWithoutDocumentation()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text helpers.</summary>
                                  /// <param name="value">The source text.</param>
                                  extension(string value)
                                  {
                                      /// <summary>Creates a token.</summary>
                                      /// <param name="offset">The start offset.</param>
                                      public int Parse(int {|#0:length|}, int offset) => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    #endregion // Tests
}