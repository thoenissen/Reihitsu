using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Cleanup;

/// <summary>
/// Tests for the cleanup of trailing whitespace and tabs, and for the content cleanup must leave alone
/// </summary>
[TestClass]
public class WhitespaceCleanupTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: the spaces behind a statement and the spaces of a blank line are removed and the blank line
    /// stays as an empty line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    public void TrailingSpacesAndWhitespaceOnlyLinesAreRemoved()
    {
        const string input = "class C\n{\n    void M()\n    {\n        int x = 1;   \n        \n        int y = 2;\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        int x = 1;\n\n        int y = 2;\n    }\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a tab in front of a trailing comment becomes four spaces regardless of the column it starts in
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-002")]
    public void TabsBeforeTrailingCommentsBecomeFourSpacesEach()
    {
        const string input = "class C\n{\n    int z;\t// e\n    int w; \t // f\n}";
        const string expected = "class C\n{\n    int z;    // e\n    int w;      // f\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: tabs between the tokens of <c>#define</c> and <c>#pragma</c> directives become four spaces each
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-003")]
    public void TabsBetweenDirectiveTokensBecomeFourSpacesEach()
    {
        const string input = "#define\tDEBUG\nclass C\n{\n#pragma warning\tdisable CS1591\n    int x;\n}";
        const string expected = "#define    DEBUG\nclass C\n{\n#pragma warning    disable CS1591\n    int x;\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab inside a region description and inside the message of a <c>#warning</c> directive is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-003")]
    public void TabsInsideRegionDescriptionAndWarningMessageAreKept()
    {
        const string input = "class C\n{\n    #region Fields\tAnd\tMore\n\n    int x;\n\n    #endregion // Fields\tAnd\tMore\n\n#warning keep\tthis\n    int y;\n}";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: tabs and trailing spaces inside comments, literals, and disabled text are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-004")]
    public void TabsAndSpacesInsideCommentsLiteralsAndDisabledTextAreKept()
    {
        const string input = "class C\n{\n    // a\tb\n    /* c\td */\n    string s = \"e\tf\";\n    string t = @\"g\th  \nline\";\n#if NEVER\n    disabled\ttext  \n#endif\n}";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that tabs inside character, interpolated, and raw string literals and inside a documentation comment are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-004")]
    public void TabsInsideOtherLiteralsAndDocumentationCommentsAreKept()
    {
        const string input = "class C\n{\n    /// <summary>\n    /// a\tb\n    /// </summary>\n    char c = '\t';\n    string i = $\"x\t{c}\";\n    string r = \"\"\"\n               y\tz\n               \"\"\";\n}";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: every line break and blank line after the last token of the file is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-005")]
    public void LineBreaksAfterTheLastTokenAreRemoved()
    {
        const string input = "class C\n{\n    int x;\n}\n\n\n";
        const string expected = "class C\n{\n    int x;\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a comment after the last token keeps its own line and only the line breaks after it are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-006")]
    public void TrailingCommentAfterTheLastTokenKeepsItsOwnLine()
    {
        const string input = "class C\n{\n    int x;\n}\n\n// end\n\n";
        const string expected = "class C\n{\n    int x;\n}\n\n// end";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a directive after the last token keeps its own line and its line break, and only the blank
    /// lines after it are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-007")]
    public void TrailingDirectiveKeepsItsLineBreak()
    {
        const string input = "class C\n{\n    int x;\n}\n\n#pragma warning restore CS1591\n\n\n";
        const string expected = "class C\n{\n    int x;\n}\n\n#pragma warning restore CS1591\n";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: disabled text at the end of the file is left unchanged, including the line break after
    /// <c>#endif</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-007")]
    public void TrailingDisabledTextIsLeftUnchanged()
    {
        const string input = "class C\n{\n    int x;\n}\n#if NEVER\nint ignored;\n#endif\n";

        AssertRuleResult(input);
    }

    #endregion // Methods
}