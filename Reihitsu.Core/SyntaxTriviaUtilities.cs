using System;
using System.Collections.Generic;
using System.Linq;

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
    /// Determines whether a trivia is a comment
    /// </summary>
    /// <param name="trivia">The trivia to check</param>
    /// <returns><see langword="true"/> if the trivia is a comment; otherwise, <see langword="false"/></returns>
    public static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Determines whether the token has a comment directly above its line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a comment is directly above the token; otherwise, <see langword="false"/></returns>
    public static bool HasCommentDirectlyAbove(SyntaxToken token)
    {
        if (token.LeadingTrivia.Any(IsCommentTrivia) == false)
        {
            return false;
        }

        if (token.SyntaxTree == null)
        {
            return true;
        }

        var line = token.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (line <= 0)
        {
            return false;
        }

        var previousLine = token.SyntaxTree.GetText().Lines[line - 1].ToString().Trim();

        return previousLine.StartsWith("//", StringComparison.Ordinal)
               || previousLine.StartsWith("/*", StringComparison.Ordinal)
               || previousLine.StartsWith("*", StringComparison.Ordinal)
               || previousLine.EndsWith("*/", StringComparison.Ordinal);
    }

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