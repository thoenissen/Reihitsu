using System.Collections.Generic;
using System.Linq;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains component tests for the syntax highlighting of both panes of the <see cref="Home"/> page
/// </summary>
[TestClass]
public class HighlightingTests
{
    #region Constants

    /// <summary>
    /// The example the page opens with, also the first example of PLAY-010
    /// </summary>
    private const string ExampleSource = "public class Sample{public int Add(int a,int b){return a+b;}}";

    /// <summary>
    /// The second example of PLAY-010
    /// </summary>
    private const string DirectiveExampleSource = "#region Methods\n#if DEBUG\ninactive text\n#endif\n// comment\nclass C { string s = $\"a{n}b\"; char c = 'x'; const int n = 42; }\n#endregion // Methods";

    /// <summary>
    /// The third example of PLAY-010
    /// </summary>
    private const string DocumentationExampleSource = "/// <summary>Uses <see cref=\"int\"/>.</summary>\npartial class C { int P { set { var x = nameof(value); dynamic d = x; } } }";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The segments of the first example of PLAY-010
    /// </summary>
    private static readonly string[] _exampleSegments = ["keyword public", "keyword class", "identifier Sample", "normal {", "keyword public", "keyword int", "identifier Add", "normal (", "keyword int", "identifier a", "normal ,", "keyword int", "identifier b", "normal ){", "keyword return", "identifier a", "normal +", "identifier b", "normal ;}}"];

