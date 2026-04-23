using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0411PrivateRecordStructsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0411PrivateRecordStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0411PrivateRecordStructsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateRecordStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private record struct {|#0:NestedMeasurement|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0411PrivateRecordStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0411MessageFormat));
    }
}