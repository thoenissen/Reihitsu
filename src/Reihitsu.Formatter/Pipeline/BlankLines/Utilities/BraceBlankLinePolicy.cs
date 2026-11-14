using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

/// <summary>
/// The one owner of the token and comment facts the blank-line rules around braces, brackets, and comments read: the
/// opening brace of a block statement that follows a statement (FMT-BLANK-017), the tokens directly before which no blank
/// line stands (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-019), which are also the tokens behind an own-line comment that no
/// blank line separates from it (FMT-BLANK-020), the comment glued to a token (FMT-BLANK-018), and the one classification of a
/// comment's position relative to the token behind it that FMT-BLANK-011, FMT-BLANK-018, and FMT-BLANK-020 read. The
/// blank-line subphases read these facts from here
/// </summary>
internal static class BraceBlankLinePolicy
{
    #region Methods

    /// <summary>
    /// Determines whether a token is the opening brace of a block statement that follows another statement of the same
    /// block or switch section (FMT-BLANK-017). A detached formatting root has no parent, so for the root block the
    /// position it has in its document decides (FMT-PIPE-024)
    /// </summary>
    /// <param name="token">The token to inspect, still part of the visited tree</param>
    /// <param name="context">The formatting context, which carries the position of the formatting root</param>
    /// <returns><see langword="true"/> if the token opens a block statement that follows a statement</returns>
    public static bool IsOpeningBraceOfBlockStatementAfterStatement(SyntaxToken token, FormattingContext context)
    {
        if (token.IsKind(SyntaxKind.OpenBraceToken) == false
            || token.Parent is not BlockSyntax block)
        {
            return false;
        }

        return block.Parent == null
                   ? context.RootListPosition.HasPreviousStatement
                   : BlankLineSpacingPolicy.IsBlockStatementAfterStatement(block);
    }

    /// <summary>
    /// Determines whether the blank lines directly before a token are removed (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-019):
    /// every opening and closing brace other than the opening brace of a block statement that follows a statement, the
    /// <c>else</c>, <c>catch</c>, and <c>finally</c> keywords, the <c>while</c> keyword of a do statement, the closing bracket
    /// of a collection expression or a list pattern, and the closing parenthesis of a parenthesized pattern. The same set
    /// decides whether the blank lines above a comment glued to the token are removed (FMT-BLANK-018), and whether the blank
    /// lines between an own-line comment and the token are removed (FMT-BLANK-020)
    /// </summary>
    /// <param name="token">The token to inspect, still part of the visited tree</param>
    /// <param name="context">The formatting context, which carries the position of the formatting root</param>
    /// <returns><see langword="true"/> if no blank line stands directly before the token</returns>
    public static bool RemovesBlankLinesDirectlyBefore(SyntaxToken token, FormattingContext context)
    {
        return token.Kind() switch
               {
                   SyntaxKind.OpenBraceToken => IsOpeningBraceOfBlockStatementAfterStatement(token, context) == false,
                   SyntaxKind.CloseBraceToken or SyntaxKind.ElseKeyword or SyntaxKind.CatchKeyword or SyntaxKind.FinallyKeyword => true,
                   SyntaxKind.WhileKeyword => token.Parent is DoStatementSyntax,
                   _ => IsClosingTokenOfPatternOrCollectionExpression(token),
               };
    }

    /// <summary>
    /// Classifies the position of every comment in a token's leading trivia relative to that token (FMT-BLANK-018): the
    /// comments from the returned index on are on the token's line, every comment in front of it is on its own line. A line
    /// break inside the text of a delimited comment does not end a line, so a comment that starts on an earlier line and
    /// ends on the token's line is on the token's line, and so is every comment in front of it on the line it starts on.
    /// Every comment is on its own line in front of a token without text, such as the end of the file, which starts no line
    /// </summary>
    /// <param name="token">The token whose leading trivia is classified</param>
    /// <returns>The index of the first content trivia on the token's line; <c>-1</c> when every comment of the gap is on its own line</returns>
    /// <remarks>
    /// FMT-BLANK-011, FMT-BLANK-018, and FMT-BLANK-020 read this one classification, so every comment shape falls into
    /// exactly one of the two positions. The line-break phase runs before the blank-line phase, so a line break it writes in
    /// front of the token, such as the one in front of a chain link behind a multi-line comment's closing line, already
    /// stands in the trivia and is read as written; no later phase's line break is predicted (FMT-PIPE-025)
    /// </remarks>
    public static int FindTokenLineStartIndex(SyntaxToken token)
    {
        if (token.Span.IsEmpty)
        {
            return -1;
        }

        return TriviaListUtilities.FindFirstContentIndexOnLine(token.LeadingTrivia, token.LeadingTrivia.Count);
    }

    /// <summary>
    /// Finds the comment glued to a token (FMT-BLANK-018): the first comment on the token's line
    /// (<see cref="FindTokenLineStartIndex"/>) when it is an ordinary comment that starts its line, with nothing but
    /// whitespace and comments, ordinary or documentation, between it and the token
    /// </summary>
    /// <param name="token">The token whose leading trivia is inspected</param>
    /// <param name="previousTokenFacts">The facts about the token that precedes <paramref name="token"/>, which decide whether the comment starts its line when nothing in front of it does</param>
    /// <returns>The leading-trivia index of the glued comment; <c>-1</c> when no comment is glued to the token</returns>
    /// <remarks>
    /// Only the trivia and the position classification decide, never the kind of the token: what the blank lines above a
    /// glued comment are is decided by the rules for the blank lines directly before its token. The blank-line subphases
    /// that insert and remove those blank lines read this one predicate
    /// </remarks>
    public static int FindGluedCommentIndex(SyntaxToken token, PrecedingTokenFacts previousTokenFacts)
    {
        var trivia = token.LeadingTrivia;
        var gluedCommentIndex = FindTokenLineStartIndex(token);

        if (gluedCommentIndex < 0
            || TriviaListUtilities.IsOrdinaryComment(trivia[gluedCommentIndex]) == false)
        {
            return -1;
        }

        for (var triviaIndex = gluedCommentIndex; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (trivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia) == false
                && SyntaxTriviaUtilities.IsCommentTrivia(trivia[triviaIndex]) == false)
            {
                return -1;
            }
        }

        return TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, gluedCommentIndex, previousTokenFacts.Exists, previousTokenFacts.TrailingTrivia)
                   ? gluedCommentIndex
                   : -1;
    }

    /// <summary>
    /// Determines whether a token is the closing bracket of a collection expression or a list pattern, or the closing
    /// parenthesis of a parenthesized pattern, before which no blank line stands (FMT-BLANK-019)
    /// </summary>
    /// <param name="token">The token to inspect, still part of the visited tree</param>
    /// <returns><see langword="true"/> if the token is such a closing bracket or parenthesis</returns>
    public static bool IsClosingTokenOfPatternOrCollectionExpression(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.CloseBracketToken => token.Parent is CollectionExpressionSyntax or ListPatternSyntax,
                   SyntaxKind.CloseParenToken => token.Parent is ParenthesizedPatternSyntax,
                   _ => false,
               };
    }

    #endregion // Methods
}