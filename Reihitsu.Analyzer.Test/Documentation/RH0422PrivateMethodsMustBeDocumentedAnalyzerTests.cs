using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0422PrivateMethodsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0422PrivateMethodsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0422PrivateMethodsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateMethodWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  private void {|#0:Execute|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0422PrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0422MessageFormat));
    }
}