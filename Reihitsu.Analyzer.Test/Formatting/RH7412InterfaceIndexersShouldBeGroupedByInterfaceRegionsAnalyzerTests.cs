using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzerTests : AnalyzerTestsBase<RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that an implicit interface indexer in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForImplicitImplementationInMatchingRegion()
    {
        const string testData = """
                                internal interface IIndexable
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestClass : IIndexable
                                {
                                    #region IIndexable

                                    public int this[int index] => index;

                                    #endregion // IIndexable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an implicit interface indexer in a generic region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitImplementationInGenericRegion()
    {
        const string testData = """
                                internal interface IIndexable
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestClass : IIndexable
                                {
                                    #region Indexers

                                    public int {|#0:this|}[int index] => index;

                                    #endregion // Indexers
                                }
                                """;

        await Verify(testData, Diagnostics(RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IIndexable")));
    }

    /// <summary>
    /// Verifies that an explicit interface indexer in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForExplicitImplementationInMatchingRegion()
    {
        const string testData = """
                                internal interface IIndexable
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestClass : IIndexable
                                {
                                    #region IIndexable

                                    int IIndexable.this[int index] => index;

                                    #endregion // IIndexable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an explicit interface indexer in a generic region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitImplementationInGenericRegion()
    {
        const string testData = """
                                internal interface IIndexable
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestClass : IIndexable
                                {
                                    #region Indexers

                                    int IIndexable.{|#0:this|}[int index] => index;

                                    #endregion // Indexers
                                }
                                """;

        await Verify(testData, Diagnostics(RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IIndexable")));
    }

    /// <summary>
    /// Verifies that an interface indexer outside of any region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplementationOutsideRegion()
    {
        const string testData = """
                                internal interface IIndexable
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestClass : IIndexable
                                {
                                    public int {|#0:this|}[int index] => index;
                                }
                                """;

        await Verify(testData, Diagnostics(RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IIndexable")));
    }

    /// <summary>
    /// Verifies that an interface property implementation does not trigger the indexers rule
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
        return string.Format(AnalyzerResources.RH7412MessageFormat, interfaceName);
    }

    #endregion // Methods
}