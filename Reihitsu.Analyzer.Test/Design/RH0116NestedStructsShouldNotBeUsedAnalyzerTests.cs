using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0116NestedStructsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0116NestedStructsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0116NestedStructsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying nested structs trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedStructsTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private struct {|#0:Nested|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0116NestedStructsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0116MessageFormat));
    }

    /// <summary>
    /// Verifying top-level structs do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTopLevelStructsDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal struct TopLevel
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying nested classes do not trigger RH0116 diagnostics
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