using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using AngleSharp.Dom;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Decides from the computed style of a styled document whether the width of a box follows the width of its text, by the CSS
/// sizing rules of the layouts the Playground uses: block, flex, and grid. A box's width follows its content when it shrinks to
/// fit, when it is a flex item that is not stretched across a column, or when it is a grid item whose column track is sized
/// from its items or has an automatic minimum the item fills with its content; size containment and a fixed width or maximum
/// width keep a box's content out of the size of every box around it
/// </summary>
internal static class PlaygroundWidths
{
    #region Methods

    /// <summary>
    /// Finds the box, from an element up to the page, whose width follows the width of the element's text: a box whose width is
    /// sized from its content, while no box from it down to the element keeps the text out of that content size
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="element">The element</param>
    /// <returns>The box and the reason its width follows its content; <see langword="null"/> when there is none</returns>
    internal static string FindWidthFromText(IDocument document, IElement element)
    {
        var path = new List<IElement>();

        for (var current = element; current != null && current.TagName != "HTML"; current = current.ParentElement)
        {
            path.Add(current);

            if (KeepsContentOutOfItsSize(document, current))
            {
                return null;
            }

            var reason = GetContentSizing(document, current);

            if (reason != null)
            {
                return $"{Describe(current)} {reason}, and no box from it down to the pane ({string.Join(" < ", path.Select(Describe))}) keeps the pane's text out of that size";
            }
        }

        return null;
    }

    /// <summary>
    /// Splits a computed <c>grid-template-columns</c> value into its tracks, expanding <c>repeat(n, …)</c> with a count
    /// </summary>
    /// <param name="value">The computed value</param>
    /// <returns>The tracks; empty when there is no explicit track</returns>
    internal static List<string> GetTracks(string value)
    {
        var tracks = new List<string>();

        foreach (var part in SplitTopLevel(value))
        {
            if (part.StartsWith("repeat(", StringComparison.Ordinal))
            {
                var arguments = part.Substring(7, part.Length - 8);
                var comma = arguments.IndexOf(',');
                var count = int.Parse(arguments.Substring(0, comma).Trim(), CultureInfo.InvariantCulture);
                var repeated = SplitTopLevel(arguments.Substring(comma + 1));

                for (var index = 0; index < count; index++)
                {
                    tracks.AddRange(repeated);
                }
            }
            else if (part != "none")
            {
                tracks.Add(part);
            }
        }

        return tracks;
    }

    /// <summary>
    /// Checks whether the content of a box is left out of its min-content and max-content contributions: its <c>contain</c>
    /// includes size containment, or its <c>width</c> or <c>max-width</c> is a fixed length
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="element">The box</param>
    /// <returns><see langword="true"/> when the box's contributions do not follow its content</returns>
    private static bool KeepsContentOutOfItsSize(IDocument document, IElement element)
    {
        var contain = PlaygroundStyles.GetComputed(document, element, "contain").Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return contain.Any(value => value is "size" or "inline-size" or "strict")
               || IsFixedLength(PlaygroundStyles.GetComputed(document, element, "width"))
               || IsFixedLength(PlaygroundStyles.GetComputed(document, element, "max-width"));
    }

    /// <summary>
    /// Gets why the width of a box is sized from its content: by its own width or display, or by the layout of its parent
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="element">The box</param>
    /// <returns>The reason; <see langword="null"/> when the box's width does not follow its content</returns>
    private static string GetContentSizing(IDocument document, IElement element)
    {
        var width = PlaygroundStyles.GetComputed(document, element, "width");

        if (width is "min-content" or "max-content" or "fit-content")
        {
            return $"has the width {width}";
        }

        if (width.Length > 0 && width != "auto")
        {
            return null;
        }

        var display = PlaygroundStyles.GetComputed(document, element, "display");

        if (display.StartsWith("inline", StringComparison.Ordinal)
            || display == "table"
            || PlaygroundStyles.GetComputed(document, element, "float") is "left" or "right"
            || PlaygroundStyles.GetComputed(document, element, "position") is "absolute" or "fixed")
        {
            return "shrinks to fit its content";
        }

        var parent = element.ParentElement;

        if (parent == null)
        {
            return null;
        }

        return PlaygroundStyles.GetComputed(document, parent, "display") switch
               {
                   "flex" or "inline-flex" => GetFlexItemSizing(document, element, parent),
                   "grid" or "inline-grid" => GetGridItemSizing(document, element, parent),
                   _ => null
               };
    }

    /// <summary>
    /// Gets why the width of a flex item is sized from its content: an item of a column that is not stretched across it, or an
    /// item of a row whose flex basis or automatic minimum comes from its content
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="element">The flex item</param>
    /// <param name="parent">The flex container</param>
    /// <returns>The reason; <see langword="null"/> when the item's width does not follow its content</returns>
    private static string GetFlexItemSizing(IDocument document, IElement element, IElement parent)
    {
        if (PlaygroundStyles.GetComputed(document, parent, "flex-direction").StartsWith("column", StringComparison.Ordinal))
        {
            var alignment = PlaygroundStyles.GetComputed(document, element, "align-self");

            if (alignment is "" or "auto")
            {
                alignment = PlaygroundStyles.GetComputed(document, parent, "align-items");
            }

            return alignment is "" or "normal" or "stretch"
                       ? null
                       : $"is a flex item aligned {alignment}, which is not stretched across its column";
        }

        return HasContentMinimum(document, element) || IsFixedLength(PlaygroundStyles.GetComputed(document, element, "flex-basis")) == false
                   ? "is an item of a flex row, whose width starts from its content"
                   : null;
    }

