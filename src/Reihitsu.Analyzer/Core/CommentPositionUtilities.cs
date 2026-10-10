using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared logic for the rules that report comments placed inside declarations and statements
/// </summary>
internal static class CommentPositionUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the trivia is an ordinary <c>//</c> or <c>/* */</c> comment. Documentation comments are
    /// excluded in every documentation mode: with <see cref="DocumentationMode.None"/> the compiler lexes them as
    /// ordinary comments, so their text decides
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is an ordinary comment</returns>
    internal static bool IsOrdinaryComment(SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
        {
            return DocumentationAnalysisUtilities.IsDocumentationLine(trivia.ToString()) == false;
        }

        if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
        {
            return IsMultiLineDocumentationText(trivia.ToString()) == false;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the trivia is a documentation comment, including one the compiler lexed as an ordinary
    /// comment because documentation comments are not parsed
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is a documentation comment</returns>
    internal static bool IsDocumentationComment(SyntaxTrivia trivia)
    {
        return SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia)
               || (SyntaxTriviaUtilities.IsCommentTrivia(trivia) && IsOrdinaryComment(trivia) == false);
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
    /// it and the region that lays out its own lines, as decided by <see cref="IsOwningConstruct"/>
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
            if (IsOwningConstruct(node, nextToken))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a node lays out the line of a token inside it itself - a brace scope, an initializer, an
    /// anonymous object, a switch expression, the interior of a collection expression, a recursive, list, or
    /// parenthesized pattern, an interpolation hole, or the clauses of a query. This is the same decision the
    /// formatter's <c>ListElementInteriorUtilities.IsOwningConstruct</c> makes before it joins a line inside a list
    /// element, and must stay in step with it
    /// </summary>
    /// <param name="node">Node on the path from the token to the region</param>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the node owns the token's line</returns>
    internal static bool IsOwningConstruct(SyntaxNode node, SyntaxToken token)
    {
        return node switch
               {
                   CollectionExpressionSyntax collection => token != collection.OpenBracketToken,
                   InitializerExpressionSyntax
                   or AnonymousObjectCreationExpressionSyntax
                   or SwitchExpressionSyntax
                   or RecursivePatternSyntax
                   or ListPatternSyntax
                   or ParenthesizedPatternSyntax
                   or AccessorListSyntax
                   or InterpolationSyntax
                   or QueryBodySyntax => true,
                   _ => SyntaxIndentationUtilities.IsIndentingScope(node)
               };
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