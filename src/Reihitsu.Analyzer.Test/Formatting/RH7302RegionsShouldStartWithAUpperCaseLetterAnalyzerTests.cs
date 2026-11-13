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
    /// Verifies that the code fix leaves an endregion text unchanged that repeats the region name twice, because it is
    /// not exactly the region name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7302-005")]
    public async Task VerifyCodeFixLeavesAnEndRegionTextThatRepeatsTheNameTwiceUnchanged()
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
                                     #endregion // members members
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

    /// <summary>
    /// Verifying the specification example: an <c>#endregion</c> that contains the name together with other text stays
    /// unchanged, also where the name occurs inside a word, and one that is exactly the name behind <c>//</c> is renamed
    /// with its spacing kept
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7302-005")]
    public async Task CodeFixLeavesAnEndRegionThatSaysMoreThanTheNameUnchanged(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH7302:#region members|}
                                    #endregion // end of members

                                    {|RH7302:#region a|}
                                    #endregion // a cat

                                    {|RH7302:#region items|}
                                    #endregion   //items
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region Members
                                     #endregion // end of members

                                     #region A
                                     #endregion // a cat

                                     #region Items
                                     #endregion   //Items
                                 }
                                 """;

        await Verify(testData.Replace("\r\n", "\n").Replace("\n", endOfLine), fixedData.Replace("\r\n", "\n").Replace("\n", endOfLine));
    }

    /// <summary>
    /// Verifying the specification example: whitespace behind the <c>#region</c> description is not part of it, so the
    /// <c>#endregion</c> that repeats the name is renamed, and the <c>#region</c> line keeps its trailing whitespace
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7302-005")]
    public async Task CodeFixRenamesTheEndRegionWhenTheRegionLineEndsWithWhitespace(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH7302:#region members··|}
                                    int _a;
                                    #endregion // members
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region Members··
                                     int _a;
                                     #endregion // Members
                                 }
                                 """;

        var source = testData.Replace("·", " ").Replace("\r\n", "\n").Replace("\n", endOfLine);
        var fixedSource = fixedData.Replace("·", " ").Replace("\r\n", "\n").Replace("\n", endOfLine);

        await Verify(source, fixedSource);

        Assert.AreEqual(fixedSource, await ApplyCodeFixAsync(source.Replace("{|RH7302:", string.Empty).Replace("|}", string.Empty)));
    }

    /// <summary>
    /// Verifying the specification example: the <c>#endregion</c> keyword is never renamed, also where the region's name is
    /// part of it; an <c>#endregion</c> without text stays unchanged, and one whose description is exactly the name has only
    /// that description renamed
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7302-005")]
    [Requirement("SHARED-FIX-003")]
    public async Task CodeFixNeverRenamesTheEndRegionKeyword(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH7302:#region end|}
                                    #endregion

                                    {|RH7302:#region e|}
                                    #endregion // e
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region End
                                     #endregion

                                     #region E
                                     #endregion // E
                                 }
                                 """;

        await Verify(testData.Replace("\r\n", "\n").Replace("\n", endOfLine), fixedData.Replace("\r\n", "\n").Replace("\n", endOfLine));
    }

    /// <summary>
    /// Verifying that the code fix leaves the <c>#endregion</c> keyword unchanged for a region named <c>region</c>, whose
    /// name is part of the keyword, and renames the description behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7302-005")]
    public async Task CodeFixLeavesTheEndRegionKeywordOfARegionNamedRegionUnchanged()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7302:#region region|}
                                    #endregion // region
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region Region
                                     #endregion // Region
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