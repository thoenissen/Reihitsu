using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzerTests : AnalyzerTestsBase<RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that explicit override events in a matching base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForExplicitOverrideEventInMatchingRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public abstract event System.EventHandler Completed;
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override event System.EventHandler Completed
                                    {
                                        add
                                        {
                                        }
                                        remove
                                        {
                                        }
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that field-like override events in generic regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForFieldLikeOverrideEventInGenericRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public abstract event System.EventHandler Completed;
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region Events

                                    public override event System.EventHandler {|#0:Completed|};

                                    #endregion // Events
                                }
                                """;

        await Verify(testData, Diagnostics(RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that override events use the type that introduced the original declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForOverrideEventsFromDifferentBaseTypes()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual event System.EventHandler Completed;
                                }

                                internal abstract class IntermediateProcessor : BaseProcessor
                                {
                                    public override event System.EventHandler {|#0:Completed|};
                                    public virtual event System.EventHandler Saved;
                                }

                                internal class DerivedProcessor : IntermediateProcessor
                                {
                                    #region Events

                                    public override event System.EventHandler {|#1:Completed|};
                                    public override event System.EventHandler {|#2:Saved|};

                                    #endregion // Events
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId,
                                 index => CreateMessage(index <= 1 ? "BaseProcessor" : "IntermediateProcessor"),
                                 3));
    }

    /// <summary>
    /// Verifies that method overrides do not trigger the events rule
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
        return string.Format(AnalyzerResources.RH7403MessageFormat, baseTypeName);
    }

    #endregion // Methods
}