using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Syntax rewriter that collapses sequences of three or more consecutive blank lines
/// to a single blank line.
/// </summary>
internal sealed class BlankLineCollapser : CSharpSyntaxRewriter
{
    #region Methods

    /// <summary>
    /// Collapses sequences of three or more consecutive blank lines in the trivia list to a single blank line.
    /// </summary>
    /// <param name="trivia">The trivia list to process.</param>
    /// <returns>The trivia list with excessive blank lines collapsed.</returns>
    private static SyntaxTriviaList CollapseBlankLinesInTrivia(SyntaxTriviaList trivia)
    {
        // Parse trivia into lines (each line ends with EndOfLine, except possibly the last)
        var lines = new List<List<SyntaxTrivia>>();
        var currentLine = new List<SyntaxTrivia>();

        foreach (var t in trivia)
        {
            currentLine.Add(t);

            if (t.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                lines.Add(currentLine);
                currentLine = new List<SyntaxTrivia>();
            }
        }

        if (currentLine.Count > 0)
        {
            lines.Add(currentLine);
        }

        // Process: collapse 3+ consecutive blank lines to 1
        var result = new List<SyntaxTrivia>();
        var blankLineBuffer = new List<List<SyntaxTrivia>>();

        foreach (var line in lines)
        {
            if (IsBlankLine(line))
            {
                blankLineBuffer.Add(line);
            }
            else
            {
                FlushBlankLines(blankLineBuffer, result);
                blankLineBuffer.Clear();
                result.AddRange(line);
            }
        }

        FlushBlankLines(blankLineBuffer, result);

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Flushes buffered blank lines into the result list, collapsing three or more to one.
    /// </summary>
    /// <param name="buffer">The buffered blank lines.</param>
    /// <param name="result">The result trivia list to append to.</param>
    private static void FlushBlankLines(List<List<SyntaxTrivia>> buffer, List<SyntaxTrivia> result)
    {
        if (buffer.Count >= 3)
        {
            // Collapse to 1 blank line: keep the first blank line only
            result.AddRange(buffer[0]);
        }
        else
        {
            foreach (var line in buffer)
            {
                result.AddRange(line);
            }
        }
    }

    /// <summary>
    /// Determines whether the specified list of trivia constitutes a blank line.
    /// </summary>
    /// <param name="line">The list of trivia representing a single line.</param>
    /// <returns><see langword="true"/> if the line contains only whitespace and an end-of-line.</returns>
    private static bool IsBlankLine(List<SyntaxTrivia> line)
    {
        var hasEndOfLine = false;

        foreach (var t in line)
        {
            var kind = t.Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                hasEndOfLine = true;
            }
            else if (kind != SyntaxKind.WhitespaceTrivia)
            {
                return false;
            }
        }

        return hasEndOfLine;
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        token = base.VisitToken(token);

        var leading = token.LeadingTrivia;

        if (leading.Count < 2)
        {
            return token;
        }

        var collapsed = CollapseBlankLinesInTrivia(leading);

        if (collapsed.Count != leading.Count)
        {
            return token.WithLeadingTrivia(collapsed);
        }

        return token;
    }

    #endregion // CSharpSyntaxRewriter
}