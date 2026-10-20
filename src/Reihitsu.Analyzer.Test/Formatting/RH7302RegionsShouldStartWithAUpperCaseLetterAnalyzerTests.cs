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
public class RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests : BatchCodeFixTestsBase<RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer, RH7302RegionsShouldStartWithAUpperCaseLetterCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that region names starting with lowercase are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7302-001")]
    [Requirement("RH7302-003")]
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
    [Requirement("RH7302-004")]
    [Requirement("RH7302-005")]
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
    [Requirement("RH7302-005")]
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
    [Requirement("RH7302-004")]
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
    [Requirement("RH7302-002")]
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
    [Requirement("RH7302-002")]
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
    [Requirement("RH7302-001")]
    [Requirement("RH7302-004")]
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

    /// <summary>
    /// Verifies that the fix renames an <c>#endregion</c> comment that repeats the name of the region it renames, and
    /// keeps an <c>#endregion</c> comment that does not repeat it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7302-005")]
    public async Task VerifyCodeFixRenamesOnlyTheEndRegionCommentThatRepeatsTheRegionName()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7302:#region members|}
                                    #endregion // members

                                    {|RH7302:#region values|}
                                    #endregion // helpers
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     #region Members
                                     #endregion // Members

                                     #region Values
                                     #endregion // helpers
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that every region whose description starts with a lower-case letter is reported, between members, in an element body, and in an excluded branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7302-001")]
    [Requirement("RH7302-003")]
    public async Task EveryRegionStartingWithALowerCaseLetterIsReported()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7302:#region äpfel und birnen|}

                                    int _a;

                                    #endregion // äpfel und birnen

                                    {|RH7302:#region   members|}

                                    int _b;

                                    #endregion // members

                                    void M()
                                    {
                                        {|RH7302:#region local|}
                                        var a = 1;
                                        #endregion
                                    }

                                #if false
                                    {|RH7302:#region test|}

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that upper-case letters, characters without an upper-case form, missing descriptions, and end directives are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7302-002")]
    public async Task OtherFirstCharactersAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                    #region   Fields

                                    int _a;

                                    #endregion // fields

                                    #region 1st fields

                                    int _b;

                                    #endregion // 1st fields

                                    #region _private

                                    int _c;

                                    #endregion // _private

                                    #region

                                    int _d;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix capitalizes the first letter and keeps the whitespace in front of the description
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7302-004")]
    public async Task CodeFixCapitalizesTheFirstLetterAndKeepsTheSpacing()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7302:#region   äpfel und birnen|}

                                    int _a;

                                    #endregion
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region   Äpfel und birnen

                                     int _a;

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix renames an end directive that repeats the old description, with or without a double slash, and leaves one naming something else
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7302-005")]
    public async Task CodeFixRenamesOnlyTheEndRegionThatRepeatsTheName()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7302:#region members|}
                                    #endregion // members

                                    {|RH7302:#region values|}
                                    #endregion values

                                    {|RH7302:#region helpers|}
                                    #endregion // other
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region Members
                                     #endregion // Members

                                     #region Values
                                     #endregion Values

                                     #region Helpers
                                     #endregion // other
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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

                                    {|#1:#region helpers|}
                                    internal bool Helper()
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
                                     #endregion // Members

                                     #region Helpers
                                     internal bool Helper()
                                     {
                                         var value = true;
                                         return value;
                                     }
                                     #endregion // Helpers
                                 }
                                 """;

        // Two lowercase regions sit back to back in the same type; each fix only capitalizes its own
        // #region/#endregion pair, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId, AnalyzerResources.RH7302MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}