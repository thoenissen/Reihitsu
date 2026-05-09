using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer"/>
/// </summary>
[TestClass]
public class RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzerTests : AnalyzerTestsBase<RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer, RH0328RegionsShouldStartWithAUpperCaseLetterCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifying that region names starting with lowercase are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLowercaseRegionNamesAreDetected()
    {
        const string testData = """
                                internal class RH0328
                                {
                                    #region Properties
                                    public int P1 { get; set; }
                                    #endregion // Properties
                                    {|#0:#region properties|}
                                    public int P2 { get; set; }
                                    #endregion // properties
                                }
                                """;
        const string fixedData = """
                                 internal class RH0328
                                 {
                                     #region Properties
                                     public int P1 { get; set; }
                                     #endregion // Properties
                                     #region Properties
                                     public int P2 { get; set; }
                                     #endregion // Properties
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH0328MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix also updates a matching endregion comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUpdatesMatchingEndRegionComment()
    {
        const string testData = """
                                internal class RH0328
                                {
                                    {|#0:#region members|}
                                    internal bool Method()
                                    {
                                        var value = true;
                                        return value;
                                    }
                                    #endregion // members
                                }
                                """;
        const string fixedData = """
                                 internal class RH0328
                                 {
                                     #region Members
                                     internal bool Method()
                                     {
                                         var value = true;
                                         return value;
                                     }
                                     #endregion // Members
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH0328MessageFormat));
    }

    /// <summary>
    /// Verifies that unrelated endregion text is not changed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotChangeUnrelatedEndRegionText()
    {
        const string testData = """
                                internal class RH0328
                                {
                                    {|#0:#region members|}
                                    internal bool Method()
                                    {
                                        var value = true;
                                        return value;
                                    }
                                    #endregion // helpers
                                }
                                """;
        const string fixedData = """
                                 internal class RH0328
                                 {
                                     #region Members
                                     internal bool Method()
                                     {
                                         var value = true;
                                         return value;
                                     }
                                     #endregion // helpers
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH0328MessageFormat));
    }

    /// <summary>
    /// Verifies that unrelated source is not changed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotChangeUnrelatedSource()
    {
        const string testData = """
                                internal class RH0328
                                {
                                    {|#0:#region members|}
                                    internal bool Method()
                                    {
                                    var value=true;return value;
                                    }
                                    #endregion // members
                                }
                                """;
        const string fixedData = """
                                 internal class RH0328
                                 {
                                     #region Members
                                     internal bool Method()
                                     {
                                     var value=true;return value;
                                     }
                                     #endregion // Members
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH0328MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix updates all exact occurrences in the matching endregion text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUpdatesAllMatchingEndRegionOccurrences()
    {
        const string testData = """
                                internal class RH0328
                                {
                                    {|#0:#region members|}
                                    internal bool Method()
                                    {
                                        var value = true;
                                        return value;
                                    }
                                    #endregion // members members
                                }
                                """;
        const string fixedData = """
                                 internal class RH0328
                                 {
                                     #region Members
                                     internal bool Method()
                                     {
                                         var value = true;
                                         return value;
                                     }
                                     #endregion // Members Members
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH0328MessageFormat));
    }

    #endregion // Members
}