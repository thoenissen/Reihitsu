using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0413PrivateEnumsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0413PrivateEnumsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0413PrivateEnumsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private enum {|#0:NestedStatus|}
                                  {
                                      Active
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0413PrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0413MessageFormat));
    }
}