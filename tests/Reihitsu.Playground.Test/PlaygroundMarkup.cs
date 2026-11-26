using System.Collections.Generic;
using System.Linq;
using System.Text;

using AngleSharp.Dom;

using Bunit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Playground.Pages;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Reads the parts of the rendered <see cref="Home"/> page that the highlighting, the error list, and the error marks
/// show. The markup contract these tests rely on:
/// <list type="bullet">
/// <item>
/// <description>
/// The input is the text area <c>#playground-input</c>; the input's column holds exactly one subtree
/// hidden from assistive technology (<c>aria-hidden="true"</c>), the coloured copy, whose text content is the input's text.
/// </description>
/// </item>
/// <item><description>The output is the element <c>#playground-output</c>, whose text content is the output text.</description></item>
/// <item>
/// <description>
/// A coloured character is inside an element with one of the classes <c>hl-keyword</c>, <c>hl-identifier</c>,
/// <c>hl-string</c>, <c>hl-number</c>, <c>hl-comment</c>, <c>hl-directive</c>, or <c>hl-disabled</c>; the nearest such
/// element decides its category, and a character inside none has the normal text colour.
/// </description>
/// </item>
/// <item>
/// <description>
/// An error mark is an element with the class <c>playground-mark</c> inside the coloured copy; its text
/// is the marked text, and a mark one character wide behind a line's or the text's last character holds no text.
/// </description>
/// </item>
/// <item><description>The message box is <c>.playground-message</c>; each of its lines is one of its child elements.</description></item>
/// </list>
/// </summary>
internal static class PlaygroundMarkup
{
    #region Constants

    /// <summary>
    /// The category of a character inside no category element
    /// </summary>
    internal const string Normal = "normal";

    /// <summary>
    /// The class prefix of a category element
    /// </summary>
    private const string CategoryClassPrefix = "hl-";

    /// <summary>
    /// Selects the two columns of the page
    /// </summary>
    private const string ColumnSelector = ".playground-grid > .playground-column";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The categories PLAY-010 names, each the suffix of its class behind <see cref="CategoryClassPrefix"/>
    /// </summary>
    internal static readonly string[] Categories = ["keyword", "identifier", "string", "number", "comment", "directive", "disabled"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Gets the coloured copy of the input from a document that holds the page's markup
    /// </summary>
    /// <param name="root">The document</param>
    /// <returns>The coloured copy</returns>
    internal static IElement GetInputCopy(IParentNode root)
    {
        return FindInputCopy(root.QuerySelectorAll(ColumnSelector).ToList());
    }

    /// <summary>
    /// Gets the coloured copy of the input of a rendered page
    /// </summary>
    /// <param name="page">The rendered page</param>
    /// <returns>The coloured copy</returns>
    internal static IElement GetInputCopy(IRenderedComponent<Home> page)
    {
        return FindInputCopy(page.FindAll(ColumnSelector).ToList());
    }

    /// <summary>
    /// Gets the output element of a rendered page
    /// </summary>
    /// <param name="page">The rendered page</param>
    /// <returns>The output element</returns>
    internal static IElement GetOutput(IRenderedComponent<Home> page)
    {
        return page.Find("#playground-output");
    }

    /// <summary>
    /// Gets the output text of a rendered page
    /// </summary>
    /// <param name="page">The rendered page</param>
    /// <returns>The output text</returns>
    internal static string GetOutputText(IRenderedComponent<Home> page)
    {
        return GetOutput(page).TextContent;
    }

    /// <summary>
    /// Gets the category of every character of an element's text content, in order
    /// </summary>
    /// <param name="root">The element that shows highlighted text</param>
    /// <returns>One category per character of <paramref name="root"/>'s text content</returns>
    internal static List<string> GetCharacterCategories(IElement root)
    {
        var categories = new List<string>();

        foreach (var text in root.GetDescendants().OfType<IText>())
        {
            var category = GetCategory(text, root);

            categories.AddRange(Enumerable.Repeat(category, text.Data.Length));
        }

        return categories;
    }

    /// <summary>
    /// Gets the highlighted segments of an element's text: each maximal run of non-whitespace characters of one category,
    /// as <c>"&lt;category&gt; &lt;text&gt;"</c>. Whitespace ends a segment and is not reported
    /// </summary>
    /// <param name="root">The element that shows highlighted text</param>
    /// <returns>The segments in text order</returns>
    internal static List<string> GetSegments(IElement root)
    {
        var text = root.TextContent;
        var categories = GetCharacterCategories(root);
        var segments = new List<string>();
        var current = new StringBuilder();
        string currentCategory = null;

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];

            if (char.IsWhiteSpace(character) || categories[index] != currentCategory)
            {
                if (current.Length > 0)
                {
                    segments.Add($"{currentCategory} {current}");
                    current.Clear();
                }

                currentCategory = char.IsWhiteSpace(character) ? null : categories[index];
            }

            if (char.IsWhiteSpace(character) == false)
            {
                current.Append(character);
            }
        }

