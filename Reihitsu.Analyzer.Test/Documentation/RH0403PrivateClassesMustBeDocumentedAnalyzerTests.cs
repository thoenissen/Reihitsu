using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0403PrivateClassesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0403PrivateClassesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0403PrivateClassesMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private class {|#0:NestedClass|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0403PrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0403MessageFormat));
    }
}