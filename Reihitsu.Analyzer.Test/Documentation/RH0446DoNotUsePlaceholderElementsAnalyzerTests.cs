using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0446DoNotUsePlaceholderElementsAnalyzer"/> and
/// <see cref="RH0446DoNotUsePlaceholderElementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0446DoNotUsePlaceholderElementsAnalyzerTests : AnalyzerTestsBase<RH0446DoNotUsePlaceholderElementsAnalyzer, RH0446DoNotUsePlaceholderElementsCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies a diagnostic and code fix for a placeholder tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPlaceholderElement()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>This method {|#0:<placeholder>does work</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>This method does work.</summary>
                                   internal class TestClass
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH0446DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH0446MessageFormat));
    }

    #endregion // Members
}