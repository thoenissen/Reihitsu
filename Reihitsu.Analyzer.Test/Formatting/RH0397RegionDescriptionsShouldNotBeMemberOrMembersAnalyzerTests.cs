using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer"/>
/// </summary>
[TestClass]
public class RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzerTests : AnalyzerTestsBase<RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer>
{
    #region Members

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

        await Verify(testData, Diagnostics(RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer.DiagnosticId, AnalyzerResources.RH0397MessageFormat));
    }

    /// <summary>
    /// Verifies that endregion descriptions equal to Members are detected case-insensitively
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, Diagnostics(RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer.DiagnosticId, AnalyzerResources.RH0397MessageFormat));
    }

    /// <summary>
    /// Verifies that longer descriptions containing the forbidden words are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Members
}