using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzerTests : AnalyzerTestsBase<RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that override methods implementing interface members still require the base-type region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7401-001")]
    public async Task VerifyDiagnosticForOverrideMethodImplementingInterfaceInInterfaceRegion()
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
                                    #region IExecutable

                                    public override void {|#0:Execute|}()
                                    {
                                    }

                                    #endregion // IExecutable
                                }
                                """;

        await Verify(testData, Diagnostics(RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that override methods in a matching base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7401-001")]
    public async Task VerifyNoDiagnosticsForOverrideMethodInMatchingRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }
                                }

                                internal class DerivedProcessor : BaseProcessor
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
    /// Verifies that override methods in generic regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7401-001")]
    public async Task VerifyDiagnosticForOverrideMethodInGenericRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region Methods

                                    public override void {|#0:Execute|}()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that regions inside method bodies do not satisfy the rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7401-001")]
    public async Task VerifyDiagnosticForRegionInsideOverrideMethodBody()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    public override void {|#0:Execute|}()
                                    {
                                        #region BaseProcessor
                                        System.Console.WriteLine();
                                        #endregion // BaseProcessor
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that override methods use the type that introduced the original declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7401-001")]
    [Requirement("SHARED-REGGROUP-003")]
    public async Task VerifyDiagnosticsForOverrideMethodsFromDifferentBaseTypes()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }
                                }

                                internal abstract class IntermediateProcessor : BaseProcessor
                                {
                                    public override void {|#0:Execute|}()
                                    {
                                    }

                                    public virtual void Save()
                                    {
                                    }
                                }

                                internal class DerivedProcessor : IntermediateProcessor
                                {
                                    #region Methods

                                    public override void {|#1:Execute|}()
                                    {
                                    }

                                    public override void {|#2:Save|}()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId,
                                 index => CreateMessage(index <= 1 ? "BaseProcessor" : "IntermediateProcessor"),
                                 3));
    }

    /// <summary>
    /// Verifies that property overrides do not trigger the methods rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7401-002")]
    public async Task VerifyNoDiagnosticsForOverrideProperty()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual string Name => string.Empty;
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    public override string Name => base.Name;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that override methods in a region of another name, in no region, or with a region only inside their body
    /// are reported, also when they implement an interface method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7401-001")]
    public async Task VerifyDiagnosticsForOverrideMethodsOutsideTheirBaseTypeRegion()
    {
        const string testData = """
                                interface IRunnable
                                {
                                    void Run();
                                }

                                abstract class B
                                {
                                    public abstract void Run();

                                    public virtual void Stop()
                                    {
                                    }

                                    public virtual void Wait()
                                    {
                                    }
                                }

                                class C : B, IRunnable
                                {
                                    #region IRunnable

                                    public override void {|RH7401:Run|}()
                                    {
                                    }

                                    #endregion

                                    #region Methods

                                    public override void {|RH7401:Stop|}()
                                    {
                                    }

                                    #endregion

                                    public override void {|RH7401:Wait|}()
                                    {
                                        #region B
                                        System.Console.WriteLine();
                                        #endregion
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a method that overrides nothing and an override property are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7401-002")]
    public async Task VerifyNoDiagnosticsForMethodThatOverridesNothingOrOverrideOfOtherKind()
    {
        const string testData = """
                                class B
                                {
                                    public virtual int P => 0;
                                }

                                class C : B
                                {
                                    public override int P => 1;

                                    public void M()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nested type is checked against its own regions and not the region of the outer type around it, and
    /// that each declaration of a partial type is checked against its own regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-001")]
    public async Task VerifyNestedTypeAndEachPartialDeclarationAreCheckedAgainstTheirOwnRegions()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }

                                    public virtual void N()
                                    {
                                    }
                                }

                                class O
                                {
                                    #region B

                                    class N : B
                                    {
                                        public override void {|RH7401:M|}()
                                        {
                                        }
                                    }

                                    #endregion
                                }

                                partial class D : B
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    #endregion
                                }

                                partial class D
                                {
                                    public override void {|RH7401:N|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that record and record struct declarations are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-001")]
    public async Task VerifyRecordAndRecordStructDeclarationsAreChecked()
    {
        const string testData = """
                                record R
                                {
                                    public override string {|RH7401:ToString|}() => "";
                                }

                                record struct S
                                {
                                    #region Object

                                    public override string ToString() => "";

                                    #endregion
                                }

                                record struct T
                                {
                                    public override string {|RH7401:ToString|}() => "";
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region matches a generic base type only by its simple name, compared ordinally, and that a type
    /// argument list, different casing, or further text does not match
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-002")]
    public async Task VerifyRegionNameMatchesOnlyTheSimpleNameOfAGenericBaseType()
    {
        const string testData = """
                                class B<T>
                                {
                                    public virtual void M()
                                    {
                                    }

                                    public virtual void N()
                                    {
                                    }

                                    public virtual void O()
                                    {
                                    }

                                    public virtual void P()
                                    {
                                    }
                                }

                                class C : B<int>
                                {
                                    #region   B

                                    public override void M()
                                    {
                                    }

                                    #endregion

                                    #region B<T>

                                    public override void {|RH7401:N|}()
                                    {
                                    }

                                    #endregion

                                    #region b

                                    public override void {|RH7401:O|}()
                                    {
                                    }

                                    #endregion

                                    #region B // base

                                    public override void {|RH7401:P|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a base type declared with a verbatim identifier is matched by its value and not by its verbatim
    /// spelling
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-002")]
    public async Task VerifyRegionNameMatchesTheValueOfAVerbatimTypeName()
    {
        const string testData = """
                                class @Base
                                {
                                    public virtual void M()
                                    {
                                    }

                                    public virtual void N()
                                    {
                                    }
                                }

                                class C : @Base
                                {
                                    #region Base

                                    public override void M()
                                    {
                                    }

                                    #endregion

                                    #region @Base

                                    public override void {|RH7401:N|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override comes from the type that introduced the member and not from the base type that overrode it
    /// last, and that the overrides of a struct come from Object
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-003")]
    public async Task VerifyOverrideComesFromTheTypeThatIntroducedTheMember()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class D : B
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    #endregion

                                    public virtual void N()
                                    {
                                    }
                                }

                                class E : D
                                {
                                    #region D

                                    public override void {|RH7401:M|}()
                                    {
                                    }

                                    public override void N()
                                    {
                                    }

                                    #endregion
                                }

                                struct S
                                {
                                    #region ValueType

                                    public override string {|RH7401:ToString|}() => "";

                                    #endregion
                                }

                                class C
                                {
                                    #region Object

                                    public override string ToString() => "";

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a sealed override and an abstract override come from the type that introduced the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-003")]
    public async Task VerifySealedAndAbstractOverridesComeFromTheTypeThatIntroducedTheMember()
    {
        const string testData = """
                                abstract class B
                                {
                                    public virtual void M()
                                    {
                                    }

                                    public virtual void N()
                                    {
                                    }
                                }

                                abstract class D : B
                                {
                                    #region B

                                    public abstract override void M();

                                    #endregion

                                    #region D

                                    public abstract override void {|RH7401:N|}();

                                    #endregion
                                }

                                class E : D
                                {
                                    #region B

                                    public sealed override void M()
                                    {
                                    }

                                    public sealed override void N()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes an override in a region nested in a top-level region: it is judged by the top-level region around it,
    /// not by the innermost region. The case is an open question about intended behavior, so this test pins today's
    /// behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOverrideInNestedRegionIsJudgedByItsTopLevelRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }

                                    public virtual void N()
                                    {
                                    }
                                }

                                class D : B
                                {
                                    #region B

                                    #region Methods

                                    public override void M()
                                    {
                                    }

                                    #endregion

                                    #endregion

                                    #region Overrides

                                    #region B

                                    public override void {|RH7401:N|}()
                                    {
                                    }

                                    #endregion

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a base type whose name starts with a lower-case letter: only the region with the exact name matches,
    /// not the capitalized name the region formatting produces. The case is an open question about intended behavior, so
    /// this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionNamedAfterLowerCaseTypeMatchesOnlyTheExactName()
    {
        const string testData = """
                                class myBase
                                {
                                    public virtual void M()
                                    {
                                    }

                                    public virtual void N()
                                    {
                                    }
                                }

                                class C : myBase
                                {
                                    #region myBase

                                    public override void M()
                                    {
                                    }

                                    #endregion

                                    #region MyBase

                                    public override void {|RH7401:N|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a partial override method whose parts lie in two regions that do not match its base type: it is
    /// reported once for each part. The case is an open question about intended behavior, so this test pins today's
    /// behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPartialOverrideMethodIsReportedOncePerPart()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                partial class C : B
                                {
                                    #region Methods

                                    public override partial void {|#0:M|}();

                                    #endregion

                                    #region Other

                                    public override partial void {|#1:M|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData, Diagnostics(RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId, CreateMessage("B"), 2));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given base type
    /// </summary>
    /// <param name="baseTypeName">Base type name</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string baseTypeName)
    {
        return string.Format(AnalyzerResources.RH7401MessageFormat, baseTypeName);
    }

    #endregion // Methods
}