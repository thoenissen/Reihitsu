using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Pipeline.Cleanup;

/// <summary>
/// Final cleanup pass that removes non-semantic trivia noise such as trailing whitespace
/// and end-of-file newlines. It moves no token within its line: every column is final after the indentation phase,
/// and tabs in whitespace were already replaced by the first phase.
/// Collapsing excessive blank lines is owned solely by <see cref="BlankLines.Rewriter.BlankLineCollapser"/>
/// </summary>
internal sealed class CleanupPhase : IFormattingPhase
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

        return rewritten.WithLeadingTrivia(leading)
                        .WithTrailingTrivia(trailing);
    }

    /// <summary>
    /// Cleans the leading trivia of a token by removing trailing whitespace
    /// and handling end-of-file formatting
    /// </summary>
    /// <param name="leading">The leading trivia list to clean</param>
    /// <param name="original">The original token</param>
    /// <returns>The cleaned leading trivia list</returns>
    private static SyntaxTriviaList CleanLeadingTrivia(SyntaxTriviaList leading, SyntaxToken original)
    {
        leading = CleanWhitespaceBeforeEndOfLine(leading, startsText: original.FullSpan.Start == 0);

        if (original.IsKind(SyntaxKind.EndOfFileToken))
        {
            leading = RemoveTrailingEndOfLineTrivia(leading);
        }

        return leading;
    }

    /// <summary>
    /// Cleans the trailing trivia of a token by removing trailing whitespace, stripping whitespace
    /// at end of line, and handling end-of-file formatting
    /// </summary>
    /// <param name="trailing">The trailing trivia list to clean</param>
    /// <param name="original">The original token</param>
    /// <returns>The cleaned trailing trivia list</returns>
    private static SyntaxTriviaList CleanTrailingTrivia(SyntaxTriviaList trailing, SyntaxToken original)
    {
        trailing = CleanWhitespaceBeforeEndOfLine(trailing, startsText: false);
        trailing = StripTrailingWhitespaceAtEndOfLine(trailing, original);

        if (original.IsKind(SyntaxKind.EndOfFileToken))
        {
            trailing = RemoveTrailingEndOfLineTrivia(trailing);
        }
        else
        {
            // includeZeroWidth: true is required here — EndOfFileToken is zero-width, so the default
            // GetNextToken() skips over it and returns None for the file's last real token exactly as
            // it would for a genuinely detached node, making the two arms indistinguishable and
            // defeating the EndOfFileToken.LeadingTrivia check below.
            var nextToken = original.GetNextToken(includeZeroWidth: true);

            // SyntaxToken.GetNextToken() skips trivia, so knowing the next token is EndOfFileToken only
            // proves no further real token follows — it says nothing about whether that token's own
            // leading trivia still holds a comment, directive, or disabled text that needs to start on
            // its own line. FindFirstSignificantTriviaIndex is that missing check, reused from the
            // shared owner instead of restated here.
            if (nextToken.IsKind(SyntaxKind.None)
                || (nextToken.IsKind(SyntaxKind.EndOfFileToken) && SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(nextToken.LeadingTrivia) < 0))
            {
                trailing = RemoveTrailingEndOfLineTrivia(trailing);
            }
        }

        return trailing;
    }

    /// <summary>
    /// Removes whitespace trivia that immediately precedes end-of-line trivia,
    /// eliminating trailing whitespace and whitespace on blank lines. The parser reads a byte order mark at the start
    /// of the text as whitespace, so when the list starts the text, a byte order mark that starts its first trivia is
    /// kept and only the whitespace after it is removed
    /// </summary>
    /// <param name="triviaList">The trivia list to clean</param>
    /// <param name="startsText">Whether the trivia list starts the text, so its first trivia may hold the byte order mark</param>
    /// <returns>The cleaned trivia list</returns>
    private static SyntaxTriviaList CleanWhitespaceBeforeEndOfLine(SyntaxTriviaList triviaList, bool startsText)
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
                if (startsText
                    && triviaIndex == 0
                    && ByteOrderMarkUtilities.StartsWithByteOrderMark(triviaList[triviaIndex]))
                {
                    var byteOrderMark = ByteOrderMarkUtilities.CreateByteOrderMarkTrivia();

                    if (triviaList[triviaIndex].ToFullString() == byteOrderMark.ToFullString())
                    {
                        result.Add(triviaList[triviaIndex]);

                        continue;
                    }

                    result.Add(byteOrderMark);
                }

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
        return EndOfLineWhitespace.StripBeforeFollowingLineBreak(trailing, originalToken.GetNextToken().LeadingTrivia);
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

    #region IFormattingPhase

    /// <inheritdoc/>
    /// <remarks>
    /// The <paramref name="context"/> is part of the uniform phase contract and is not used by this phase
    /// </remarks>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return Execute(root, cancellationToken);
    }

    #endregion // IFormattingPhase
}