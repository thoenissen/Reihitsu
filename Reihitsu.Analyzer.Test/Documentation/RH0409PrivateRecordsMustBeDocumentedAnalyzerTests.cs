using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0409PrivateRecordsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0409PrivateRecordsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0409PrivateRecordsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private record {|#0:NestedRecord|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0409PrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0409MessageFormat));
    }
}