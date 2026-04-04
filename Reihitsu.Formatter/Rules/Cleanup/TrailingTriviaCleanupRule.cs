using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.Cleanup;

/// <summary>
/// Final cleanup rule that removes trailing whitespace, collapses consecutive blank lines
/// to a maximum of one, and ensures a single trailing newline at end of file.
/// </summary>
internal sealed class TrailingTriviaCleanupRule : FormattingRuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public TrailingTriviaCleanupRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region IFormattingRule

    /// <inheritdoc/>
    public override FormattingPhase Phase => FormattingPhase.Cleanup;

    #endregion // IFormattingRule

    #region FormattingRuleBase

    /// <inheritdoc/>
    public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var visited = (CompilationUnitSyntax)base.VisitCompilationUnit(node);

        return RemoveTrailingNewlines(visited);
    }

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        token = base.VisitToken(token);

        if (token.IsKind(SyntaxKind.None))
        {
            return token;
        }

        var isFirstToken = token.GetPreviousToken().IsKind(SyntaxKind.None);
        var hasBom = isFirstToken
                     && token.LeadingTrivia.Count > 0
                     && IsBomTrivia(token.LeadingTrivia[0]);

        var newLeading = CleanTrivia(token.LeadingTrivia);
        var newTrailing = CleanTrivia(token.TrailingTrivia);

        if (hasBom
            && (newLeading.Count == 0 || IsBomTrivia(newLeading[0]) == false))
        {
            var restored = new List<SyntaxTrivia>(newLeading.Count + 1);
            restored.Add(SyntaxFactory.Whitespace("\uFEFF"));
            restored.AddRange(newLeading);
            newLeading = SyntaxFactory.TriviaList(restored);
        }

        // Strip trailing whitespace when the next token's leading trivia starts
        // with an end-of-line. The CleanTrivia method only examines a single
        // trivia list, so cross-token boundary trailing whitespace must be
        // handled here.
        if (newTrailing.Count > 0
            && newTrailing[newTrailing.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            var nextToken = token.GetNextToken();

            if (nextToken != default
                && nextToken.LeadingTrivia.Count > 0
                && nextToken.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                var trimmed = new List<SyntaxTrivia>(newTrailing);

                while (trimmed.Count > 0 && trimmed[trimmed.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    trimmed.RemoveAt(trimmed.Count - 1);
                }

                newTrailing = SyntaxFactory.TriviaList(trimmed);
            }
        }

        return token.WithLeadingTrivia(newLeading)
                    .WithTrailingTrivia(newTrailing);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        var visited = (BlockSyntax)base.VisitBlock(node);

        if (visited.Statements.Count == 0)
        {
            return visited;
        }

        if (visited.OpenBraceToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
        {
            return visited;
        }

        var firstStatement = visited.Statements[0];
        var newFirstStatement = StripLeadingEndOfLines(firstStatement);

        if (newFirstStatement != firstStatement)
        {
            visited = visited.WithStatements(visited.Statements.Replace(firstStatement, newFirstStatement));
        }

        return visited;
    }

    #endregion // FormattingRuleBase

    #region Methods

    /// <summary>
    /// Determines whether the trivia at the specified index is immediately followed
    /// by an end-of-line trivia (skipping no elements).
    /// </summary>
    /// <param name="triviaList">The trivia list.</param>
    /// <param name="index">The index of the current trivia.</param>
    /// <returns><c>true</c> if the next trivia is an end-of-line; otherwise, <c>false</c>.</returns>
    private static bool IsFollowedByEndOfLine(SyntaxTriviaList triviaList, int index)
    {
        var nextIndex = index + 1;

        if (nextIndex < triviaList.Count)
        {
            return triviaList[nextIndex].IsKind(SyntaxKind.EndOfLineTrivia);
        }

        return false;
    }

    /// <summary>
    /// Removes trailing whitespace trivia entries from the end of the result list.
    /// This is called before adding an end-of-line to strip any whitespace that precedes it.
    /// </summary>
    /// <param name="result">The result list to modify.</param>
    private static void RemoveTrailingWhitespace(List<SyntaxTrivia> result)
    {
        while (result.Count > 0 && result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            result.RemoveAt(result.Count - 1);
        }
    }

    /// <summary>
    /// Removes all trailing end-of-line and whitespace trivia from the given trivia list,
    /// returning the trimmed entries as a mutable list.
    /// </summary>
    /// <param name="triviaList">The trivia list to trim.</param>
    /// <returns>A mutable list with trailing end-of-line and whitespace entries removed.</returns>
    private static List<SyntaxTrivia> TrimTrailingEndOfLines(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>(triviaList.Count);

        foreach (var trivia in triviaList)
        {
            result.Add(trivia);
        }

        while (result.Count > 0
               && (result[result.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia)
                   || result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    /// <summary>
    /// Removes all leading end-of-line trivia from the given node's leading trivia.
    /// </summary>
    /// <typeparam name="T">The type of syntax node.</typeparam>
    /// <param name="node">The node to modify.</param>
    /// <returns>The node with leading end-of-line trivia removed.</returns>
    private static T StripLeadingEndOfLines<T>(T node)
        where T : SyntaxNode
    {
        var leading = node.GetLeadingTrivia();
        var result = new List<SyntaxTrivia>(leading.Count);
        var foundNonEndOfLine = false;

        foreach (var trivia in leading)
        {
            if (foundNonEndOfLine == false && trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            foundNonEndOfLine = true;

            result.Add(trivia);
        }

        if (result.Count == leading.Count)
        {
            return node;
        }

        return node.WithLeadingTrivia(SyntaxFactory.TriviaList(result));
    }

    /// <summary>
    /// Determines whether the given trivia represents a UTF-8 BOM marker.
    /// </summary>
    /// <param name="trivia">The trivia to check.</param>
    /// <returns><c>true</c> if the trivia is a BOM marker; otherwise, <c>false</c>.</returns>
    private static bool IsBomTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.WhitespaceTrivia)
               && trivia.ToString() == "\uFEFF";
    }

    /// <summary>
    /// Cleans a trivia list by removing trailing whitespace before end-of-line trivia
    /// and collapsing consecutive blank lines to a maximum of one.
    /// </summary>
    /// <param name="triviaList">The trivia list to clean.</param>
    /// <returns>The cleaned trivia list.</returns>
    private SyntaxTriviaList CleanTrivia(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>(triviaList.Count);
        var consecutiveEndOfLineCount = 0;

        for (var i = 0; i < triviaList.Count; i++)
        {
            var trivia = triviaList[i];

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                RemoveTrailingWhitespace(result);

                consecutiveEndOfLineCount++;

                if (consecutiveEndOfLineCount <= 1)
                {
                    result.Add(SyntaxFactory.EndOfLine(Context.EndOfLine));
                }
            }
            else if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (IsFollowedByEndOfLine(triviaList, i) == false)
                {
                    result.Add(trivia);
                }

                // Do not reset consecutiveEndOfLineCount for whitespace between EOLs
            }
            else
            {
                consecutiveEndOfLineCount = -1;

                result.Add(trivia);
            }
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Removes all trailing newlines from the end of the compilation unit.
    /// The file should end with the last content character.
    /// </summary>
    /// <param name="compilationUnit">The compilation unit to process.</param>
    /// <returns>The compilation unit with trailing newlines removed.</returns>
    private CompilationUnitSyntax RemoveTrailingNewlines(CompilationUnitSyntax compilationUnit)
    {
        var eofToken = compilationUnit.EndOfFileToken;
        var leadingTrivia = eofToken.LeadingTrivia;

        var trimmed = TrimTrailingEndOfLines(leadingTrivia);

        var newEofToken = eofToken.WithLeadingTrivia(SyntaxFactory.TriviaList(trimmed));

        var result = compilationUnit.WithEndOfFileToken(newEofToken);

        var lastToken = result.EndOfFileToken.GetPreviousToken();

        if (lastToken != default)
        {
            var lastTrailing = TrimTrailingEndOfLines(lastToken.TrailingTrivia);

            if (lastTrailing.Count != lastToken.TrailingTrivia.Count)
            {
                result = result.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(SyntaxFactory.TriviaList(lastTrailing)));
            }
        }

        return result;
    }

    #endregion // Methods
}