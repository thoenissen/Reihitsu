using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0406NonPrivateInterfacesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForInterfaceWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal interface {|#0:ITestContract|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0406MessageFormat));
    }
}