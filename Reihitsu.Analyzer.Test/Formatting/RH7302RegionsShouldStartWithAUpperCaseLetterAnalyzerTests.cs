using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer"/>
/// </summary>
[TestClass]
public class RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests : AnalyzerTestsBase<RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer, RH7302RegionsShouldStartWithAUpperCaseLetterCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that region names starting with lowercase are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLowercaseRegionNamesAreDetected()
    {
        const string testData = """
                                internal class RH7302
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
                                 internal class RH7302
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
                     Diagnostics(RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH7302MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix also updates a matching endregion comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUpdatesMatchingEndRegionComment()
    {
        const string testData = """
                                internal class RH7302
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
                                 internal class RH7302
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
                     Diagnostics(RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH7302MessageFormat));
    }

    /// <summary>
    /// Verifies that unrelated endregion text is not changed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotChangeUnrelatedEndRegionText()
    {
        const string testData = """
                                internal class RH7302
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
                                 internal class RH7302
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
                     Diagnostics(RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH7302MessageFormat));
    }

    /// <summary>
    /// Verifies that unrelated source is not changed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotChangeUnrelatedSource()
    {
        const string testData = """
                                internal class RH7302
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
                                 internal class RH7302
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
                     Diagnostics(RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH7302MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix updates all exact occurrences in the matching endregion text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixUpdatesAllMatchingEndRegionOccurrences()
    {
        const string testData = """
                                internal class RH7302
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
                                 internal class RH7302
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
                     Diagnostics(RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH7302MessageFormat));
    }

    /// <summary>
    /// Verifies that an uppercase description with extra leading whitespace is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUppercaseNameWithExtraLeadingWhitespace()
    {
        const string testData = """
                                internal class RH7302
                                {
                                    #region  Fields
                                    public int P1 { get; set; }
                                    #endregion // Fields
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a description starting with a digit is not flagged because it cannot be capitalized
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDigitLeadingRegionName()
    {
        const string testData = """
                                internal class RH7302
                                {
                                    #region 1Values
                                    public int P1 { get; set; }
                                    #endregion // 1Values
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a lowercase description with extra leading whitespace is detected and capitalized
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLowercaseRegionNameWithExtraLeadingWhitespaceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH7302
                                {
                                    {|#0:#region  members|}
                                    internal bool Method()
                                    {
                                        var value = true;
                                        return value;
                                    }
                                    #endregion // members
                                }
                                """;
        const string fixedData = """
                                 internal class RH7302
                                 {
                                     #region  Members
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
                     Diagnostics(RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH7302MessageFormat));
    }

    #endregion // Tests
}