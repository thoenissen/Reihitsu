using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.Cleanup;

/// <summary>
/// Final cleanup pass that removes trailing whitespace, collapses consecutive blank lines,
/// removes blank lines after opening braces, and ensures proper end-of-file formatting
/// </summary>
internal static class CleanupPhase
{
    #region Methods

    /// <summary>
    /// Executes the cleanup phase on the given syntax tree
    /// </summary>
    /// <param name="root">The root syntax node to clean up</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cleaned-up syntax node</returns>
    public static SyntaxNode Execute(SyntaxNode root, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return root.ReplaceTokens(root.DescendantTokens(), (original, rewritten) => CleanToken(original, rewritten, cancellationToken));
    }

    /// <summary>
    /// Cleans a single token by processing its leading and trailing trivia
    /// </summary>
    /// <param name="original">The original token from the syntax tree</param>
    /// <param name="rewritten">The rewritten token to clean</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cleaned token</returns>
    private static SyntaxToken CleanToken(SyntaxToken original, SyntaxToken rewritten, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var leading = CleanLeadingTrivia(rewritten.LeadingTrivia, original);
        var trailing = CleanTrailingTrivia(rewritten.TrailingTrivia, original);

        // BOM is preserved automatically because only WhitespaceTrivia and EndOfLineTrivia are removed.
        return rewritten.WithLeadingTrivia(leading)
                        .WithTrailingTrivia(trailing);
    }

    /// <summary>
    /// Cleans the leading trivia of a token by removing trailing whitespace, collapsing blank lines,
    /// removing blank lines after braces, and handling end-of-file formatting
    /// </summary>
    /// <param name="leading">The leading trivia list to clean</param>
    /// <param name="original">The original token</param>
    /// <returns>The cleaned leading trivia list</returns>
    private static SyntaxTriviaList CleanLeadingTrivia(SyntaxTriviaList leading, SyntaxToken original)
    {
        leading = CleanWhitespaceBeforeEndOfLine(leading);
        leading = CollapseConsecutiveEndOfLines(leading);

        if (original.GetPreviousToken().IsKind(SyntaxKind.OpenBraceToken))
        {
            leading = RemoveBlankLinesAfterBrace(leading, original.GetPreviousToken());
        }

        if (original.IsKind(SyntaxKind.EndOfFileToken))
        {
            leading = RemoveTrailingEndOfLineTrivia(leading);
        }

        return leading;
    }

    /// <summary>
    /// Cleans the trailing trivia of a token by removing trailing whitespace, stripping whitespace
    /// at end of line, collapsing blank lines, and handling end-of-file formatting
    /// </summary>
    /// <param name="trailing">The trailing trivia list to clean</param>
    /// <param name="original">The original token</param>
    /// <returns>The cleaned trailing trivia list</returns>
    private static SyntaxTriviaList CleanTrailingTrivia(SyntaxTriviaList trailing, SyntaxToken original)
    {
        trailing = CleanWhitespaceBeforeEndOfLine(trailing);
        trailing = StripTrailingWhitespaceAtEndOfLine(trailing, original);
        trailing = CollapseConsecutiveEndOfLines(trailing);

        if (original.IsKind(SyntaxKind.EndOfFileToken))
        {
            trailing = RemoveTrailingEndOfLineTrivia(trailing);
        }
        else
        {
            var nextToken = original.GetNextToken();

            if (nextToken.IsKind(SyntaxKind.None) || nextToken.IsKind(SyntaxKind.EndOfFileToken))
            {
                trailing = RemoveTrailingEndOfLineTrivia(trailing);
            }
        }

        return trailing;
    }

    /// <summary>
    /// Removes whitespace trivia that immediately precedes end-of-line trivia,
    /// eliminating trailing whitespace and whitespace on blank lines
    /// </summary>
    /// <param name="triviaList">The trivia list to clean</param>
    /// <returns>The cleaned trivia list</returns>
    private static SyntaxTriviaList CleanWhitespaceBeforeEndOfLine(SyntaxTriviaList triviaList)
    {
        if (triviaList.Count == 0)
        {
            return triviaList;
        }

        var result = new List<SyntaxTrivia>(triviaList.Count);
        var changed = false;

        for (var triviaIndex = 0; triviaIndex < triviaList.Count; triviaIndex++)
        {
            if (triviaList[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia)
                && triviaIndex + 1 < triviaList.Count
                && triviaList[triviaIndex + 1].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                changed = true;

                continue;
            }

            result.Add(triviaList[triviaIndex]);
        }

        return changed
                   ? SyntaxFactory.TriviaList(result)
                   : triviaList;
    }

