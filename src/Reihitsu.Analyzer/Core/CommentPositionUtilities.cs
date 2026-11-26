using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared logic for the rules that report comments placed inside declarations and statements, and for the rules that
/// check where a line starts, which take a comment that starts the line as its first element (SHARED-COMMENT-011)
/// </summary>
internal static class CommentPositionUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the trivia is an ordinary comment (SHARED-COMMENT-001): a <c>//</c> or <c>/* */</c> comment, or a
    /// <c>///</c> or <c>/** */</c> comment that documents nothing (<see cref="IsDocumentingComment"/>). With
    /// <see cref="DocumentationMode.None"/> the compiler lexes documentation comments as ordinary comments, so their text
    /// decides which kind they are
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is an ordinary comment</returns>
    internal static bool IsOrdinaryComment(SyntaxTrivia trivia)
    {
        return SyntaxTriviaUtilities.IsCommentTrivia(trivia)
               && IsDocumentingComment(trivia) == false;
    }

    /// <summary>
    /// Determines whether the trivia is a documentation comment, including one the compiler lexed as an ordinary
    /// comment because documentation comments are not parsed, whether or not it documents a declaration
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is a documentation comment</returns>
    internal static bool IsDocumentationComment(SyntaxTrivia trivia)
    {
        return trivia.Kind() switch
               {
                   SyntaxKind.SingleLineCommentTrivia => DocumentationAnalysisUtilities.IsDocumentationLine(trivia.ToString()),
                   SyntaxKind.MultiLineCommentTrivia => IsMultiLineDocumentationText(trivia.ToString()),
                   _ => SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia)
               };
    }

    /// <summary>
    /// Determines whether the trivia is a documentation comment that documents a declaration, as the compiler reads it
    /// (SHARED-COMMENT-001): it stands in the leading trivia of the first token of a type, a delegate, a member, or an enum
    /// member, and no later documentation comment follows it there with an ordinary comment or a directive between the
    /// two
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia documents a declaration</returns>
    internal static bool IsDocumentingComment(SyntaxTrivia trivia)
    {
        if (IsDocumentationComment(trivia) == false
            || GetDeclarationStartingWith(trivia.Token) == null)
        {
            return false;
        }

        var leadingTrivia = trivia.Token.LeadingTrivia;
        var commentIndex = leadingTrivia.IndexOf(trivia);
        var isSeparated = false;

        if (commentIndex < 0)
        {
            return false;
        }

        for (var triviaIndex = commentIndex + 1; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            var followingTrivia = leadingTrivia[triviaIndex];

            if (IsDocumentationComment(followingTrivia))
            {
                if (isSeparated)
                {
                    return false;
                }
            }
            else
            {
                isSeparated |= SyntaxTriviaUtilities.IsCommentTrivia(followingTrivia) || followingTrivia.IsDirective;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the documentable declaration that starts with the given token: a type, a delegate, a member, or an enum
    /// member, ahead of its attribute lists and modifiers
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>The declaration, or <see langword="null"/> if the token does not start one</returns>
    internal static MemberDeclarationSyntax GetDeclarationStartingWith(SyntaxToken token)
    {
        for (var node = token.Parent; node != null; node = node.Parent)
        {
            if (node.GetFirstToken() != token)
            {
                return null;
            }

            if (node is MemberDeclarationSyntax declaration)
            {
                return declaration is BaseNamespaceDeclarationSyntax or GlobalStatementSyntax or IncompleteMemberSyntax
                           ? null
                           : declaration;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the location of a comment: from its first character — the <c>///</c> or <c>/**</c> of a documentation
    /// comment included — to the end of its text, without the line break that ends a <c>///</c> comment
    /// </summary>
    /// <param name="comment">Comment trivia</param>
    /// <returns>The location of the comment</returns>
    internal static Location GetCommentLocation(SyntaxTrivia comment)
    {
        var text = comment.ToFullString();
        var length = text.TrimEnd('\r', '\n').Length;

        return Location.Create(comment.SyntaxTree, new TextSpan(comment.FullSpan.Start, length));
    }

    /// <summary>
    /// Gets the locations a comment in a reported position is reported at (SHARED-COMMENT-003): one for each line of a
    /// run of <c>///</c> lines, from that line's <c>///</c> to the end of its text, as when documentation comments are not
    /// parsed and every line is a comment of its own; for any other comment, its <see cref="GetCommentLocation"/>
    /// </summary>
    /// <param name="comment">Comment trivia</param>
    /// <returns>The locations of the comment</returns>
    internal static IEnumerable<Location> GetCommentLocations(SyntaxTrivia comment)
    {
        if (comment.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) == false)
        {
            yield return GetCommentLocation(comment);

            yield break;
        }

        var text = comment.SyntaxTree.GetText();
        var lineSpan = comment.SyntaxTree.GetLineSpan(comment.FullSpan);

        for (var lineNumber = lineSpan.StartLinePosition.Line; lineNumber <= lineSpan.EndLinePosition.Line; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var start = Math.Max(line.Start, comment.FullSpan.Start);
            var end = Math.Min(line.End, comment.FullSpan.End);

            while (start < end
                   && char.IsWhiteSpace(text[start]))
            {
                start++;
            }

            if (start < end)
            {
                yield return Location.Create(comment.SyntaxTree, TextSpan.FromBounds(start, end));
            }
        }
    }

    /// <summary>
    /// Tries to get the comment that starts the line of a token (SHARED-COMMENT-011): a delimited comment in the token's
    /// leading trivia that only whitespace precedes on the line it starts on, with nothing but whitespace and further
    /// comments between it and the token. Unless <paramref name="isEarlierLineAllowed"/> is set, the comment starts on the
    /// token's line; with it, a comment that starts an earlier line and ends on the token's line counts as well
    /// (SHARED-INIT-001, RH7304-001)
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="isEarlierLineAllowed">Whether a comment that starts an earlier line counts</param>
    /// <param name="comment">The comment that starts the line</param>
    /// <returns><see langword="true"/> if a comment starts the token's line</returns>
    internal static bool TryGetLineStartComment(SyntaxToken token, bool isEarlierLineAllowed, out SyntaxTrivia comment)
    {
        var leadingTrivia = token.LeadingTrivia;

        for (var triviaIndex = leadingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            comment = leadingTrivia[triviaIndex];

            if (comment.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            if (comment.IsKind(SyntaxKind.MultiLineCommentTrivia) == false
                && comment.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) == false)
            {
                break;
            }

            if (TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, triviaIndex))
            {
                if (isEarlierLineAllowed
                    || GetStartPosition(comment).Line == SyntaxTokenPositionUtilities.GetLine(token))
                {
                    return true;
                }

                break;
            }
        }

        comment = default;

        return false;
    }

    /// <summary>
    /// Gets the column of the first element of a token's line (SHARED-COMMENT-011): the column of the comment that starts
    /// the token's line in front of it (<see cref="TryGetLineStartComment"/>), or otherwise the token's own column
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>The column of the line's first element</returns>
    internal static int GetLineStartColumn(SyntaxToken token)
    {
        return TryGetLineStartComment(token, false, out var comment)
                   ? GetStartPosition(comment).Character
                   : SyntaxTokenPositionUtilities.GetColumn(token);
    }

    /// <summary>
    /// Gets the position where a comment starts, at its <c>/*</c>, <c>/**</c>, or <c>///</c>
    /// </summary>
    /// <param name="comment">Comment trivia</param>
    /// <returns>The line and column the comment starts at</returns>
    internal static LinePosition GetStartPosition(SyntaxTrivia comment)
    {
        return comment.SyntaxTree.GetLineSpan(new TextSpan(comment.FullSpan.Start, 0)).StartLinePosition;
    }

    /// <summary>
    /// Gets the tokens on either side of a comment. A comment in the leading trivia of a token lies between the
    /// previous token and that token; a comment in the trailing trivia of a token lies between that token and the
    /// next one
    /// </summary>
    /// <param name="comment">Comment trivia</param>
    /// <param name="previousToken">Token before the comment</param>
    /// <param name="nextToken">Token after the comment</param>
    /// <returns>
    /// <see langword="true"/> if the token after the comment exists and neither token is missing. The token
    /// before the comment is <see cref="SyntaxKind.None"/> when the comment precedes the first token of the file
    /// </returns>
    internal static bool TryGetSurroundingTokens(SyntaxTrivia comment, out SyntaxToken previousToken, out SyntaxToken nextToken)
    {
        var token = comment.Token;

        if (comment.SpanStart < token.SpanStart)
        {
            previousToken = token.GetPreviousToken();
            nextToken = token;
        }
        else
        {
            previousToken = token;
            nextToken = token.GetNextToken();
        }

        return previousToken.IsMissing == false
               && IsPresent(nextToken);
    }

    /// <summary>
    /// Determines whether the gap between two tokens lies between the given delimiters, both included
    /// </summary>
    /// <param name="previousToken">Token before the gap</param>
    /// <param name="nextToken">Token after the gap</param>
    /// <param name="openToken">Opening delimiter</param>
    /// <param name="closeToken">Closing delimiter</param>
    /// <returns><see langword="true"/> if the gap lies between the delimiters</returns>
    internal static bool IsGapWithin(SyntaxToken previousToken, SyntaxToken nextToken, SyntaxToken openToken, SyntaxToken closeToken)
    {
        return IsPresent(openToken)
               && IsPresent(closeToken)
               && previousToken.SpanStart >= openToken.SpanStart
               && nextToken.SpanStart <= closeToken.SpanStart;
    }

    /// <summary>
    /// Determines whether a gap inside a region belongs to a construct nested in the region rather than to the region
    /// itself. A gap directly before one of the region's own tokens, or before the first token of one of its
    /// elements, always belongs to the region. Any other gap belongs to the innermost construct between the token after
    /// it and the region that lays out its own lines, as decided by <see cref="SyntaxIndentationUtilities.IsOwningConstruct"/>
    /// </summary>
    /// <param name="region">Region node</param>
    /// <param name="nextToken">Token after the gap</param>
    /// <returns><see langword="true"/> if a nested construct owns the gap</returns>
    internal static bool IsOwnedByNestedConstruct(SyntaxNode region, SyntaxToken nextToken)
    {
        var element = nextToken.Parent;

        if (element == region)
        {
            return false;
        }

        while (element != null
               && element.Parent != region)
        {
            element = element.Parent;
        }

        if (element == null
            || element.GetFirstToken() == nextToken)
        {
            return false;
        }

        for (var node = nextToken.Parent; node != null && node != region; node = node.Parent)
        {
            if (SyntaxIndentationUtilities.IsOwningConstruct(node, nextToken))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the token is the operator of a binary expression
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token is a binary operator</returns>
    internal static bool IsBinaryOperator(SyntaxToken token)
    {
        return token.Parent is BinaryExpressionSyntax binaryExpression
               && binaryExpression.OperatorToken == token;
    }

    /// <summary>
    /// Determines whether a comment directly before the token stays valid inside a list or a statement header: a
    /// comment before a binary operator or before a later call of a fluent chain describes the operand or call that
    /// follows it
    /// </summary>
    /// <param name="nextToken">Token after the comment</param>
    /// <returns><see langword="true"/> if the comment position stays valid</returns>
    internal static bool IsValidInsideRegion(SyntaxToken nextToken)
    {
        return IsBinaryOperator(nextToken)
               || FluentChainAnalysisHelper.IsLaterChainLink(nextToken);
    }

    /// <summary>
    /// Gets the header and the body of an <see langword="if"/>, <see langword="while"/>, <see langword="for"/>,
    /// <see langword="foreach"/>, <see langword="using"/>, <see langword="lock"/>, or <see langword="fixed"/> statement.
    /// The header runs from the statement keyword, or the <see langword="await"/> keyword when present, to the closing
    /// parenthesis
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="headerStart">First token of the header</param>
    /// <param name="closeParenToken">Closing parenthesis of the header</param>
    /// <param name="body">Embedded statement</param>
    /// <returns><see langword="true"/> if the node is one of these statements</returns>
    internal static bool TryGetStatementHeader(SyntaxNode node, out SyntaxToken headerStart, out SyntaxToken closeParenToken, out StatementSyntax body)
    {
        (headerStart, closeParenToken, body) = node switch
                                               {
                                                   IfStatementSyntax statement => (statement.IfKeyword, statement.CloseParenToken, statement.Statement),
                                                   WhileStatementSyntax statement => (statement.WhileKeyword, statement.CloseParenToken, statement.Statement),
                                                   ForStatementSyntax statement => (statement.ForKeyword, statement.CloseParenToken, statement.Statement),
                                                   CommonForEachStatementSyntax statement => (GetHeaderStart(statement.AwaitKeyword, statement.ForEachKeyword), statement.CloseParenToken, statement.Statement),
                                                   UsingStatementSyntax statement => (GetHeaderStart(statement.AwaitKeyword, statement.UsingKeyword), statement.CloseParenToken, statement.Statement),
                                                   LockStatementSyntax statement => (statement.LockKeyword, statement.CloseParenToken, statement.Statement),
                                                   FixedStatementSyntax statement => (statement.FixedKeyword, statement.CloseParenToken, statement.Statement),
                                                   _ => (default, default, null)
                                               };

        return body != null;
    }

    /// <summary>
    /// Gets the first token of a statement header, which is the <see langword="await"/> keyword when present
    /// </summary>
    /// <param name="awaitKeyword">Optional <see langword="await"/> keyword</param>
    /// <param name="keyword">Statement keyword</param>
    /// <returns>The first token of the header</returns>
    private static SyntaxToken GetHeaderStart(SyntaxToken awaitKeyword, SyntaxToken keyword)
    {
        return awaitKeyword.IsKind(SyntaxKind.None)
                   ? keyword
                   : awaitKeyword;
    }

    /// <summary>
    /// Determines whether a token exists and was not synthesized for a syntax error
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token is present</returns>
    private static bool IsPresent(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.None) == false
               && token.IsMissing == false;
    }

    /// <summary>
    /// Determines whether multi-line comment text opens a documentation comment
    /// </summary>
    /// <param name="text">Comment text</param>
    /// <returns><see langword="true"/> if the text starts with <c>/**</c> but is not the empty comment <c>/**/</c></returns>
    private static bool IsMultiLineDocumentationText(string text)
    {
        return text.StartsWith("/**", StringComparison.Ordinal)
               && text.StartsWith("/**/", StringComparison.Ordinal) == false;
    }

    #endregion // Methods
}