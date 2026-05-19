using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0807NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0807NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzerTests : AnalyzerTestsBase<RH0807NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that non-override properties inside override regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, Diagnostics(RH0807NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that mixed override kinds in the same base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given region
    /// </summary>
    /// <param name="regionName">Region name</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionName)
    {
        return string.Format(AnalyzerResources.RH0807MessageFormat, regionName);
    }

    #endregion // Methods
}