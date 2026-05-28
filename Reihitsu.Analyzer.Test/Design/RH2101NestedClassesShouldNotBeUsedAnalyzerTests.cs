using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2101NestedClassesShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2101NestedClassesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2101NestedClassesShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that nested classes trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedClassesTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private class {|#0:Nested|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH2101NestedClassesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2101MessageFormat));
    }

    /// <summary>
    /// Verifying that static nested classes do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStaticNestedClassesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  public static class Helper
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that top-level classes do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTopLevelClassesDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that other nested type kinds do not trigger RH2101 diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOtherNestedTypeKindsDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private struct NestedStruct
                                  {
                                  }

                                  private interface INested
                                  {
                                  }

                                  private record NestedRecord(int Id);

                                  private enum NestedEnum
                                  {
                                      One,
                                  }

                                  private delegate void NestedDelegate();
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}