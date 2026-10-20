using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer"/>
/// </summary>
[TestClass]
public class RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzerTests : AnalyzerTestsBase<RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that descriptive region labels do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDescriptiveRegionLabels()
    {
        const string testData = """
                                internal class Example
                                {
                                    #region Methods

                                    internal void DoWork()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region descriptions equal to Member are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7307-001")]
    public async Task VerifyRegionDescriptionEqualToMemberIsDetected()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:#region Member|}

                                    internal void DoWork()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer.DiagnosticId, AnalyzerResources.RH7307MessageFormat));
    }

    /// <summary>
    /// Verifies that endregion descriptions equal to Members are detected case-insensitively
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7307-001")]
    public async Task VerifyEndRegionDescriptionEqualToMembersIsDetectedCaseInsensitively()
    {
        const string testData = """
                                internal class Example
                                {
                                    #region Methods

                                    internal void DoWork()
                                    {
                                    }

                                    {|#0:#endregion // mEmBeRs|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer.DiagnosticId, AnalyzerResources.RH7307MessageFormat));
    }

    /// <summary>
    /// Verifies that longer descriptions containing the forbidden words are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7307-002")]
    public async Task VerifyLongerDescriptionsContainingForbiddenWordsAreIgnored()
    {
        const string testData = """
                                internal class Example
                                {
                                    #region Member helpers

                                    internal void DoWork()
                                    {
                                    }

                                    #endregion // Members of Example
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region and end directives whose whole description is Member or Members, in any casing, are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7307-001")]
    [Requirement("SHARED-REGDESC-001")]
    [Requirement("SHARED-REGDESC-002")]
    public async Task DescriptionThatIsOnlyMemberOrMembersIsReported()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7307:#region members|}

                                    int _a;

                                    {|RH7307:#endregion //Member|}

                                    #region Fields

                                    int _b;

                                    {|RH7307:#endregion MEMBERS|}
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a description containing Member or Members together with other text is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7307-002")]
    public async Task LongerDescriptionIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    #region Static members

                                    static int _a;

                                    #endregion // Static members

                                    #region Member list

                                    int _b;

                                    #endregion // Member list
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the region and the end directive are each checked and reported on their own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7307-001")]
    [Requirement("SHARED-REGDESC-002")]
    public async Task EveryRegionDirectiveIsReportedOnItsOwn()
    {
        const string testData = """
                                class C
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    {|RH7307:#endregion // Members|}

                                    {|RH7307:#region Members|}

                                    int _a;

                                    {|RH7307:#endregion // Members|}
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region directives inside an element body and inside an excluded branch are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7307-001")]
    [Requirement("SHARED-REGDESC-002")]
    public async Task DirectivesInAnElementBodyAndAnExcludedBranchAreChecked()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        {|RH7307:#region Members|}
                                        var a = 1;
                                        #endregion // Locals
                                    }

                                #if false
                                    {|RH7307:#region Members|}

                                    #endregion // Inactive
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}