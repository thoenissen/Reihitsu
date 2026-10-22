using System.Text.RegularExpressions;

using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains component tests for the playground <see cref="Home"/> page, rendered with bUnit
/// </summary>
[TestClass]
public class HomePageTests
{
    #region Constants

    /// <summary>
    /// The example the page opens with
    /// </summary>
    private const string ExampleSource = "public class Sample{public int Add(int a,int b){return a+b;}}";

    /// <summary>
    /// The formatted example the output shows on load
    /// </summary>
    private const string FormattedExampleSource = """
                                                  public class Sample
                                                  {
                                                      public int Add(int a, int b)
                                                      {
                                                          return a + b;
                                                      }
                                                  }
                                                  """;

    /// <summary>
    /// The message shown when the input has a syntax error
    /// </summary>
    private const string SyntaxErrorMessage = "The input contains syntax errors and was left unchanged.";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the page states, above the editors, that formatting runs in the browser and nothing is uploaded
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-001")]
    public void PageStatesThatFormattingRunsLocallyAboveTheEditors()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var paragraph = page.Find("p");

            Assert.AreEqual("Paste C# source below and format it with the Reihitsu formatter. Formatting runs entirely in your browser; nothing is uploaded anywhere.",
                            NormalizeWhitespace(paragraph.TextContent));
            Assert.IsLessThan(page.Markup.IndexOf("playground-grid", System.StringComparison.Ordinal),
                              page.Markup.IndexOf("nothing is uploaded anywhere", System.StringComparison.Ordinal),
                              "The statement must appear above the editors.");
        }
    }

    /// <summary>
    /// Verifies that the page formats on load and on "Format" without any registered service and without any JavaScript
    /// interop call, so no input can leave the component through HTTP or the browser
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-001")]
    public void FormattingRunsWithoutServicesOrJavaScriptInterop()
    {
        using (var context = new BunitContext())
        {
            context.JSInterop.Mode = JSRuntimeMode.Strict;

            var page = context.Render<Home>();

            page.Find("#playground-input").Input("class C{int x;}");
            page.Find("button").Click();

            Assert.AreEqual("class C\n{\n    int x;\n}", page.Find("#playground-output").GetAttribute("value"));
            Assert.IsEmpty(context.JSInterop.Invocations, "Formatting must not call into JavaScript.");
        }
    }

    /// <summary>
    /// Verifies that the page shows an editable input text area and a read-only output text area side by side, both
    /// without spell checking, with the "Format" button below them
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-002")]
    public void PageShowsEditableInputAndReadOnlyOutputWithFormatButtonBelow()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();
            var columns = page.FindAll(".playground-grid > .playground-column");

            Assert.HasCount(2, columns);

            var inputLabel = columns[0].QuerySelector("label");
            var input = columns[0].QuerySelector("textarea");
            var outputLabel = columns[1].QuerySelector("label");
            var output = columns[1].QuerySelector("textarea");

            Assert.AreEqual("Input", inputLabel?.TextContent);
            Assert.AreEqual("playground-input", input?.Id);
            Assert.AreEqual("false", input?.GetAttribute("spellcheck"));
            Assert.IsFalse(input?.HasAttribute("readonly") ?? true, "The input must be editable.");

            Assert.AreEqual("Formatted output", outputLabel?.TextContent);
            Assert.AreEqual("playground-output", output?.Id);
            Assert.AreEqual("false", output?.GetAttribute("spellcheck"));
            Assert.IsTrue(output?.HasAttribute("readonly") ?? false, "The output must be read-only.");

            var button = page.Find("button");

            Assert.AreEqual("Format", button.TextContent);
            Assert.AreEqual("playground-grid", button.PreviousElementSibling?.ClassName, "The button must follow the editors.");
        }
    }

    /// <summary>
    /// Verifies that the page opens with the example in the input and the formatted example in the output
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-003")]
    public void PageOpensWithTheFormattedExample()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            Assert.AreEqual(ExampleSource, page.Find("#playground-input").GetAttribute("value"));
            Assert.AreEqual(FormattedExampleSource, page.Find("#playground-output").GetAttribute("value"));
            Assert.IsEmpty(page.FindAll(".playground-message"), "The example must format without a message.");
        }
    }

    /// <summary>
    /// Verifies that typing changes only the input and that the output is replaced only when "Format" is pressed
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-004")]
    public void OutputChangesOnlyWhenFormatIsPressed()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input("class C{");
            page.Find("#playground-input").Input("class C{int x;}");

            Assert.AreEqual("class C{int x;}", page.Find("#playground-input").GetAttribute("value"));
            Assert.AreEqual(FormattedExampleSource, page.Find("#playground-output").GetAttribute("value"), "Typing must not format.");
            Assert.IsEmpty(page.FindAll(".playground-message"), "Typing half-written code must not show the syntax error message.");

            page.Find("button").Click();

            Assert.AreEqual("class C\n{\n    int x;\n}", page.Find("#playground-output").GetAttribute("value"));
        }
    }

    /// <summary>
    /// Verifies that input with a syntax error is shown unchanged with the syntax error message below the button
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void InputWithSyntaxErrorIsShownUnchangedWithMessage()
    {
        using (var context = new BunitContext())
        {
            const string input = "public class Sample{ int x }";

            var page = context.Render<Home>();

            page.Find("#playground-input").Input(input);
            page.Find("button").Click();

            var message = page.Find(".playground-message");

            Assert.AreEqual(input, page.Find("#playground-output").GetAttribute("value"));
            Assert.AreEqual(SyntaxErrorMessage, message.TextContent);
            Assert.AreEqual("BUTTON", message.PreviousElementSibling?.TagName, "The message must appear below the button.");
        }
    }

    /// <summary>
    /// Verifies that the syntax error message disappears on the next successful format
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-006")]
    public void SyntaxErrorMessageDisappearsOnNextSuccessfulFormat()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input("public class Sample{ int x }");
            page.Find("button").Click();

            Assert.HasCount(1, page.FindAll(".playground-message"));

            page.Find("#playground-input").Input("public class Sample{ int x; }");
            page.Find("button").Click();

            Assert.IsEmpty(page.FindAll(".playground-message"));
            Assert.AreEqual("public class Sample\n{\n    int x;\n}", page.Find("#playground-output").GetAttribute("value"));
        }
    }

    /// <summary>
    /// Collapses every run of whitespace to one space, so text split across markup lines compares as rendered
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <returns>The normalized text</returns>
    private static string NormalizeWhitespace(string text)
    {
        return Regex.Replace(text, @"\s+", " ", RegexOptions.None, Regex.InfiniteMatchTimeout).Trim();
    }

    #endregion // Methods
}