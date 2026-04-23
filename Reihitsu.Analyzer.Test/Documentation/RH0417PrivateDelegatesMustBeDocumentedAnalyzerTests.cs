using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0417PrivateDelegatesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0417PrivateDelegatesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0417PrivateDelegatesMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private delegate void {|#0:NestedHandler|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH0417PrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0417MessageFormat));
    }
}