using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2104NestedStructsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2104NestedStructsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2104NestedStructsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying nested structs trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2104-001")]
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

        await Verify(source, Diagnostics(RH2104NestedStructsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2104MessageFormat));
    }

    /// <summary>
    /// Verifying top-level structs do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-NESTED-001")]
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
    /// Verifying nested classes do not trigger RH2104 diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2104-001")]
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
    /// Verifying that structs nested in a class or an interface are reported and a nested record struct is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2104-001")]
    [Requirement("SHARED-NESTED-001")]
    public async Task VerifyStructsNestedInClassAndInterfaceAreReportedAndRecordStructIsNot()
    {
        const string source = """
                              class Outer
                              {
                                  struct {|#0:Entry|}
                                  {
                                      struct {|#1:Deeper|}
                                      {
                                      }
                                  }

                                  record struct Point(int X);
                              }

                              interface I
                              {
                                  struct {|#2:Value|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH2104NestedStructsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2104MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that each part of a nested partial struct is reported on its own identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2104-001")]
    [Requirement("SHARED-NESTED-002")]
    public async Task VerifyEachPartOfANestedPartialStructIsReported()
    {
        const string source = """
                              class Outer
                              {
                                  partial struct {|#0:Part|}
                                  {
                                  }

                                  partial struct {|#1:Part|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH2104NestedStructsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2104MessageFormat, 2));
    }

    #endregion // Tests
}