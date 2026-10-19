using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8024NonPrivateIndexersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8024NonPrivateIndexersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8024NonPrivateIndexersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8024-001")]
    [Requirement("RH8024-002")]
    public async Task VerifyDiagnosticForIndexerWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  internal int {|#0:this|}[int index] => index;
                              }
                              """;

        await Verify(source, Diagnostics(RH8024NonPrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8024MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8024-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { internal int this[int index] => index; }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: undocumented non-private indexers are reported on their this keyword, while documented and private indexers are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8024-001")]
    [Requirement("RH8024-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedNonPrivateIndexers()
    {
        const string source = """
                              /// <summary>
                              /// Buffer
                              /// </summary>
                              public class Buffer
                              {
                                  public int {|#0:this|}[int index] => index;

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  internal int {|#1:this|}[string key] => 0;

                                  /// <summary>
                                  /// Gets the value at a position
                                  /// </summary>
                                  /// <param name="index">Index</param>
                                  /// <returns>Value</returns>
                                  public int this[long index] => 0;

                                  private int this[short index] => index;
                              }

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface IContract
                              {
                                  int {|#2:this|}[int index] { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8024NonPrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8024MessageFormat, 3));
    }

    /// <summary>
    /// Verifies undocumented non-private indexers declared in a struct, a record, and a record struct are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8024-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForIndexersOfStructRecordAndRecordStruct()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  public int {|#0:this|}[int index] => index;
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  public int {|#1:this|}[int index] => index;
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  internal int {|#2:this|}[int index] => index;
                              }
                              """;

        await Verify(source, Diagnostics(RH8024NonPrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8024MessageFormat, 3));
    }

    /// <summary>
    /// Verifies an indexer whose accessor is more restricted than the indexer stays in the non-private group, and a private indexer is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8024-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyGroupIsDecidedByIndexerModifiersNotByAccessorModifiers()
    {
        const string source = """
                              /// <summary>Buffer</summary>
                              public class Buffer
                              {
                                  public int {|#0:this|}[int index]
                                  {
                                      get => index;
                                      private set
                                      {
                                      }
                                  }

                                  private int this[long index]
                                  {
                                      get => 0;
                                      set
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8024NonPrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8024MessageFormat));
    }

    /// <summary>
    /// Verifies an explicit indexer implementation declared in an interface is not reported, while an undocumented interface indexer is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8024-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForExplicitIndexerImplementationInInterface()
    {
        const string source = """
                              /// <summary>Store</summary>
                              public interface IStore
                              {
                                  /// <summary>Gets a value</summary>
                                  /// <param name="index">Index</param>
                                  int this[int index] { get; }
                              }

                              /// <summary>Derived store</summary>
                              public interface IDerived : IStore
                              {
                                  int IStore.this[int index] => index;

                                  int {|#0:this|}[string key] { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8024NonPrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8024MessageFormat));
    }

    /// <summary>
    /// Verifies an undocumented non-private property is not reported as an indexer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8024-001")]
    public async Task VerifyNoDiagnosticForProperty()
    {
        const string source = """
                              /// <summary>Buffer</summary>
                              public class Buffer
                              {
                                  public int Length { get; }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}