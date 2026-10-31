using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Names, for each physical line of a syntax tree, the position at which the line's text starts: the comment, directive,
/// documentation comment, or disabled text that starts the line, or the token with text that starts it. Whether an
/// element starts its line is the shared line-start fact — SHARED-BLANK-002 for trivia, SHARED-BLANK-008 for tokens — and
/// the line it stands on is the one Roslyn's line table gives its position, so every line terminator counts alike.
/// A line that starts inside text begun on an earlier line, such as the last line of a multi-line comment or a line of a
/// string literal, and a line that holds only whitespace, have no start. The layout computer measures a line's tokens
/// from this start and the rewriter writes the line's column in front of it and nowhere else on the line
/// </summary>
internal sealed class LineStartIndex
{
    #region Fields

    /// <summary>
    /// The index of each tree, built once per tree
    /// </summary>
    private static readonly ConditionalWeakTable<SyntaxTree, LineStartIndex> _indexes = new();

    /// <summary>
    /// The text of the tree
    /// </summary>
    private readonly SourceText _text;

    /// <summary>
    /// Maps each line that has a start to the position of that start
    /// </summary>
    private readonly Dictionary<int, int> _positionsByLine = [];

    /// <summary>
    /// Maps the position of each line start to its line
    /// </summary>
    private readonly Dictionary<int, int> _linesByPosition = [];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tree">The syntax tree</param>
    private LineStartIndex(SyntaxTree tree)
    {
        _text = tree.GetText();

        foreach (var token in tree.GetRoot().DescendantTokens())
        {
            if (token.IsMissing)
            {
                continue;
            }

            var leadingTrivia = token.LeadingTrivia;

            for (var triviaIndex = 0; triviaIndex < leadingTrivia.Count; triviaIndex++)
            {
                var trivia = leadingTrivia[triviaIndex];

                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                    || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    continue;
                }

                if (TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, triviaIndex))
                {
                    Add(trivia.FullSpan.Start);
                }
            }

            if (SyntaxTokenPositionUtilities.StartsLine(token))
            {
                Add(token.SpanStart);
            }
        }
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the index of a syntax tree
    /// </summary>
    /// <param name="tree">The syntax tree</param>
    /// <returns>The index of the tree</returns>
    public static LineStartIndex For(SyntaxTree tree)
    {
        return _indexes.GetValue(tree, static syntaxTree => new LineStartIndex(syntaxTree));
    }

    /// <summary>
    /// Determines whether the text of a line starts at the specified position
    /// </summary>
    /// <param name="position">The position of a token or of a trivia's full span</param>
    /// <param name="lineNumber">The 0-based line whose text starts there</param>
    /// <returns><see langword="true"/> if a line's text starts at the position; otherwise, <see langword="false"/></returns>
    public bool IsLineStart(int position, out int lineNumber)
    {
        return _linesByPosition.TryGetValue(position, out lineNumber);
    }

    /// <summary>
    /// Gets the column at which the text of a line starts
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="column">The column at which the line's text starts</param>
    /// <returns><see langword="true"/> if the line has a start; otherwise, <see langword="false"/></returns>
    public bool TryGetLineStartColumn(int lineNumber, out int column)
    {
        if (_positionsByLine.TryGetValue(lineNumber, out var position) == false)
        {
            column = 0;

            return false;
        }

        column = position - _text.Lines[lineNumber].Start;

        return true;
    }

    /// <summary>
    /// Gets the 0-based line of a position, as Roslyn's line table numbers it
    /// </summary>
    /// <param name="position">The position</param>
    /// <returns>The line number</returns>
    public int GetLine(int position)
    {
        return _text.Lines.GetLineFromPosition(position).LineNumber;
    }

    /// <summary>
    /// Records a line start. A line keeps the first start found on it, so no line gets a second one
    /// </summary>
    /// <param name="position">The position at which the line's text starts</param>
    private void Add(int position)
    {
        var lineNumber = GetLine(position);

        if (_positionsByLine.ContainsKey(lineNumber))
        {
            return;
        }

        _positionsByLine[lineNumber] = position;
        _linesByPosition[position] = lineNumber;
    }

    #endregion // Methods
}