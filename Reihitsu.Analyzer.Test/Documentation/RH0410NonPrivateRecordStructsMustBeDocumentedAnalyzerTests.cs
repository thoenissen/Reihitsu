using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0410NonPrivateRecordStructsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0410NonPrivateRecordStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0410NonPrivateRecordStructsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForRecordStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal record struct {|#0:Measurement|};
                              """;

        await Verify(source, Diagnostics(RH0410NonPrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0410MessageFormat));
    }
}