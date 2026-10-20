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
    [Requirement("RH2108-001")]
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
    [Requirement("SHARED-NESTED-001")]
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
    [Requirement("RH2108-001")]
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

    /// <summary>
    /// Verifying that delegates nested in a class and in an interface are reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2108-001")]
    [Requirement("SHARED-NESTED-001")]
    [Requirement("SHARED-NESTED-002")]
    public async Task VerifyDelegatesNestedInClassAndInterfaceAreReported()
    {
        const string source = """
                              class Outer
                              {
                                  delegate void {|#0:Callback|}(int value);
                              }

                              interface I
                              {
                                  delegate int {|#1:Selector|}();
                              }

                              record R
                              {
                                  delegate void {|#2:InRecord|}();
                              }
                              """;

        await Verify(source, Diagnostics(RH2108NestedDelegatesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2108MessageFormat, 3));
    }

    #endregion // Tests
}