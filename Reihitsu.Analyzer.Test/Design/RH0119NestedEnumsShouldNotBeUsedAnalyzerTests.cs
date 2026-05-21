using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0119NestedEnumsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0119NestedEnumsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0119NestedEnumsShouldNotBeUsedAnalyzer>
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

        await Verify(source, Diagnostics(RH0119NestedEnumsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0119MessageFormat));
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
    /// Verifying nested classes do not trigger RH0119 diagnostics
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