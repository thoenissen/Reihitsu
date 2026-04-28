using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0424PrivatePropertiesMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0424PrivatePropertiesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0424PrivatePropertiesMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivatePropertyWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  private int {|#0:Value|} { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH0424PrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0424MessageFormat));
    }
}