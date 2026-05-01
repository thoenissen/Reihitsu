using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0419PrivateConstructorsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0419PrivateConstructorsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0419PrivateConstructorsMustBeDocumentedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrivateConstructorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  private {|#0:TestClass|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0419PrivateConstructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0419MessageFormat));
    }

    #endregion // Members
}