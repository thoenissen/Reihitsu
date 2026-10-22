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
    /// Verifies that override methods implementing interface members are governed by the base-type region rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-002")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyNoDiagnosticsForOverrideMethodImplementingInterfaceInBaseTypeRegion()
    {
        const string testData = """
                                internal interface IExecutable
                                {
                                    void Execute();
                                }

                                internal abstract class BaseProcessor
                                {
                                    public abstract void Execute();
                                }

                                internal class DerivedProcessor : BaseProcessor, IExecutable
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an implicit interface method in a matching interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-001")]
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
    [Requirement("RH7409-001")]
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
    [Requirement("RH7409-001")]
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
    [Requirement("RH7409-001")]
    [Requirement("SHARED-REGGROUP-005")]
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
    [Requirement("RH7409-001")]
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
    /// Verifies that a method implementing a member from an inherited interface is accepted in the declaring interface region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-001")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyNoDiagnosticsForInheritedInterfaceMemberInDeclaringInterfaceRegion()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void Execute();
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestClass : IDerived
                                {
                                    #region IBase

                                    public void Execute()
                                    {
                                    }

                                    #endregion // IBase
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a method implementing a member from an inherited interface uses the declaring interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-001")]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyDiagnosticForInheritedInterfaceMemberUsesDeclaringInterface()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void Execute();
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestClass : IDerived
                                {
                                    #region Methods

                                    public void {|#0:Execute|}()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IBase")));
    }

    /// <summary>
    /// Verifies that methods are grouped by the interface that declares each implemented member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-001")]
    public async Task VerifyDiagnosticsForMembersFromDifferentInterfaces()
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
    [Requirement("RH7409-002")]
    public async Task VerifyNoDiagnosticsForNonInterfaceMember()
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
    /// Verifies that an interface member of another kind does not trigger the methods rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-002")]
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

    /// <summary>
    /// Verifies that implicit, explicit, and static interface implementation methods in a region of another name or in no
    /// region are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-001")]
    public async Task VerifyDiagnosticsForImplicitExplicitAndStaticImplementationsOutsideTheirInterfaceRegion()
    {
        const string testData = """
                                interface IRunnable
                                {
                                    void Run();
                                }

                                interface IStoppable
                                {
                                    void Stop();
                                }

                                interface ICreatable
                                {
                                    static abstract void Create();
                                }

                                class C : IRunnable, IStoppable, ICreatable
                                {
                                    #region Methods

                                    public void {|RH7409:Run|}()
                                    {
                                    }

                                    void IStoppable.{|RH7409:Stop|}()
                                    {
                                    }

                                    #endregion

                                    public static void {|RH7409:Create|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that implicit and explicit interface implementation methods in the regions named after their interfaces are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-001")]
    public async Task VerifyNoDiagnosticsForImplicitAndExplicitImplementationsInTheirInterfaceRegions()
    {
        const string testData = """
                                interface IRunnable
                                {
                                    void Run();
                                }

                                interface IStoppable
                                {
                                    void Stop();
                                }

                                class C : IRunnable, IStoppable
                                {
                                    #region IRunnable

                                    public void Run()
                                    {
                                    }

                                    #endregion

                                    #region IStoppable

                                    void IStoppable.Stop()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a method that implements no interface member and an interface implementation that is a property are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-002")]
    public async Task VerifyNoDiagnosticsForMethodImplementingNothingAndPropertyImplementation()
    {
        const string testData = """
                                interface INamed
                                {
                                    string Name { get; }
                                }

                                class C : INamed
                                {
                                    #region Properties

                                    public string Name => "";

                                    #endregion

                                    #region Methods

                                    public void M()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an operator that implements a static abstract interface operator is not reported, because it is not a
    /// method declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-002")]
    public async Task VerifyNoDiagnosticsForOperatorImplementingStaticAbstractOperator()
    {
        const string testData = """
                                interface IAddable<T>
                                    where T : IAddable<T>
                                {
                                    static abstract T operator +(T left, T right);
                                }

                                class C : IAddable<C>
                                {
                                    public static C operator +(C left, C right) => left;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an interface declaration is not checked, so an explicit implementation in an interface body is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-001")]
    public async Task VerifyInterfaceDeclarationIsNotChecked()
    {
        const string testData = """
                                interface IA
                                {
                                    void M();
                                }

                                interface IB : IA
                                {
                                    void IA.M()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region matches a generic interface only by its simple name, so a region that carries the type
    /// argument list does not match
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-002")]
    public async Task VerifyRegionNameMatchesOnlyTheSimpleNameOfAGenericInterface()
    {
        const string testData = """
                                using System;

                                class C : IComparable<C>
                                {
                                    #region IComparable<C>

                                    int IComparable<C>.{|RH7409:CompareTo|}(C other) => 0;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an implicit implementation comes from the interface that declares the member, also when the type lists
    /// only an interface derived from it, and that an explicit implementation comes from the interface its name is
    /// qualified with
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-004")]
    public async Task VerifyImplementationComesFromTheInterfaceThatDeclaresTheMember()
    {
        const string testData = """
                                interface IA
                                {
                                    void M();
                                }

                                interface IB : IA
                                {
                                    void N();
                                }

                                interface IC
                                {
                                    void O();
                                }

                                class C : IB, IC
                                {
                                    #region IB

                                    public void {|RH7409:M|}()
                                    {
                                    }

                                    public void N()
                                    {
                                    }

                                    #endregion

                                    #region IC

                                    void IC.O()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override that implements an interface member does not come from the interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-004")]
    [Requirement("RH7409-002")]
    public async Task VerifyOverrideImplementingInterfaceMethodDoesNotComeFromTheInterface()
    {
        const string testData = """
                                interface IA
                                {
                                    void M();
                                }

                                abstract class B
                                {
                                    public abstract void M();
                                }

                                class D : B, IA
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a user-written typed Equals method of a record is an implementation of the equatable interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7409-001")]
    public async Task VerifyRecordEqualsIsGroupedByEquatableInterface()
    {
        const string testData = """
                                record R
                                {
                                    public virtual bool {|#0:Equals|}(R other) => true;

                                    public override int GetHashCode() => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IEquatable")));
    }

    /// <summary>
    /// Characterizes a method that implements the members of two interfaces: only the first interface in base-list order is
    /// accepted, so the verdict changes with the order of the base list. The case is an open question about intended
    /// behavior, so this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMethodImplementingTwoInterfacesIsJudgedByTheFirstInBaseListOrder()
    {
        const string testData = """
                                interface IA
                                {
                                    void M();
                                }

                                interface IB
                                {
                                    void M();
                                }

                                class C : IA, IB
                                {
                                    #region IB

                                    public void {|#0:M|}()
                                    {
                                    }

                                    #endregion
                                }

                                class D : IB, IA
                                {
                                    #region IB

                                    public void M()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData, Diagnostics(RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("IA")));
    }

    /// <summary>
    /// Characterizes a partial method that implements an interface method: it is judged and reported only at its defining
    /// part, and the region of its implementing part is not considered. This test pins today's behavior; the decided
    /// behavior is stated by SHARED-DIAG-004, which judges a partial method by its symbol, and its change belongs to an
    /// existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPartialImplementationMethodIsJudgedAtItsDefiningPart()
    {
        const string testData = """
                                interface I
                                {
                                    void M();

                                    void N();
                                }

                                partial class C : I
                                {
                                    #region Methods

                                    public partial void {|#0:M|}();

                                    public partial void N()
                                    {
                                    }

                                    #endregion

                                    #region I

                                    public partial void M()
                                    {
                                    }

                                    public partial void N();

                                    #endregion
                                }
                                """;

        await Verify(testData, Diagnostics(RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer.DiagnosticId, CreateMessage("I")));
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