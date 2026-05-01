using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0421NonPrivateMethodsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0421NonPrivateMethodsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0421NonPrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMethodWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Creates values.</summary>
                              internal class TestClass
                              {
                                  internal void {|#0:Execute|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0421NonPrivateMethodsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0421MessageFormat));
    }

    #endregion // Members
}