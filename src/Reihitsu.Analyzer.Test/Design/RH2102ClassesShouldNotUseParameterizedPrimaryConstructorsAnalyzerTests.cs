using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer"/>
/// </summary>
[TestClass]
public class RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzerTests : AnalyzerTestsBase<RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying parameterized class primary constructors trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForParameterizedClassPrimaryConstructors()
    {
        const string source = """
                              namespace Example;

                              internal class {|#0:Service|}(int timeout)
                              {
                              }

                              internal sealed class {|#1:Worker|}(string name, int retries)
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer.DiagnosticId, AnalyzerResources.RH2102MessageFormat, 2));
    }

    /// <summary>
    /// Verifying parameterless class primary constructors do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForParameterlessClassPrimaryConstructors()
    {
        const string source = """
                              namespace Example;

                              internal class Service()
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying regular class declarations do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForClassesWithoutPrimaryConstructors()
    {
        const string source = """
                              namespace Example;

                              internal class Service
                              {
                                  internal Service(int timeout)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying records are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRecords()
    {
        const string source = """
                              namespace Example;

                              internal record User(int Id, string Name);
                              internal record class Customer(int Id);
                              """;

        await Verify(source);
    }

    #endregion // Tests
}