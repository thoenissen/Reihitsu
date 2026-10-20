using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer"/>
/// </summary>
[TestClass]
public class RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzerTests : AnalyzerTestsBase<RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying parameterized struct primary constructors trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2103-001")]
    [Requirement("RH2103-002")]
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

        await Verify(source, Diagnostics(RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer.DiagnosticId, AnalyzerResources.RH2103MessageFormat, 2));
    }

    /// <summary>
    /// Verifying parameterless struct primary constructors do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2103-001")]
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
    [Requirement("RH2103-001")]
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
    [Requirement("RH2103-001")]
    public async Task VerifyNoDiagnosticsForRecordStructs()
    {
        const string source = """
                              namespace Example;

                              internal record struct Dimensions(int Width, int Height);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that plain, readonly, and partial structs with primary-constructor parameters are reported on their
    /// identifier, and that empty primary constructors, structs without one, and record structs are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2103-001")]
    [Requirement("RH2103-002")]
    public async Task VerifyReadonlyAndPartialStructsWithPrimaryConstructorParametersAreReported()
    {
        const string source = """
                              struct {|#0:Size|}(int width, int height)
                              {
                                  public int Area => width * height;
                              }

                              readonly struct {|#1:Id|}(int value)
                              {
                                  public int Value => value;
                              }

                              partial struct {|#2:Part|}(int value)
                              {
                              }

                              struct Empty()
                              {
                              }

                              struct Plain
                              {
                              }

                              record struct Point(int X);
                              """;

        await Verify(source, Diagnostics(RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer.DiagnosticId, AnalyzerResources.RH2103MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that the diagnostic is on the struct identifier when the parameter list spans several lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2103-002")]
    public async Task VerifyDiagnosticIsOnIdentifierWhenParameterListSpansSeveralLines()
    {
        const string source = """
                              struct {|#0:Size|}(
                                  int width,
                                  int height)
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer.DiagnosticId, AnalyzerResources.RH2103MessageFormat));
    }

    #endregion // Tests
}