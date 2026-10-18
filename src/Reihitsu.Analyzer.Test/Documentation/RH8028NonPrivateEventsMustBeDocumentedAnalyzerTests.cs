using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8028NonPrivateEventsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8028NonPrivateEventsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8028NonPrivateEventsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    [Requirement("RH8028-002")]
    public async Task VerifyDiagnosticForEventWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Raises notifications.</summary>
                              internal class TestClass
                              {
                                  internal event System.Action {|#0:Changed|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8028NonPrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8028MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an event with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    public async Task VerifyNoDiagnosticForEventWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class TestClass
                              {
                                  /// <summary>Notifies when the value changed.</summary>
                                  internal event System.Action Changed;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an explicit event with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    public async Task VerifyNoDiagnosticForExplicitEventWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class TestClass
                              {
                                  private System.Action _changed;
                                  
                                  /// <summary>Notifies when the value changed.</summary>
                                  internal event System.Action Changed
                                  {
                                      add { _changed += value; }
                                      remove { _changed -= value; }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { internal event System.Action Changed; }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: undocumented non-private field-like events and events with accessors are reported on their names, while documented and private events are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    [Requirement("RH8028-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedNonPrivateEvents()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Publisher
                              /// </summary>
                              public class Publisher
                              {
                                  public event Action {|#0:Changed|};

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  internal event Action {|#1:Closed|}
                                  {
                                      add
                                      {
                                      }
                                      remove
                                      {
                                      }
                                  }

                                  protected event Action {|#2:Opened|}, Moved;

                                  /// <summary>
                                  /// Raised after saving
                                  /// </summary>
                                  public event Action Saved;

                                  private event Action Hidden;
                              }

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface IContract
                              {
                                  event Action {|#3:Raised|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8028NonPrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8028MessageFormat, 4));
    }

    /// <summary>
    /// Verifies an undocumented field-like declaration of several events is reported once on its first name, and a documented one documents all its events
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    [Requirement("RH8028-002")]
    public async Task VerifyDiagnosticForFieldLikeDeclarationOfSeveralEventsIsLocatedOnFirstName()
    {
        const string source = """
                              using System;

                              /// <summary>Publisher</summary>
                              public class Publisher
                              {
                                  public event Action {|#0:Opened|}, Closed, Moved;

                                  /// <summary>Documented</summary>
                                  public event Action Saved, Loaded;
                              }
                              """;

        await Verify(source, Diagnostics(RH8028NonPrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8028MessageFormat));
    }

    /// <summary>
    /// Verifies undocumented non-private events declared in a struct, a record, a record struct, and an interface, a static abstract one included, are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    [Requirement("SHARED-DOC-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForEventsOfStructRecordRecordStructAndInterface()
    {
        const string source = """
                              using System;

                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  public event Action {|#0:Moved|};
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  public event Action {|#1:Renamed|};
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  internal event Action {|#2:Swapped|};
                              }

                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  event Action {|#3:Raised|};

                                  static abstract event Action {|#4:Shared|};
                              }
                              """;

        await Verify(source, Diagnostics(RH8028NonPrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8028MessageFormat, 5));
    }

    /// <summary>
    /// Verifies an explicit event implementation declared in an interface is not reported, while an undocumented interface event is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForExplicitEventImplementationInInterface()
    {
        const string source = """
                              using System;

                              /// <summary>Notifier</summary>
                              public interface INotifier
                              {
                                  /// <summary>Raised on notification</summary>
                                  event Action Notified;
                              }

                              /// <summary>Derived notifier</summary>
                              public interface IDerived : INotifier
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

        await Verify(source, Diagnostics(RH8028NonPrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8028MessageFormat));
    }

    /// <summary>
    /// Verifies an undocumented private protected event is reported, while private and implicitly private events are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8028-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyGroupOfEvents()
    {
        const string source = """
                              using System;

                              /// <summary>Publisher</summary>
                              public class Publisher
                              {
                                  private protected event Action {|#0:Shared|};

                                  private event Action Hidden;

                                  event Action Implicit
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

        await Verify(source, Diagnostics(RH8028NonPrivateEventsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8028MessageFormat));
    }

    #endregion // Tests
}