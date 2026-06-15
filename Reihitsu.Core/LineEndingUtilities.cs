using System;
using System.Collections.Generic;
using System.Linq;

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
        var endOfLines = node.DescendantTrivia(descendIntoTrivia: true)
                             .Where(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                             .Select(static trivia => trivia.ToString())
                             .ToList();

        if (endOfLines.Count == 0)
        {
            return Environment.NewLine;
        }

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var endOfLine in endOfLines)
        {
            counts[endOfLine] = counts.TryGetValue(endOfLine, out var count)
                                    ? count + 1
                                    : 1;
        }

        var predominantCount = counts.Values.Max();

        foreach (var endOfLine in endOfLines)
        {
            if (counts[endOfLine] == predominantCount)
            {
                return endOfLine;
            }
        }

        return Environment.NewLine;
    }

    #endregion // Methods
}