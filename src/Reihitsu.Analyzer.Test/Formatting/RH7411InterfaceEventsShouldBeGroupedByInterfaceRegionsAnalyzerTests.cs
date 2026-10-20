using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzerTests : AnalyzerTestsBase<RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that override events implementing interface members are governed by the base-type region rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-002")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyNoDiagnosticsForOverrideEventImplementingInterfaceInBaseTypeRegion()
    {
        const string testData = """
                                internal interface ICompletable
                                {
                                    event System.EventHandler Completed;
                                }

                                internal abstract class BaseProcessor
                                {
                                    public abstract event System.EventHandler Completed;
                                }

                                internal class DerivedProcessor : BaseProcessor, ICompletable
                                {
                                    #region BaseProcessor

                                    public override event System.EventHandler Completed;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that accessor-style override events implementing interface members are governed by the base-type region rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-002")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyNoDiagnosticsForAccessorOverrideEventImplementingInterfaceInBaseTypeRegion()
    {
        const string testData = """
                                internal interface ICompletable
                                {
                                    event System.EventHandler Completed;
                                }

                                internal abstract class BaseProcessor
                                {
                                    public abstract event System.EventHandler Completed;
                                }

                                internal class DerivedProcessor : BaseProcessor, ICompletable
                                {
                                    #region BaseProcessor

                                    public override event System.EventHandler Completed
                                    {
                                        add
                                        {
                                        }
                                        remove
                                        {
                                        }
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an implicit interface event in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    public async Task VerifyNoDiagnosticsForImplicitImplementationInMatchingRegion()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IChangeable
                                {
                                    event ChangedHandler Changed;
                                }

                                internal class TestClass : IChangeable
                                {
                                    #region IChangeable

                                    public event ChangedHandler Changed;

                                    #endregion // IChangeable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an implicit interface event in a generic region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    [Requirement("SHARED-REGGROUP-005")]
    public async Task VerifyDiagnosticForImplicitImplementationInGenericRegion()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IChangeable
                                {
                                    event ChangedHandler Changed;
                                }

                                internal class TestClass : IChangeable
                                {
                                    #region Events

                                    public event ChangedHandler {|#0:Changed|};

                                    #endregion // Events
                                }
                                """;

        await Verify(testData, Diagnostics(RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IChangeable")));
    }

    /// <summary>
    /// Verifies that an explicit interface event in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    public async Task VerifyNoDiagnosticsForExplicitImplementationInMatchingRegion()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IChangeable
                                {
                                    event ChangedHandler Changed;
                                }

                                internal class TestClass : IChangeable
                                {
                                    #region IChangeable

                                    event ChangedHandler IChangeable.Changed
                                    {
                                        add
                                        {
                                        }
                                        remove
                                        {
                                        }
                                    }

                                    #endregion // IChangeable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an explicit interface event in a generic region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    [Requirement("SHARED-REGGROUP-005")]
    public async Task VerifyDiagnosticForExplicitImplementationInGenericRegion()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IChangeable
                                {
                                    event ChangedHandler Changed;
                                }

                                internal class TestClass : IChangeable
                                {
                                    #region Events

                                    event ChangedHandler IChangeable.{|#0:Changed|}
                                    {
                                        add
                                        {
                                        }
                                        remove
                                        {
                                        }
                                    }

                                    #endregion // Events
                                }
                                """;

        await Verify(testData, Diagnostics(RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IChangeable")));
    }

    /// <summary>
    /// Verifies that an interface event outside of any region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    public async Task VerifyDiagnosticForImplementationOutsideRegion()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IChangeable
                                {
                                    event ChangedHandler Changed;
                                }

                                internal class TestClass : IChangeable
                                {
                                    public event ChangedHandler {|#0:Changed|};
                                }
                                """;

        await Verify(testData, Diagnostics(RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IChangeable")));
    }

    /// <summary>
    /// Verifies that an event implementing a member from an inherited interface is accepted in the declaring interface region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyNoDiagnosticsForInheritedInterfaceMemberInDeclaringInterfaceRegion()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IBase
                                {
                                    event ChangedHandler Changed;
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestClass : IDerived
                                {
                                    #region IBase

                                    public event ChangedHandler Changed;

                                    #endregion // IBase
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an event implementing a member from an inherited interface uses the declaring interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyDiagnosticForInheritedInterfaceMemberUsesDeclaringInterface()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IBase
                                {
                                    event ChangedHandler Changed;
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestClass : IDerived
                                {
                                    #region Events

                                    public event ChangedHandler {|#0:Changed|};

                                    #endregion // Events
                                }
                                """;

        await Verify(testData, Diagnostics(RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IBase")));
    }

    /// <summary>
    /// Verifies that events are grouped by the interface that declares each implemented member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    public async Task VerifyDiagnosticsForMembersFromDifferentInterfaces()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IChangeable
                                {
                                    event ChangedHandler Changed;
                                }

                                internal interface ISavable
                                {
                                    event ChangedHandler Saved;
                                }

                                internal class TestClass : IChangeable, ISavable
                                {
                                    #region Events

                                    public event ChangedHandler {|#0:Changed|};

                                    public event ChangedHandler {|#1:Saved|};

                                    #endregion // Events
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId,
                                 index => CreateMessage(index == 0 ? "IChangeable" : "ISavable"),
                                 2));
    }

    /// <summary>
    /// Verifies that an event that does not implement an interface member does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-002")]
    public async Task VerifyNoDiagnosticsForNonInterfaceMember()
    {
        const string testData = """
                                internal delegate void ChangedHandler();

                                internal interface IChangeable
                                {
                                    event ChangedHandler Changed;
                                }

                                internal class TestClass : IChangeable
                                {
                                    #region IChangeable

                                    public event ChangedHandler Changed;

                                    #endregion // IChangeable

                                    #region Events

                                    public event ChangedHandler Saved;

                                    #endregion // Events
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an interface member of another kind does not trigger the events rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-002")]
    public async Task VerifyNoDiagnosticsForMemberOfOtherKind()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal class TestClass : IExecutable
                                {
                                    #region Methods

                                    public void Execute()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that field-like and accessor interface implementation events, implicit and explicit, in a region of another
    /// name or in no region are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    public async Task VerifyDiagnosticsForFieldLikeAndAccessorImplementationsOutsideTheirInterfaceRegion()
    {
        const string testData = """
                                using System;

                                interface INotifier
                                {
                                    event EventHandler Changed;
                                }

                                interface ISaver
                                {
                                    event EventHandler Saved;
                                }

                                interface ICloser
                                {
                                    event EventHandler Closed;
                                }

                                class C : INotifier, ISaver, ICloser
                                {
                                    #region Events

                                    public event EventHandler {|RH7411:Changed|};

                                    event EventHandler ISaver.{|RH7411:Saved|}
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }

                                    #endregion

                                    public event EventHandler {|RH7411:Closed|}
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a field-like and an explicit interface implementation event in the regions named after their
    /// interfaces are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-001")]
    public async Task VerifyNoDiagnosticsForFieldLikeAndExplicitImplementationsInTheirInterfaceRegions()
    {
        const string testData = """
                                using System;

                                interface INotifier
                                {
                                    event EventHandler Changed;
                                }

                                interface ISaver
                                {
                                    event EventHandler Saved;
                                }

                                class C : INotifier, ISaver
                                {
                                    #region INotifier

                                    public event EventHandler Changed;

                                    #endregion

                                    #region ISaver

                                    event EventHandler ISaver.Saved
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an event that implements no interface member and an interface implementation that is a property are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7411-002")]
    public async Task VerifyNoDiagnosticsForEventImplementingNothingAndPropertyImplementation()
    {
        const string testData = """
                                using System;

                                interface INamed
                                {
                                    string Name { get; }
                                }

                                class C : INamed
                                {
                                    #region Properties

                                    public string Name => "";

                                    #endregion

                                    #region Events

                                    public event EventHandler Changed;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the diagnostic of a field-like interface implementation event with one declarator is located on the
    /// identifier of that declarator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-005")]
    public async Task VerifyDiagnosticForFieldLikeEventIsOnIdentifierOfItsDeclarator()
    {
        const string testData = """
                                using System;

                                interface I
                                {
                                    int P { get; }

                                    int this[int i] { get; }

                                    event EventHandler E;
                                }

                                class C : I
                                {
                                    int I.P => 0;

                                    public int this[int i] => 0;

                                    public event EventHandler {|RH7411:E|};
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a field-like event with several declarators that implement members of two interfaces: only its first
    /// declarator is judged, so the second declarator is not reported in the region of the first one's interface. The case
    /// is an open question about intended behavior, so this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFieldLikeImplementationEventWithSeveralDeclaratorsIsJudgedByItsFirstDeclarator()
    {
        const string testData = """
                                using System;

                                interface IA
                                {
                                    event EventHandler A;
                                }

                                interface IB
                                {
                                    event EventHandler X;
                                }

                                class C : IA, IB
                                {
                                    #region IA

                                    public event EventHandler A, X;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a field-like event with several declarators outside its interface's region: it is reported once, at
    /// its first declarator. The case is an open question about intended behavior, so this test pins today's behavior
    /// without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFieldLikeImplementationEventWithSeveralDeclaratorsIsReportedOnceAtFirstDeclarator()
    {
        const string testData = """
                                using System;

                                interface IA
                                {
                                    event EventHandler A;

                                    event EventHandler X;
                                }

                                class C : IA
                                {
                                    public event EventHandler {|#0:A|}, X;
                                }
                                """;

        await Verify(testData, Diagnostics(RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IA")));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given interface
    /// </summary>
    /// <param name="interfaceName">Interface name</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string interfaceName)
    {
        return string.Format(AnalyzerResources.RH7411MessageFormat, interfaceName);
    }

    #endregion // Methods
}