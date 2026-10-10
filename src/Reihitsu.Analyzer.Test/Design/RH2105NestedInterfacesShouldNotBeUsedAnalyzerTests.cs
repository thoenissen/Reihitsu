using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2105NestedInterfacesShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2105NestedInterfacesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2105NestedInterfacesShouldNotBeUsedAnalyzer>
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

        await Verify(source, Diagnostics(RH2105NestedInterfacesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2105MessageFormat));
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
    /// Verifying nested classes do not trigger RH2105 diagnostics
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