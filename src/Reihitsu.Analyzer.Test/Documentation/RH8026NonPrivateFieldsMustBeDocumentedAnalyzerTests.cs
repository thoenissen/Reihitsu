using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8026NonPrivateFieldsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8026NonPrivateFieldsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8026NonPrivateFieldsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8026-001")]
    [Requirement("RH8026-002")]
    public async Task VerifyDiagnosticForFieldWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  internal int {|#0:currentValue|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8026NonPrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8026MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8026-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { internal int currentValue; }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: undocumented non-private field declarations are reported on the name of their first field, while documented and private fields and events are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8026-001")]
    [Requirement("RH8026-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedNonPrivateFields()
    {
        const string source = """
                              /// <summary>
                              /// Counters
                              /// </summary>
                              public class Counters
                              {
                                  public int {|#0:Current|};

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  internal static readonly int {|#1:Maximum|} = 10;

                                  public const int {|#2:Limit|} = 5;

                                  protected int {|#3:First|}, Second;

                                  /// <summary>
                                  /// The total count
                                  /// </summary>
                                  public int Total;

                                  private int _hidden;

                                  public event System.Action Changed;
                              }

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface IContract
                              {
                                  static int {|#4:Shared|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8026NonPrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8026MessageFormat, 5));
    }

    /// <summary>
    /// Verifies an undocumented declaration of several fields is reported once on its first name, and a documented one documents all its fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8026-001")]
    [Requirement("RH8026-002")]
    public async Task VerifyDiagnosticForDeclarationOfSeveralFieldsIsLocatedOnFirstName()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public class Point
                              {
                                  public int {|#0:X|}, Y, Z;

                                  /// <summary>Documented</summary>
                                  public int A, B;
                              }
                              """;

        await Verify(source, Diagnostics(RH8026NonPrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8026MessageFormat));
    }

    /// <summary>
    /// Verifies undocumented non-private fields declared in a struct, a record, and a record struct are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8026-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForFieldsOfStructRecordAndRecordStruct()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  public int {|#0:X|};
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  public string {|#1:Name|};
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  internal int {|#2:Left|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8026NonPrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8026MessageFormat, 3));
    }

    /// <summary>
    /// Verifies undocumented private and implicitly private fields, a field-like event, and an enum member are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8026-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForPrivateFieldsEventsAndEnumMembers()
    {
        const string source = """
                              /// <summary>Counters</summary>
                              public class Counters
                              {
                                  private int _first;

                                  int _second;

                                  public event System.Action Changed;
                              }

                              /// <summary>Status</summary>
                              public enum Status
                              {
                                  Active,
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a private protected field and a public field of a private nested type are in the non-private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8026-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForPrivateProtectedFieldAndPublicFieldOfPrivateType()
    {
        const string source = """
                              /// <summary>Outer</summary>
                              public class Outer
                              {
                                  private protected int {|#0:Shared|};

                                  /// <summary>Hidden</summary>
                                  private class Hidden
                                  {
                                      public int {|#1:Visible|};
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8026NonPrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8026MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the diagnostic for an undocumented field with a verbatim identifier covers the whole identifier, the at sign included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8026-002")]
    public async Task VerifyDiagnosticForFieldWithVerbatimIdentifierCoversTheAtSign()
    {
        const string source = """
                              /// <summary>Settings</summary>
                              public class Settings
                              {
                                  public int {|#0:@class|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8026NonPrivateFieldsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8026MessageFormat));
    }

    #endregion // Tests
}