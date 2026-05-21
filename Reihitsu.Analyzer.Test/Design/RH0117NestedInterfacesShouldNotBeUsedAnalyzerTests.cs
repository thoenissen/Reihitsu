using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0117NestedInterfacesShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0117NestedInterfacesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0117NestedInterfacesShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying nested interfaces trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedInterfacesTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private interface {|#0:INested|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0117NestedInterfacesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0117MessageFormat));
    }

    /// <summary>
    /// Verifying top-level interfaces do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTopLevelInterfacesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal interface ITopLevel
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying nested classes do not trigger RH0117 diagnostics
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