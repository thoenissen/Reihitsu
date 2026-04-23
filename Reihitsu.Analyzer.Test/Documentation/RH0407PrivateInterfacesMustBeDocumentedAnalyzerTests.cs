using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0407PrivateInterfacesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0407PrivateInterfacesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0407PrivateInterfacesMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateInterfaceWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private interface {|#0:INestedContract|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0407PrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0407MessageFormat));
    }
}