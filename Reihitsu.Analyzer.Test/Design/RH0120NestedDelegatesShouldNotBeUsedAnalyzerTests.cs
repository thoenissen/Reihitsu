using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0120NestedDelegatesShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0120NestedDelegatesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0120NestedDelegatesShouldNotBeUsedAnalyzer>
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

        await Verify(source, Diagnostics(RH0120NestedDelegatesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0120MessageFormat));
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
    /// Verifying nested classes do not trigger RH0120 diagnostics
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