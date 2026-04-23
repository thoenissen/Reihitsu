using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0415PrivateEnumMembersMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0415PrivateEnumMembersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0415PrivateEnumMembersMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for an enum member in a private enum without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberInPrivateEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class Container
                              {
                                  private enum Status
                                  {
                                      {|#0:Active|},
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0415PrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0415MessageFormat));
    }
}