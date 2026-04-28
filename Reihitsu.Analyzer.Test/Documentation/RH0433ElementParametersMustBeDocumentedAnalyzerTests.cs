using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0433ElementParametersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0433ElementParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0433ElementParametersMustBeDocumentedAnalyzer>
{
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

        await Verify(source, Diagnostics(RH0433ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0433MessageFormat));
    }
}