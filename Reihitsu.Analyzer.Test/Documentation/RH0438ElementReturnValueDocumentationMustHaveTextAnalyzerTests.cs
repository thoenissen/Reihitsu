using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0438ElementReturnValueDocumentationMustHaveTextAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0438ElementReturnValueDocumentationMustHaveTextAnalyzerTests : AnalyzerTestsBase<RH0438ElementReturnValueDocumentationMustHaveTextAnalyzer>
{
    /// <summary>
    /// Verifies a diagnostic is reported for an empty returns tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEmptyReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Gets the value.</summary>
                                  /// {|#0:<returns></returns>|}
                                  internal int GetValue()
                                  {
                                      return 1;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0438ElementReturnValueDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH0438MessageFormat));
    }
}