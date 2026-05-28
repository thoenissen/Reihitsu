using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer"/>
/// </summary>
[TestClass]
public class RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzerTests : AnalyzerTestsBase<RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for parameter documentation in the wrong order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForParameterDocumentationInWrongOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <param name="firstValue">First value.</param>
                                  /// {|#0:<param name="missingValue">Missing value.</param>|}
                                  internal void TestMethod(int firstValue, int secondValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat));
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
                                  /// <param name="firstValue">First value.</param>
                                  /// {|#0:<param name="missingValue">Missing value.</param>|}
                                  internal void TestMethod(int firstValue, int secondValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}