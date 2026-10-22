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
    [Requirement("RH2102-001")]
    [Requirement("RH2102-002")]
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
    [Requirement("RH2102-001")]
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
    [Requirement("RH2102-001")]
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
    [Requirement("RH2102-001")]
    public async Task VerifyNoDiagnosticsForRecords()
    {
        const string source = """
                              namespace Example;

                              internal record User(int Id, string Name);
                              internal record class Customer(int Id);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that nested, partial, and generic classes with primary-constructor parameters are reported on their
    /// identifier, and that empty primary constructors, classes without one, and records are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2102-001")]
    [Requirement("RH2102-002")]
    public async Task VerifyNestedPartialAndGenericClassesWithPrimaryConstructorParametersAreReported()
    {
        const string source = """
                              class {|#0:Service|}(int id)
                              {
                                  public int Id => id;
                              }

                              class Empty()
                              {
                              }

                              class Plain
                              {
                              }

                              partial class {|#1:Part|}(int value)
                              {
                              }

                              class {|#2:Box|}<T>(T value)
                              {
                                  public T Value => value;
                              }

                              record Point(int X);

                              class Outer
                              {
                                  class {|#3:Inner|}(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer.DiagnosticId, AnalyzerResources.RH2102MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that the diagnostic is on the class identifier when the parameter list spans several lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2102-002")]
    public async Task VerifyDiagnosticIsOnIdentifierWhenParameterListSpansSeveralLines()
    {
        const string source = """
                              class {|#0:Service|}(
                                  int id,
                                  string name)
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer.DiagnosticId, AnalyzerResources.RH2102MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a partial class declared in two parts: the class is reported once, on the
    /// part that declares the primary constructor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-004")]
    [Requirement("RH2102-001")]
    public async Task VerifyPartialClassDeclaredInTwoPartsIsReportedOnce()
    {
        const string source = """
                              partial class {|#0:P|}(int value)
                              {
                              }

                              partial class P
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer.DiagnosticId, AnalyzerResources.RH2102MessageFormat));
    }

    #endregion // Tests
}