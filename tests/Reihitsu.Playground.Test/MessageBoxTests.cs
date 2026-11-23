using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains component tests for the alert box that shows the messages of the <see cref="Home"/> page
/// </summary>
[TestClass]
public class MessageBoxTests
{
    #region Constants

    /// <summary>
    /// The input of the PLAY-009 example
    /// </summary>
    private const string HiddenCodeSource = "class C\n{\n    void M()\n    {\n#if !A\n        /*\n#else\n        var s = @\"\n        */\n        int  x = 1; //\";\n#endif\n    }\n}";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: the input of PLAY-006's second example shows the box below the button, announced
    /// as an alert, with its sentence and its two error lines
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-013")]
    public void SpecExampleSyntaxErrorsShowAnAlertBoxBelowTheButton()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, "public class Sample{public int Add(int a,int b){return a+b}");

            var box = page.Find(".playground-message");

            Assert.AreEqual("alert", box.GetAttribute("role"));
            Assert.AreEqual("BUTTON", box.PreviousElementSibling?.TagName, "The box must appear below the button.");
            Assert.AreSequenceEqual(new[] { "The input contains syntax errors and was left unchanged.", "line 1, column 59 — CS1002: ; expected", "line 1, column 60 — CS1513: } expected" }, PlaygroundMarkup.GetMessageLines(page));
        }
    }

    /// <summary>
    /// Verifies the specification example: the example the page opens with shows no box
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-013")]
    public void SpecExampleNoMessageShowsNoBox()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            Assert.IsEmpty(page.FindAll(".playground-message"));
            Assert.IsEmpty(page.FindAll("[role='alert']"), "No alert must be announced without a message.");
        }
    }

    /// <summary>
    /// Verifies that the box disappears when a later format shows no message
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-013")]
    public void BoxDisappearsWhenTheNextFormatShowsNoMessage()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, "public class Sample{ int x }");

            Assert.HasCount(1, page.FindAll("[role='alert']"));

            PlaygroundMarkup.Format(page, "public class Sample{ int x; }");

            Assert.IsEmpty(page.FindAll(".playground-message"));
            Assert.IsEmpty(page.FindAll("[role='alert']"));
        }
    }

    /// <summary>
    /// Verifies that the unsupported-syntax message is shown in the alert box below the button
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-013")]
    public void UnsupportedSyntaxMessageIsShownInTheAlertBox()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, "class C\n{\n    void M()\n    {\n        int /* c */ a = 1;\n    }\n}");

            var box = page.Find(".playground-message");

            Assert.AreEqual("alert", box.GetAttribute("role"));
            Assert.AreEqual("BUTTON", box.PreviousElementSibling?.TagName, "The box must appear below the button.");
            Assert.AreSequenceEqual(new[] { "The input contains syntax the formatter does not support and was left unchanged: line 5, column 13 — a comment between a variable declaration's own tokens." }, PlaygroundMarkup.GetMessageLines(page));
        }
    }

    /// <summary>
    /// Verifies that the hidden-code message is shown in the alert box below the button
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-013")]
    public void HiddenCodeMessageIsShownInTheAlertBox()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, HiddenCodeSource);

            var box = page.Find(".playground-message");

            Assert.AreEqual("alert", box.GetAttribute("role"));
            Assert.AreEqual("BUTTON", box.PreviousElementSibling?.TagName, "The box must appear below the button.");
            Assert.AreSequenceEqual(new[] { "The input contains code hidden in another build configuration and was left unchanged." }, PlaygroundMarkup.GetMessageLines(page));
        }
    }

    /// <summary>
    /// Verifies that the box is set apart from the page's text in both colour schemes: it has a border or a background of its
    /// own
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-013")]
    public void BoxIsSetApartFromThePageText()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, "public class Sample{ int x }");

            foreach (var dark in new[] { false, true })
            {
                var document = PlaygroundStyles.CreateDocument(page.Markup, dark);
                var box = document.QuerySelector(".playground-message");
                var pageBackground = PlaygroundStyles.GetComputed(document, document.Body, "background-color");
                var boxBackground = PlaygroundStyles.GetComputed(document, box, "background-color");
                var hasBorder = PlaygroundStyles.GetComputed(document, box, "border-top-style") is not ("" or "none" or "hidden")
                                || PlaygroundStyles.GetComputed(document, box, "border-left-style") is not ("" or "none" or "hidden");
                var hasBackground = boxBackground is not ("" or "transparent" or "rgba(0, 0, 0, 0)") && boxBackground != pageBackground;

                Assert.IsTrue(hasBorder || hasBackground, $"The box must have a border or a background of its own ({(dark ? "dark" : "light")} scheme).");
            }
        }
    }

    #endregion // Methods
}