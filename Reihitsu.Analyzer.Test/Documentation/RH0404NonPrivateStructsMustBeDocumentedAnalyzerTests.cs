using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0404NonPrivateStructsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0404NonPrivateStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0404NonPrivateStructsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal struct {|#0:TestStruct|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0404NonPrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0404MessageFormat));
    }
}