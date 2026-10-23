using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzerTests : AnalyzerTestsBase<RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that non-override methods inside override regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    public async Task VerifyDiagnosticForNonOverrideMethodInOverrideRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

                                    private static string {|#0:BuildCacheKey|}()
                                    {
                                        return "report";
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData, Diagnostics(RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that mixed override kinds in the same base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    public async Task VerifyNoDiagnosticsForMixedOverrideKindsInSameRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }

                                    public virtual string Name => string.Empty;
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

                                    public override string Name => base.Name;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that non-override methods in generic regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    public async Task VerifyNoDiagnosticsForNonOverrideMethodInGenericRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

                                    #endregion // BaseProcessor

                                    #region Methods

                                    private static string BuildCacheKey()
                                    {
                                        return "report";
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that base-type named regions without override methods do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyNoDiagnosticsForBaseTypeNamedRegionWithoutOverrideMethods()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    private static string BuildCacheKey()
                                    {
                                        return "report";
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a static method, a new method, and an interface implementation that override nothing are reported in
    /// an override region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    public async Task VerifyDiagnosticsForStaticNewAndInterfaceMethodsInOverrideRegion()
    {
        const string testData = """
                                interface IRunnable
                                {
                                    void Run();
                                }

                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B, IRunnable
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    public static void {|RH7405:S|}()
                                    {
                                    }

                                    public new string {|RH7405:ToString|}() => "";

                                    public void {|RH7405:Run|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override method, a constructor, and an operator in an override region and a method in an ordinary
    /// region are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    public async Task VerifyNoDiagnosticsForConstructorOperatorAndMethodInOrdinaryRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    public C()
                                    {
                                    }

                                    public static C operator +(C left, C right) => left;

                                    #endregion

                                    #region Methods

                                    private void N()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region named after the base type of an override is an override region also when the override lies
    /// outside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-006")]
    [Requirement("RH7405-001")]
    public async Task VerifyRegionIsOverrideRegionWhenOverrideLiesOutsideIt()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B
                                {
                                    public override void M()
                                    {
                                    }

                                    #region B

                                    public void {|RH7405:N|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a region that only an override property names: it is not an override region of methods, so a method
    /// that overrides nothing in it is not reported. This test pins today's behavior; the decided behavior is stated by
    /// SHARED-DIAG-009, whose verdict does not depend on whether the type declares a member of another kind, and its
    /// change belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOverrideOfOtherMemberKindDoesNotMakeMethodOverrideRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual int P => 0;
                                }

                                class C : B
                                {
                                    #region B

                                    public override int P => 1;

                                    public void N()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a partial type whose override lies in another declaration than the region named after its base type:
    /// the region is not an override region of the declaration that holds it. This test pins today's behavior; the decided
    /// behavior is stated by SHARED-DIAG-009, whose verdict does not depend on which part of a partial type holds the
    /// override, and its change belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOverrideInOtherPartialDeclarationDoesNotMakeOverrideRegion()
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
                                    public override void M()
                                    {
                                    }
                                }

                                partial class C
                                {
                                    #region B

                                    public void N()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes methods in regions nested in a top-level region: they are judged by the top-level region around them,
    /// not by the innermost region. This test pins today's behavior; the decided behavior is stated by
    /// SHARED-TYPEREGION-006, which judges a member by the innermost region that encloses it, and its change belongs to an
    /// existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMethodInNestedRegionIsJudgedByItsTopLevelRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    #region Helpers

                                    private void {|RH7405:H|}()
                                    {
                                    }

                                    #endregion

                                    #endregion

                                    #region Helpers2

                                    #region B

                                    private void K()
                                    {
                                    }

                                    #endregion

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given region
    /// </summary>
    /// <param name="regionName">Region name</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionName)
    {
        return string.Format(AnalyzerResources.RH7405MessageFormat, regionName);
    }

    #endregion // Methods
}