using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains component tests for the highlighting of the input while the user types on the <see cref="Home"/> page
/// </summary>
[TestClass]
public class InputHighlightingTests
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: typing <c>class C{</c> colours the input at once although it does not parse,
    /// while the output keeps its text and its colours
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-011")]
    public void SpecExampleTypingColoursTheInputAtOnceAndLeavesTheOutput()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var outputBefore = PlaygroundMarkup.GetOutput(page).InnerHtml;

            page.Find("#playground-input").Input("class C{");

            var copy = PlaygroundMarkup.GetInputCopy(page);

            Assert.AreEqual("class C{", copy.TextContent);
            Assert.AreSequenceEqual(new[] { "keyword class", "identifier C", "normal {" }, PlaygroundMarkup.GetSegments(copy));
            Assert.AreEqual(outputBefore, PlaygroundMarkup.GetOutput(page).InnerHtml, "Typing must not change the output's text or colours.");
        }
    }

    /// <summary>
    /// Verifies that the colours follow every change of the input: a word is an identifier while it is not a keyword yet, a
    /// keyword once it is one, and an identifier again when it stops being one
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-011")]
    public void ColoursFollowEveryChangeOfTheInput()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var input = page.Find("#playground-input");

            input.Input("clas");

            Assert.AreSequenceEqual(new[] { "identifier clas" }, PlaygroundMarkup.GetSegments(PlaygroundMarkup.GetInputCopy(page)));

            page.Find("#playground-input").Input("class");

            Assert.AreSequenceEqual(new[] { "keyword class" }, PlaygroundMarkup.GetSegments(PlaygroundMarkup.GetInputCopy(page)));

            page.Find("#playground-input").Input("classy");

            Assert.AreSequenceEqual(new[] { "identifier classy" }, PlaygroundMarkup.GetSegments(PlaygroundMarkup.GetInputCopy(page)));

            page.Find("#playground-input").Input(string.Empty);

            Assert.AreEqual(string.Empty, PlaygroundMarkup.GetInputCopy(page).TextContent);
        }
    }

    /// <summary>
    /// Verifies that the input's text is read once by assistive technology: the coloured copy is hidden from it, and the
    /// text area is not
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-011")]
    public void ColouredCopyIsHiddenFromAssistiveTechnologyAndTheTextAreaIsNot()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var copy = PlaygroundMarkup.GetInputCopy(page);
            var input = page.Find("#playground-input");

            Assert.AreEqual("true", copy.GetAttribute("aria-hidden"));
            Assert.IsNull(input.Closest("[aria-hidden='true']"), "The text area must stay visible to assistive technology.");
            Assert.IsEmpty(copy.QuerySelectorAll("textarea, input, [tabindex]"), "The coloured copy must not hold a control or a focus stop.");
        }
    }

    /// <summary>
    /// Verifies that the text area and its coloured copy compute to the same font, size, line height, and tab size in both
    /// colour schemes, so the colours lie under the characters they colour
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-011")]
    public void TextAreaAndColouredCopyShareTheirTextMetrics()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            foreach (var dark in new[] { false, true })
            {
                var document = PlaygroundStyles.CreateDocument(page.Markup, dark);
                var input = document.QuerySelector("#playground-input");
                var copy = PlaygroundMarkup.GetInputCopy(document);

                Assert.IsFalse(string.IsNullOrEmpty(PlaygroundStyles.GetComputed(document, copy, "font-family")), "The coloured copy must have a font.");

                foreach (var property in new[] { "font-family", "font-size", "line-height", "letter-spacing", "tab-size" })
                {
                    Assert.AreEqual(PlaygroundStyles.GetComputed(document, input, property), PlaygroundStyles.GetComputed(document, copy, property), $"'{property}' must be the same for the text area and its coloured copy.");
                }
            }
        }
    }

    #endregion // Methods
}