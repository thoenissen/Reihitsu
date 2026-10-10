using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2107NestedEnumsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2107NestedEnumsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2107NestedEnumsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying nested enums trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedEnumsTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private enum {|#0:Nested|}
                                  {
                                      One,
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH2107NestedEnumsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2107MessageFormat));
    }

    /// <summary>
    /// Verifying top-level enums do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTopLevelEnumsDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal enum TopLevel
                              {
                                  One,
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying nested classes do not trigger RH2107 diagnostics
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