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
    /// Verifies that override indexers implementing interface members are governed by the base-type region rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOverrideIndexerImplementingInterfaceInBaseTypeRegion()
    {
        const string testData = """
                                internal interface IIndexable
                                {
                                    string this[int index] { get; }
                                }

                                internal abstract class BaseProcessor
                                {
                                    public abstract string this[int index] { get; }
                                }

                                internal class DerivedProcessor : BaseProcessor, IIndexable
                                {
                                    #region BaseProcessor

                                    public override string this[int index] => string.Empty;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

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
    /// Verifies that an indexer implementing a member from an inherited interface is accepted in the declaring interface region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForInheritedInterfaceMemberInDeclaringInterfaceRegion()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    int this[int index] { get; }
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestClass : IDerived
                                {
                                    #region IBase

                                    public int this[int index] => index;

                                    #endregion // IBase
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an indexer implementing a member from an inherited interface uses the declaring interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForInheritedInterfaceMemberUsesDeclaringInterface()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    int this[int index] { get; }
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestClass : IDerived
                                {
                                    #region Indexers

                                    public int {|#0:this|}[int index] => index;

                                    #endregion // Indexers
                                }
                                """;

        await Verify(testData, Diagnostics(RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IBase")));
    }

    /// <summary>
    /// Verifies that indexers are grouped by the interface that declares each implemented member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMembersFromDifferentInterfaces()
    {
        const string testData = """
                                internal interface IIndexable
                                {
                                    int this[int index] { get; }
                                }

                                internal interface INamedIndexable
                                {
                                    int this[string key] { get; }
                                }

                                internal class TestClass : IIndexable, INamedIndexable
                                {
                                    #region Indexers

                                    public int {|#0:this|}[int index] => index;

                                    public int {|#1:this|}[string key] => 0;

                                    #endregion // Indexers
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId,
                                 index => CreateMessage(index == 0 ? "IIndexable" : "INamedIndexable"),
                                 2));
    }

    /// <summary>
    /// Verifies that an indexer that does not implement an interface member does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNonInterfaceMember()
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

                                    #region Indexers

                                    public int this[string key] => 0;

                                    #endregion // Indexers
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an interface member of another kind does not trigger the indexers rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForMemberOfOtherKind()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal class TestClass : IIdentifiable
                                {
                                    #region Properties

                                    public int Id => 0;

                                    #endregion // Properties
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