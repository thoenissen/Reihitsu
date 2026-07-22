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
    /// Determines whether a trivia is a preprocessor directive or disabled (conditionally compiled out) text.
    /// Such trivia carries conditional-compilation meaning that must not be silently dropped or joined
    /// mid-line by a formatting rewrite
    /// </summary>
    /// <param name="trivia">The trivia to check</param>
    /// <returns><see langword="true"/> if the trivia is a directive or disabled text; otherwise, <see langword="false"/></returns>
    public static bool IsDirectiveOrDisabledTextTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsDirective || trivia.IsKind(SyntaxKind.DisabledTextTrivia);
    }

    /// <summary>
    /// Finds the index of the first trivia that is neither whitespace nor an end-of-line marker
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns>The index of the first significant trivia, or -1 when none exists</returns>
    public static int FindFirstSignificantTriviaIndex(SyntaxTriviaList triviaList)
    {
        for (var triviaIndex = 0; triviaIndex < triviaList.Count; triviaIndex++)
        {
            var trivia = triviaList[triviaIndex];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                return triviaIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Determines whether a trivia list contains a comment, preprocessor directive, or disabled text
    /// that prevents adjacent tokens from being safely joined onto one line
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if the trivia prevents joining; otherwise, <see langword="false"/></returns>
    public static bool ContainsUnjoinableTrivia(SyntaxTriviaList triviaList)
    {
        return triviaList.Any(static trivia => IsCommentTrivia(trivia)
                                               || IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Determines whether collapsing <paramref name="movedToken"/> onto the line that ends with
    /// <paramref name="anchorToken"/> would cross trivia that must keep its own line — a comment (including a
    /// documentation comment), a preprocessor directive, or disabled text sitting in the join gap. The formatter
    /// refuses that collapse, so analyzers and code fixes call this shared predicate to stay in lock-step and never
    /// flag (or offer a no-op fix for) a shape the formatter will not reshape
    /// </summary>
    /// <param name="anchorToken">The token whose trailing trivia the join would consume</param>
    /// <param name="movedToken">The token whose leading trivia the join would consume</param>
    /// <returns><see langword="true"/> if the join gap contains unjoinable trivia; otherwise, <see langword="false"/></returns>
    public static bool WouldJoinAcrossUnjoinableTrivia(SyntaxToken anchorToken,
                                                       SyntaxToken movedToken)
    {
        return ContainsUnjoinableTrivia(anchorToken.TrailingTrivia)
               || ContainsUnjoinableTrivia(movedToken.LeadingTrivia);
    }

    /// <summary>
    /// Determines whether the specified position falls inside a comment or preprocessor-disabled text
    /// interior. The formatter never rewrites that content, and it may be semantically meaningful (for
    /// example, aligned example output or deliberately preserved inactive code), so violations there are
    /// exempt. Preprocessor directives themselves (for example the <c>#pragma</c> keyword line) are not
    /// included, since the formatter can rewrite that content
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="position">Position to inspect</param>
    /// <returns><see langword="true"/> if the position is inside a comment or disabled-text interior; otherwise, <see langword="false"/></returns>
    public static bool IsInsideCommentOrDisabledText(SyntaxNode root, int position)
    {
        var trivia = root.FindTrivia(position);

        return IsCommentTrivia(trivia)
               || trivia.IsKind(SyntaxKind.DisabledTextTrivia);
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
    /// Determines whether the last non-whitespace, non-end-of-line trivia in the specified sequence is a
    /// preprocessor directive
    /// </summary>
    /// <param name="trivia">
    /// The trivia sequence to inspect, typically the trivia immediately preceding a
    /// statement or comment
    /// </param>
    /// <returns><see langword="true"/> if the sequence is immediately preceded by a preprocessor directive</returns>
    public static bool IsPrecededByDirective(IEnumerable<SyntaxTrivia> trivia)
    {
        var lastContentTrivia = trivia.LastOrDefault(candidate => candidate.IsKind(SyntaxKind.WhitespaceTrivia) == false
                                                                  && candidate.IsKind(SyntaxKind.EndOfLineTrivia) == false);

        return lastContentTrivia is { IsDirective: true };
    }

    /// <summary>
    /// Determines whether the first non-whitespace, non-end-of-line trivia in the specified sequence is a
    /// preprocessor directive
    /// </summary>
    /// <param name="trivia">
    /// The trivia sequence to inspect, typically the trivia immediately following a
    /// statement
    /// </param>
    /// <returns><see langword="true"/> if the sequence is immediately followed by a preprocessor directive</returns>
    public static bool IsFollowedByDirective(IEnumerable<SyntaxTrivia> trivia)
    {
        var firstContentTrivia = trivia.FirstOrDefault(candidate => candidate.IsKind(SyntaxKind.WhitespaceTrivia) == false
                                                                    && candidate.IsKind(SyntaxKind.EndOfLineTrivia) == false);

        return firstContentTrivia is { IsDirective: true };
    }

    /// <summary>
    /// Finds the trivia index immediately after the last preprocessor directive in the specified leading
    /// trivia, or index 0 when no directive is present
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia to scan for directives</param>
    /// <returns>The trivia index at which content following the leading directives begins</returns>
    public static int FindIndexAfterLeadingDirectives(SyntaxTriviaList leadingTrivia)
    {
        var insertIndex = 0;

        for (var triviaIndex = 0; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            if (leadingTrivia[triviaIndex].IsDirective)
            {
                insertIndex = triviaIndex + 1;
            }
        }

        return insertIndex;
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