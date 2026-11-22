using System;
using System.IO;
using System.Linq;
using System.Text;

using AngleSharp;
using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Dom;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Applies the Playground's stylesheet <c>wwwroot/css/app.css</c> to rendered markup, so a test can read the style an
/// element computes to in the light or the dark colour scheme. Media rules other than <c>prefers-color-scheme</c> are left
/// out, which gives the wide layout
/// </summary>
internal static class PlaygroundStyles
{
    #region Methods

    /// <summary>
    /// Reads the Playground's stylesheet
    /// </summary>
    /// <returns>The stylesheet's text</returns>
    internal static string ReadStyleSheet()
    {
        return File.ReadAllText(Path.Combine(FindPlaygroundDirectory(), "wwwroot", "css", "app.css"), Encoding.UTF8);
    }

    /// <summary>
    /// Reads the Playground's host page
    /// </summary>
    /// <returns>The host page's text</returns>
    internal static string ReadHostPage()
    {
        return File.ReadAllText(Path.Combine(FindPlaygroundDirectory(), "wwwroot", "index.html"), Encoding.UTF8);
    }

    /// <summary>
    /// Gets the style rules of the stylesheet whose selector names a class, in both colour schemes
    /// </summary>
    /// <param name="className">The class name, without the dot</param>
    /// <returns>The text of each matching rule</returns>
    internal static string[] GetRulesNaming(string className)
    {
        var sheet = new CssParser().ParseStyleSheet(ReadStyleSheet());

        return sheet.Rules
                    .SelectMany(rule => rule is ICssMediaRule media ? media.Rules.AsEnumerable() : [rule])
                    .OfType<ICssStyleRule>()
                    .Where(rule => rule.SelectorText.Contains("." + className, StringComparison.Ordinal))
                    .Select(rule => rule.CssText)
                    .ToArray();
    }

    /// <summary>
    /// Creates a styled document from rendered markup
    /// </summary>
    /// <param name="markup">The rendered markup</param>
    /// <param name="dark">Whether the dark colour scheme applies</param>
    /// <returns>The document, whose default view computes styles</returns>
    internal static IDocument CreateDocument(string markup, bool dark)
    {
        var html = $"<!DOCTYPE html><html><head><style>{CreateSchemeStyleSheet(dark)}</style></head><body>{markup}</body></html>";
        var device = new DefaultRenderDevice
                     {
                         DeviceWidth = 1280,
                         DeviceHeight = 800,
                         ViewPortWidth = 1280,
                         ViewPortHeight = 800
                     };
        var context = BrowsingContext.New(Configuration.Default.WithCss().WithRenderDevice(device));

        return context.OpenAsync(request => request.Content(html)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a computed property of an element
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="element">The element</param>
    /// <param name="property">The CSS property name</param>
    /// <returns>The computed value</returns>
    internal static string GetComputed(IDocument document, IElement element, string property)
    {
        return document.DefaultView.GetComputedStyle(element).GetPropertyValue(property);
    }

    /// <summary>
    /// Creates the stylesheet of one colour scheme: the rules of a <c>prefers-color-scheme</c> media rule of that scheme
    /// in place of the media rule, without the other scheme's and every other media rule
    /// </summary>
    /// <param name="dark">Whether the dark colour scheme applies</param>
    /// <returns>The stylesheet's text</returns>
    private static string CreateSchemeStyleSheet(bool dark)
    {
        var sheet = new CssParser().ParseStyleSheet(ReadStyleSheet());
        var builder = new StringBuilder();
        var scheme = dark ? "dark" : "light";

        foreach (var rule in sheet.Rules)
        {
            if (rule is ICssMediaRule media)
            {
                var condition = media.ConditionText.Replace(" ", string.Empty, StringComparison.Ordinal);

                if (condition.Contains("prefers-color-scheme:" + scheme, StringComparison.Ordinal))
                {
                    foreach (var inner in media.Rules)
                    {
                        builder.AppendLine(inner.CssText);
                    }
                }
            }
            else
            {
                builder.AppendLine(rule.CssText);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Finds the Playground's project directory by walking up from the test assembly's location to the solution root
    /// </summary>
    /// <returns>The absolute path of <c>src/Reihitsu.Playground</c></returns>
    private static string FindPlaygroundDirectory()
    {
        var directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "Reihitsu.sln")))
            {
                return Path.Combine(directory, "src", "Reihitsu.Playground");
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new InvalidOperationException("Could not find the solution root (Reihitsu.sln).");
    }

    #endregion // Methods
}