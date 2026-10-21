using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzerTests : AnalyzerTestsBase<RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that non-override indexers inside override regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7408-001")]
    [Requirement("SHARED-REGGROUP-005")]
    public async Task VerifyDiagnosticForNonOverrideIndexerInOverrideRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual string this[int index]
                                    {
                                        get
                                        {
                                            return string.Empty;
                                        }
                                    }
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override string this[int index]
                                    {
                                        get
                                        {
                                            return base[index];
                                        }
                                    }

                                    public string {|#0:this|}[string key]
                                    {
                                        get
                                        {
                                            return key;
                                        }
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData, Diagnostics(RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that mixed override kinds in the same base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7408-002")]
    public async Task VerifyNoDiagnosticsForMixedOverrideKindsInSameRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }

                                    public virtual string this[int index]
                                    {
                                        get
                                        {
                                            return string.Empty;
                                        }
                                    }
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

                                    public override string this[int index]
                                    {
                                        get
                                        {
                                            return base[index];
                                        }
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that non-override indexers in generic regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7408-002")]
    public async Task VerifyNoDiagnosticsForNonOverrideIndexerInGenericRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual string this[int index]
                                    {
                                        get
                                        {
                                            return string.Empty;
                                        }
                                    }
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override string this[int index]
                                    {
                                        get
                                        {
                                            return base[index];
                                        }
                                    }

                                    #endregion // BaseProcessor

                                    #region Indexers

                                    public string this[string key]
                                    {
                                        get
                                        {
                                            return key;
                                        }
                                    }

                                    #endregion // Indexers
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that base-type named regions without override indexers do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7408-002")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyNoDiagnosticsForBaseTypeNamedRegionWithoutOverrideIndexers()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public string this[string key]
                                    {
                                        get
                                        {
                                            return key;
                                        }
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an indexer that overrides nothing is reported in an override region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7408-001")]
    public async Task VerifyDiagnosticForIndexerOfOwnInOverrideRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual int this[int index] => 0;
                                }

                                class C : B
                                {
                                    #region B

                                    public override int this[int index] => 1;

                                    public int {|RH7408:this|}[string key] => 0;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override indexer in an override region and an indexer that overrides nothing in an ordinary region
    /// are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7408-002")]
    public async Task VerifyNoDiagnosticsForOverrideIndexerAndIndexerInOrdinaryRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual int this[int index] => 0;
                                }

                                class C : B
                                {
                                    #region B

                                    public override int this[int index] => 1;

                                    #endregion

                                    #region Indexers

                                    public int this[string key] => 0;

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
        return string.Format(AnalyzerResources.RH7408MessageFormat, regionName);
    }

    #endregion // Methods
}