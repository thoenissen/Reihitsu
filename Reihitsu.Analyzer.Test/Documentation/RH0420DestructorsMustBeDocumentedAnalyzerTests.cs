using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0420DestructorsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0420DestructorsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0420DestructorsMustBeDocumentedAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDestructorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Releases resources.</summary>
                              internal class TestClass
                              {
                                  ~{|#0:TestClass|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0420DestructorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0420MessageFormat));
    }
}