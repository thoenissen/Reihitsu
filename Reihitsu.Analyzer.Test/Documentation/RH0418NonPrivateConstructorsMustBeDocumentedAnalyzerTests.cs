using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0418NonPrivateConstructorsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0418NonPrivateConstructorsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0418NonPrivateConstructorsMustBeDocumentedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForConstructorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  internal {|#0:TestClass|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0418NonPrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0418MessageFormat));
    }

    #endregion // Members
}