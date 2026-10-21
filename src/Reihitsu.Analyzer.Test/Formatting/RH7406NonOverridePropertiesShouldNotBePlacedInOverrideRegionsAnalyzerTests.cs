using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzerTests : AnalyzerTestsBase<RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that non-override properties inside override regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7406-001")]
    public async Task VerifyDiagnosticForNonOverridePropertyInOverrideRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual string Name => string.Empty;
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override string Name => base.Name;
                                    public string {|#0:DisplayName|} => Name;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData, Diagnostics(RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that mixed override kinds in the same base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7406-002")]
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
    /// Verifies that non-override properties in generic regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7406-002")]
    public async Task VerifyNoDiagnosticsForNonOverridePropertyInGenericRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual string Name => string.Empty;
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override string Name => base.Name;

                                    #endregion // BaseProcessor

                                    #region Properties

                                    public string DisplayName => Name;

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that base-type named regions without override properties do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7406-002")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyNoDiagnosticsForBaseTypeNamedRegionWithoutOverrideProperties()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public string DisplayName => string.Empty;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a property that overrides nothing is reported in an override region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7406-001")]
    public async Task VerifyDiagnosticForPropertyOfOwnInOverrideRegion()
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

                                    public int {|RH7406:Q|} => 0;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override property in an override region and a property that overrides nothing in an ordinary region
    /// are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7406-002")]
    public async Task VerifyNoDiagnosticsForOverridePropertyAndPropertyInOrdinaryRegion()
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

                                    #endregion

                                    #region Properties

                                    public int Q => 0;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a static property, a new property, and an interface implementation that override nothing are reported
    /// in an override region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7406-001")]
    public async Task VerifyDiagnosticsForStaticNewAndInterfacePropertiesInOverrideRegion()
    {
        const string testData = """
                                interface INamed
                                {
                                    string Name { get; }
                                }

                                class B
                                {
                                    public virtual int P => 0;

                                    public int H => 0;
                                }

                                class C : B, INamed
                                {
                                    #region B

                                    public override int P => 1;

                                    public static int {|RH7406:S|} => 0;

                                    public new int {|RH7406:H|} => 1;

                                    public string {|RH7406:Name|} => "";

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a region that only an override method names: it is not an override region of properties, so a property
    /// that overrides nothing in it is not reported. The case is an open question about intended behavior, so this test
    /// pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOverrideOfOtherMemberKindDoesNotMakePropertyOverrideRegion()
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

                                    public int Q => 0;

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
        return string.Format(AnalyzerResources.RH7406MessageFormat, regionName);
    }

    #endregion // Methods
}