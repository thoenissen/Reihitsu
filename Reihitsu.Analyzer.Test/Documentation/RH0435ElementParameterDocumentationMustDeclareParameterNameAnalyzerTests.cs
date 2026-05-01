using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0435ElementParameterDocumentationMustDeclareParameterNameAnalyzer"/>
/// </summary>
[TestClass]
public class RH0435ElementParameterDocumentationMustDeclareParameterNameAnalyzerTests : AnalyzerTestsBase<RH0435ElementParameterDocumentationMustDeclareParameterNameAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies a diagnostic is reported for a missing parameter name attribute
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingParameterName()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<param>First value.</param>|}
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0435ElementParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH0435MessageFormat));
    }

    #endregion // Members
}