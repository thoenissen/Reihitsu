using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for trivia analysis and rewrites
/// </summary>
public static class SyntaxTriviaUtilities
{
    #region Methods

    /// <summary>
    /// Extracts the indentation trivia that follows the last end-of-line in a trivia list
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia from which indentation should be extracted</param>
    /// <returns>Indentation trivia for the current line</returns>
    public static SyntaxTriviaList GetLineIndentationTrivia(SyntaxTriviaList leadingTrivia)
    {
        var lastEndOfLineIndex = -1;

        for (var index = 0; index < leadingTrivia.Count; index++)
        {
            if (leadingTrivia[index].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                lastEndOfLineIndex = index;
            }
        }

        var indentation = new List<SyntaxTrivia>();
        var startIndex = lastEndOfLineIndex >= 0 ? lastEndOfLineIndex + 1 : 0;

        for (var index = startIndex; index < leadingTrivia.Count; index++)
        {
            var trivia = leadingTrivia[index];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                indentation.Add(trivia);

                continue;
            }

            break;
        }

        return SyntaxFactory.TriviaList(indentation);
    }

    #endregion // Methods
}