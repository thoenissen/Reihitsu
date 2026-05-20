using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0115StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0115StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzerTests : AnalyzerTestsBase<RH0115StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying parameterized struct primary constructors trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForParameterizedStructPrimaryConstructors()
    {
        const string source = """
                              namespace Example;

                              internal struct {|#0:Dimensions|}(int width, int height)
                              {
                              }

                              internal readonly struct {|#1:Range|}(int start, int end)
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0115StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer.DiagnosticId, AnalyzerResources.RH0115MessageFormat, 2));
    }

    /// <summary>
    /// Verifying parameterless struct primary constructors do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForParameterlessStructPrimaryConstructors()
    {
        const string source = """
                              namespace Example;

                              internal struct Dimensions()
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying regular struct declarations do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForStructsWithoutPrimaryConstructors()
    {
        const string source = """
                              namespace Example;

                              internal struct Dimensions
                              {
                                  internal Dimensions(int width, int height)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying record structs are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRecordStructs()
    {
        const string source = """
                              namespace Example;

                              internal record struct Dimensions(int Width, int Height);
                              """;

        await Verify(source);
    }

    #endregion // Tests
}