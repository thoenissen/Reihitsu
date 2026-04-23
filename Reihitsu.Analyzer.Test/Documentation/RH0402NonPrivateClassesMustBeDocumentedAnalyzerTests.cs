using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0402NonPrivateClassesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0402NonPrivateClassesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0402NonPrivateClassesMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForClassWithoutSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0402NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0402MessageFormat));
    }
}