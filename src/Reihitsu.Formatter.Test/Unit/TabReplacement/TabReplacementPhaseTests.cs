using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.TabReplacement;

/// <summary>
/// Tests for the output of the tab replacement, the first phase of the pipeline, observed through the production pipeline
/// under LF and CRLF line endings
/// </summary>
[TestClass]
public class TabReplacementPhaseTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: a tab in front of a trailing comment becomes four spaces in the first phase, whatever column
    /// it starts in
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void TabsBeforeTrailingCommentsBecomeFourSpacesEach()
    {
        const string input = "class C\n{\n    int z;\t// e\n    int w; \t // f\n}";
        const string expected = "class C\n{\n    int z;    // e\n    int w;      // f\n}";

        AssertFirstPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies that tabs in front of a line's first token, between two tokens, and at the end of a line become four spaces
    /// each, also when the tab starts in a column that is not a multiple of four
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void TabsInIndentationBetweenTokensAndAtLineEndBecomeFourSpacesEach()
    {
        const string input = "class C\n{\n\tint\tx;\t\n  \tint y;\n}";
        const string expected = "class C\n{\n    int    x;    \n      int y;\n}";

        AssertFirstPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab in front of the first token of a hole line of a multi-line interpolated raw string becomes four
    /// spaces in the first phase, while the content lines are left as they are
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void TabInFrontOfHoleLineOfMultiLineInterpolatedRawStringBecomesFourSpaces()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var name = \"x\";\n        var a = $\"\"\"\n    Hello {\n\t    name\n    }\n    \"\"\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var name = \"x\";\n        var a = $\"\"\"\n    Hello {\n        name\n    }\n    \"\"\";\n    }\n}";

        AssertFirstPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab in the code of an interpolation hole becomes four spaces, while a tab in the text of the same
    /// interpolated string is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-TAB-003")]
    public void TabInHoleCodeIsReplacedAndTabInInterpolatedTextIsKept()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var c = 1;\n        var i = $\"x\t{\tc}\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var c = 1;\n        var i = $\"x\t{    c}\";\n    }\n}";

        AssertFirstPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: tabs between the tokens of <c>#define</c> and <c>#pragma</c> directives become four spaces
    /// each in the first phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-002")]
    public void TabsBetweenDirectiveTokensBecomeFourSpacesEach()
    {
        const string input = "#define\tDEBUG\nclass C\n{\n#pragma warning\tdisable CS1591\n    int x;\n}";
        const string expected = "#define    DEBUG\nclass C\n{\n#pragma warning    disable CS1591\n    int x;\n}";

        AssertFirstPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab between <c>#region</c> and its description becomes four spaces in the first phase, while a tab
    /// inside the description, inside the <c>#endregion</c> comment, and inside a <c>#warning</c> message is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-002")]
    public void TabBetweenRegionKeywordAndDescriptionIsReplacedAndTabsInsideFreeTextAreKept()
    {
        const string input = "class C\n{\n    #region\tBaz\tqux\n\n    int z;\n\n    #endregion // Baz\tqux\n\n#warning keep\tthis\n    int y;\n}";
        const string expected = "class C\n{\n    #region    Baz\tqux\n\n    int z;\n\n    #endregion // Baz\tqux\n\n#warning keep\tthis\n    int y;\n}";

        AssertFirstPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: tabs inside comments, the text of literals, and disabled text are kept by the first phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-003")]
    public void TabsInsideCommentsLiteralsAndDisabledTextAreKept()
    {
        const string input = "class C\n{\n    // a\tb\n    /* c\td */\n    string s = \"e\tf\";\n    string t = @\"g\th\nline\";\n#if NEVER\n    disabled\ttext\n#endif\n}";

        AssertFirstPhaseResult(input, input);
    }

    /// <summary>
    /// Verifies that tabs inside character, interpolated, and raw string literals are kept by the first phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-003")]
    public void TabsInsideCharacterInterpolatedAndRawStringLiteralsAreKept()
    {
        const string input = "class C\n{\n    char c = '\t';\n    string i = $\"x\t{c}\";\n    string r = \"\"\"\n               y\tz\n               \"\"\";\n}";

        AssertFirstPhaseResult(input, input);
    }

    /// <summary>
    /// Verifies the other side of the disabled-text boundary: a tab in front of the <c>#endif</c> that closes an inactive
    /// branch is whitespace of the code and becomes four spaces, while the tab inside the disabled text is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-TAB-003")]
    public void TabInFrontOfEndifAfterDisabledTextIsReplacedAndTabInsideDisabledTextIsKept()
    {
        const string input = "class C\n{\n#if NEVER\n    disabled\ttext\n\t#endif\n    int x;\n}";
        const string expected = "class C\n{\n#if NEVER\n    disabled\ttext\n    #endif\n    int x;\n}";

        AssertFirstPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a tab between the parts of an XML element in a documentation comment is kept, while a tab
    /// in front of the member is whitespace of the code and becomes four spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-TAB-003")]
    public void TabInsideDocumentationCommentIsKeptAndTabInFrontOfMemberIsReplaced()
    {
        const string input = "class C\n{\n    /// <param\tname=\"x\">b</param>\n\tvoid M(int x)\n    {\n    }\n}";
        const string expected = "class C\n{\n    /// <param\tname=\"x\">b</param>\n    void M(int x)\n    {\n    }\n}";

        AssertFirstPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies that the whitespace in front of the <c>///</c> marker of a documentation comment's continuation line is not
    /// replaced by the first phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-003")]
    public void TabInFrontOfDocumentationCommentContinuationLineIsKept()
    {
        const string input = "class C\n{\n    /// <summary>\n\t/// a\n    /// </summary>\n    void M()\n    {\n    }\n}";

        AssertFirstPhaseResult(input, input);
    }

    /// <summary>
    /// Asserts, under LF and CRLF line endings, that the first phase of the pipeline is the tab replacement and returns the
    /// expected text
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="expected">The expected output of the first phase</param>
    private static void AssertFirstPhaseResult(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var actual = ObservedPipeline.RunTabReplacementPhase(NormalizeLineEndings(input, endOfLine), endOfLine);

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), actual, $"Output of the first phase mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // Methods
}