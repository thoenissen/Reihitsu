using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0405PrivateStructsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0405PrivateStructsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0405PrivateStructsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateStructWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private struct {|#0:NestedStruct|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0405PrivateStructsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0405MessageFormat));
    }
}