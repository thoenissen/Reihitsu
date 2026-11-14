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
    /// Verifies that every declarator of a field-like override event makes the region named after its base type an
    /// override region, so an event that overrides nothing in the region of the second declarator's base type is
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyEveryDeclaratorOfOverrideEventNamesOverrideRegion()
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

                                    public event EventHandler {|RH7407:F|};

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that every declarator of a field-like override event makes the region named after its base type an
    /// override region when the declarators are in the opposite order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyEveryDeclaratorOfOverrideEventNamesOverrideRegionInReversedOrder()
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
                                    public override event EventHandler X, A;

                                    #region B1

                                    public event EventHandler {|RH7407:F|};

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a field-like event with several declarators in an override region is reported once per declarator,
    /// each on its identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    [Requirement("SHARED-REGGROUP-005")]
    public async Task VerifyEventWithSeveralDeclaratorsInOverrideRegionIsReportedOncePerDeclarator()
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

                                    public event EventHandler {|RH7407:F|}, {|RH7407:G|};

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that each declarator of a partial field-like event that overrides nothing and lies in an override region
    /// is reported once, on its declarator in the defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    [Requirement("SHARED-REGGROUP-005")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyEachDeclaratorOfPartialFieldLikeEventInOverrideRegionIsReportedOnItsDefiningPart()
    {
        const string testData = """
                                using System;

                                class B
                                {
                                    public virtual event EventHandler E;
                                }

                                partial class C : B
                                {
                                    #region B

                                    public override event EventHandler E;

                                    public partial event EventHandler {|RH7407:F|}, {|RH7407:G|};

                                    #endregion

                                    public partial event EventHandler F
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }

                                    public partial event EventHandler G
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of a <see langword="partial"/> event that overrides nothing and both
    /// of whose parts lie in an override region: it is reported once, on its defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    [Requirement("SHARED-DIAG-004")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialEventIsReportedOnceOnItsDefiningPart()
    {
        const string testData = """
                                using System;

                                class B
                                {
                                    public virtual event Action Changed;
                                }

                                partial class C : B
                                {
                                    #region B

                                    public override event Action Changed;

                                    public partial event Action {|#0:E|};

                                    public partial event Action E
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

        await Verify(testData, Diagnostics(RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("B")));
    }

    /// <summary>
    /// Verifies that a <see langword="partial"/> non-override event whose defining part lies in generated code, with
    /// both parts in an override region, is reported once, on its implementing part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialNonOverrideEventWithGeneratedDefiningPartIsReportedOnItsImplementingPart()
    {
        const string userPart = """
                                using System;

                                class B
                                {
                                    public virtual event Action Changed;
                                }

                                partial class C : B
                                {
                                    #region B

                                    public override event Action Changed;

                                    public partial event Action {|#0:E|}
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

        const string generatedPart = """
                                     using System;

                                     partial class C
                                     {
                                         #region B

                                         public partial event Action E;

                                         #endregion
                                     }
                                     """;

        await Verify(userPart,
                     test => test.TestState.Sources.Add(("/0/C.g.cs", generatedPart)),
                     Diagnostics(RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("B")));
    }

    /// <summary>
    /// Verifies that an override method makes the region named after its base type an override region, so a event that
    /// overrides nothing in it is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7407-001")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyOverrideOfOtherMemberKindMakesEventOverrideRegion()
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

                                    public event System.EventHandler {|RH7407:E|};

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
        return string.Format(AnalyzerResources.RH7407MessageFormat, regionName);
    }

    #endregion // Methods
}