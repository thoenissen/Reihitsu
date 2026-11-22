using System;
using System.Linq;

using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains component tests for the marks the <see cref="Home"/> page sets in the input at the location of each error. A mark
/// is read as <c>"&lt;offset&gt;:&lt;marked text&gt;"</c>, so it is placed by its position in the text, not found by its text
/// </summary>
[TestClass]
public class ErrorMarkTests
{
    #region Constants

    /// <summary>
    /// The input of PLAY-006's second example
    /// </summary>
    private const string MissingSemicolonAndBraceSource = "public class Sample{public int Add(int a,int b){return a+b}";

    /// <summary>
    /// The input of the PLAY-008 example
    /// </summary>
    private const string UnsupportedSource = "class C\n{\n    void M()\n    {\n        int /* c */ a = 1;\n    }\n}";

    /// <summary>
    /// The input of the PLAY-009 example
    /// </summary>
    private const string HiddenCodeSource = "class C\n{\n    void M()\n    {\n#if !A\n        /*\n#else\n        var s = @\"\n        */\n        int  x = 1; //\";\n#endif\n    }\n}";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: the <c>}</c> at column 59 is marked, and the place behind it at the end of the
    /// text, one character wide
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void SpecExampleCharacterAndEndOfTextAreMarked()
    {
        AssertMarks(MissingSemicolonAndBraceSource, "58:}", "59:");
    }

    /// <summary>
    /// Verifies the specification example: an error whose empty span sits at a line break is marked directly behind the
    /// line's last character, and the line break is not marked
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void SpecExampleEmptySpanAtALineBreakIsMarkedBehindTheLastCharacterOfTheLine()
    {
        AssertMarks("class C\n{\n    int x\n}\n", "19:");
    }

    /// <summary>
    /// Verifies that errors at the line breaks of two lines are each marked behind their own line's last character
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void EmptySpansAtTheLineBreaksOfTwoLinesAreMarkedOnTheirOwnLines()
    {
        AssertMarks("class C\n{\n    int x\n    int y\n}", "19:", "29:");
    }

    /// <summary>
    /// Verifies the specification example: an empty span in front of an ordinary character marks that character, and an empty
    /// span at the end of the text marks the place behind the text's last character
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void SpecExampleEmptySpansInFrontOfACharacterAndAtTheEndOfTheText()
    {
        AssertMarks("class C { /* open", "10:/", "17:");
    }

    /// <summary>
    /// Verifies that an empty span in front of a quote, followed by a line break later on the line, marks the quote
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void EmptySpanInFrontOfAQuoteMarksTheQuote()
    {
        AssertMarks("class C { string s = \"abc\n; }", "21:\"");
    }

    /// <summary>
    /// Verifies that a span of several characters is marked as a whole, on a line that holds the marked text's neighbours
    /// several times
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void SpanOfSeveralCharactersIsMarkedAsAWhole()
    {
        AssertMarks("class C { void M() { foo bar baz; } }", "29:baz");
    }

    /// <summary>
    /// Verifies that a one-character span behind a tab is marked by its position in the text, next to the empty span at the
    /// end of the text
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void SpanBehindATabIsMarkedByItsPosition()
    {
        AssertMarks("class C { void M() { int x = 1\t}", "31:}", "32:");
    }

    /// <summary>
    /// Verifies that an error the parser reports twice at one position is marked twice, as it is listed twice
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void ErrorReportedTwiceIsMarkedTwice()
    {
        AssertMarks("class C { void M() { if (true) { }", "34:", "34:");
    }

    /// <summary>
    /// Verifies the specification example: the comment of the unsupported shape in the PLAY-008 example is marked
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void SpecExampleUnsupportedShapeIsMarked()
    {
        AssertMarks(UnsupportedSource, "41:/* c */");
    }

    /// <summary>
    /// Verifies that an unsupported shape whose reported span ends in a line break is marked with exactly that span
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void UnsupportedShapeEndingInALineBreakIsMarkedWithItsSpan()
    {
        AssertMarks("class C\n{\n    int a = 1, b = 2\n#if X\n        , c = 3\n#endif\n        ;\n}", "31:#if X\n");
    }

    /// <summary>
    /// Verifies the specification example: the hidden-code message comes with no mark
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void SpecExampleHiddenCodeIsNotMarked()
    {
        AssertMarks(HiddenCodeSource);
    }

    /// <summary>
    /// Verifies that the page shows no mark on load and none while the user types input with a syntax error, before "Format"
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void TypingInputWithSyntaxErrorsSetsNoMark()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            Assert.IsEmpty(page.FindAll(".playground-mark"), "The example the page opens with must show no mark.");

            page.Find("#playground-input").Input(MissingSemicolonAndBraceSource);

            Assert.AreEqual(MissingSemicolonAndBraceSource, PlaygroundMarkup.GetInputCopy(page).TextContent);
            Assert.IsEmpty(page.FindAll(".playground-mark"), "Typing must not set a mark.");
        }
    }

    /// <summary>
    /// Verifies the specification example: after "Format" set marks, the first change of the input removes every mark while the
    /// message box keeps its lines, and the next "Format" sets the marks of the new input
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void SpecExampleMarksDisappearAtTheFirstEditAndComeBackWithTheNextFormat()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, MissingSemicolonAndBraceSource);

            var lines = PlaygroundMarkup.GetMessageLines(page);

            Assert.AreSequenceEqual(new[] { "58:}", "59:" }, PlaygroundMarkup.GetMarks(PlaygroundMarkup.GetInputCopy(page)));

            page.Find("#playground-input").Input("i" + MissingSemicolonAndBraceSource);

            Assert.IsEmpty(page.FindAll(".playground-mark"), "The first change must remove every mark.");
            Assert.AreSequenceEqual(lines, PlaygroundMarkup.GetMessageLines(page), "Editing must not change the message box.");

            page.Find("#playground-input").Input("int y;" + MissingSemicolonAndBraceSource);

            Assert.IsEmpty(page.FindAll(".playground-mark"), "Further changes must not bring the marks back.");
            Assert.AreSequenceEqual(lines, PlaygroundMarkup.GetMessageLines(page), "Editing must not change the message box.");

            page.Find("button").Click();

            Assert.AreSequenceEqual(new[] { "64:}", "65:" }, PlaygroundMarkup.GetMarks(PlaygroundMarkup.GetInputCopy(page)));
        }
    }

    /// <summary>
    /// Verifies that the first change removes the marks of an unsupported shape too
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void MarkOfAnUnsupportedShapeDisappearsAtTheFirstEdit()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, UnsupportedSource);

            Assert.HasCount(1, page.FindAll(".playground-mark"));

            page.Find("#playground-input").Input(UnsupportedSource + "\n");

            Assert.IsEmpty(page.FindAll(".playground-mark"));
            Assert.HasCount(1, page.FindAll(".playground-message"), "Editing must not remove the message box.");
        }
    }

    /// <summary>
    /// Verifies that pressing "Format" again on the same input sets the same marks once, not twice
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void FormatAgainSetsTheSameMarksOnce()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, MissingSemicolonAndBraceSource);
            page.Find("button").Click();

            Assert.AreSequenceEqual(new[] { "58:}", "59:" }, PlaygroundMarkup.GetMarks(PlaygroundMarkup.GetInputCopy(page)));
        }
    }

    /// <summary>
    /// Verifies that a format that shows no message leaves no mark, and that a format that shows the hidden-code message
    /// leaves none either, after a format that set marks
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void FormatWithoutALocationLeavesNoMark()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, MissingSemicolonAndBraceSource);

            Assert.HasCount(2, page.FindAll(".playground-mark"));

            PlaygroundMarkup.Format(page, "public class Sample{ int x; }");

            Assert.IsEmpty(page.FindAll(".playground-message"));
            Assert.IsEmpty(page.FindAll(".playground-mark"), "A format that shows no message must leave no mark.");

            PlaygroundMarkup.Format(page, MissingSemicolonAndBraceSource);

            Assert.HasCount(2, page.FindAll(".playground-mark"));

            PlaygroundMarkup.Format(page, HiddenCodeSource);

            Assert.IsEmpty(page.FindAll(".playground-mark"), "The hidden-code message must come with no mark.");
        }
    }

    /// <summary>
    /// Verifies that the marks are drawn with a wavy underline: a rule of the stylesheet that names the mark's class declares
    /// a wavy line
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-014")]
    public void MarksAreDrawnAsAWavyUnderline()
    {
        var rules = PlaygroundStyles.GetRulesNaming("playground-mark");

        Assert.IsNotEmpty(rules, "The stylesheet must style the marks.");
        Assert.Contains(rule => rule.Contains("wavy", StringComparison.Ordinal), rules, $"The marks must be a wavy underline: {string.Join(" ", rules)}");
    }

    /// <summary>
    /// Formats an input and asserts the marks of the input: the marks are in the coloured copy of the input only, the copy's
    /// text is the input unchanged, and the output holds no mark
    /// </summary>
    /// <param name="input">The input</param>
    /// <param name="expected">The expected marks</param>
    private static void AssertMarks(string input, params string[] expected)
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, input);

            var copy = PlaygroundMarkup.GetInputCopy(page);

            Assert.HasCount(1, page.FindAll(".playground-message"), "The input must show a message.");
            Assert.AreEqual(input, copy.TextContent, "A mark must not add, drop, or change a character.");
            Assert.AreSequenceEqual(expected, PlaygroundMarkup.GetMarks(copy), "Actual: " + string.Join(" | ", PlaygroundMarkup.GetMarks(copy)));
            Assert.HasCount(expected.Length, page.FindAll(".playground-mark"), "Only the input carries marks.");
            Assert.IsEmpty(PlaygroundMarkup.GetOutput(page).QuerySelectorAll(".playground-mark"), "The output carries no mark.");
        }
    }

    #endregion // Methods
}