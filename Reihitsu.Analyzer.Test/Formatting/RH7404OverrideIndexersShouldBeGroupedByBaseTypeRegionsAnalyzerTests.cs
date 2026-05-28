using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzerTests : AnalyzerTestsBase<RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that override indexers in a matching base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOverrideIndexerInMatchingRegion()
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
                                        set
                                        {
                                        }
                                    }
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override string this[int index]
                                    {
                                        get
                                        {
                                            return base[index];
                                        }
                                        set
                                        {
                                            base[index] = value;
                                        }
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that override indexers in generic regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForOverrideIndexerInGenericRegion()
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
                                        set
                                        {
                                        }
                                    }
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region Indexers

                                    public override string {|#0:this|}[int index]
                                    {
                                        get
                                        {
                                            return base[index];
                                        }
                                        set
                                        {
                                            base[index] = value;
                                        }
                                    }

                                    #endregion // Indexers
                                }
                                """;

        await Verify(testData, Diagnostics(RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that override indexers use the type that introduced the original declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForOverrideIndexersFromDifferentBaseTypes()
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
                                        set
                                        {
                                        }
                                    }
                                }

                                internal abstract class IntermediateProcessor : BaseProcessor
                                {
                                    public override string {|#0:this|}[int index]
                                    {
                                        get
                                        {
                                            return base[index];
                                        }
                                        set
                                        {
                                            base[index] = value;
                                        }
                                    }

                                    public virtual string this[string key]
                                    {
                                        get
                                        {
                                            return string.Empty;
                                        }
                                        set
                                        {
                                        }
                                    }
                                }

                                internal class DerivedProcessor : IntermediateProcessor
                                {
                                    #region Indexers

                                    public override string {|#1:this|}[int index]
                                    {
                                        get
                                        {
                                            return base[index];
                                        }
                                        set
                                        {
                                            base[index] = value;
                                        }
                                    }

                                    public override string {|#2:this|}[string key]
                                    {
                                        get
                                        {
                                            return base[key];
                                        }
                                        set
                                        {
                                            base[key] = value;
                                        }
                                    }

                                    #endregion // Indexers
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId,
                                 index => CreateMessage(index <= 1 ? "BaseProcessor" : "IntermediateProcessor"),
                                 3));
    }

    /// <summary>
    /// Verifies that property overrides do not trigger the indexers rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given base type
    /// </summary>
    /// <param name="baseTypeName">Base type name</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string baseTypeName)
    {
        return string.Format(AnalyzerResources.RH7404MessageFormat, baseTypeName);
    }

    #endregion // Methods
}