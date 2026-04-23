using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0408NonPrivateRecordsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0408NonPrivateRecordsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0408NonPrivateRecordsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForRecordWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal record {|#0:CustomerRecord|};
                              """;

        await Verify(source, Diagnostics(RH0408NonPrivateRecordsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0408MessageFormat));
    }
}