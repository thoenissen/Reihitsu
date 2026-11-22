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
                Assert.IsTrue(PlaygroundStyles.GetComputed(document, output, "white-space") == "pre" || (output.TagName == "PRE" && PlaygroundStyles.GetComputed(document, output, "white-space") is "" or "normal"), "The output must keep every line on one line and keep its spaces.");
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