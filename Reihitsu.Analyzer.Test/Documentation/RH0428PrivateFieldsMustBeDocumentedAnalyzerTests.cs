using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0428PrivateFieldsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0428PrivateFieldsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0428PrivateFieldsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateFieldWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  private int {|#0:currentValue|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0428PrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0428MessageFormat));
    }
}