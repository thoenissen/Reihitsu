using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2108NestedDelegatesShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2108NestedDelegatesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2108NestedDelegatesShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying nested delegates trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedDelegatesTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private delegate void {|#0:Nested|}(int value);
                              }
                              """;

        await Verify(source, Diagnostics(RH2108NestedDelegatesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2108MessageFormat));
    }

    /// <summary>
    /// Verifying top-level delegates do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTopLevelDelegatesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal delegate void TopLevel(int value);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying nested classes do not trigger RH2108 diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedClassesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private class Nested
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}