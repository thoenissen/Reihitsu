using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0412NonPrivateEnumsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0412NonPrivateEnumsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0412NonPrivateEnumsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal enum {|#0:Status|}
                              {
                                  Active
                              }
                              """;

        await Verify(source, Diagnostics(RH0412NonPrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0412MessageFormat));
    }
}