using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

/// <summary>
/// Moves a token onto its own line during line-break formatting. It writes line breaks only and keeps the blank lines of
/// every gap it does not join, which the blank-line phase decides (FMT-BREAK-030)
/// </summary>
internal sealed class TokenGapNormalizer
{
    #region Fields

    /// <summary>
    /// The end-of-line sequence to emit when inserting line breaks
    /// </summary>
    private readonly string _endOfLine;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endOfLine">The end-of-line sequence to emit when inserting line breaks</param>
    public TokenGapNormalizer(string endOfLine)
    {
        _endOfLine = endOfLine;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Moves a token onto its own line behind the previous token, keeping every blank line of its gap (FMT-BREAK-030)
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token that should start its own line</param>
    /// <param name="withToken">Function that updates the token on the owning node</param>
    /// <returns>The updated node</returns>
    /// <remarks>
    /// The previous token's trailing whitespace stays; the cleanup phase removes it (FMT-BREAK-029)
    /// </remarks>
    public TNode NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia<TNode>(TNode node,
                                                                             SyntaxToken token,
                                                                             Func<TNode, SyntaxToken, TNode> withToken)
        where TNode : SyntaxNode
    {
        return PlaceTokenOnOwnLine(node, token, withToken, trimPreviousWhitespace: false);
    }

    /// <summary>
    /// Moves a token owned directly by a syntax node onto its own line behind the previous token, even when the previous
    /// token lies outside that node, keeping every blank line of its gap (FMT-BREAK-030)
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token that should start its own line</param>
    /// <param name="withToken">Function that updates the token on the owning node</param>
    /// <returns>The updated node</returns>
    public TNode NormalizeGapBeforeOwnedToken<TNode>(TNode node,
                                                     SyntaxToken token,
                                                     Func<TNode, SyntaxToken, TNode> withToken)
        where TNode : SyntaxNode
    {
        return PlaceTokenOnOwnLine(node, token, withToken, trimPreviousWhitespace: true);
    }

    /// <summary>
    /// Moves a token onto its own line behind the previous token, keeping every blank line of its gap (FMT-BREAK-030)
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token that should start its own line</param>
    /// <returns>The updated node</returns>
    public TNode NormalizeGapBeforeToken<TNode>(TNode node,
                                                SyntaxToken token)
        where TNode : SyntaxNode
    {
        token = TokenLocator.GetCurrentToken(node, token);

        return PlaceTokenOnOwnLine(node, token, (owner, newToken) => owner.ReplaceToken(token, newToken), trimPreviousWhitespace: true);
    }

    /// <summary>
    /// Moves a token onto a line of its own when its line starts behind other content (SHARED-BLANK-008). The token's line
    /// begins with the first content in front of it on that line: the token itself, or a comment glued to it, such as a
    /// delimited documentation comment (<c>/** */</c>) or a comment that starts its line above the token; a multi-line
    /// comment belongs to the line it starts on. That line starts behind a <c>//</c> comment, a <c>///</c> comment, a
    /// directive, or an end-of-line trivia, but not behind the last line of a multi-line comment or the previous token. A
    /// token whose line starts that way stays where it is. Otherwise the line break goes in front of that first content, so
    /// the token and every comment glued to it start the new line together, while every comment that trailed the previous
    /// token stays on that token's line
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token that should start its own line</param>
    /// <returns>The updated node</returns>
    /// <remarks>
    /// The previous token's trailing whitespace stays; the cleanup phase removes it (FMT-BREAK-029)
    /// </remarks>
    public TNode EnsureTokenStartsLine<TNode>(TNode node,
                                              SyntaxToken token)
        where TNode : SyntaxNode
    {
        token = TokenLocator.GetCurrentToken(node, token);

        if (token.IsMissing
            || TokenContainmentUtilities.ContainsToken(node, token) == false
            || TokenLocator.TryGetPreviousToken(node, token, out _) == false)
        {
            return node;
        }

        var leadingTrivia = token.LeadingTrivia;
        var lineStartIndex = TriviaListUtilities.FindFirstContentIndexOnLine(leadingTrivia, leadingTrivia.Count);

        if (lineStartIndex < 0)
        {
            lineStartIndex = leadingTrivia.Count;
        }

        if (TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, lineStartIndex))
        {
            return node;
        }

        while (lineStartIndex > 0
               && leadingTrivia[lineStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            lineStartIndex--;
        }

        var newToken = token.WithLeadingTrivia(leadingTrivia.Insert(lineStartIndex, SyntaxFactory.EndOfLine(_endOfLine)));

        return node.ReplaceToken(token, newToken);
    }

    /// <summary>
    /// Finds the last structured trivia (a documentation comment or a directive) in a leading trivia list. The search is
    /// confined to the list itself, so it does not depend on whether the preceding code lies inside the node being
    /// rewritten
    /// </summary>
    /// <param name="leadingTrivia">The leading trivia to search</param>
    /// <returns>The index of the last structured trivia; <c>-1</c> when the list carries none</returns>
    private static int FindLastStructuredTriviaIndex(SyntaxTriviaList leadingTrivia)
    {
        for (var triviaIndex = leadingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (leadingTrivia[triviaIndex].HasStructure)
            {
                return triviaIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Moves a token onto its own line when no line break separates it from the previous token. A gap that already holds
    /// a line break is left as it is, together with its blank lines, which the blank-line phase decided (FMT-BREAK-030).
    /// The only exception is a gap that ends with a documentation comment, see <see cref="TryPlaceTokenBehindLeadingDocumentationComment"/>
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token that should start its own line</param>
    /// <param name="withToken">Function that updates the token on the owning node</param>
    /// <param name="trimPreviousWhitespace">Whether the trailing whitespace of a previous token inside the node is removed</param>
    /// <returns>The updated node</returns>
    private TNode PlaceTokenOnOwnLine<TNode>(TNode node,
                                             SyntaxToken token,
                                             Func<TNode, SyntaxToken, TNode> withToken,
                                             bool trimPreviousWhitespace)
        where TNode : SyntaxNode
    {
        token = TokenLocator.GetCurrentToken(node, token);

        if (token.IsMissing || TokenContainmentUtilities.ContainsToken(node, token) == false)
        {
            return node;
        }

        // A token with no predecessor (the first token of the formatted root) has no gap to place it in
        if (TokenLocator.TryGetPreviousToken(node, token, out var previousToken) == false)
        {
            return node;
        }

        if (TryPlaceTokenBehindLeadingDocumentationComment(token, out var placedToken))
        {
            return placedToken == token
                       ? node
                       : withToken(node, placedToken);
        }

        var previousInsideNode = TokenContainmentUtilities.ContainsToken(node, previousToken);

        if (previousInsideNode)
        {
            previousToken = TokenLocator.GetCurrentToken(node, previousToken);
        }

        if (TokenGapUtilities.HasLineBreakBetween(previousToken, token))
        {
            return node;
        }

        // Without a line break in the gap, the gap holds no blank line either; one line break in front of its trivia
        // moves the token, and every comment glued to it, onto a new line
        var newToken = token.WithLeadingTrivia(token.LeadingTrivia.Insert(0, SyntaxFactory.EndOfLine(_endOfLine)));

        if (previousInsideNode == false
            || trimPreviousWhitespace == false)
        {
            return withToken(node, newToken);
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(previousToken.TrailingTrivia));

        return node.ReplaceTokens(new[] { previousToken, token },
                                  (originalToken, _) => originalToken == previousToken
                                                            ? newPreviousToken
                                                            : newToken);
    }

    /// <summary>
    /// Places a token whose leading trivia ends with a documentation comment, optionally followed by ordinary comments and
    /// nothing else but line breaks and indentation. The line break in the gap may sit inside the documentation comment, so
    /// the gap holding one does not mean the token starts its line: comments glued to the token behind other code do not
    /// start a line, and the token is moved onto its own line behind them. When the token already starts its line, or the
    /// comments that start their own line are glued to it, the gap is left as it is, with its blank lines
    /// </summary>
    /// <param name="token">The token to place</param>
    /// <param name="placedToken">The placed token, or <paramref name="token"/> when the gap stays unchanged</param>
    /// <returns><see langword="true"/> if the gap was decided here; <see langword="false"/> if the regular placement applies</returns>
    private bool TryPlaceTokenBehindLeadingDocumentationComment(SyntaxToken token, out SyntaxToken placedToken)
    {
        placedToken = token;

        var structuredTriviaIndex = FindLastStructuredTriviaIndex(token.LeadingTrivia);

        if (structuredTriviaIndex < 0
            || SyntaxTriviaUtilities.IsDocumentationCommentTrivia(token.LeadingTrivia[structuredTriviaIndex]) == false)
        {
            return false;
        }

        var lastContentIndex = structuredTriviaIndex;
        var trailingRunHasLineBreak = false;

        for (var triviaIndex = structuredTriviaIndex + 1; triviaIndex < token.LeadingTrivia.Count; triviaIndex++)
        {
            var trivia = token.LeadingTrivia[triviaIndex];

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                trailingRunHasLineBreak = true;

                continue;
            }

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            if (TriviaListUtilities.IsOrdinaryComment(trivia) == false)
            {
                return false;
            }

            lastContentIndex = triviaIndex;
            trailingRunHasLineBreak = false;
        }

        if (trailingRunHasLineBreak
            || SyntaxTriviaUtilities.EndsWithLineBreak(token.LeadingTrivia[lastContentIndex])
            || TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, TriviaListUtilities.FindFirstContentIndexOnLine(token.LeadingTrivia, lastContentIndex + 1)))
        {
            return true;
        }

        placedToken = token.WithLeadingTrivia(token.LeadingTrivia.Insert(lastContentIndex + 1, SyntaxFactory.EndOfLine(_endOfLine)));

        return true;
    }

    #endregion // Methods
}