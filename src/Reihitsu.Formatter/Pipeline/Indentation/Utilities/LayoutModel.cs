using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Maps first-on-line tokens (identified by line number) to their desired indentation column.
/// The layout model is the output of the compute phase and input to the apply phase. Lines that a token starts inside
/// an interpolation hole of a multi-line interpolated raw string are placed only by constructs inside that hole; the
/// others keep their column and move with the literal in the raw-string phase
/// </summary>
internal sealed class LayoutModel
{
    #region Fields

    /// <summary>
    /// Maps line numbers to their desired token layout (column and source)
    /// </summary>
    private readonly Dictionary<int, TokenLayout> _layouts = [];

    /// <summary>
    /// Maps each line that a token starts inside an interpolation hole of a multi-line interpolated raw string to
    /// the innermost such hole and to the column the line's first token has before indentation
    /// </summary>
    private readonly Dictionary<int, (InterpolationSyntax Hole, int Column)> _rawStringHoleLines = [];

    /// <summary>
    /// Lines of <see cref="_rawStringHoleLines"/> that a construct inside their hole has placed
    /// </summary>
    private readonly HashSet<int> _alignedRawStringHoleLines = [];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Number of entries in the layout model
    /// </summary>
    public int Count => _layouts.Count;

    /// <summary>
    /// The node whose alignment contributors are writing to the model, or <see langword="null"/> while block
    /// indentation and comment alignment write to it. Only a contributor of a node inside an interpolation hole of a
    /// multi-line interpolated raw string may place a line of that hole
    /// </summary>
    public SyntaxNode Contributor { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Sets the desired layout for a token on the given line.
    /// Later calls override earlier ones (alignment overrides block indentation). A line of an interpolation
    /// hole ignores the write of a contributor of a node outside that hole
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="layout">The desired layout</param>
    public void Set(int lineNumber, TokenLayout layout)
    {
        if (AcceptsWrite(lineNumber))
        {
            _layouts[lineNumber] = layout;
        }
    }

    /// <summary>
    /// Registers a line that a token starts inside an interpolation hole of a multi-line interpolated raw string.
    /// Such a line keeps the column it has, and moves only with the literal, unless a construct inside the hole
    /// places it. Registering a line again replaces the hole, so registering outer holes first leaves the innermost
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="hole">The innermost interpolation hole of a multi-line interpolated raw string around the line</param>
    /// <param name="column">The column of the line's first token</param>
    public void RegisterRawStringHoleLine(int lineNumber, InterpolationSyntax hole, int column)
    {
        _rawStringHoleLines[lineNumber] = (hole, column);
    }

    /// <summary>
    /// Gets the column at which the line's first token will start: the layout's column, or, for a line of an
    /// interpolation hole that no construct inside the hole placed, the column the token already has
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="column">The column if the line has one</param>
    /// <returns><see langword="true"/> if the line has a column; otherwise, <see langword="false"/></returns>
    public bool TryGetPlacedColumn(int lineNumber, out int column)
    {
        if (IsUnplacedRawStringHoleLine(lineNumber, out var holeLine))
        {
            column = holeLine.Column;

            return true;
        }

        if (_layouts.TryGetValue(lineNumber, out var layout))
        {
            column = layout.Column;

            return true;
        }

        column = 0;

        return false;
    }

    /// <summary>
    /// Removes the layout of every line of an interpolation hole that no construct inside the hole placed, so the
    /// line keeps its whitespace and moves only with the literal
    /// </summary>
    public void ReleaseUnplacedRawStringHoleLines()
    {
        foreach (var lineNumber in _rawStringHoleLines.Keys.Where(lineNumber => _alignedRawStringHoleLines.Contains(lineNumber) == false))
        {
            _layouts.Remove(lineNumber);
        }
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
    /// Shifts the column of all existing entries within the specified line range by the given delta. A line of an
    /// interpolation hole is skipped when the contributor shifting the range belongs to a node outside that hole
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
            if (AcceptsWrite(key) == false)
            {
                continue;
            }

            var existing = _layouts[key];
            var newColumn = existing.Column + delta;

            if (newColumn < 0)
            {
                newColumn = 0;
            }

            _layouts[key] = new TokenLayout(newColumn, source);
        }
    }

    /// <summary>
    /// Determines whether the line is a line of an interpolation hole that no construct inside the hole placed
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="holeLine">The hole around the line and the column of its first token</param>
    /// <returns><see langword="true"/> if the line is such a line; otherwise, <see langword="false"/></returns>
    private bool IsUnplacedRawStringHoleLine(int lineNumber, out (InterpolationSyntax Hole, int Column) holeLine)
    {
        return _rawStringHoleLines.TryGetValue(lineNumber, out holeLine)
               && _alignedRawStringHoleLines.Contains(lineNumber) == false;
    }

    /// <summary>
    /// Determines whether a write to the line is accepted. A line outside interpolation holes accepts every write.
    /// A line of a hole accepts the writes of block indentation and comment alignment, whose column only stands in
    /// until a construct inside the hole places the line, and the writes of a contributor of a node inside the hole,
    /// which place the line; it ignores a contributor of a node outside the hole, whose column does not move with
    /// the literal
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <returns><see langword="true"/> if the write is accepted; otherwise, <see langword="false"/></returns>
    private bool AcceptsWrite(int lineNumber)
    {
        if (Contributor == null
            || _rawStringHoleLines.TryGetValue(lineNumber, out var holeLine) == false)
        {
            return true;
        }

        if (holeLine.Hole.Span.Contains(Contributor.Span) == false)
        {
            return false;
        }

        _alignedRawStringHoleLines.Add(lineNumber);

        return true;
    }

    #endregion // Methods
}