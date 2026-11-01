using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

/// <summary>
/// The one owner of the token and comment facts the blank-line rules around braces, brackets, and comments read: the
/// opening brace of a block statement that follows a statement (FMT-BLANK-017), the tokens directly before which no blank
/// line stands (FMT-BLANK-007, FMT-BLANK-008, FMT-BLANK-019), the comment glued to a token (FMT-BLANK-018), and the tokens
/// behind an own-line comment that no blank line separates from it (FMT-BLANK-020), and the one classification of a
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
    /// decides whether the blank lines above a comment glued to the token are removed (FMT-BLANK-018)
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
    /// Determines whether the blank lines between an own-line comment and the token behind it are removed (FMT-BLANK-020):
    /// every opening and closing brace other than the braces of an array initializer, an extension block, or an
    /// interpolation, the opening brace of a property pattern, the closing brace of a collection initializer assigned to a
    /// member, and the opening brace of a block statement that follows a statement; the closing bracket of a collection
    /// expression or a list pattern; and the closing parenthesis of a parenthesized pattern
    /// </summary>
    /// <param name="token">The token to inspect, still part of the visited tree</param>
    /// <param name="context">The formatting context, which carries the facts about the formatting root</param>
    /// <returns><see langword="true"/> if the token is one FMT-BLANK-020 names</returns>
    /// <remarks>
    /// Only the kind of the token and the construct it belongs to decide, never the layout of that construct: the line-break
    /// phase runs later and may change that layout, so a decision read from it would change between two passes
    /// </remarks>
    public static bool RemovesBlankLinesBehindOwnLineComment(SyntaxToken token, FormattingContext context)
    {
        switch (token.Kind())
        {
            case SyntaxKind.OpenBraceToken:
                {
                    return token.Parent switch
                           {
                               InitializerExpressionSyntax initializer => initializer.IsKind(SyntaxKind.ArrayInitializerExpression) == false,
                               PropertyPatternClauseSyntax or ExtensionBlockDeclarationSyntax or InterpolationSyntax => false,
                               _ => IsOpeningBraceOfBlockStatementAfterStatement(token, context) == false,
                           };
                }

            case SyntaxKind.CloseBraceToken:
                {
                    return token.Parent switch
                           {
                               InitializerExpressionSyntax initializer => initializer.IsKind(SyntaxKind.ArrayInitializerExpression) == false
                                                                          && LineBreakInitializerRewriter.IsAssignedCollectionInitializer(initializer, context.RootConstruct) == false,
                               ExtensionBlockDeclarationSyntax or InterpolationSyntax => false,
                               _ => true,
                           };
                }

            default:
                {
                    return IsClosingTokenOfPatternOrCollectionExpression(token);
                }
        }
    }

    /// <summary>
    /// Classifies the position of every comment in a token's leading trivia relative to that token (FMT-BLANK-018): the
    /// comments from the returned index on are on the token's line, every comment in front of it is on its own line. A line
    /// break inside the text of a delimited comment does not end a line, so a comment that starts on an earlier line and
    /// ends on the token's line is on the token's line, and so is every comment in front of it on the line it starts on.
    /// Every comment is on its own line in front of a token without text, such as the end of the file, which starts no line,
    /// and in front of a token that the chain layout moves off the closing line of a multi-line comment
    /// (<see cref="ChainLineBreakRewriter.BreaksLineBehindMultiLineCommentTail"/>)
    /// </summary>
    /// <param name="token">The token whose leading trivia is classified; a token the cleanup rewriter has already detached from its tree has no parent, so it is classified without the chain-link case, which is exact for every token that is not a chain link operator</param>
    /// <param name="context">The formatting context, which carries the facts about the code above the formatting root</param>
    /// <returns>The index of the first content trivia on the token's line; <c>-1</c> when every comment of the gap is on its own line</returns>
    /// <remarks>
    /// FMT-BLANK-011, FMT-BLANK-018, and FMT-BLANK-020 read this one classification, so every comment shape falls into
    /// exactly one of the two positions
    /// </remarks>
    public static int FindTokenLineStartIndex(SyntaxToken token, FormattingContext context)
    {
        if (token.Span.IsEmpty
            || ChainLineBreakRewriter.BreaksLineBehindMultiLineCommentTail(token, context.RootChainBoundary))
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
    /// <param name="token">The token whose leading trivia is inspected; a token the cleanup rewriter has already detached from its tree is classified without the chain-link case, which is exact for every token that is not a chain link operator</param>
    /// <param name="previousTokenFacts">The facts about the token that precedes <paramref name="token"/>, which decide whether the comment starts its line when nothing in front of it does</param>
    /// <param name="context">The formatting context, which carries the facts about the code above the formatting root</param>
    /// <returns>The leading-trivia index of the glued comment; <c>-1</c> when no comment is glued to the token</returns>
    /// <remarks>
    /// Only the trivia and the position classification decide, never the kind of the token: what the blank lines above a
    /// glued comment are is decided by the rules for the blank lines directly before its token. The blank-line subphases
    /// that insert and remove those blank lines read this one predicate
    /// </remarks>
    public static int FindGluedCommentIndex(SyntaxToken token, PrecedingTokenFacts previousTokenFacts, FormattingContext context)
    {
        var trivia = token.LeadingTrivia;
        var gluedCommentIndex = FindTokenLineStartIndex(token, context);

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