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