using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0425NonPrivateIndexersMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0425NonPrivateIndexersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0425NonPrivateIndexersMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForIndexerWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  internal int {|#0:this|}[int index] => index;
                              }
                              """;

        await Verify(source, Diagnostics(RH0425NonPrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0425MessageFormat));
    }
}