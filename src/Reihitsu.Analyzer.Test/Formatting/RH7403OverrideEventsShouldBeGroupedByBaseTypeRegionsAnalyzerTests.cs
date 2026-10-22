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
    /// Verifies that override events implementing interface members still require the base-type region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7403-001")]
    public async Task VerifyDiagnosticForOverrideEventImplementingInterfaceInInterfaceRegion()
    {
        const string testData = """
                                internal interface ICompletable
                                {
                                    event System.EventHandler Completed;
                                }

                                internal abstract class BaseProcessor
                                {
                                    public abstract event System.EventHandler Completed;
                                }

                                internal class DerivedProcessor : BaseProcessor, ICompletable
                                {
                                    #region ICompletable

                                    public override event System.EventHandler {|#0:Completed|};

                                    #endregion // ICompletable
                                }
                                """;

        await Verify(testData, Diagnostics(RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that explicit override events in a matching base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7403-001")]
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
    [Requirement("RH7403-001")]
    [Requirement("SHARED-REGGROUP-005")]
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
    [Requirement("RH7403-001")]
    [Requirement("SHARED-REGGROUP-003")]
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
    [Requirement("RH7403-002")]
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

    /// <summary>
    /// Verifies that field-like and accessor override events in a region of another name or in no region are reported, also
    /// when they implement an interface event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7403-001")]
    public async Task VerifyDiagnosticsForOverrideEventsOutsideTheirBaseTypeRegion()
    {
        const string testData = """
                                using System;

                                interface INotifier
                                {
                                    event EventHandler Changed;
                                }

                                abstract class B
                                {
                                    public abstract event EventHandler Changed;

                                    public virtual event EventHandler Saved;

                                    public virtual event EventHandler Closed;
                                }

                                class C : B, INotifier
                                {
                                    #region INotifier

                                    public override event EventHandler {|RH7403:Changed|};

                                    #endregion

                                    #region Events

                                    public override event EventHandler {|RH7403:Saved|}
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }

                                    #endregion

                                    public override event EventHandler {|RH7403:Closed|};
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a field-like override event in the region named after its base type is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7403-001")]
    public async Task VerifyNoDiagnosticsForFieldLikeOverrideEventInMatchingRegion()
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
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an event that overrides nothing and an override method are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7403-002")]
    public async Task VerifyNoDiagnosticsForEventThatOverridesNothingOrOverrideOfOtherKind()
    {
        const string testData = """
                                using System;

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

                                    public event EventHandler Changed;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a field-like override event with several declarators: only its first declarator is judged, so a second
    /// declarator from another base type is not reported in the region of the first one's base type. This test pins
    /// today's behavior; the decided behavior is stated by SHARED-DIAG-006, which checks every declarator, and its change
    /// belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFieldLikeOverrideEventWithSeveralDeclaratorsIsJudgedByItsFirstDeclarator()
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
                                    #region B1

                                    public override event EventHandler A, X;

                                    #endregion
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