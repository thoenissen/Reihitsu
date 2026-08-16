using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Maps first-on-line tokens (identified by line number) to their desired indentation column.
/// The layout model is the output of the compute phase and input to the apply phase
/// </summary>
internal sealed class LayoutModel
{
    #region Fields

    /// <summary>
    /// Maps line numbers to their desired token layout (column and source)
    /// </summary>
    private readonly Dictionary<int, TokenLayout> _layouts = [];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Number of entries in the layout model
    /// </summary>
    public int Count => _layouts.Count;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Sets the desired layout for a token on the given line.
    /// Later calls override earlier ones (alignment overrides block indentation)
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="layout">The desired layout</param>
    public void Set(int lineNumber, TokenLayout layout)
    {
        _layouts[lineNumber] = layout;
    }

    /// <summary>
    /// Tries to get the layout for the given line number
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="layout">The layout if found</param>
    /// <returns><see langword="true"/> if a layout was found; otherwise, <see langword="false"/></returns>
    public bool TryGetLayout(int lineNumber, out TokenLayout layout)
    {
        return _layouts.TryGetValue(lineNumber, out layout);
    }

    /// <summary>
    /// Captures the current column of every entry, so a later state can be compared against it.
    /// Only the columns are captured: the <see cref="TokenLayout.Source"/> label is a debug aid and
    /// two states that differ only in which contributor last wrote the same column are equivalent
    /// </summary>
    /// <returns>A snapshot mapping line numbers to their currently desired column</returns>
    public Dictionary<int, int> CaptureColumns()
    {
        var columns = new Dictionary<int, int>(_layouts.Count);

        foreach (var entry in _layouts)
        {
            columns[entry.Key] = entry.Value.Column;
        }

        return columns;
    }

    /// <summary>
    /// Determines whether every entry still holds the column it held in the given snapshot
    /// </summary>
    /// <param name="columns">A snapshot previously returned by <see cref="CaptureColumns"/></param>
    /// <returns><see langword="true"/> if no column changed and no entry was added; otherwise, <see langword="false"/></returns>
    public bool MatchesColumns(Dictionary<int, int> columns)
    {
        if (_layouts.Count != columns.Count)
        {
            return false;
        }

        foreach (var entry in _layouts)
        {
            if (columns.TryGetValue(entry.Key, out var column) == false
                || column != entry.Value.Column)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Tries to get the layout for the given token by looking up its line number
    /// </summary>
    /// <param name="token">The syntax token</param>
    /// <param name="layout">The layout if found</param>
    /// <returns><see langword="true"/> if a layout was found; otherwise, <see langword="false"/></returns>
    public bool TryGetLayout(SyntaxToken token, out TokenLayout layout)
    {
        var lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line;

        return _layouts.TryGetValue(lineNumber, out layout);
    }

    /// <summary>
    /// Shifts the column of all existing entries within the specified line range by the given delta
    /// </summary>
    /// <param name="startLine">The inclusive start line</param>
    /// <param name="endLine">The inclusive end line</param>
    /// <param name="delta">The column offset to apply (positive shifts right, negative shifts left)</param>
    /// <param name="source">The source identifier for the shifted entries</param>
    public void ShiftRange(int startLine, int endLine, int delta, string source)
    {
        if (delta == 0)
        {
            return;
        }

        var keysToUpdate = _layouts.Keys
                                   .Where(key => key >= startLine && key <= endLine)
                                   .ToList();

        foreach (var key in keysToUpdate)
        {
            var existing = _layouts[key];
            var newColumn = existing.Column + delta;

            if (newColumn < 0)
            {
                newColumn = 0;
            }

            _layouts[key] = new TokenLayout(newColumn, source);
        }
    }

    #endregion // Methods
}