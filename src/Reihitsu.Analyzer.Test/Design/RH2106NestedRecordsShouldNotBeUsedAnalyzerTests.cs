using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2106NestedRecordsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2106NestedRecordsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2106NestedRecordsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying nested record declarations trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2106-001")]
    public async Task VerifyNestedRecordDeclarationsTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal class Container
                              {
                                  private record {|#0:NestedRecord|}(int Id);
                                  private readonly record struct {|#1:NestedRecordStruct|}(int Id);
                              }
                              """;

        await Verify(source, Diagnostics(RH2106NestedRecordsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2106MessageFormat, 2));
    }

    /// <summary>
    /// Verifying top-level record declarations do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-NESTED-001")]
    public async Task VerifyTopLevelRecordsDoNotTriggerDiagnostics()
    {
        const string source = """
                              namespace Example;

                              internal record TopLevelRecord(int Id);
                              internal readonly record struct TopLevelRecordStruct(int Id);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying nested classes do not trigger RH2106 diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2106-001")]
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
    /// Verifying that a nested record, record class, and record struct are reported and a nested class is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2106-001")]
    [Requirement("SHARED-NESTED-001")]
    public async Task VerifyNestedRecordRecordClassAndRecordStructAreReported()
    {
        const string source = """
                              class Outer
                              {
                                  record {|#0:Point|}(int X);

                                  record class {|#1:Line|}(int Length);

                                  record struct {|#2:Size|}(int Width);

                                  class Helper
                                  {
                                  }
                              }

                              record Container
                              {
                                  record {|#3:InRecord|}(int X);
                              }
                              """;

        await Verify(source, Diagnostics(RH2106NestedRecordsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2106MessageFormat, 4));
    }

    #endregion // Tests
}