    /// <summary>
    /// Strips trailing <see cref="SyntaxKind.WhitespaceTrivia"/> from the end of a token's trailing trivia
    /// when the line break lives in the next token's leading trivia rather than this token's trailing trivia
    /// </summary>
    /// <param name="trailing">The trailing trivia list (already cleaned)</param>
    /// <param name="originalToken">The original token (used to locate the next token)</param>
    /// <returns>The trailing trivia list with end-of-line trailing whitespace removed</returns>
    private static SyntaxTriviaList StripTrailingWhitespaceAtEndOfLine(SyntaxTriviaList trailing, SyntaxToken originalToken)
    {
        if (trailing.Count == 0 || trailing[trailing.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia) == false)
        {
            return trailing;
        }

        if (trailing.Any(SyntaxKind.EndOfLineTrivia))
        {
            return trailing;
        }

        var nextToken = originalToken.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None))
        {
            return trailing;
        }

        if (nextToken.LeadingTrivia.Count == 0 || nextToken.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return trailing;
        }

        var result = new List<SyntaxTrivia>(trailing.Count);

        for (var triviaIndex = 0; triviaIndex < trailing.Count - 1; triviaIndex++)
        {
            result.Add(trailing[triviaIndex]);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Collapses consecutive <see cref="SyntaxKind.EndOfLineTrivia"/> to at most two within a trivia list,
    /// ensuring at most one blank line between code elements.
    /// Two consecutive EndOfLine trivia represent a single blank line (the first ends the preceding
    /// content line, the second is the blank line itself). Three or more are collapsed to two
    /// </summary>
    /// <param name="triviaList">The trivia list to process</param>
    /// <returns>The trivia list with consecutive blank lines collapsed</returns>
    private static SyntaxTriviaList CollapseConsecutiveEndOfLines(SyntaxTriviaList triviaList)
    {
        if (triviaList.Count < 3)
        {
            return triviaList;
        }

        var result = new List<SyntaxTrivia>(triviaList.Count);
        var consecutiveEndOfLineCount = 0;
        var changed = false;

        for (var triviaIndex = 0; triviaIndex < triviaList.Count; triviaIndex++)
        {
            if (triviaList[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                consecutiveEndOfLineCount++;

                if (consecutiveEndOfLineCount <= 2)
                {
                    result.Add(triviaList[triviaIndex]);
                }
                else
                {
                    changed = true;
                }
            }
            else
            {
                consecutiveEndOfLineCount = 0;
                result.Add(triviaList[triviaIndex]);
            }
        }

        return changed
                   ? SyntaxFactory.TriviaList(result)
                   : triviaList;
    }

    /// <summary>
    /// Removes blank lines (extra <see cref="SyntaxKind.EndOfLineTrivia"/>) from the beginning of a trivia list
    /// that follows an opening brace, preserving one end-of-line if the brace's trailing trivia does not already contain one
    /// </summary>
    /// <param name="triviaList">The trivia list to process</param>
    /// <param name="openBrace">The preceding open brace token</param>
    /// <returns>The trivia list with blank lines after the brace removed</returns>
    private static SyntaxTriviaList RemoveBlankLinesAfterBrace(SyntaxTriviaList triviaList, SyntaxToken openBrace)
    {
        if (triviaList.Count == 0 || triviaList[0].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return triviaList;
        }

        var braceHasTrailingEol = openBrace.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

        // If the brace already has trailing EOL, all leading EOLs on the next token are blank lines
        // If the brace does NOT have trailing EOL, the first leading EOL is the necessary line break
        var eolsToKeep = braceHasTrailingEol ? 0 : 1;
        var result = new List<SyntaxTrivia>(triviaList.Count);
        var eolsSeen = 0;
        var skipping = true;

        for (var triviaIndex = 0; triviaIndex < triviaList.Count; triviaIndex++)
        {
            if (skipping && triviaList[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                eolsSeen++;

                if (eolsSeen <= eolsToKeep)
                {
                    result.Add(triviaList[triviaIndex]);
                }

                continue;
            }

            skipping = false;
            result.Add(triviaList[triviaIndex]);
        }

        return eolsSeen > eolsToKeep
                   ? SyntaxFactory.TriviaList(result)
                   : triviaList;
    }

    /// <summary>
    /// Removes <see cref="SyntaxKind.EndOfLineTrivia"/> from the end of a trivia list,
    /// ensuring the file does not end with a trailing newline
    /// </summary>
    /// <param name="triviaList">The trivia list to process</param>
    /// <returns>The trivia list with trailing EndOfLineTrivia removed</returns>
    private static SyntaxTriviaList RemoveTrailingEndOfLineTrivia(SyntaxTriviaList triviaList)
    {
        if (triviaList.Count == 0 || triviaList[triviaList.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return triviaList;
        }

        var lastNonEndOfLine = triviaList.Count - 1;

        while (lastNonEndOfLine >= 0 && triviaList[lastNonEndOfLine].IsKind(SyntaxKind.EndOfLineTrivia))
        {
            lastNonEndOfLine--;
        }

        var result = new List<SyntaxTrivia>(lastNonEndOfLine + 1);

        for (var triviaIndex = 0; triviaIndex <= lastNonEndOfLine; triviaIndex++)
        {
            result.Add(triviaList[triviaIndex]);
        }

        return SyntaxFactory.TriviaList(result);
    }

    #endregion // Methods
}