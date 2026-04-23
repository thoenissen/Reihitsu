using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for an enum member without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberInNonPrivateEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal enum Status
                              {
                                  {|#0:Active|},
                              }
                              """;

        await Verify(source, Diagnostics(RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0414MessageFormat));
    }
}