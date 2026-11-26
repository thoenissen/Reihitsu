using AngleSharp.Dom;

using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains tests for the line layout of both panes of the <see cref="Home"/> page, read from the rendered markup and the
/// computed style of the Playground's stylesheet
/// </summary>
[TestClass]
public class LayoutTests
{
    #region Constants

    /// <summary>
    /// The input of the PLAY-012 example
    /// </summary>
    private const string LongLineSource = "public class Sample { public int Add(int first, int second, int third, int fourth, int fifth, int sixth, int seventh) { return first + second + third + fourth + fifth + sixth + seventh; } }";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: the long line stays one line in the input and its long line stays one line in the
    /// output, with no line break added to the text
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-012")]
    public void SpecExampleLongLinesStayOneLine()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, LongLineSource);

            Assert.AreEqual(LongLineSource, PlaygroundMarkup.GetInputCopy(page).TextContent);
            Assert.Contains("\n    public int Add(int first, int second, int third, int fourth, int fifth, int sixth, int seventh)\n", PlaygroundMarkup.GetOutputText(page));
        }
    }

    /// <summary>
    /// Verifies that neither the text area, nor its coloured copy, nor the output wraps a line, in both colour schemes
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-012")]
    public void PanesDoNotWrapLines()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            foreach (var dark in new[] { false, true })
            {
                var document = PlaygroundStyles.CreateDocument(page.Markup, dark);
                var input = document.QuerySelector("#playground-input");
                var copy = PlaygroundMarkup.GetInputCopy(document);
                var output = document.QuerySelector("#playground-output");

                Assert.IsTrue(input.GetAttribute("wrap") == "off" || PlaygroundStyles.GetComputed(document, input, "white-space") == "pre", "The text area must not wrap lines.");
                Assert.AreEqual("pre", PlaygroundStyles.GetComputed(document, copy, "white-space"), "The coloured copy must keep every line on one line and keep its spaces.");
                Assert.IsTrue(PlaygroundStyles.GetComputed(document, output, "white-space") == "pre" || (output.TagName == "PRE" && PlaygroundStyles.GetComputed(document, output, "white-space") == string.Empty), "The output must keep every line on one line and keep its spaces.");
            }
        }
    }

    /// <summary>
    /// Verifies that a pane whose text is wider than the pane scrolls horizontally: the output or a box around it inside its
    /// column scrolls, and the text area, which scrolls by itself, is not cut off unless a box around it inside its column scrolls
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-012")]
    public void PanesScrollHorizontally()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var document = PlaygroundStyles.CreateDocument(page.Markup, false);
            var input = document.QuerySelector("#playground-input");
            var output = document.QuerySelector("#playground-output");

            Assert.IsTrue(ScrollsHorizontally(document, output), "The output must scroll horizontally.");
            Assert.IsTrue(ScrollsHorizontally(document, input) || PlaygroundStyles.GetComputed(document, input, "overflow-x") is not ("hidden" or "clip"), "The input must scroll horizontally.");
        }
    }

    /// <summary>
    /// Verifies that a pane whose text is wider than the pane keeps the width of its column and scrolls inside itself, on a wide
    /// and on a narrow screen: no box from the pane up to the page takes its width from the pane's text. The check follows the
    /// CSS sizing rules for the layouts the page uses (block, flex, and grid), on the computed style of each box
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-012")]
    public void PanesKeepTheirColumnWidthWhenTheirTextIsWider()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            foreach (var narrow in new[] { false, true })
            {
                var document = PlaygroundStyles.CreateDocument(page.Markup, false, narrow);

                foreach (var (name, selector) in new[] { ("input", ".playground-editor"), ("output", "#playground-output") })
                {
                    var pane = document.QuerySelector(selector);
                    var growth = PlaygroundWidths.FindWidthFromText(document, pane);

                    Assert.IsNull(growth, $"On a {(narrow ? "narrow" : "wide")} screen the {name} pane grows to the width of its text instead of scrolling inside itself: {growth}");
                }
            }
        }
    }

    /// <summary>
    /// Verifies that a keyboard user can reach the output to scroll it: the box that scrolls the output horizontally, the output
    /// or a box around it inside its column, is a focus stop in the tab order
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-012")]
    public void OutputIsReachableByKeyboard()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var document = PlaygroundStyles.CreateDocument(page.Markup, false);
            var scroller = document.QuerySelector("#playground-output");

            while (scroller != null && PlaygroundStyles.GetComputed(document, scroller, "overflow-x") is not ("auto" or "scroll"))
            {
                scroller = scroller.ParentElement;
            }

            Assert.IsNotNull(scroller, "The output must scroll horizontally.");
            Assert.IsTrue(int.TryParse(scroller.GetAttribute("tabindex"), out var index) && index >= 0, $"The box that scrolls the output must be a focus stop, so a keyboard user can scroll it: {scroller.OuterHtml.Split('>')[0]}>");
        }
    }

    /// <summary>
    /// Verifies that on a narrow screen the columns are stacked and both panes take the narrow-screen height, which differs from
    /// the height they take on a wide screen, in both colour schemes
    /// </summary>
    [TestMethod]
    public void NarrowScreenStacksTheColumnsAndAppliesItsPaneHeight()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            foreach (var dark in new[] { false, true })
            {
                var wide = PlaygroundStyles.CreateDocument(page.Markup, dark);
                var narrow = PlaygroundStyles.CreateDocument(page.Markup, dark, narrow: true);
                var wideHeight = PlaygroundStyles.GetComputed(wide, wide.QuerySelector(".playground-editor"), "height");
                var editorHeight = PlaygroundStyles.GetComputed(narrow, narrow.QuerySelector(".playground-editor"), "height");
                var outputHeight = PlaygroundStyles.GetComputed(narrow, narrow.QuerySelector("#playground-output"), "height");

                Assert.AreEqual(PlaygroundStyles.GetComputed(wide, wide.QuerySelector("#playground-output"), "height"), wideHeight, "On a wide screen both panes must have one height.");
                Assert.HasCount(1, PlaygroundWidths.GetTracks(PlaygroundStyles.GetComputed(narrow, narrow.QuerySelector(".playground-grid"), "grid-template-columns")), "On a narrow screen the columns must be stacked in one track.");
                Assert.AreEqual(editorHeight, outputHeight, "On a narrow screen both panes must have one height.");
                Assert.AreNotEqual(wideHeight, editorHeight, $"On a narrow screen the panes must take the narrow-screen height, not the wide one ({wideHeight}).");
            }
        }
    }

    /// <summary>
    /// Checks whether an element, or a box around it inside its column, scrolls horizontally
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="element">The element</param>
    /// <returns><see langword="true"/> when one of them computes <c>overflow-x</c> to <c>auto</c> or <c>scroll</c></returns>
    private static bool ScrollsHorizontally(IDocument document, IElement element)
    {
        for (var current = element; current != null && current.ClassList.Contains("playground-column") == false; current = current.ParentElement)
        {
            if (PlaygroundStyles.GetComputed(document, current, "overflow-x") is "auto" or "scroll")
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods
}