using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzerTests : AnalyzerTestsBase<RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that an implicit interface method in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForImplicitImplementationInMatchingRegion()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal class TestClass : IExecutable
                                {
                                    #region IExecutable

                                    public void Execute()
                                    {
                                    }

                                    #endregion // IExecutable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an implicit interface method in a generic region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitImplementationInGenericRegion()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal class TestClass : IExecutable
                                {
                                    #region Methods

                                    public void {|#0:Execute|}()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IExecutable")));
    }

    /// <summary>
    /// Verifies that an explicit interface method in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForExplicitImplementationInMatchingRegion()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal class TestClass : IExecutable
                                {
                                    #region IExecutable

                                    void IExecutable.Execute()
                                    {
                                    }

                                    #endregion // IExecutable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an explicit interface method in a generic region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitImplementationInGenericRegion()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal class TestClass : IExecutable
                                {
                                    #region Methods

                                    void IExecutable.{|#0:Execute|}()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IExecutable")));
    }

    /// <summary>
    /// Verifies that an interface method outside of any region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplementationOutsideRegion()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal class TestClass : IExecutable
                                {
                                    public void {|#0:Execute|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IExecutable")));
    }

    /// <summary>
    /// Verifies that the implementing interface is used when several interfaces are implemented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMethodsFromDifferentInterfaces()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal interface ISavable
                                {
                                    void Save();
                                }

                                internal class TestClass : IExecutable, ISavable
                                {
                                    #region Methods

                                    public void {|#0:Execute|}()
                                    {
                                    }

                                    public void {|#1:Save|}()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId,
                                 index => CreateMessage(index == 0 ? "IExecutable" : "ISavable"),
                                 2));
    }

    /// <summary>
    /// Verifies that a method that does not implement an interface member does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNonInterfaceMethod()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal class TestClass : IExecutable
                                {
                                    #region IExecutable

                                    public void Execute()
                                    {
                                    }

                                    #endregion // IExecutable

                                    #region Methods

                                    public void Save()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an interface property implementation does not trigger the methods rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForInterfaceProperty()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal class TestClass : IIdentifiable
                                {
                                    public int Id => 0;
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
        return string.Format(AnalyzerResources.RH7409MessageFormat, interfaceName);
    }

    #endregion // Methods
}