    /// <summary>
    /// The segments of the second example of PLAY-010
    /// </summary>
    private static readonly string[] _directiveExampleSegments = ["directive #region", "directive Methods", "directive #if", "directive DEBUG", "disabled inactive", "disabled text", "directive #endif", "comment //", "comment comment", "keyword class", "identifier C", "normal {", "keyword string", "identifier s", "normal =", "string $\"a", "normal {", "identifier n", "normal }", "string b\"", "normal ;", "keyword char", "identifier c", "normal =", "string 'x'", "normal ;", "keyword const", "keyword int", "identifier n", "normal =", "number 42", "normal ;", "normal }", "directive #endregion", "comment //", "comment Methods"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies the first specification example in the input on load: keywords and identifiers have their categories, and
    /// punctuation and operators keep the normal text colour
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void SpecExampleInputIsHighlightedByTokenKind()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var copy = PlaygroundMarkup.GetInputCopy(page);

            Assert.AreEqual(ExampleSource, copy.TextContent);
            Assert.AreSequenceEqual(_exampleSegments, PlaygroundMarkup.GetSegments(copy));
        }
    }

    /// <summary>
    /// Verifies the first specification example in the output on load: the formatted text is highlighted with the same
    /// categories
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void SpecExampleOutputIsHighlightedByTokenKind()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var output = PlaygroundMarkup.GetOutput(page);

            Assert.AreEqual("public class Sample\n{\n    public int Add(int a, int b)\n    {\n        return a + b;\n    }\n}", output.TextContent);
            Assert.AreSequenceEqual(new[] { "keyword public", "keyword class", "identifier Sample", "normal {", "keyword public", "keyword int", "identifier Add", "normal (", "keyword int", "identifier a", "normal ,", "keyword int", "identifier b", "normal )", "normal {", "keyword return", "identifier a", "normal +", "identifier b", "normal ;", "normal }", "normal }" },
                                    PlaygroundMarkup.GetSegments(output));
        }
    }

    /// <summary>
    /// Verifies the second specification example in the input: directives, the comment behind <c>#endregion</c>, disabled
    /// text, a comment, the parts of an interpolated string, a character literal, and a number
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void SpecExampleDirectivesCommentsStringsNumbersAndDisabledTextAreHighlightedInTheInput()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input(DirectiveExampleSource);

            var copy = PlaygroundMarkup.GetInputCopy(page);

            Assert.AreEqual(DirectiveExampleSource, copy.TextContent);
            Assert.AreSequenceEqual(_directiveExampleSegments, PlaygroundMarkup.GetSegments(copy));
        }
    }

    /// <summary>
    /// Verifies the second specification example in the output after "Format": the formatter changes only whitespace here,
    /// so the output carries the same segments
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void SpecExampleDirectivesCommentsStringsNumbersAndDisabledTextAreHighlightedInTheOutput()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, DirectiveExampleSource);

            Assert.IsEmpty(page.FindAll(".playground-message"), "The example must format.");
            Assert.AreSequenceEqual(_directiveExampleSegments, PlaygroundMarkup.GetSegments(PlaygroundMarkup.GetOutput(page)));
        }
    }

    /// <summary>
    /// Verifies the third specification example: the documentation comment is a comment as a whole, the contextual keywords
    /// the parser reads as keywords are keywords, and the names the parser reads as identifiers are identifiers
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void SpecExampleDocumentationCommentAndContextualKeywordsFollowTheParser()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input(DocumentationExampleSource);

            var copy = PlaygroundMarkup.GetInputCopy(page);
            var firstLineLength = DocumentationExampleSource.IndexOf('\n', System.StringComparison.Ordinal);

            Assert.AreEqual(DocumentationExampleSource, copy.TextContent);
            Assert.AreSequenceEqual(new[] { "comment" }, PlaygroundMarkup.GetCategoriesOfRange(copy, 0, firstLineLength), "The whole documentation comment, int and see included, must be a comment.");
            Assert.AreSequenceEqual(new[] { "keyword partial", "keyword class", "identifier C", "normal {", "keyword int", "identifier P", "normal {", "keyword set", "normal {", "identifier var", "identifier x", "normal =", "identifier nameof", "normal (", "identifier value", "normal );", "identifier dynamic", "identifier d", "normal =", "identifier x", "normal ;", "normal }", "normal }", "normal }" },
                                    PlaygroundMarkup.GetSegments(copy).Skip(GetSegmentCount(DocumentationExampleSource[..firstLineLength])).ToList());
        }
    }

    /// <summary>
    /// Verifies that a delimited documentation comment is a comment as a whole, the keyword inside its <c>cref</c> included
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void DelimitedDocumentationCommentIsACommentAsAWhole()
    {
        AssertInputSegments("/** <see cref=\"int\"/> */\nclass C { }",
                            "comment /**",
                            "comment <see",
                            "comment cref=\"int\"/>",
                            "comment */",
                            "keyword class",
                            "identifier C",
                            "normal {",
                            "normal }");
    }

    /// <summary>
    /// Verifies that text that looks like a comment inside a string literal is a string, and that a verbatim identifier is an
    /// identifier although its text holds a keyword
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void CommentLookingStringIsAStringAndVerbatimIdentifierIsAnIdentifier()
    {
        AssertInputSegments("class C { string s = \"// not a comment\"; int @class; }",
                            "keyword class",
                            "identifier C",
                            "normal {",
                            "keyword string",
                            "identifier s",
                            "normal =",
                            "string \"//",
                            "string not",
                            "string a",
                            "string comment\"",
                            "normal ;",
                            "keyword int",
                            "identifier @class",
                            "normal ;",
                            "normal }");
    }

    /// <summary>
    /// Verifies that the format clause of an interpolation is a string, while the hole's braces, its colon, and its
    /// expression are not
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void InterpolationFormatClauseIsAStringAndTheHoleIsNot()
    {
        AssertInputSegments("class C { string s = $\"a{n:N2}b\"; }",
                            "keyword class",
                            "identifier C",
                            "normal {",
                            "keyword string",
                            "identifier s",
                            "normal =",
                            "string $\"a",
                            "normal {",
                            "identifier n",
                            "normal :",
                            "string N2",
                            "normal }",
                            "string b\"",
                            "normal ;",
                            "normal }");
    }

    /// <summary>
    /// Verifies that the delimiters and the text parts of a raw string literal and of an interpolated raw string literal are
    /// strings, while the hole's braces and expression are not
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void RawStringLiteralsAreStringsExceptTheirHoles()
    {
        AssertInputSegments("class C { string r = \"\"\"raw\"\"\"; string s = $$\"\"\"a{{x}}b\"\"\"; }",
                            "keyword class",
                            "identifier C",
                            "normal {",
                            "keyword string",
                            "identifier r",
                            "normal =",
                            "string \"\"\"raw\"\"\"",
                            "normal ;",
                            "keyword string",
                            "identifier s",
                            "normal =",
                            "string $$\"\"\"a",
                            "normal {{",
                            "identifier x",
                            "normal }}",
                            "string b\"\"\"",
                            "normal ;",
                            "normal }");
    }

    /// <summary>
    /// Verifies that numeric literals of every form are numbers and a character literal is a string
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void NumericLiteralsAreNumbersAndCharacterLiteralsAreStrings()
    {
        AssertInputSegments("class C { void M() { var x = 1.5f + 0x1F + 'a'; } }",
                            "keyword class",
                            "identifier C",
                            "normal {",
                            "keyword void",
                            "identifier M",
                            "normal ()",
                            "normal {",
                            "identifier var",
                            "identifier x",
                            "normal =",
                            "number 1.5f",
                            "normal +",
                            "number 0x1F",
                            "normal +",
                            "string 'a'",
                            "normal ;",
                            "normal }",
                            "normal }");
    }

    /// <summary>
    /// Verifies that a comment trivia behind a directive is a comment, while text the parser reads as a directive's message is
    /// a directive although it looks like a comment
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void CommentBehindADirectiveIsACommentAndADirectiveMessageIsADirective()
    {
        AssertInputSegments("#pragma warning disable CS1 // why\n#region X /* c */\nclass C { }\n#endregion",
                            "directive #pragma",
                            "directive warning",
                            "directive disable",
                            "directive CS1",
                            "comment //",
                            "comment why",
                            "directive #region",
                            "directive X",
                            "directive /*",
                            "directive c",
                            "directive */",
                            "keyword class",
                            "identifier C",
                            "normal {",
                            "normal }",
                            "directive #endregion");
    }

    /// <summary>
    /// Verifies that the text of a <c>#error</c> directive is the directive's message, also where it looks like a comment
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void ErrorDirectiveMessageThatLooksLikeACommentIsADirective()
    {
        AssertInputSegments("#error a // b\nclass C { }",
                            "directive #error",
                            "directive a",
                            "directive //",
                            "directive b",
                            "keyword class",
                            "identifier C",
                            "normal {",
                            "normal }");
    }

    /// <summary>
    /// Verifies that the coloured copy of the input and the output hold exactly the text they colour: tabs, runs of spaces,
    /// blank lines, a line break at the end, and characters outside ASCII are neither dropped nor added
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void HighlightedTextIsExactlyTheTextItColours()
    {
        using (var context = new BunitContext())
        {
            const string input = "\tclass  C\n{\n\n    string s = \"äöü — 日本\";   \n\tint\n";

            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, input);

            Assert.HasCount(1, page.FindAll(".playground-message"), "The input must have a syntax error, so the output is the input unchanged.");
            Assert.AreEqual(input, PlaygroundMarkup.GetInputCopy(page).TextContent);
            Assert.AreEqual(input, PlaygroundMarkup.GetOutputText(page));
        }
    }

    /// <summary>
    /// Verifies that the page loads no script or stylesheet from another origin, so the colouring is computed in the page
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void PageLoadsNoScriptOrStylesheetFromAnotherOrigin()
    {
        var hostPage = new HtmlParser().ParseDocument(PlaygroundStyles.ReadHostPage());

        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            Assert.IsEmpty(page.FindAll("script, link, iframe"), "The page must not load a script, a stylesheet, or a frame.");
        }

        var references = hostPage.QuerySelectorAll("script[src]")
                                 .Select(element => element.GetAttribute("src"))
                                 .Concat(hostPage.QuerySelectorAll("link[href]").Select(element => element.GetAttribute("href")))
                                 .ToList();

        Assert.IsNotEmpty(references);

        foreach (var reference in references)
        {
            Assert.IsFalse(reference.Contains("//", System.StringComparison.Ordinal) || reference.Contains(':', System.StringComparison.Ordinal), $"'{reference}' must be a path on the page's own origin.");
        }
    }

    /// <summary>
    /// Verifies that each category has a colour of its own in both colour schemes, different from every other category and
    /// from the normal text colour, and that punctuation keeps the normal text colour
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-010")]
    public void EachCategoryHasItsOwnColourInEachColourScheme()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, DirectiveExampleSource);

            foreach (var dark in new[] { false, true })
            {
                var document = PlaygroundStyles.CreateDocument(page.Markup, dark);
                var normal = PlaygroundStyles.GetComputed(document, document.Body, "color");

                Assert.IsFalse(string.IsNullOrEmpty(normal), "The page must have a normal text colour.");

                AssertPaneColours(document, PlaygroundMarkup.GetInputCopy(document), normal, dark);
                AssertPaneColours(document, document.QuerySelector("#playground-output"), normal, dark);
            }
        }
    }

    /// <summary>
    /// Asserts that every category in a pane has one colour of its own, different from every other category and from the
    /// normal text colour, and that every character in no category has the normal text colour
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="pane">The coloured copy of the input or the output</param>
    /// <param name="normal">The normal text colour</param>
    /// <param name="dark">Whether the dark colour scheme applies</param>
    private static void AssertPaneColours(IDocument document, IElement pane, string normal, bool dark)
    {
        var scheme = dark ? "dark" : "light";
        var colours = new Dictionary<string, string>();

        foreach (var category in PlaygroundMarkup.Categories)
        {
            var categoryColours = pane.QuerySelectorAll($".hl-{category}")
                                      .Select(element => PlaygroundStyles.GetComputed(document, element, "color"))
                                      .Distinct()
                                      .ToList();

            Assert.HasCount(1, categoryColours, $"Every {category} must have one colour ({scheme} scheme).");
            Assert.AreNotEqual(normal, categoryColours[0], $"The {category} colour must differ from the normal text colour ({scheme} scheme).");

            colours[category] = categoryColours[0];
        }

        Assert.HasCount(PlaygroundMarkup.Categories.Length, colours.Values.Distinct(), $"Each category must have its own colour ({scheme} scheme): {string.Join(", ", colours)}.");

        foreach (var text in pane.GetDescendants().OfType<IText>().Where(text => text.Data.Trim().Length > 0 && text.ParentElement.Closest("[class*='hl-']") == null))
        {
            Assert.AreEqual(normal, PlaygroundStyles.GetComputed(document, text.ParentElement, "color"), $"'{text.Data}' must keep the normal text colour ({scheme} scheme).");
        }
    }

    /// <summary>
    /// Enters an input and asserts the segments of its coloured copy
    /// </summary>
    /// <param name="input">The input</param>
    /// <param name="expected">The expected segments</param>
    private static void AssertInputSegments(string input, params string[] expected)
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input(input);

            var copy = PlaygroundMarkup.GetInputCopy(page);

            Assert.AreEqual(input, copy.TextContent);
            Assert.AreSequenceEqual(expected, PlaygroundMarkup.GetSegments(copy), "Actual: " + string.Join(" | ", PlaygroundMarkup.GetSegments(copy)));
        }
    }

    /// <summary>
    /// Counts the whitespace-separated words of a text
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The number of words</returns>
    private static int GetSegmentCount(string text)
    {
        return text.Split([' ', '\n'], System.StringSplitOptions.RemoveEmptyEntries).Length;
    }

    #endregion // Methods
}