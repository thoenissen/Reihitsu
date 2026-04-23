using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0416NonPrivateDelegatesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDelegateWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal delegate void {|#0:Handler|}();
                              """;

        await Verify(source, Diagnostics(RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0416MessageFormat));
    }
}