        if (current.Length > 0)
        {
            segments.Add($"{currentCategory} {current}");
        }

        return segments;
    }

    /// <summary>
    /// Gets the categories of the characters of one occurrence of a text inside an element's text content
    /// </summary>
    /// <param name="root">The element that shows highlighted text</param>
    /// <param name="offset">Offset of the first character in <paramref name="root"/>'s text content</param>
    /// <param name="length">Number of characters</param>
    /// <returns>The distinct categories of the non-whitespace characters of the range</returns>
    internal static List<string> GetCategoriesOfRange(IElement root, int offset, int length)
    {
        var text = root.TextContent;
        var categories = GetCharacterCategories(root);

        return Enumerable.Range(offset, length)
                         .Where(index => char.IsWhiteSpace(text[index]) == false)
                         .Select(index => categories[index])
                         .Distinct()
                         .ToList();
    }

    /// <summary>
    /// Gets the error marks inside an element, each as <c>"&lt;offset&gt;:&lt;marked text&gt;"</c>, where the offset is the
    /// position of the mark in the element's text content
    /// </summary>
    /// <param name="root">The element that holds the marks</param>
    /// <returns>The marks in text order</returns>
    internal static List<string> GetMarks(IElement root)
    {
        var marks = new List<string>();
        var offset = 0;

        foreach (var node in root.GetDescendants())
        {
            if (node is IText text)
            {
                offset += text.Data.Length;
            }
            else if (node is IElement element && element.ClassList.Contains("playground-mark"))
            {
                marks.Add($"{offset}:{element.TextContent}");
            }
        }

        return marks;
    }

    /// <summary>
    /// Gets the lines of the message box
    /// </summary>
    /// <param name="page">The rendered page</param>
    /// <returns>The text of each line</returns>
    internal static List<string> GetMessageLines(IRenderedComponent<Home> page)
    {
        var box = page.Find(".playground-message");

        return box.Children.Select(line => line.TextContent.Trim()).ToList();
    }

    /// <summary>
    /// Enters a text into the input and presses "Format"
    /// </summary>
    /// <param name="page">The rendered page</param>
    /// <param name="input">The text to format</param>
    internal static void Format(IRenderedComponent<Home> page, string input)
    {
        page.Find("#playground-input").Input(input);
        page.Find("button").Click();
    }

    /// <summary>
    /// Finds the coloured copy of the input: the one subtree of the input's column hidden from assistive technology
    /// </summary>
    /// <param name="columns">The page's columns</param>
    /// <returns>The coloured copy</returns>
    private static IElement FindInputCopy(List<IElement> columns)
    {
        Assert.HasCount(2, columns, "The page must show two columns.");

        var hidden = columns[0].QuerySelectorAll("[aria-hidden='true']")
                               .Where(element => element.ParentElement?.Closest("[aria-hidden='true']") == null)
                               .ToList();

        Assert.HasCount(1, hidden, "The input's column must hold exactly one coloured copy hidden from assistive technology.");

        return hidden[0];
    }

    /// <summary>
    /// Gets the category of a text node: the nearest category element around it, up to the root
    /// </summary>
    /// <param name="text">The text node</param>
    /// <param name="root">The element that shows highlighted text</param>
    /// <returns>The category</returns>
    private static string GetCategory(INode text, IElement root)
    {
        for (var element = text.ParentElement; element != null; element = element.ParentElement)
        {
            var category = element.ClassList.Where(name => name.StartsWith(CategoryClassPrefix, System.StringComparison.Ordinal))
                                            .Select(name => name.Substring(CategoryClassPrefix.Length))
                                            .FirstOrDefault(Categories.Contains);

            if (category != null)
            {
                return category;
            }

            if (element == root)
            {
                break;
            }
        }

        return Normal;
    }

    #endregion // Methods
}