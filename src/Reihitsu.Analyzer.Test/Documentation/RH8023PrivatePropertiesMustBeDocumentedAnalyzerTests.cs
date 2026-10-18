using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8023PrivatePropertiesMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8023PrivatePropertiesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8023PrivatePropertiesMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8023-001")]
    [Requirement("RH8023-002")]
    public async Task VerifyDiagnosticForPrivatePropertyWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  private int {|#0:Value|} { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8023PrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8023MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8023-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { private int Value { get; } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: undocumented private properties are reported on their names, while documented, non-private, and explicitly implemented properties are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8023-001")]
    [Requirement("RH8023-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedPrivateProperties()
    {
        const string source = """
                              /// <summary>
                              /// Entity
                              /// </summary>
                              public interface IEntity
                              {
                                  /// <summary>
                                  /// Gets the name
                                  /// </summary>
                                  string Name { get; }
                              }

                              /// <summary>
                              /// Settings
                              /// </summary>
                              public class Settings : IEntity
                              {
                                  private string {|#0:Cache|} { get; set; }

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  int {|#1:Count|} => 0;

                                  /// <summary>
                                  /// Gets the profile
                                  /// </summary>
                                  private string Profile { get; }

                                  public int Version { get; private set; }

                                  string IEntity.Name => "";
                              }

                              /// <summary>
                              /// Point
                              /// </summary>
                              public struct Point
                              {
                                  int {|#2:Length|} => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8023PrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8023MessageFormat, 3));
    }

    /// <summary>
    /// Verifies an explicit property implementation in a class is not reported although it has no access modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8023-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForExplicitPropertyImplementationInClass()
    {
        const string source = """
                              /// <summary>Entity contract</summary>
                              public interface IEntity
                              {
                                  /// <summary>Gets the name</summary>
                                  string Name { get; }
                              }

                              /// <summary>Entity</summary>
                              public class Entity : IEntity
                              {
                                  string IEntity.Name => "";

                                  string {|#0:Alias|} => "";
                              }
                              """;

        await Verify(source, Diagnostics(RH8023PrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8023MessageFormat));
    }

    /// <summary>
    /// Verifies undocumented properties without an access modifier in a struct, a record, a record struct, and an extension block are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8023-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForImplicitlyPrivatePropertiesOfStructRecordRecordStructAndExtensionBlock()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  int {|#0:X|} => 0;
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  int {|#1:Age|} => 0;
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  int {|#2:Left|} => 0;
                              }

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Text values</summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                      int {|#3:Hidden|} => 0;

                                      private int {|#4:Explicit|} => 0;

                                      public int Visible => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8023PrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8023MessageFormat, 5));
    }

    /// <summary>
    /// Verifies undocumented private protected, protected, internal, and public properties, a public property with a private setter, and an interface property without a modifier are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8023-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForNonPrivateProperties()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public class Base
                              {
                                  private protected int Shared { get; }

                                  protected int Derived { get; }

                                  internal int Assembly { get; }

                                  public int Visible { get; private set; }
                              }

                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  int Value { get; }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies an undocumented private property declared in an interface is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8023-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateInterfaceProperty()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  private int {|#0:Hidden|} => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8023PrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8023MessageFormat));
    }

    /// <summary>
    /// Verifies an undocumented private indexer is not reported as a property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8023-001")]
    public async Task VerifyNoDiagnosticForPrivateIndexer()
    {
        const string source = """
                              /// <summary>Buffer</summary>
                              public class Buffer
                              {
                                  private int this[int index] => index;
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}