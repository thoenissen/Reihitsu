using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzerTests : AnalyzerTestsBase<RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that non-override events inside override regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    [Requirement("SHARED-REGGROUP-005")]
    public async Task VerifyDiagnosticForNonOverrideEventInOverrideRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public abstract event System.EventHandler Completed;
                                }

                                internal sealed class ReportProcessor : BaseProcessor
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

                                    public event System.EventHandler {|#0:Saved|};

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData, Diagnostics(RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that mixed override kinds in the same base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-002")]
    public async Task VerifyNoDiagnosticsForMixedOverrideKindsInSameRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }

                                    public abstract event System.EventHandler Completed;
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

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
    /// Verifies that non-override events in generic regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-002")]
    public async Task VerifyNoDiagnosticsForNonOverrideEventInGenericRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public abstract event System.EventHandler Completed;
                                }

                                internal sealed class ReportProcessor : BaseProcessor
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

                                    #region Events

                                    public event System.EventHandler Saved;

                                    #endregion // Events
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that base-type named regions without override events do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-002")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyNoDiagnosticsForBaseTypeNamedRegionWithoutOverrideEvents()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public event System.EventHandler Saved;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a field-like event and an accessor event that override nothing are reported in an override region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    public async Task VerifyDiagnosticsForFieldLikeAndAccessorEventsOfOwnInOverrideRegion()
    {
        const string testData = """
                                using System;

                                class B
                                {
                                    public virtual event EventHandler Changed;
                                }

                                class C : B
                                {
                                    #region B

                                    public override event EventHandler Changed;

                                    public event EventHandler {|RH7407:Saved|};

                                    public event EventHandler {|RH7407:Closed|}
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override event with accessors makes a region an override region like a field-like override event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    public async Task VerifyAccessorOverrideEventMakesOverrideRegion()
    {
        const string testData = """
                                using System;

                                class B
                                {
                                    public virtual event EventHandler Changed;
                                }

                                class C : B
                                {
                                    #region B

                                    public override event EventHandler Changed
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }

                                    public event EventHandler {|RH7407:Saved|};

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override event in an override region and an event that overrides nothing in an ordinary region are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-002")]
    public async Task VerifyNoDiagnosticsForOverrideEventAndEventInOrdinaryRegion()
    {
        const string testData = """
                                using System;

                                class B
                                {
                                    public virtual event EventHandler Changed;
                                }

                                class C : B
                                {
                                    #region B

                                    public override event EventHandler Changed;

                                    #endregion

                                    #region Events

                                    public event EventHandler Saved;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a field-like override event with several declarators: only the base type of its first declarator names
    /// an override region, so an event in the region of the second declarator's base type is not reported. This test pins
    /// today's behavior; the decided behavior is stated by SHARED-DIAG-006, which checks every declarator, and its change
    /// belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOnlyFirstDeclaratorOfOverrideEventNamesOverrideRegion()
    {
        const string testData = """
                                using System;

                                class B1
                                {
                                    public virtual event EventHandler A;
                                }

                                class B2 : B1
                                {
                                    public virtual event EventHandler X;
                                }

                                class D : B2
                                {
                                    public override event EventHandler A, X;

                                    #region B2

                                    public event EventHandler F;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a field-like event with several declarators in an override region: it is reported once, at its first
    /// declarator. This test pins today's behavior; the decided behavior is stated by SHARED-DIAG-006, which reports one
    /// diagnostic per reported declarator, and its change belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEventWithSeveralDeclaratorsInOverrideRegionIsReportedOnceAtFirstDeclarator()
    {
        const string testData = """
                                using System;

                                class B
                                {
                                    public virtual event EventHandler E;
                                }

                                class C : B
                                {
                                    #region B

                                    public override event EventHandler E;

                                    public event EventHandler {|#0:F|}, G;

                                    #endregion
                                }
                                """;

        await Verify(testData, Diagnostics(RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("B")));
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
        return string.Format(AnalyzerResources.RH7407MessageFormat, regionName);
    }

    #endregion // Methods
}