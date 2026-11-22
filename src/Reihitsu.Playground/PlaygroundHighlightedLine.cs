using Microsoft.AspNetCore.Components;

namespace Reihitsu.Playground;

/// <summary>
/// One rendered line of a highlighted text. The page keys the line's element by this instance, so a line an edit leaves
/// intact keeps its element and only the lines the edit changes are replaced
/// </summary>
internal sealed class PlaygroundHighlightedLine
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="length">The number of characters of the text the line covers, its line break included</param>
    /// <param name="content">The content that shows the line's coloured parts and marks, or <see langword="null"/> for an empty line</param>
    public PlaygroundHighlightedLine(int length, RenderFragment content)
    {
        Length = length;
        Content = content;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The number of characters of the text the line covers, its line break included
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// The content that shows the line's coloured parts and marks, or <see langword="null"/> for an empty line
    /// </summary>
    public RenderFragment Content { get; }

    #endregion // Properties
}