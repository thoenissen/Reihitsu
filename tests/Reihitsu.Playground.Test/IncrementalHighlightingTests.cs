using System.Collections.Generic;

using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains tests that the coloured copy of the input after a series of edits is the same as the copy a freshly rendered
/// page shows for the final text, however the page updates the copy from one edit to the next
/// </summary>
[TestClass]
public class IncrementalHighlightingTests
{
    #region Constants

    /// <summary>
    /// A source with a documentation comment, conditional directives, a constraint, an interpolated string, and comments
    /// </summary>
    private const string DirectiveSource = "/// <summary>Doc</summary>\n#if A\nclass B { }\n#else\npartial class C<T> where T : class\n{\n    string s = $\"a{1}b\";\n    /* c */ int n = 42;\n}\n#endif\n";

    /// <summary>
    /// A source with a verbatim string and a raw string over several lines, a CR LF, and contextual keywords
    /// </summary>
    private const string LiteralSource = "var a = @\"x\ny\";\nvar b = \"\"\"\n  raw\n  \"\"\";\n// end\r\nasync Task M() => await N();\n";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The texts inserted at every position of a source: each can change the syntax of text behind it that it leaves as it is
    /// </summary>
    private static readonly string[] _insertions = ["/*", "*/", "\"", "\"\"\"", "'", "@", "$", "{", "}", "x", " ", "///", "\n", "\r\n", "#if A\n", "#else\n", "#endif\n", "#region R\n"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that typing a program one character at a time leaves the coloured copy as a fresh page shows the text after
    /// every key
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void TypingCharacterByCharacterMatchesAFreshPageAfterEveryKey()
    {
        const string program = "/// <summary>A</summary>\npartial class C<T> where T : class\n{\n    string s = $\"a{n}b\"; /* c */\n#if DEBUG\n    int n = 42;\n#endif\n}\n";

        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input(string.Empty);

            AssertCopyMatchesFreshPage(context, page, string.Empty);

            for (var length = 1; length <= program.Length; length++)
            {
                var text = program.Substring(0, length);

                page.Find("#playground-input").Input(text);

                AssertCopyMatchesFreshPage(context, page, text);
            }
        }
    }

    /// <summary>
    /// Verifies that deleting a program one character at a time from its end, and then from its start, leaves the coloured
    /// copy as a fresh page shows the text after every key
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void DeletingCharacterByCharacterMatchesAFreshPageAfterEveryKey()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var text = DirectiveSource;

            page.Find("#playground-input").Input(text);

            AssertCopyMatchesFreshPage(context, page, text);

            while (text.Length > DirectiveSource.Length / 2)
            {
                text = text.Substring(0, text.Length - 1);

                page.Find("#playground-input").Input(text);

                AssertCopyMatchesFreshPage(context, page, text);
            }

