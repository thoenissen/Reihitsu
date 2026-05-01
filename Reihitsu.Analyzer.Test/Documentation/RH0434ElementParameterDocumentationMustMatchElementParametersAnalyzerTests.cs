using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0434ElementParameterDocumentationMustMatchElementParametersAnalyzer"/>
/// </summary>
[TestClass]
public class RH0434ElementParameterDocumentationMustMatchElementParametersAnalyzerTests : AnalyzerTestsBase<RH0434ElementParameterDocumentationMustMatchElementParametersAnalyzer>
{
    #region Members

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

        await Verify(source, Diagnostics(RH0434ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH0434MessageFormat));
    }

    #endregion // Members
}