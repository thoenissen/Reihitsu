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
    /// Verifies that override properties implementing interface members are governed by the base-type region rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-002")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyNoDiagnosticsForOverridePropertyImplementingInterfaceInBaseTypeRegion()
    {
        const string testData = """
                                internal interface INamed
                                {
                                    string Name { get; }
                                }

                                internal abstract class BaseProcessor
                                {
                                    public abstract string Name { get; }
                                }

                                internal class DerivedProcessor : BaseProcessor, INamed
                                {
                                    #region BaseProcessor

                                    public override string Name => string.Empty;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an implicit interface property in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-001")]
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
    [Requirement("RH7410-001")]
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
    [Requirement("RH7410-001")]
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
    [Requirement("RH7410-001")]
    [Requirement("SHARED-REGGROUP-005")]
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
    [Requirement("RH7410-001")]
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
    /// Verifies that a property implementing a member from an inherited interface is accepted in the declaring interface region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-001")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyNoDiagnosticsForInheritedInterfaceMemberInDeclaringInterfaceRegion()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    int Id { get; }
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestClass : IDerived
                                {
                                    #region IBase

                                    public int Id => 0;

                                    #endregion // IBase
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a property implementing a member from an inherited interface uses the declaring interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-001")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyDiagnosticForInheritedInterfaceMemberUsesDeclaringInterface()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    int Id { get; }
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestClass : IDerived
                                {
                                    #region Properties

                                    public int {|#0:Id|} => 0;

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData, Diagnostics(RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IBase")));
    }

    /// <summary>
    /// Verifies that properties are grouped by the interface that declares each implemented member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-001")]
    public async Task VerifyDiagnosticsForMembersFromDifferentInterfaces()
    {
        const string testData = """
                                internal interface IIdentifiable
                                {
                                    int Id { get; }
                                }

                                internal interface INamed
                                {
                                    string Name { get; }
                                }

                                internal class TestClass : IIdentifiable, INamed
                                {
                                    #region Properties

                                    public int {|#0:Id|} => 0;

                                    public string {|#1:Name|} => string.Empty;

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId,
                                 index => CreateMessage(index == 0 ? "IIdentifiable" : "INamed"),
                                 2));
    }

    /// <summary>
    /// Verifies that a property that does not implement an interface member does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-002")]
    public async Task VerifyNoDiagnosticsForNonInterfaceMember()
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
    /// Verifies that an interface member of another kind does not trigger the properties rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-002")]
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
    /// Verifies that implicit and explicit interface implementation properties in a region of another name or in no region
    /// are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-001")]
    public async Task VerifyDiagnosticsForImplicitAndExplicitImplementationsOutsideTheirInterfaceRegion()
    {
        const string testData = """
                                interface INamed
                                {
                                    string Name { get; }
                                }

                                interface ISized
                                {
                                    int Size { get; }
                                }

                                interface ICounted
                                {
                                    int Count { get; }
                                }

                                class C : INamed, ISized, ICounted
                                {
                                    #region Properties

                                    public string {|RH7410:Name|} => "";

                                    int ISized.{|RH7410:Size|} => 0;

                                    #endregion

                                    public int {|RH7410:Count|} => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that implicit and explicit interface implementation properties in the regions named after their interfaces
    /// are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-001")]
    public async Task VerifyNoDiagnosticsForImplicitAndExplicitImplementationsInTheirInterfaceRegions()
    {
        const string testData = """
                                interface INamed
                                {
                                    string Name { get; }
                                }

                                interface ISized
                                {
                                    int Size { get; }
                                }

                                class C : INamed, ISized
                                {
                                    #region INamed

                                    public string Name => "";

                                    #endregion

                                    #region ISized

                                    int ISized.Size => 0;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a property that implements no interface member and an interface implementation that is an indexer are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7410-002")]
    public async Task VerifyNoDiagnosticsForPropertyImplementingNothingAndIndexerImplementation()
    {
        const string testData = """
                                interface IIndexed
                                {
                                    int this[int index] { get; }
                                }

                                class C : IIndexed
                                {
                                    #region Indexers

                                    public int this[int index] => index;

                                    #endregion

                                    #region Properties

                                    public int P => 0;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the diagnostic of an explicit interface implementation property is located on the identifier after the
    /// interface name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-005")]
    public async Task VerifyDiagnosticForExplicitImplementationIsOnIdentifierAfterInterfaceName()
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
                                    int I.{|RH7410:P|} => 0;

                                    public int this[int i] => 0;

                                    public event EventHandler E;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a partial property that implements an interface property: it is judged and reported only at its
    /// defining part, and the region of its implementing part is not considered. This test pins today's behavior; the
    /// decided behavior is stated by SHARED-DIAG-004, which judges a partial property by its symbol, and its change belongs
    /// to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPartialImplementationPropertyIsJudgedAtItsDefiningPart()
    {
        const string testData = """
                                interface I
                                {
                                    int P { get; }

                                    int Q { get; }
                                }

                                partial class C : I
                                {
                                    #region Properties

                                    public partial int {|#0:P|} { get; }

                                    public partial int Q { get => 0; }

                                    #endregion

                                    #region I

                                    public partial int P { get => 0; }

                                    public partial int Q { get; }

                                    #endregion
                                }
                                """;

        await Verify(testData, Diagnostics(RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("I")));
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