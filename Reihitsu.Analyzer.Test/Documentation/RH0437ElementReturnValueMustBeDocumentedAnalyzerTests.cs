using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0437ElementReturnValueMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0437ElementReturnValueMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0437ElementReturnValueMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a missing returns tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Gets the value.</summary>
                                  internal {|#0:int|} GetValue()
                                  {
                                      return 1;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0437ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0437MessageFormat));
    }
}