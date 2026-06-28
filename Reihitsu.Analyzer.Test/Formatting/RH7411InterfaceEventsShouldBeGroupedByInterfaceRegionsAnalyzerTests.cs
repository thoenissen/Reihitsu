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
    /// Verifies that an implicit interface event in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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