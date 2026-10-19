using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8029PrivateEventsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8029PrivateEventsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8029PrivateEventsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8029-001")]
    [Requirement("RH8029-002")]
    public async Task VerifyDiagnosticForPrivateEventWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Raises notifications.</summary>
                              internal class TestClass
                              {
                                  private event System.Action {|#0:Changed|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8029PrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8029MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8029-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { private event System.Action Changed; }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: undocumented private events are reported on their names, while documented, non-private, and explicitly implemented events are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8029-001")]
    [Requirement("RH8029-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedPrivateEvents()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Notifier
                              /// </summary>
                              public interface INotifier
                              {
                                  /// <summary>
                                  /// Raised on notification
                                  /// </summary>
                                  event Action Notified;
                              }

                              /// <summary>
                              /// Publisher
                              /// </summary>
                              public class Publisher : INotifier
                              {
                                  private event Action {|#0:Changed|};

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  event Action {|#1:Closed|}
                                  {
                                      add
                                      {
                                      }
                                      remove
                                      {
                                      }
                                  }

                                  private event Action {|#2:Opened|}, Moved;

                                  /// <summary>
                                  /// Raised after saving
                                  /// </summary>
                                  private event Action Saved;

                                  public event Action Visible;

                                  event Action INotifier.Notified
                                  {
                                      add
                                      {
                                      }
                                      remove
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8029PrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8029MessageFormat, 3));
    }

    /// <summary>
    /// Verifies an explicit event implementation in a class is not reported although it has no access modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8029-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForExplicitEventImplementationInClass()
    {
        const string source = """
                              using System;

                              /// <summary>Notifier contract</summary>
                              public interface INotifier
                              {
                                  /// <summary>Raised on notification</summary>
                                  event Action Notified;
                              }

                              /// <summary>Notifier</summary>
                              public class Notifier : INotifier
                              {
                                  event Action INotifier.Notified
                                  {
                                      add
                                      {
                                      }
                                      remove
                                      {
                                      }
                                  }

                                  event Action {|#0:Other|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8029PrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8029MessageFormat));
    }

    /// <summary>
    /// Verifies undocumented events without an access modifier in a struct, a record, and a record struct are reported, a field-like declaration of several events once on its first name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8029-001")]
    [Requirement("RH8029-002")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForImplicitlyPrivateEventsOfStructRecordAndRecordStruct()
    {
        const string source = """
                              using System;

                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  event Action {|#0:Moved|};
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  event Action {|#1:Renamed|}, Retired;
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  event Action {|#2:Swapped|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8029PrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8029MessageFormat, 3));
    }

    /// <summary>
    /// Verifies undocumented private protected, protected, internal, and public events and an interface event without a modifier are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8029-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyNoDiagnosticForNonPrivateEvents()
    {
        const string source = """
                              using System;

                              /// <summary>Base</summary>
                              public class Base
                              {
                                  private protected event Action Shared;

                                  protected event Action Derived;

                                  internal event Action Assembly;

                                  public event Action Visible;
                              }

                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  event Action Raised;
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}