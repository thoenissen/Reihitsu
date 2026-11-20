using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for line endings: the one predicate for what terminates a line, which every component that scans text
/// for line breaks uses, the one definition of the trivia and tokens that carry a line break in a syntax tree, the
/// detection of the predominant end-of-line sequence of a syntax node, and the one rewrite of a node's line breaks to a
/// line ending, which the formatter's line-ending phase and the mixed-line-ending code fix share
/// </summary>
public static class LineEndingUtilities
{
    #region Methods

    /// <summary>
    /// Gets every trivia of a node that carries a line break: the end-of-line trivia between tokens, including the trivia
    /// that ends a directive and the trivia inside structured trivia such as documentation comments. Together with
    /// <see cref="GetLineBreakTokens"/>, this is the one definition of the line breaks the line-ending detection, the
    /// mixed-line-ending analyzer, and the line-ending normalization count, report, and rewrite; line breaks inside
    /// literals, comments that are not documentation comments, and disabled text are not carried by either
    /// </summary>
    /// <param name="node">The syntax node to inspect</param>
    /// <returns>The end-of-line trivia of the node, in document order</returns>
    public static IEnumerable<SyntaxTrivia> GetLineBreakTrivia(SyntaxNode node)
    {
        return node.DescendantTrivia(descendIntoTrivia: true)
                   .Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Gets every token of a node that carries a line break: the newline tokens of documentation comment XML, in its
    /// text, CDATA sections, XML comments, and attribute values. Together with <see cref="GetLineBreakTrivia"/>, this is
    /// the one definition of the line breaks the line-ending detection, the mixed-line-ending analyzer, and the
    /// line-ending normalization count, report, and rewrite
    /// </summary>
    /// <param name="node">The syntax node to inspect</param>
    /// <returns>The documentation newline tokens of the node, in document order</returns>
    public static IEnumerable<SyntaxToken> GetLineBreakTokens(SyntaxNode node)
    {
        return node.DescendantTokens(descendIntoTrivia: true)
                   .Where(token => token.IsKind(SyntaxKind.XmlTextLiteralNewLineToken));
    }

    /// <summary>
    /// Detects the predominant end-of-line sequence in the syntax node, counting the line breaks of
    /// <see cref="GetLineBreakTrivia"/> and <see cref="GetLineBreakTokens"/>: a carriage return followed by a line feed
    /// counts for "\r\n", every single-character terminator for "\n", and a tie goes to "\r\n".
    /// Falls back to <see cref="Environment.NewLine"/> if the node carries no line break
    /// </summary>
    /// <param name="node">The syntax node to analyze</param>
    /// <returns>The detected end-of-line sequence</returns>
    public static string DetectEndOfLine(SyntaxNode node)
    {
        var carriageReturnLineFeedCount = 0;
        var lineFeedCount = 0;

        foreach (var trivia in GetLineBreakTrivia(node))
        {
            // End-of-line trivia is either "\r\n" (length 2) or a single-character terminator (length 1),
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

        foreach (var token in GetLineBreakTokens(node))
        {
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
    /// Rewrites every line break of a node that differs from the given line ending to that line ending: documentation XML
    /// newline tokens (<see cref="GetLineBreakTokens"/>) first, then end-of-line trivia (<see cref="GetLineBreakTrivia"/>),
    /// including the trivia that ends a directive. Line breaks inside literals, other comments, and disabled text are not
    /// trivia or tokens of these kinds and keep their characters. The method applies no other check, so it rewrites every
    /// node it is given. Without a line break to rewrite, the input node itself is returned
    /// </summary>
    /// <param name="node">The syntax node to normalize</param>
    /// <param name="endOfLine">The line ending every line break should have (<see cref="DetectEndOfLine"/>)</param>
    /// <returns>The node with every line break rewritten to <paramref name="endOfLine"/>, or <paramref name="node"/> itself when none differs</returns>
    public static SyntaxNode NormalizeLineEndings(SyntaxNode node, string endOfLine)
    {
        var documentationEndOfLinesToReplace = GetLineBreakTokens(node).Where(token => token.Text != endOfLine)
                                                                       .ToArray();
        var normalizedNode = documentationEndOfLinesToReplace.Length == 0
                                 ? node
                                 : node.ReplaceTokens(documentationEndOfLinesToReplace,
                                                      (original, _) => original.CopyAnnotationsTo(SyntaxFactory.XmlTextNewLine(original.LeadingTrivia,
                                                                                                                               endOfLine,
                                                                                                                               original.ValueText,
                                                                                                                               original.TrailingTrivia)));
        var endOfLinesToReplace = GetLineBreakTrivia(normalizedNode).Where(trivia => trivia.ToString() != endOfLine)
                                                                    .ToArray();

        if (endOfLinesToReplace.Length == 0)
        {
            return normalizedNode;
        }

        return normalizedNode.ReplaceTrivia(endOfLinesToReplace, (_, _) => SyntaxFactory.EndOfLine(endOfLine));
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
    /// Determines whether a span of a text holds a line break other than the given line ending, such as an LF in a CRLF
    /// document or a lone carriage return: a fix that copies the span as written would keep that line break, so it is
    /// withheld for such a span (SHARED-FIX-008)
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="span">The span to inspect; a line break counts when it starts inside the span</param>
    /// <param name="endOfLine">The line ending every line break should have (<see cref="DetectEndOfLine"/>)</param>
    /// <returns><see langword="true"/> when the span holds another line break; otherwise, <see langword="false"/></returns>
    public static bool HoldsOtherLineBreak(SourceText text, TextSpan span, string endOfLine)
    {
        for (var lineNumber = text.Lines.GetLineFromPosition(span.Start).LineNumber; lineNumber < text.Lines.Count && text.Lines[lineNumber].End < span.End; lineNumber++)
        {
            var line = text.Lines[lineNumber];

            if (line.End >= span.Start
                && line.EndIncludingLineBreak > line.End
                && text.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak)) != endOfLine)
            {
                return true;
            }
        }

        return false;
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