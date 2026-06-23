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
    /// Verifies that an event that does not implement an interface member does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNonInterfaceEvent()
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