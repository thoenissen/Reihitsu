using System.Linq;

using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains component tests that the <see cref="Home"/> page shows a null character (U+0000) of the text unchanged in the
/// coloured copy of the input and in the output. The panes are read from <see cref="PlaygroundBrowserDocument"/>, the
/// document a browser builds from the rendered page, because the HTML parser behind bUnit's markup changes a null
/// character wherever it reads one, also where the browser never parses it
/// </summary>
[TestClass]
public class NullCharacterTests
{
    #region Constants

    /// <summary>
    /// An input that formats without a message and holds a null character in a comment and in a string literal
    /// </summary>
    private const string FormattedSource = "// x\0y\r\nclass C { string s = \"a\0b\"; }\r\n";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that a null character typed into a string literal of the input is shown as it is in the coloured copy
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void NullCharacterTypedIntoTheInputIsKeptInTheCopy()
    {
        AssertCopyAfterTyping("class C { string s = \"a\0b\"; }");
    }

    /// <summary>
    /// Verifies that a null character is kept in the copy wherever it stands: alone on a line, in a comment, in a string, in
    /// disabled text, at the start and at the end of the text, on lines ended by a line feed and by a carriage return and
    /// line feed
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void NullCharacterIsKeptInTheCopyOnEveryKindOfLine()
    {
        AssertCopyAfterTyping("\0\nclass C { } // \0\r\nvar s = \"\0\";\n#if A\n\0 x\n#endif\n\0\0\r\n\0");
    }

    /// <summary>
    /// Verifies that a successful format keeps a null character in the coloured copy of the input, in a comment and in a
    /// string literal, on lines ended by a carriage return and line feed
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void NullCharacterIsKeptInTheCopyAfterFormat()
    {
        using (var context = new BunitContext())
        {
            var page = FormatWithoutMessage(context, FormattedSource);

            AssertText(FormattedSource, PlaygroundMarkup.GetInputCopy(PlaygroundBrowserDocument.Build(context, page)).TextContent, "copy");
        }
    }

    /// <summary>
    /// Verifies that the output of a successful format shows a null character of the formatted text, in a comment and in a
    /// string literal
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void NullCharacterIsShownInTheOutput()
    {
        using (var context = new BunitContext())
        {
            var page = FormatWithoutMessage(context, FormattedSource);
            var output = PlaygroundBrowserDocument.Build(context, page).QuerySelector("#playground-output");

            AssertText("// x\0y\r\nclass C\r\n{\r\n    string s = \"a\0b\";\r\n}", output.TextContent, "output");
        }
    }

    /// <summary>
    /// Verifies that a format that marks an error keeps a null character in the marked copy, and that the mark stays at its
    /// place behind the last character
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-014")]
    public void NullCharacterIsKeptInACopyWithErrorMarks()
    {
        const string input = "class C { string s = \"a\0b\"; ";

        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, input);

            Assert.Contains("line 1, column 29 — CS1513: } expected", PlaygroundMarkup.GetMessageLines(page), "The input must show the message of PLAY-006.");

            var copy = PlaygroundMarkup.GetInputCopy(PlaygroundBrowserDocument.Build(context, page));

            AssertText(input, copy.TextContent, "copy");
            Assert.AreSequenceEqual(new[] { "28:" }, PlaygroundMarkup.GetMarks(copy));
        }
    }

    /// <summary>
    /// Verifies the counterpart of the null character: the other characters the HTML parser reads specially in text, markup
    /// syntax, a lone carriage return, and control and line separator characters it keeps, are shown as they are in the copy
    /// and in the output
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void CarriageReturnAndOtherControlCharactersAreKeptInTheCopyAndTheOutput()
    {
        const string input = "// a \u0001\u0008\u000b\u000c\u007f\ufeff & < >\r// b\u0085// c\u2028// d\nclass C { }";

        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, input);

            var document = PlaygroundBrowserDocument.Build(context, page);

            AssertText(input, PlaygroundMarkup.GetInputCopy(document).TextContent, "copy");
            AssertText(PlaygroundFormatter.Format(input).Text, document.QuerySelector("#playground-output").TextContent, "output");
        }
    }

    /// <summary>
    /// Renders the page, formats a text, and asserts that the format shows no message
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="input">The text</param>
    /// <returns>The rendered page</returns>
    private static IRenderedComponent<Home> FormatWithoutMessage(BunitContext context, string input)
    {
        var page = context.Render<Home>();

        PlaygroundMarkup.Format(page, input);

        Assert.IsEmpty(page.FindAll(".playground-message"), "The input must format without a message.");

        return page;
    }

    /// <summary>
    /// Types a text into the input and asserts that the coloured copy the browser shows is exactly that text
    /// </summary>
    /// <param name="input">The text</param>
    private static void AssertCopyAfterTyping(string input)
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input(input);

            AssertText(input, PlaygroundMarkup.GetInputCopy(PlaygroundBrowserDocument.Build(context, page)).TextContent, "copy");
        }
    }

    /// <summary>
    /// Asserts that a pane shows exactly a text, comparing code units so that a changed control character is named
    /// </summary>
    /// <param name="expected">The text the pane must show</param>
    /// <param name="actual">The text the pane shows</param>
    /// <param name="pane">The pane, for the message</param>
    private static void AssertText(string expected, string actual, string pane)
    {
        Assert.AreEqual(ToCodes(expected), ToCodes(actual), $"The {pane} must show every character of its text unchanged.");
    }

    /// <summary>
    /// Writes a text as its code units in hexadecimal
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The code units, separated by spaces</returns>
    private static string ToCodes(string text)
    {
        return string.Join(" ", text.Select(character => ((int)character).ToString("x4", System.Globalization.CultureInfo.InvariantCulture)));
    }

    #endregion // Methods
}