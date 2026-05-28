using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzerTests : AnalyzerTestsBase<RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that override properties in a matching base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOverridePropertyInMatchingRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual string Name => string.Empty;
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override string Name => base.Name;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that override properties outside any top-level region produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForOverridePropertyOutsideRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual string Name => string.Empty;
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    public override string {|#0:Name|} => base.Name;
                                }
                                """;

        await Verify(testData, Diagnostics(RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that properties from different declaring base types are validated independently
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForOverridePropertiesFromDifferentBaseTypes()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual string Name => string.Empty;
                                }

                                internal abstract class IntermediateProcessor : BaseProcessor
                                {
                                    public override string {|#0:Name|}  => base.Name;
                                    public virtual string Title => string.Empty;
                                }

                                internal class DerivedProcessor : IntermediateProcessor
                                {
                                    #region Properties

                                    public override string {|#1:Name|} => base.Name;
                                    public override string {|#2:Title|} => base.Title;

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId,
                                 index => CreateMessage(index <= 1 ? "BaseProcessor" : "IntermediateProcessor"),
                                 3));
    }

    /// <summary>
    /// Verifies that method overrides do not trigger the properties rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOverrideMethod()
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
                                    public override void Execute()
                                    {
                                    }
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
        return string.Format(AnalyzerResources.RH7402MessageFormat, baseTypeName);
    }

    #endregion // Methods
}