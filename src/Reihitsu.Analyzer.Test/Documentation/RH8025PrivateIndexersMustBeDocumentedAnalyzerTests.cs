using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8025PrivateIndexersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8025PrivateIndexersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8025PrivateIndexersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8025-001")]
    [Requirement("RH8025-002")]
    public async Task VerifyDiagnosticForPrivateIndexerWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  private int {|#0:this|}[int index] => index;
                              }
                              """;

        await Verify(source, Diagnostics(RH8025PrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8025MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8025-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { private int this[int index] => index; }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: undocumented private indexers are reported on their this keyword, while documented, non-private, and explicitly implemented indexers are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8025-001")]
    [Requirement("RH8025-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedPrivateIndexers()
    {
        const string source = """
                              /// <summary>
                              /// Store
                              /// </summary>
                              public interface IStore
                              {
                                  /// <summary>
                                  /// Gets a value
                                  /// </summary>
                                  /// <param name="index">Index</param>
                                  int this[int index] { get; }
                              }

                              /// <summary>
                              /// Buffer
                              /// </summary>
                              public class Buffer : IStore
                              {
                                  private int {|#0:this|}[string key] => 0;

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  int {|#1:this|}[long index] => 0;

                                  /// <summary>
                                  /// Gets the value for a flag
                                  /// </summary>
                                  /// <param name="flag">Flag</param>
                                  private int this[bool flag] => 0;

                                  public int this[short index] => index;

                                  int IStore.this[int index] => index;
                              }
                              """;

        await Verify(source, Diagnostics(RH8025PrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8025MessageFormat, 2));
    }

    /// <summary>
    /// Verifies an explicit indexer implementation in a class is not reported although it has no access modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8025-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForExplicitIndexerImplementationInClass()
    {
        const string source = """
                              /// <summary>Store contract</summary>
                              public interface IStore
                              {
                                  /// <summary>Gets a value</summary>
                                  /// <param name="index">Index</param>
                                  int this[int index] { get; }
                              }

                              /// <summary>Store</summary>
                              public class Store : IStore
                              {
                                  int IStore.this[int index] => index;

                                  int {|#0:this|}[string key] => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8025PrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8025MessageFormat));
    }

    /// <summary>
    /// Verifies undocumented indexers without an access modifier in a struct, a record, and a record struct are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8025-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForImplicitlyPrivateIndexersOfStructRecordAndRecordStruct()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  int {|#0:this|}[int index] => index;
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  int {|#1:this|}[int index] => index;
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  int {|#2:this|}[int index] => index;
                              }
                              """;

        await Verify(source, Diagnostics(RH8025PrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8025MessageFormat, 3));
    }

    /// <summary>
    /// Verifies undocumented private protected, protected, internal, and public indexers and an interface indexer without a modifier are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8025-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForNonPrivateIndexers()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public class Base
                              {
                                  private protected int this[int index] => index;

                                  protected int this[long index] => 0;

                                  internal int this[short index] => index;

                                  public int this[byte index] => index;
                              }

                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  int this[int index] { get; }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies an undocumented private indexer declared in an interface is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8025-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateInterfaceIndexer()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  private int {|#0:this|}[int index] => index;
                              }
                              """;

        await Verify(source, Diagnostics(RH8025PrivateIndexersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8025MessageFormat));
    }

    #endregion // Tests
}