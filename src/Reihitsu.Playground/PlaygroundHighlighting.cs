using System.Collections.Generic;

using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Playground;

/// <summary>
/// A text coloured by its syntax tree (PLAY-010) with its error marks (PLAY-014), created by
/// <see cref="PlaygroundHighlighter.Highlight"/>; it keeps what the highlighting of the next version of the text reuses
/// </summary>
public sealed class PlaygroundHighlighting
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="tree">The syntax tree of the text</param>
    /// <param name="categories">The coloured spans in text order, each with its class</param>
    /// <param name="lines">The rendered lines, in text order</param>
    /// <param name="isMarked">Whether the text carries marks, which <paramref name="lines"/> then renders as one line</param>
    /// <param name="content">The content that shows the text</param>
    internal PlaygroundHighlighting(string text, SyntaxTree tree, List<(TextSpan Span, string ClassName)> categories, List<PlaygroundHighlightedLine> lines, bool isMarked, RenderFragment content)
    {
        Text = text;
        Tree = tree;
        Categories = categories;
        Lines = lines;
        IsMarked = isMarked;
        Content = content;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The content that shows the text
    /// </summary>
    public RenderFragment Content { get; }

    /// <summary>
    /// The text
    /// </summary>
    internal string Text { get; }

    /// <summary>
    /// The syntax tree of the text
    /// </summary>
    internal SyntaxTree Tree { get; }

    /// <summary>
    /// The coloured spans in text order, each with its class; text outside them keeps the normal colour
    /// </summary>
    internal List<(TextSpan Span, string ClassName)> Categories { get; }

    /// <summary>
    /// The rendered lines, in text order
    /// </summary>
    internal List<PlaygroundHighlightedLine> Lines { get; }

    /// <summary>
    /// Whether the text carries marks, which <see cref="Lines"/> then renders as one line, so that a mark across a line break
    /// stays one element
    /// </summary>
    internal bool IsMarked { get; }

    #endregion // Properties
}