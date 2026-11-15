using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// Verifies that the regions of an <c>#if</c> and an <c>#else</c> branch with the same description are not compared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task SameDescriptionInIfAndElseBranchIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                #if DEBUG
                                    #region Fields

                                    int _a;

                                    #endregion
                                #else
                                    #region Fields

                                    int _b;

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the regions of an <c>#if</c> and an <c>#else</c> branch with the same description are not compared
    /// when the <c>#if</c> branch is the one the configuration compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task SameDescriptionInIfAndElseBranchIsNotReportedWhenTheIfBranchIsCompiled()
    {
        const string testData = """
                                class C
                                {
                                #if DEBUG
                                    #region Fields

                                    int _a;

                                    #endregion
                                #else
                                    #region Fields

                                    int _b;

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies that the regions of an <c>#if</c> and an <c>#elif</c> branch with the same description are not compared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task SameDescriptionInIfAndElifBranchIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                #if DEBUG
                                    #region Fields

                                    int _a;

                                    #endregion
                                #elif TRACE
                                    #region Fields

                                    int _b;

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example in which the regions of an <c>#if</c> and an <c>#else</c> branch, and a region
    /// in a branch and one outside every branch, are not compared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task RegionsInBranchesAndARegionOutsideEveryBranchAreNotCompared()
    {
        const string testData = """
                                class C
                                {
                                #if DEBUG
                                    #region Fields

                                    int _a;

                                    #endregion
                                #else
                                    #region Fields

                                    int _b;

                                    #endregion
                                #endif

                                    #region Fields

                                    int _c;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two regions outside every branch are still compared when a conditional directive with a region of
    /// the same description stands between them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task RegionsOutsideEveryBranchAreComparedAcrossAConditionalDirective()
    {
        const string testData = """
                                class C
                                {
                                    #region Fields

                                    int _a;

                                    #endregion

                                #if DEBUG
                                    #region Fields

                                    int _b;

                                    #endregion
                                #endif

                                    {|RH7311:#region Fields|}

                                    int _c;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example in which the region of a nested enum is not compared with the regions of the
    /// type that holds the enum
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionOfANestedEnumIsNotComparedWithTheRegionsOfTheType()
    {
        const string testData = """
                                class C
                                {
                                    #region Values

                                    int _a;

                                    #endregion

                                    enum E
                                    {
                                        #region Values
                                        A,
                                        #endregion
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the region of a nested enum that stands in front of the regions of the type is not compared with them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionOfANestedEnumInFrontOfTheRegionsOfTheTypeIsNotCompared()
    {
        const string testData = """
                                class C
                                {
                                    enum E
                                    {
                                        #region Values
                                        A,
                                        #endregion
                                    }

                                    #region Values

                                    int _a;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two regions with the same description inside a nested enum are not top-level regions of the type
    /// that holds the enum
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task TwoRegionsWithTheSameDescriptionInsideANestedEnumAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                    enum E
                                    {
                                        #region Values
                                        A,
                                        #endregion

                                        #region Values
                                        B,
                                        #endregion
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the top-level regions of a type are still compared when a nested enum with a region of the same
    /// description stands between them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    [Requirement("RH7311-002")]
    public async Task TopLevelRegionsAreComparedAcrossANestedEnumWithARegion()
    {
        const string testData = """
                                class C
                                {
                                    #region Values

                                    int _a;

                                    #endregion

                                    enum E
                                    {
                                        #region Values
                                        A,
                                        #endregion
                                    }

                                    {|RH7311:#region Values|}

                                    int _b;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example in which a conditional directive encloses the whole declaration, so its
    /// regions lie in no branch and are compared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task RegionsOfADeclarationThatAConditionalDirectiveEnclosesAreCompared()
    {
        const string testData = """
                                #if !NEVER
                                class C
                                {
                                    #region Methods

                                    void A()
                                    {
                                    }

                                    #endregion

                                    {|RH7311:#region Methods|}

                                    void B()
                                    {
                                    }

                                    #endregion
                                }
                                #endif
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two regions in one branch of a conditional directive that starts inside the declaration are not
    /// compared, also when the branch is the one the configuration compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task RegionsInABranchThatStartsInsideTheDeclarationAreNotCompared()
    {
        const string testData = """
                                class C
                                {
                                #if !NEVER
                                    #region Methods

                                    void A()
                                    {
                                    }

                                    #endregion

                                    #region Methods

                                    void B()
                                    {
                                    }

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region in a branch that starts inside the declaration is not compared, also when another
    /// conditional directive encloses the whole declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7311-001")]
    public async Task RegionInABranchInsideADeclarationThatAConditionalDirectiveEnclosesIsNotCompared()
    {
        const string testData = """
                                #if !NEVER
                                class C
                                {
                                    #region Methods

                                    void A()
                                    {
                                    }

                                    #endregion

                                #if !NEVER
                                    #region Methods

                                    void B()
                                    {
                                    }

                                    #endregion
                                #endif
                                }
                                #endif
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Defines the preprocessor symbol <c>DEBUG</c> for the test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Solution</returns>
    private static Solution DefineDebug(Solution solution, ProjectId projectId)
    {
        return solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                   ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("DEBUG"))
                   : solution;
    }

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