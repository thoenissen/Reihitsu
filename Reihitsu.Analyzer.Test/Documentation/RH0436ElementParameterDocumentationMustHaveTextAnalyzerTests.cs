using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0436ElementParameterDocumentationMustHaveTextAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0436ElementParameterDocumentationMustHaveTextAnalyzerTests : AnalyzerTestsBase<RH0436ElementParameterDocumentationMustHaveTextAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for an empty parameter tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEmptyParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<param name="firstValue"></param>|}
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0436ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH0436MessageFormat));
    }
}