            while (text.Length > 0)
            {
                text = text.Substring(1);

                page.Find("#playground-input").Input(text);

                AssertCopyMatchesFreshPage(context, page, text);
            }
        }
    }

    /// <summary>
    /// Verifies that an inserted <c>#else</c> recolours the text behind it, although that text is unchanged: the
    /// documentation comment behind the new <c>#else</c> becomes disabled text, and becomes a comment again when the
    /// <c>#else</c> is removed
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void InsertedElseRecoloursTheUnchangedTextBehindIt()
    {
        const string before = "#if A\nclass B { }\n/// <summary>Doc</summary>\nclass C { }\n#endif\nclass D { }\n";
        const string after = "#if A\nclass B { }\n#else\n/// <summary>Doc</summary>\nclass C { }\n#endif\nclass D { }\n";

        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input(before);

            AssertCopyMatchesFreshPage(context, page, before);

            page.Find("#playground-input").Input(after);

            AssertCopyMatchesFreshPage(context, page, after);

            page.Find("#playground-input").Input(before);

            AssertCopyMatchesFreshPage(context, page, before);
        }
    }

    /// <summary>
    /// Verifies that an edit that opens or closes a delimited comment or a string recolours the unchanged text behind it, up
    /// to the end of the text, and that undoing the edit restores the colours
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void OpeningAndClosingCommentsAndStringsRecolourTheUnchangedTextBehindThem()
    {
        var texts = new[]
                    {
                        "class C\n{\n    int a = 1;\n    string b = \"x\";\n}\n",
                        "class C\n{\n/*    int a = 1;\n    string b = \"x\";\n}\n",
                        "class C\n{\n/*    int a = 1;*/\n    string b = \"x\";\n}\n",
                        "class C\n{\n/*    int a = 1;\n    string b = \"x\";\n}\n",
                        "class C\n{\n    int a = 1;\n    string b = \"x\";\n}\n",
                        "class C\n{\n    int a = \"1;\n    string b = \"x\";\n}\n",
                        "class C\n{\n    int a = @\"1;\n    string b = \"x\";\n}\n",
                        "class C\n{\n    int a = 1;\n    string b = \"x\";\n}\n"
                    };

        AssertSeriesMatchesFreshPage(texts);
    }

    /// <summary>
    /// Verifies that a token that becomes shorter next to the changed text, on a line behind the edit, is recoloured with the
    /// lines it no longer covers: a delimited comment over three lines is shortened by an inserted <c>*/</c> on its first line,
    /// and a raw string over three lines by an inserted closing delimiter
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void TokenThatShrinksNextToTheEditRecoloursTheLinesItNoLongerCovers()
    {
        var texts = new[]
                    {
                        "/* one\ntwo\nthree */\nint x;\n",
                        "/* one*/\ntwo\nthree */\nint x;\n",
                        "/* one\ntwo\nthree */\nint x;\n",
                        "var s = \"\"\"\n  one\n  two\n  \"\"\";\nint x;\n",
                        "var s = \"\"\"\n  one\n  \"\"\"\n  two\n  \"\"\";\nint x;\n",
                        "var s = \"\"\"\n  one\n  two\n  \"\"\";\nint x;\n"
                    };

        AssertSeriesMatchesFreshPage(texts);
    }

    /// <summary>
    /// Verifies that the copy matches a fresh page across "Format": after a format that marks errors, the first edit removes
    /// the marks and the copy is coloured as a fresh page colours the text; a successful format of edited text leaves the
    /// copy as a fresh page shows it, too
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void CopyMatchesAFreshPageAcrossFormat()
    {
        const string invalid = "class C\n{\n    int a = 1\n    /* open\n";
        const string edited = "class C\n{\n    int a = 1;\n    /* open\n";
        const string valid = "class C\n{\n    int a = 1;\n    /* closed */\n}\n";

        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, invalid);

            var copy = PlaygroundMarkup.GetInputCopy(page);

            Assert.IsNotEmpty(PlaygroundMarkup.GetMarks(copy), "The format must mark the errors of the invalid input.");
            Assert.AreEqual(invalid, copy.TextContent);
            Assert.AreSequenceEqual(GetFreshCategories(context, invalid), PlaygroundMarkup.GetCharacterCategories(copy), "The marks must not change the colours.");

            page.Find("#playground-input").Input(edited);

            AssertCopyMatchesFreshPage(context, page, edited);

            page.Find("#playground-input").Input(valid);

            AssertCopyMatchesFreshPage(context, page, valid);

            page.Find("button").Click();

            AssertCopyMatchesFreshPage(context, page, valid);

            page.Find("#playground-input").Input(invalid);

            AssertCopyMatchesFreshPage(context, page, invalid);
        }
    }

    /// <summary>
    /// Verifies, on the highlighting the page keeps from one edit to the next, that inserting each of several texts that can
    /// change the syntax of the text behind them at every position of a source with directives and a documentation comment,
    /// and removing it again, gives the highlighting of a fresh page for each text
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void InsertingAndRemovingAtEveryPositionOfADirectiveSourceMatchesAFreshHighlighting()
    {
        AssertEveryInsertionMatchesFreshHighlighting(DirectiveSource);
    }

    /// <summary>
    /// Verifies, on the highlighting the page keeps from one edit to the next, that inserting each of several texts that can
    /// change the syntax of the text behind them at every position of a source with multi-line literals and a CR LF, and
    /// removing it again, gives the highlighting of a fresh page for each text
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void InsertingAndRemovingAtEveryPositionOfALiteralSourceMatchesAFreshHighlighting()
    {
        AssertEveryInsertionMatchesFreshHighlighting(LiteralSource);
    }

    /// <summary>
    /// Verifies, on the highlighting the page keeps from one edit to the next, that deleting one and two characters at every
    /// position of the sources, and restoring them, gives the highlighting of a fresh page for each text
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    [Requirement("PLAY-011")]
    public void DeletingAndRestoringAtEveryPositionMatchesAFreshHighlighting()
    {
        using (var context = new BunitContext())
        {
            foreach (var source in new[] { DirectiveSource, LiteralSource })
            {
                var original = PlaygroundHighlighter.Highlight(source, []);

                for (var length = 1; length <= 2; length++)
                {
                    for (var position = 0; position + length <= source.Length; position++)
                    {
                        AssertEditMatchesFreshHighlighting(context, source, original, source.Remove(position, length));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Enters a series of texts into the input of a page, one after the other, and asserts after each that the coloured copy
    /// is the one a fresh page shows for it
    /// </summary>
    /// <param name="texts">The texts</param>
    private static void AssertSeriesMatchesFreshPage(IReadOnlyList<string> texts)
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            foreach (var text in texts)
            {
                page.Find("#playground-input").Input(text);

                AssertCopyMatchesFreshPage(context, page, text);
            }
        }
    }

    /// <summary>
    /// Asserts that the coloured copy of a page that carries no marks is the one a freshly rendered page shows for the text
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="page">The page</param>
    /// <param name="text">The input's text</param>
    private static void AssertCopyMatchesFreshPage(BunitContext context, IRenderedComponent<Home> page, string text)
    {
        var copy = PlaygroundMarkup.GetInputCopy(page);

        Assert.AreEqual(text, copy.TextContent, "The coloured copy must hold the input's text.");
        Assert.AreSequenceEqual(GetFreshCategories(context, text), PlaygroundMarkup.GetCharacterCategories(copy), $"The colours of the copy must be those a fresh page shows for: {Escape(text)}");
        Assert.AreEqual(RenderFresh(context, text), copy.InnerHtml, $"The copy must be the one a fresh page shows for: {Escape(text)}");
    }

    /// <summary>
    /// Inserts each of <see cref="_insertions"/> at every position of a source and removes it again, each time on the
    /// highlighting of the previous text, and asserts that each result is the highlighting of a fresh page
    /// </summary>
    /// <param name="source">The source</param>
    private static void AssertEveryInsertionMatchesFreshHighlighting(string source)
    {
        using (var context = new BunitContext())
        {
            var original = PlaygroundHighlighter.Highlight(source, []);

            foreach (var insertion in _insertions)
            {
                for (var position = 0; position <= source.Length; position++)
                {
                    AssertEditMatchesFreshHighlighting(context, source, original, source.Insert(position, insertion));
                }
            }
        }
    }

    /// <summary>
    /// Changes a highlighted text into another one and back, each time on the highlighting of the text before, and asserts
    /// that both results are the highlighting of a fresh page
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="source">The original text</param>
    /// <param name="original">The highlighting of the original text</param>
    /// <param name="edited">The edited text</param>
    private static void AssertEditMatchesFreshHighlighting(BunitContext context, string source, PlaygroundHighlighting original, string edited)
    {
        var forward = PlaygroundHighlighter.Highlight(edited, [], original);

        Assert.AreEqual(RenderFresh(context, edited), context.Render(forward.Content).Markup, $"Editing {Escape(source)} into {Escape(edited)} must give a fresh page's highlighting.");

        var back = PlaygroundHighlighter.Highlight(source, [], forward);

        Assert.AreEqual(RenderFresh(context, source), context.Render(back.Content).Markup, $"Editing {Escape(edited)} back into {Escape(source)} must give a fresh page's highlighting.");
    }

    /// <summary>
    /// Renders the highlighting a freshly rendered page shows for a text without marks: the one computed without an earlier
    /// version of the text
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="text">The text</param>
    /// <returns>The markup</returns>
    private static string RenderFresh(BunitContext context, string text)
    {
        return context.Render(PlaygroundHighlighter.Highlight(text, []).Content).Markup;
    }

    /// <summary>
    /// Gets the category of every character of the highlighting a freshly rendered page shows for a text
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="text">The text</param>
    /// <returns>One category per character</returns>
    private static List<string> GetFreshCategories(BunitContext context, string text)
    {
        var fragment = context.Render(builder =>
                                      {
                                          builder.OpenElement(0, "pre");
                                          builder.AddContent(1, PlaygroundHighlighter.Highlight(text, []).Content);
                                          builder.CloseElement();
                                      });

        return PlaygroundMarkup.GetCharacterCategories(fragment.Find("pre"));
    }

    /// <summary>
    /// Writes a text with its line breaks visible, for a failure message
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The text with <c>\r</c> and <c>\n</c> escaped</returns>
    private static string Escape(string text)
    {
        return $"\"{text.Replace("\r", "\\r").Replace("\n", "\\n")}\"";
    }

    #endregion // Methods
}