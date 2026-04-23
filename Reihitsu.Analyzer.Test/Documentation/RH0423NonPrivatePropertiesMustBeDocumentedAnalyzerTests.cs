using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0423NonPrivatePropertiesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0423NonPrivatePropertiesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0423NonPrivatePropertiesMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertyWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  internal int {|#0:Value|} { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH0423NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0423MessageFormat));
    }
}