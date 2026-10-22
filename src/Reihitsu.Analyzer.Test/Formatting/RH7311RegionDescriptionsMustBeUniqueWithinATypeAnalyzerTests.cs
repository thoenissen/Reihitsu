using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer"/>
/// </summary>
[TestClass]
public class RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests : AnalyzerTestsBase<RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that two regions sharing the same description produce a diagnostic on the second one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    [Requirement("RH7311-003")]
    public async Task VerifyDuplicateRegionDescriptionIsReported()
    {
        const string testData = """
                                public class C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that region descriptions are compared case-insensitively
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task VerifyDuplicateRegionDescriptionIsReportedCaseInsensitively()
    {
        const string testData = """
                                public class C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("methods")));
    }

    /// <summary>
    /// Verifies that the third and every further repeat of a description is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    [Requirement("RH7311-003")]
    public async Task VerifyEveryFurtherDuplicateRegionDescriptionIsReported()
    {
        const string testData = """
                                public class C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#1:#region Methods|}

                                    public void Third()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods"), 2));
    }

    /// <summary>
    /// Verifies that duplicate region descriptions are reported inside an interface declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task VerifyDuplicateRegionDescriptionIsReportedInInterface()
    {
        const string testData = """
                                public interface IWidget
                                {
                                    #region Methods

                                    void First();

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    void Second();

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that duplicate region descriptions are reported inside a struct declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task VerifyDuplicateRegionDescriptionIsReportedInStruct()
    {
        const string testData = """
                                public struct C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that duplicate region descriptions are reported inside a record declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task VerifyDuplicateRegionDescriptionIsReportedInRecord()
    {
        const string testData = """
                                public record C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that duplicate region descriptions are reported inside a record struct declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task VerifyDuplicateRegionDescriptionIsReportedInRecordStruct()
    {
        const string testData = """
                                public record struct C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that distinct region descriptions do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task VerifyNoDiagnosticForDistinctRegionDescriptions()
    {
        const string testData = """
                                public class C
                                {
                                    #region Fields

                                    private int _value;

                                    #endregion // Fields

                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleRegion()
    {
        const string testData = """
                                public class C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that identical region descriptions in two unrelated types do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-002")]
    public async Task VerifyNoDiagnosticForDuplicateDescriptionsInDifferentTypes()
    {
        const string testData = """
                                public class First
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion // Methods
                                }

                                public class Second
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region in a nested type does not collide with the same description in the outer type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task VerifyNoDiagnosticForDuplicateDescriptionInNestedType()
    {
        const string testData = """
                                public class Outer
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion // Methods

                                    #region Nested

                                    public class Inner
                                    {
                                        #region Methods

                                        public void Method()
                                        {
                                        }

                                        #endregion // Methods
                                    }

                                    #endregion // Nested
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two regions without a description do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task VerifyNoDiagnosticForRegionsWithoutADescription()
    {
        const string testData = """
                                public class C
                                {
                                    #region

                                    public void First()
                                    {
                                    }

                                    #endregion

                                    #region

                                    public void Second()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that every later top-level region repeating an earlier description, ignoring case, is reported, while regions without a description are not compared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    [Requirement("RH7311-003")]
    [Requirement("SHARED-REGDESC-001")]
    public async Task RepeatedDescriptionsAreReportedOnTheLaterRegions()
    {
        const string testData = """
                                class C
                                {
                                    #region Methods

                                    void A()
                                    {
                                    }

                                    #endregion

                                    {|RH7311:#region  methods|}

                                    void B()
                                    {
                                    }

                                    #endregion

                                    {|RH7311:#region Methods|}

                                    void D()
                                    {
                                    }

                                    #endregion

                                    #region

                                    int _a;

                                    #endregion

                                    #region

                                    int _b;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the regions of two declarations of a partial type are not compared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionsOfAnotherPartialDeclarationAreNotCompared()
    {
        const string testData = """
                                partial class P
                                {
                                    #region Methods

                                    void A()
                                    {
                                    }

                                    #endregion
                                }

                                partial class P
                                {
                                    #region Methods

                                    void B()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nested region and a region of a nested type are not top-level regions of the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task NestedRegionsAndRegionsOfNestedTypesAreNotTopLevelRegions()
    {
        const string testData = """
                                class C
                                {
                                    #region Methods

                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion

                                    #endregion

                                    class D
                                    {
                                        #region Methods

                                        void N()
                                        {
                                        }

                                        #endregion
                                    }

                                    {|RH7311:#region Methods|}

                                    void O()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes the current behavior for the same description in an <c>#if</c> and an <c>#else</c> branch: the region
    /// of the second branch is reported. This test pins today's behavior; the decided behavior is stated by
    /// SHARED-TYPEREGION-006, which does not compare the regions of different branches of one conditional directive, and
    /// its change belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SameDescriptionInIfAndElseBranchIsCurrentlyReported()
    {
        const string testData = """
                                class C
                                {
                                #if DEBUG
                                    #region Fields
                                    int _a;
                                    #endregion
                                #else
                                    {|RH7311:#region Fields|}
                                    int _b;
                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given region description
    /// </summary>
    /// <param name="regionDescription">Region description</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionDescription)
    {
        return string.Format(AnalyzerResources.RH7311MessageFormat, regionDescription);
    }

    #endregion // Methods
}