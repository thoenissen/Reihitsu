using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0118NestedRecordsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0118NestedRecordsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0118NestedRecordsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying nested record declarations trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(source, Diagnostics(RH0118NestedRecordsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0118MessageFormat, 2));
    }

    /// <summary>
    /// Verifying top-level record declarations do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying nested classes do not trigger RH0118 diagnostics
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