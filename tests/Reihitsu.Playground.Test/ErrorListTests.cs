using System.Globalization;
using System.Linq;

using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains component tests for the list of syntax errors in the message of the <see cref="Home"/> page
/// </summary>
[TestClass]
public class ErrorListTests
{
    #region Constants

    /// <summary>
    /// The sentence the syntax error message starts with
    /// </summary>
    private const string SyntaxErrorMessage = "The input contains syntax errors and was left unchanged.";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the first specification example: one error, listed below the sentence with line, column, ID, and message
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void SpecExampleOneErrorIsListedBelowTheSentence()
    {
        AssertMessageLines("public class Sample{ int x }",
                           "line 1, column 28 — CS1002: ; expected");
    }

    /// <summary>
    /// Verifies the second specification example: two errors, listed by position, the second one with an empty span at the
    /// end of the text
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void SpecExampleTwoErrorsAreListedByPosition()
    {
        AssertMessageLines("public class Sample{public int Add(int a,int b){return a+b}",
                           "line 1, column 59 — CS1002: ; expected",
                           "line 1, column 60 — CS1513: } expected");
    }

    /// <summary>
    /// Verifies that the line and the column are counted over several lines, for errors on different lines
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void LinesAndColumnsAreCountedOverSeveralLines()
    {
        AssertMessageLines("class C\n{\n    int x\n    int y\n}",
                           "line 3, column 10 — CS1002: ; expected",
                           "line 4, column 10 — CS1002: ; expected");
    }

    /// <summary>
    /// Verifies that a tab counts as one column
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void TabCountsAsOneColumn()
    {
        AssertMessageLines("class C { void M() { int x = 1\t}",
                           "line 1, column 32 — CS1002: ; expected",
                           "line 1, column 33 — CS1513: } expected");
    }

    /// <summary>
    /// Verifies that an error the parser reports twice at one position is listed twice
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void ErrorReportedTwiceAtOnePositionIsListedTwice()
    {
        AssertMessageLines("class C { void M() { if (true) { }",
                           "line 1, column 35 — CS1513: } expected",
                           "line 1, column 35 — CS1513: } expected");
    }

    /// <summary>
    /// Verifies that errors at the same position keep the order the parser reports them in, also when their spans differ
    /// in length
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void ErrorsAtOnePositionKeepTheParserOrder()
    {
        AssertMessageLines("class C { int @ x; int y }",
                           "line 1, column 15 — CS1519: Invalid token '@' in a member declaration",
                           "line 1, column 15 — CS1646: Keyword, identifier, or string expected after verbatim specifier: @",
                           "line 1, column 15 — CS1519: Invalid token '@' in a member declaration",
                           "line 1, column 18 — CS1519: Invalid token ';' in a member declaration",
                           "line 1, column 18 — CS1519: Invalid token ';' in a member declaration",
                           "line 1, column 26 — CS1002: ; expected");
    }

    /// <summary>
    /// Verifies that an error a directive reports is listed like any other error
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void ErrorOfAnErrorDirectiveIsListed()
    {
        AssertMessageLines("#error e\nclass C { int x }",
                           "line 1, column 8 — CS1029: #error: 'e'",
                           "line 2, column 17 — CS1002: ; expected");
    }

    /// <summary>
    /// Verifies that a parser warning is not listed next to an error
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void WarningIsNotListedBesideAnError()
    {
        AssertMessageLines("#warning w\nclass C { int x }",
                           "line 2, column 17 — CS1002: ; expected");
    }

    /// <summary>
    /// Verifies that input with a parser warning and no error is formatted without a message
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void WarningWithoutAnErrorShowsNoMessage()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, "#warning w\nclass C { }");

            Assert.IsEmpty(page.FindAll(".playground-message"));
        }
    }

    /// <summary>
    /// Verifies that the parser's messages are listed in English when the user interface culture is another language
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void MessagesAreEnglishUnderAnotherCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            CultureInfo.CurrentUICulture = new CultureInfo("de-DE");

            AssertMessageLines("class C { /* open",
                               "line 1, column 11 — CS1035: End-of-file found, '*/' expected",
                               "line 1, column 18 — CS1513: } expected");
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
        }
    }

    /// <summary>
    /// Verifies that the list is replaced on the next format of another input with syntax errors and not extended
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void ListIsReplacedOnTheNextFormat()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, "public class Sample{public int Add(int a,int b){return a+b}");
            PlaygroundMarkup.Format(page, "public class Sample{ int x }");

            Assert.AreSequenceEqual(new[] { SyntaxErrorMessage, "line 1, column 28 — CS1002: ; expected" }, PlaygroundMarkup.GetMessageLines(page));

            PlaygroundMarkup.Format(page, "public class Sample{ int x }");

            Assert.AreSequenceEqual(new[] { SyntaxErrorMessage, "line 1, column 28 — CS1002: ; expected" }, PlaygroundMarkup.GetMessageLines(page));
        }
    }

    /// <summary>
    /// Formats an input and asserts the lines of the message: the sentence, followed by the given error lines
    /// </summary>
    /// <param name="input">The input</param>
    /// <param name="errorLines">The expected error lines</param>
    private static void AssertMessageLines(string input, params string[] errorLines)
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, input);

            Assert.AreEqual(input, PlaygroundMarkup.GetOutputText(page), "Input with a syntax error must be shown unchanged.");
            Assert.AreSequenceEqual(new[] { SyntaxErrorMessage }.Concat(errorLines).ToList(), PlaygroundMarkup.GetMessageLines(page), "Actual: " + string.Join(" | ", PlaygroundMarkup.GetMessageLines(page)));
        }
    }

    #endregion // Methods
}