using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0430PrivateEventsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0430PrivateEventsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0430PrivateEventsMustBeDocumentedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateEventWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Raises notifications.</summary>
                              internal class TestClass
                              {
                                  private event System.Action {|#0:Changed|};
                              }
                              """;

        await Verify(source, Diagnostics(RH0430PrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0430MessageFormat));
    }

    #endregion // Members
}