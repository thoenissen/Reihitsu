using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for line endings: the one predicate for what terminates a line, which every component that scans text
/// for line breaks uses, and the detection of the predominant end-of-line sequence of a syntax node
/// </summary>
public static class LineEndingUtilities
{
    #region Methods

    /// <summary>
    /// Detects the predominant end-of-line sequence in the syntax node.
    /// Falls back to <see cref="Environment.NewLine"/> if no end-of-line trivia is found
    /// </summary>
    /// <param name="node">The syntax node to analyze</param>
    /// <returns>The detected end-of-line sequence</returns>
    public static string DetectEndOfLine(SyntaxNode node)
    {
        var carriageReturnLineFeedCount = 0;
        var lineFeedCount = 0;

        foreach (var trivia in node.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                continue;
            }

            // End-of-line trivia is either "\r\n" (length 2) or a single "\n"/"\r" (length 1),
            // two counters are enough to determine the predominant sequence without materializing strings.
            if (trivia.Span.Length >= 2)
            {
                carriageReturnLineFeedCount++;
            }
            else
            {
                lineFeedCount++;
            }
        }

        foreach (var token in node.DescendantTokens(descendIntoTrivia: true))
        {
            if (token.IsKind(SyntaxKind.XmlTextLiteralNewLineToken) == false)
            {
                continue;
            }

            if (token.Text.Length >= 2)
            {
                carriageReturnLineFeedCount++;
            }
            else
            {
                lineFeedCount++;
            }
        }

        if (carriageReturnLineFeedCount == 0 && lineFeedCount == 0)
        {
            return Environment.NewLine;
        }

        return carriageReturnLineFeedCount >= lineFeedCount
                   ? "\r\n"
                   : "\n";
    }

    /// <summary>
    /// Determines whether a character terminates a line. C# knows five line terminators: carriage return, line feed,
    /// next line (U+0085), line separator (U+2028), and paragraph separator (U+2029); Roslyn's line table breaks a text
    /// at each of them, so a component that counts lines in the text of a trivia has to count the same characters to
    /// land on the line the table numbers
    /// </summary>
    /// <param name="character">Character</param>
    /// <returns><see langword="true"/> when the character terminates a line; otherwise, <see langword="false"/></returns>
    public static bool IsLineTerminator(char character)
    {
        return character is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029';
    }

    /// <summary>
    /// Gets the length of the line terminator that starts at the specified index: two for a carriage return followed by a
    /// line feed, one for any other line terminator (<see cref="IsLineTerminator"/>), and zero when no line terminator
    /// starts there
    /// </summary>
    /// <param name="text">The text to inspect</param>
    /// <param name="index">The index to inspect</param>
    /// <returns>The length of the line terminator; otherwise, <c>0</c></returns>
    public static int GetLineTerminatorLength(string text, int index)
    {
        if (IsLineTerminator(text[index]) == false)
        {
            return 0;
        }

        return text[index] == '\r'
               && index + 1 < text.Length
               && text[index + 1] == '\n'
                   ? 2
                   : 1;
    }

    /// <summary>
    /// Counts the line terminators in a text, a carriage return followed by a line feed counting once
    /// </summary>
    /// <param name="text">The text to inspect</param>
    /// <returns>The number of line terminators</returns>
    public static int CountLineTerminators(string text)
    {
        var count = 0;
        var index = 0;

        while (index < text.Length)
        {
            var length = GetLineTerminatorLength(text, index);

            if (length == 0)
            {
                index++;

                continue;
            }

            count++;
            index += length;
        }

        return count;
    }

    /// <summary>
    /// Splits a text into its lines at every line terminator (<see cref="GetLineTerminatorLength"/>); the terminators are
    /// not part of the lines, and a text that ends with a terminator ends with an empty line
    /// </summary>
    /// <param name="text">The text to split</param>
    /// <returns>The lines of the text</returns>
    public static List<string> SplitLines(string text)
    {
        var lines = new List<string>();
        var lineStart = 0;
        var index = 0;

        while (index < text.Length)
        {
            var length = GetLineTerminatorLength(text, index);

            if (length == 0)
            {
                index++;

                continue;
            }

            lines.Add(text.Substring(lineStart, index - lineStart));

            index += length;
            lineStart = index;
        }

        lines.Add(text.Substring(lineStart));

        return lines;
    }

    /// <summary>
    /// Determines whether a text holds a line terminator
    /// </summary>
    /// <param name="text">The text to inspect</param>
    /// <returns><see langword="true"/> when the text holds a line terminator; otherwise, <see langword="false"/></returns>
    public static bool ContainsLineTerminator(string text)
    {
        return text.Any(IsLineTerminator);
    }

    /// <summary>
    /// Determines whether a text ends with a line terminator
    /// </summary>
    /// <param name="text">The text to inspect</param>
    /// <returns><see langword="true"/> when the last character of the text terminates a line; otherwise, <see langword="false"/></returns>
    public static bool EndsWithLineTerminator(string text)
    {
        return text.Length > 0
               && IsLineTerminator(text[text.Length - 1]);
    }

    #endregion // Methods
}