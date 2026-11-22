using System.Linq;
using System.Text.RegularExpressions;

using AngleSharp.Dom;

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

    /// <summary>
    /// The message shown when the input holds code that another build configuration hides
    /// </summary>
    private const string HiddenCodeMessage = "The input contains code hidden in another build configuration and was left unchanged.";

    /// <summary>
    /// The input of the PLAY-009 example: the comment starts inside the <c>#if !A</c> group and holds the line <c>#else</c>
    /// </summary>
    private const string HiddenCodeSource = "class C\n{\n    void M()\n    {\n#if !A\n        /*\n#else\n        var s = @\"\n        */\n        int  x = 1; //\";\n#endif\n    }\n}";

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

            Assert.AreEqual("class C\n{\n    int x;\n}", PlaygroundMarkup.GetOutputText(page));
            Assert.IsEmpty(context.JSInterop.Invocations, "Formatting must not call into JavaScript.");
        }
    }

    /// <summary>
    /// Verifies that the page shows an editable input text area without spell checking and a read-only output code view side
    /// by side, with the "Format" button below them
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
            var input = columns[0].QuerySelector("#playground-input");
            var outputLabel = columns[1].QuerySelector("label");
            var output = columns[1].QuerySelector("#playground-output");

            Assert.AreEqual("Input", inputLabel?.TextContent);
            Assert.AreEqual("TEXTAREA", input?.TagName, "The input must be a text area, which keeps the browser's own editing, selection, and undo.");
            Assert.AreEqual("false", input?.GetAttribute("spellcheck"));
            Assert.IsFalse(input?.HasAttribute("readonly") ?? true, "The input must be editable.");
            Assert.IsFalse(input?.HasAttribute("disabled") ?? true, "The input must be editable.");

            Assert.AreEqual("Formatted output", outputLabel?.TextContent);
            Assert.IsNotNull(output, "The output must be in the second column.");
            Assert.AreNotEqual("TEXTAREA", output.TagName, "The output must be a code view, not a text area.");
            Assert.AreNotEqual("INPUT", output.TagName, "The output must be a code view, not an input.");
            Assert.IsEmpty(output.QuerySelectorAll("textarea, input, [contenteditable]:not([contenteditable='false'])"), "The output must not be editable.");
            Assert.IsTrue(output.GetAttribute("contenteditable") is null or "false", "The output must not be editable.");

            var button = page.Find("button");

            Assert.AreEqual("Format", button.TextContent);
            Assert.AreEqual("playground-grid", button.PreviousElementSibling?.ClassName, "The button must follow the editors.");
        }
    }

    /// <summary>
    /// Verifies that the output's text is exactly the output text, so selecting and copying it gives that text, and that no
    /// rule of the stylesheet makes the output unselectable
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-002")]
    public void OutputTextIsSelectableAndExactlyTheOutputText()
    {
        using (var context = new BunitContext())
        {
            const string input = "class C\n{\n\tint  x = 1 ;\n\n}\n";

            var page = context.Render<Home>();

            PlaygroundMarkup.Format(page, "class C{int x;}");

            Assert.AreEqual("class C\n{\n    int x;\n}", PlaygroundMarkup.GetOutputText(page));

            // An input with a syntax error is shown unchanged, so every character of it, tabs and blank lines included, must reach the output
            PlaygroundMarkup.Format(page, input + "int");

            Assert.AreEqual(input + "int", PlaygroundMarkup.GetOutputText(page));

            foreach (var dark in new[] { false, true })
            {
                var document = PlaygroundStyles.CreateDocument(page.Markup, dark);
                var output = document.QuerySelector("#playground-output");

                foreach (var element in output.GetDescendants().OfType<IElement>().Prepend(output))
                {
                    Assert.AreNotEqual("none", PlaygroundStyles.GetComputed(document, element, "user-select"), "The output's text must be selectable.");
                }
            }
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
            Assert.AreEqual(FormattedExampleSource, PlaygroundMarkup.GetOutputText(page));
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
            Assert.AreEqual(FormattedExampleSource, PlaygroundMarkup.GetOutputText(page), "Typing must not format.");
            Assert.IsEmpty(page.FindAll(".playground-message"), "Typing half-written code must not show the syntax error message.");

            page.Find("button").Click();

            Assert.AreEqual("class C\n{\n    int x;\n}", PlaygroundMarkup.GetOutputText(page));
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

            Assert.AreEqual(input, PlaygroundMarkup.GetOutputText(page));
            Assert.AreSequenceEqual(new[] { SyntaxErrorMessage, "line 1, column 28 — CS1002: ; expected" }, PlaygroundMarkup.GetMessageLines(page));
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
            Assert.AreEqual("public class Sample\n{\n    int x;\n}", PlaygroundMarkup.GetOutputText(page));
        }
    }

    /// <summary>
    /// Verifies the specification example: input with unsupported syntax is shown unchanged as the output, and the message names
    /// the line, the column, and the reason below the button
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-008")]
    public void SpecExampleInputWithUnsupportedSyntaxIsShownUnchangedWithMessage()
    {
        using (var context = new BunitContext())
        {
            const string input = "class C\n{\n    void M()\n    {\n        int /* c */ a = 1;\n    }\n}";

            var page = context.Render<Home>();

            page.Find("#playground-input").Input(input);
            page.Find("button").Click();

            var message = page.Find(".playground-message");

            Assert.AreEqual(input, PlaygroundMarkup.GetOutputText(page));
            Assert.AreSequenceEqual(new[] { "The input contains syntax the formatter does not support and was left unchanged: line 5, column 13 — a comment between a variable declaration's own tokens." }, PlaygroundMarkup.GetMessageLines(page));
            Assert.AreEqual("BUTTON", message.PreviousElementSibling?.TagName, "The message must appear below the button.");
        }
    }

    /// <summary>
    /// Verifies that the unsupported-syntax message disappears on the next format of an input the formatter does not refuse
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-008")]
    public void UnsupportedSyntaxMessageDisappearsOnNextSuccessfulFormat()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input("public class Sample{ int /* c */ x; }");
            page.Find("button").Click();

            Assert.HasCount(1, page.FindAll(".playground-message"));

            page.Find("#playground-input").Input("public class Sample{ int x; }");
            page.Find("button").Click();

            Assert.IsEmpty(page.FindAll(".playground-message"));
            Assert.AreEqual("public class Sample\n{\n    int x;\n}", PlaygroundMarkup.GetOutputText(page));
        }
    }

    /// <summary>
    /// Verifies the specification example: input that holds a comment another build compiles as code is shown unchanged as the
    /// output, with the message below the button
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-009")]
    public void SpecExampleInputWithHiddenCodeIsShownUnchangedWithMessage()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input(HiddenCodeSource);
            page.Find("button").Click();

            var message = page.Find(".playground-message");

            Assert.AreEqual(HiddenCodeSource, PlaygroundMarkup.GetOutputText(page));
            Assert.AreSequenceEqual(new[] { HiddenCodeMessage }, PlaygroundMarkup.GetMessageLines(page));
            Assert.AreEqual("BUTTON", message.PreviousElementSibling?.TagName, "The message must appear below the button.");
        }
    }

    /// <summary>
    /// Verifies that the hidden-code message disappears on the next successful format
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-009")]
    public void HiddenCodeMessageDisappearsOnNextSuccessfulFormat()
    {
        using (var context = new BunitContext())
        {
            var page = context.Render<Home>();

            page.Find("#playground-input").Input(HiddenCodeSource);
            page.Find("button").Click();

            Assert.HasCount(1, page.FindAll(".playground-message"));

            page.Find("#playground-input").Input("public class Sample{ int x; }");
            page.Find("button").Click();

            Assert.IsEmpty(page.FindAll(".playground-message"));
            Assert.AreEqual("public class Sample\n{\n    int x;\n}", PlaygroundMarkup.GetOutputText(page));
        }
    }

    /// <summary>
    /// Verifies that input that holds a hiding carrier and a shape the formatter refuses as unsupported shows the hidden-code
    /// message, not the unsupported-syntax message
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-009")]
    public void HiddenCodeBesideAnUnsupportedShapeShowsTheHiddenCodeMessage()
    {
        using (var context = new BunitContext())
        {
            var input = HiddenCodeSource.Replace("#endif\n", "#endif\n        int /* c */ y = 1;\n", System.StringComparison.Ordinal);

            var page = context.Render<Home>();

            page.Find("#playground-input").Input(input);
            page.Find("button").Click();

            Assert.AreEqual(input, PlaygroundMarkup.GetOutputText(page));
            Assert.AreSequenceEqual(new[] { HiddenCodeMessage }, PlaygroundMarkup.GetMessageLines(page));
        }
    }

    /// <summary>
    /// Verifies that input with a syntax error that also holds a hiding carrier shows the syntax error message
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-009")]
    public void HiddenCodeBesideASyntaxErrorShowsTheSyntaxErrorMessage()
    {
        using (var context = new BunitContext())
        {
            var input = HiddenCodeSource.Replace("    }\n}", "    }\n\n    void N(\n}", System.StringComparison.Ordinal);

            var page = context.Render<Home>();

            page.Find("#playground-input").Input(input);
            page.Find("button").Click();

            Assert.AreEqual(input, PlaygroundMarkup.GetOutputText(page));
            Assert.AreEqual(SyntaxErrorMessage, PlaygroundMarkup.GetMessageLines(page)[0]);
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