using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8027PrivateFieldsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8027PrivateFieldsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8027PrivateFieldsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8027-001")]
    [Requirement("RH8027-002")]
    public async Task VerifyDiagnosticForPrivateFieldWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  private int {|#0:currentValue|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8027PrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8027MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8027-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { private int currentValue; }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: undocumented private field declarations are reported on the name of their first field, while documented and non-private fields and events are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8027-001")]
    [Requirement("RH8027-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedPrivateFields()
    {
        const string source = """
                              /// <summary>
                              /// Counters
                              /// </summary>
                              public class Counters
                              {
                                  private int {|#0:_current|};

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  static readonly int {|#1:_maximum|} = 10;

                                  private const int {|#2:Limit|} = 5;

                                  private int {|#3:_first|}, _second;

                                  /// <summary>
                                  /// The total count
                                  /// </summary>
                                  private int _total;

                                  private protected int Shared;

                                  private event System.Action Changed;
                              }

                              /// <summary>
                              /// Point
                              /// </summary>
                              public struct Point
                              {
                                  int {|#4:_x|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8027PrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8027MessageFormat, 5));
    }

    /// <summary>
    /// Verifies undocumented fields without an access modifier in a class, a struct, a record, and a record struct are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8027-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForImplicitlyPrivateFieldsOfStructRecordAndRecordStruct()
    {
        const string source = """
                              /// <summary>Holder</summary>
                              public class Holder
                              {
                                  int {|#0:_value|};
                              }

                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  int {|#1:_x|};
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  string {|#2:_name|};
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  int {|#3:_left|}, _right;
                              }
                              """;

        await Verify(source, Diagnostics(RH8027PrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8027MessageFormat, 4));
    }

    /// <summary>
    /// Verifies undocumented private protected, protected, internal, and public fields, an interface field without a modifier, and a private event are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8027-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForNonPrivateFieldsAndPrivateEvents()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public class Base
                              {
                                  private protected int Shared;

                                  protected int Derived;

                                  internal int Assembly;

                                  public int Visible;

                                  private event System.Action Changed;
                              }

                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  static int Value;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies an undocumented private static field declared in an interface is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8027-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateStaticInterfaceField()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  private static int {|#0:_hidden|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8027PrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8027MessageFormat));
    }

    #endregion // Tests
}