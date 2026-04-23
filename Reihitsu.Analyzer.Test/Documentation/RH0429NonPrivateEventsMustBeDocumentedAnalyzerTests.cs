using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0429NonPrivateEventsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0429NonPrivateEventsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0429NonPrivateEventsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEventWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Raises notifications.</summary>
                              internal class TestClass
                              {
                                  internal event System.Action {|#0:Changed|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0429NonPrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0429MessageFormat));
    }
}