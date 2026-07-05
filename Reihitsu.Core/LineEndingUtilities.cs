using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for detecting line endings in syntax nodes
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

        if (carriageReturnLineFeedCount == 0 && lineFeedCount == 0)
        {
            return Environment.NewLine;
        }

        return carriageReturnLineFeedCount >= lineFeedCount
                   ? "\r\n"
                   : "\n";
    }

    #endregion // Methods
}