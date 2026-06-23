using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzerTests : AnalyzerTestsBase<RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that an implicit interface property in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForImplicitImplementationInMatchingRegion()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal class TestClass : IIdentifiable
                                {
                                    #region IIdentifiable

                                    public int Id => 0;

                                    #endregion // IIdentifiable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an implicit interface property in a generic region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitImplementationInGenericRegion()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal class TestClass : IIdentifiable
                                {
                                    #region Properties

                                    public int {|#0:Id|} => 0;

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData, Diagnostics(RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IIdentifiable")));
    }

    /// <summary>
    /// Verifies that an explicit interface property in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForExplicitImplementationInMatchingRegion()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal class TestClass : IIdentifiable
                                {
                                    #region IIdentifiable

                                    int IIdentifiable.Id => 0;

                                    #endregion // IIdentifiable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an explicit interface property in a generic region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitImplementationInGenericRegion()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal class TestClass : IIdentifiable
                                {
                                    #region Properties

                                    int IIdentifiable.{|#0:Id|} => 0;

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData, Diagnostics(RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IIdentifiable")));
    }

    /// <summary>
    /// Verifies that an interface property outside of any region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplementationOutsideRegion()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal class TestClass : IIdentifiable
                                {
                                    public int {|#0:Id|} => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IIdentifiable")));
    }

    /// <summary>
    /// Verifies that a property that does not implement an interface member does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNonInterfaceProperty()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal class TestClass : IIdentifiable
                                {
                                    #region IIdentifiable

                                    public int Id => 0;

                                    #endregion // IIdentifiable

                                    #region Properties

                                    public string Name => string.Empty;

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an interface method implementation does not trigger the properties rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForInterfaceMethod()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal class TestClass : IExecutable
                                {
                                    public void Execute()
                                    {
                                    }
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
        return string.Format(AnalyzerResources.RH7410MessageFormat, interfaceName);
    }

    #endregion // Methods
}