    /// <summary>
    /// Gets why the width of an auto-placed grid item is sized from its content: its column track is sized from its items, or
    /// the track has an automatic minimum and the item's automatic minimum is its content
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="element">The grid item</param>
    /// <param name="parent">The grid container</param>
    /// <returns>The reason; <see langword="null"/> when the item's width does not follow its content</returns>
    private static string GetGridItemSizing(IDocument document, IElement element, IElement parent)
    {
        if (PlaygroundStyles.GetComputed(document, element, "grid-column") is not ("" or "auto" or "auto / auto"))
        {
            throw new InvalidOperationException($"{Describe(element)} is placed explicitly; this check reads auto-placed grid items only.");
        }

        var tracks = GetTracks(PlaygroundStyles.GetComputed(document, parent, "grid-template-columns"));
        var track = tracks.Count == 0 ? "auto" : tracks[parent.Children.ToList().IndexOf(element) % tracks.Count];
        var (minimum, maximum) = GetTrackSizing(track);

        if (maximum is "min-content" or "max-content")
        {
            return $"is a grid item in the track '{track}', which is sized from its items";
        }

        if (minimum is "min-content" or "max-content"
            || (minimum == "auto" && (maximum == "auto" || HasContentMinimum(document, element))))
        {
            var minWidth = PlaygroundStyles.GetComputed(document, element, "min-width");

            return $"is a grid item in the track '{track}', whose minimum it fills with its content (its min-width is '{(minWidth.Length == 0 ? "auto" : minWidth)}' and it does not scroll)";
        }

        return null;
    }

    /// <summary>
    /// Checks whether the automatic minimum width of a flex or grid item is its content: its <c>min-width</c> is <c>auto</c>
    /// and it is not a scroll container
    /// </summary>
    /// <param name="document">The styled document</param>
    /// <param name="element">The item</param>
    /// <returns><see langword="true"/> when the item's minimum width follows its content</returns>
    private static bool HasContentMinimum(IDocument document, IElement element)
    {
        return PlaygroundStyles.GetComputed(document, element, "min-width") is "" or "auto"
               && PlaygroundStyles.GetComputed(document, element, "overflow-x") is "" or "visible" or "clip";
    }

    /// <summary>
    /// Splits a value at the spaces outside parentheses
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The parts</returns>
    private static List<string> SplitTopLevel(string value)
    {
        var parts = new List<string>();
        var depth = 0;
        var start = 0;

        for (var index = 0; index <= value.Length; index++)
        {
            if (index == value.Length || (value[index] == ' ' && depth == 0))
            {
                var part = value.Substring(start, index - start).Trim();

                if (part.Length > 0)
                {
                    parts.Add(part);
                }

                start = index + 1;
            }
            else
            {
                depth += value[index] switch
                         {
                             '(' => 1,
                             ')' => -1,
                             _ => 0
                         };
            }
        }

        return parts;
    }

    /// <summary>
    /// Gets the minimum and maximum sizing function of a track: <c>minmax(a, b)</c> is <c>a</c> and <c>b</c>, a flexible
    /// track such as <c>1fr</c> is <c>auto</c> and itself, <c>fit-content(…)</c> is <c>auto</c> and <c>max-content</c>, and
    /// any other track is its own minimum and maximum
    /// </summary>
    /// <param name="track">The track</param>
    /// <returns>The minimum and the maximum sizing function</returns>
    private static (string Minimum, string Maximum) GetTrackSizing(string track)
    {
        if (track.StartsWith("minmax(", StringComparison.Ordinal))
        {
            var arguments = track.Substring(7, track.Length - 8).Split(',', StringSplitOptions.TrimEntries);

            return (arguments[0], arguments[1]);
        }

        if (track.EndsWith("fr", StringComparison.Ordinal))
        {
            return ("auto", track);
        }

        return track.StartsWith("fit-content(", StringComparison.Ordinal) ? ("auto", "max-content") : (track, track);
    }

    /// <summary>
    /// Checks whether a computed size is a fixed length, such as <c>0</c>, <c>0px</c>, or <c>40rem</c>: neither empty,
    /// <c>auto</c>, <c>none</c>, a percentage, nor a content keyword
    /// </summary>
    /// <param name="value">The computed value</param>
    /// <returns><see langword="true"/> when the value is a fixed length</returns>
    private static bool IsFixedLength(string value)
    {
        return value.Length > 0
               && (char.IsDigit(value[0]) || value[0] == '.')
               && value.Contains('%') == false;
    }

    /// <summary>
    /// Describes a box by its tag, its id, and its classes
    /// </summary>
    /// <param name="element">The box</param>
    /// <returns>The description</returns>
    private static string Describe(IElement element)
    {
        var id = element.Id is { Length: > 0 } ? $"#{element.Id}" : string.Empty;
        var classes = string.Concat(element.ClassList.Select(name => $".{name}"));

        return $"{element.TagName.ToLowerInvariant()}{id}{classes}";
    }

    #endregion // Methods
}