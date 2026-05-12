using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Shared base syntax rewriter for focused line-break subphases.
/// Only manipulates <see cref="SyntaxKind.EndOfLineTrivia"/>; does not set indentation
/// </summary>
internal abstract class LineBreakRewriter : CSharpSyntaxRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected LineBreakRewriter(FormattingContext context,
                                CancellationToken cancellationToken)
    {
        Context = context;
        CancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Gets the formatting context
    /// </summary>
    protected FormattingContext Context { get; }

    /// <summary>
    /// Gets the cancellation token
    /// </summary>
    protected CancellationToken CancellationToken { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Determines whether an accessor list belongs to an auto-property
    /// (all accessors have neither a body nor an expression body)
    /// </summary>
    /// <param name="accessorList">The accessor list to inspect</param>
    /// <returns><see langword="true"/> if the accessor list is part of an auto-property; otherwise, <see langword="false"/></returns>
    protected static bool IsAutoPropertyAccessorList(AccessorListSyntax accessorList)
    {
        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Collapses a token to the same line as the previous token by removing any
    /// end-of-line trivia from both the token's leading trivia and the previous
    /// token's trailing trivia
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node containing the token</param>
    /// <param name="token">The token to collapse to the previous line</param>
    /// <returns>The node with the token collapsed to the same line</returns>
    protected static TNode CollapseTokenToSameLine<TNode>(TNode node,
                                                          SyntaxToken token)
        where TNode : SyntaxNode
    {
        var newToken = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(token);
        var hasPreviousToken = TryGetPreviousToken(node, token, out var previousToken);

        if (hasPreviousToken && LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken))
        {
            var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

            return node.ReplaceTokens([previousToken, token],
                                      (original, _) =>
                                      {
                                          if (original == previousToken)
                                          {
                                              return newPreviousToken;
                                          }

                                          return newToken;
                                      });
        }

        return node.ReplaceToken(token, newToken);
    }

    /// <summary>
    /// Determines whether a syntax node spans multiple lines
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node spans multiple lines; otherwise, <see langword="false"/></returns>
    protected static bool IsMultiLine(SyntaxNode node)
    {
        var text = node.GetText();

        return text.Lines.Count > 1;
    }

    /// <summary>
    /// Attempts to resolve the token that immediately precedes the specified token within the given syntax node
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The syntax node that contains the tokens</param>
    /// <param name="token">The token whose predecessor should be resolved</param>
    /// <param name="previousToken">Receives the previous token when one exists</param>
    /// <returns><see langword="true"/> if a previous token was found; otherwise, <see langword="false"/></returns>
    protected static bool TryGetPreviousToken<TNode>(TNode node,
                                                     SyntaxToken token,
                                                     out SyntaxToken previousToken)
        where TNode : SyntaxNode
    {
        previousToken = default;

        var lastToken = default(SyntaxToken);

        foreach (var currentToken in node.DescendantTokens(descendIntoTrivia: true))
        {
            if (currentToken == token)
            {
                if (lastToken != default && lastToken.IsKind(SyntaxKind.None) == false)
                {
                    previousToken = lastToken;

                    return true;
                }

                break;
            }

            if (currentToken.IsMissing == false)
            {
                lastToken = currentToken;
            }
        }

        previousToken = token.GetPreviousToken();

        while (previousToken != default
               && previousToken.IsKind(SyntaxKind.None) == false
               && previousToken.IsMissing)
        {
            previousToken = previousToken.GetPreviousToken();
        }

        return previousToken != default
               && previousToken.IsKind(SyntaxKind.None) == false
               && previousToken.IsMissing == false;
    }

    /// <summary>
    /// Determines whether the specified token is contained within the syntax node span
    /// </summary>
    /// <param name="node">The syntax node to inspect</param>
    /// <param name="token">The token to test</param>
    /// <returns><see langword="true"/> if the token lies within the node span; otherwise, <see langword="false"/></returns>
    protected static bool ContainsToken(SyntaxNode node,
                                        SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.None) == false
               && token.FullSpan.Start >= node.FullSpan.Start
               && token.FullSpan.End <= node.FullSpan.End;
    }

    /// <summary>
    /// Gets the current token from the specified node that corresponds to the given token span and kind
    /// </summary>
    /// <param name="node">The syntax node containing the token</param>
    /// <param name="token">The token to refresh</param>
    /// <returns>The current token from the node if it can be found; otherwise, the original token</returns>
    protected static SyntaxToken GetCurrentToken(SyntaxNode node,
                                                 SyntaxToken token)
    {
        if (ContainsToken(node, token) == false)
        {
            return token;
        }

        var currentToken = node.FindToken(token.SpanStart, findInsideTrivia: true);

        if (currentToken.RawKind == token.RawKind && currentToken.SpanStart == token.SpanStart)
        {
            return currentToken;
        }

        return token;
    }

    /// <summary>
    /// Moves a token to a new line by prepending an end-of-line trivia to its leading trivia.
    /// Also strips any trailing whitespace from the previous token to avoid orphaned spaces
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node containing the token</param>
    /// <param name="token">The token to move to a new line</param>
    /// <returns>The node with the token moved to a new line</returns>
    protected TNode MoveTokenToNewLine<TNode>(TNode node,
                                              SyntaxToken token)
        where TNode : SyntaxNode
    {
        var newToken = PrependEndOfLine(token);
        var previousToken = token.GetPreviousToken();

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken) == false
            && previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia))
        {
            if (ContainsToken(node, previousToken) == false)
            {
                return node.ReplaceToken(token, newToken);
            }

            var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(previousToken.TrailingTrivia));

            return node.ReplaceTokens([previousToken, token],
                                      (original, _) =>
                                      {
                                          if (original == previousToken)
                                          {
                                              return newPreviousToken;
                                          }

                                          return newToken;
                                      });
        }

        return node.ReplaceToken(token, newToken);
    }

    /// <summary>
    /// Normalizes the leading gap in a token to the requested number of blank lines
    /// </summary>
    /// <param name="token">The token whose leading trivia should be normalized</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <returns>The updated token</returns>
    protected SyntaxToken NormalizeLeadingGap(SyntaxToken token,
                                              int blankLineCount)
    {
        var suffixStart = 0;
        var lastLeadingEndOfLineIndex = -1;
        var sawNonWhitespaceTrivia = false;

        for (var triviaIndex = 0; triviaIndex < token.LeadingTrivia.Count; triviaIndex++)
        {
            var trivia = token.LeadingTrivia[triviaIndex];

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                lastLeadingEndOfLineIndex = triviaIndex;

                continue;
            }

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            sawNonWhitespaceTrivia = true;

            break;
        }

        if (sawNonWhitespaceTrivia || lastLeadingEndOfLineIndex >= 0)
        {
            suffixStart = lastLeadingEndOfLineIndex + 1;
        }

        var preservedLeadingTrivia = new List<SyntaxTrivia>(token.LeadingTrivia.Count - suffixStart);

        for (var triviaIndex = suffixStart; triviaIndex < token.LeadingTrivia.Count; triviaIndex++)
        {
            preservedLeadingTrivia.Add(token.LeadingTrivia[triviaIndex]);
        }

        var newLeadingTrivia = new List<SyntaxTrivia>(blankLineCount + preservedLeadingTrivia.Count + 1);

        for (var lineBreakIndex = 0; lineBreakIndex <= blankLineCount; lineBreakIndex++)
        {
            newLeadingTrivia.Add(SyntaxFactory.EndOfLine(Context.EndOfLine));
        }

        newLeadingTrivia.AddRange(preservedLeadingTrivia);

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newLeadingTrivia));
    }

    /// <summary>
    /// Normalizes the gap before a token without changing the previous token's trailing trivia
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token whose preceding gap should be normalized</param>
    /// <param name="withToken">Function that updates the token on the owning node</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <returns>The updated node</returns>
    protected TNode NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia<TNode>(TNode node,
                                                                                SyntaxToken token,
                                                                                Func<TNode, SyntaxToken, TNode> withToken,
                                                                                int blankLineCount)
        where TNode : SyntaxNode
    {
        token = GetCurrentToken(node, token);

        if (token.IsMissing || ContainsToken(node, token) == false)
        {
            return node;
        }

        var hasPreviousToken = TryGetPreviousToken(node, token, out var previousToken);
        var hasLineBreak = hasPreviousToken && TokenGapUtilities.HasLineBreakBetween(previousToken, token);
        var currentBlankLineCount = hasPreviousToken
                                        ? TokenGapUtilities.CountBlankLinesBetween(previousToken,
                                                                                   token)
                                        : 0;

        if (hasLineBreak && currentBlankLineCount == blankLineCount)
        {
            return node;
        }

        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (hasPreviousToken == false || ContainsToken(node, previousToken) == false)
        {
            return withToken(node, newToken);
        }

        previousToken = GetCurrentToken(node, previousToken);

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

        return node.ReplaceTokens(new[] { previousToken, token },
                                  (originalToken, _) => originalToken == previousToken
                                                            ? newPreviousToken
                                                            : newToken);
    }

    /// <summary>
    /// Normalizes the gap before a token owned directly by a syntax node, even when the previous token lies outside that node
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token whose preceding gap should be normalized</param>
    /// <param name="withToken">Function that updates the token on the owning node</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <returns>The updated node</returns>
    protected TNode NormalizeGapBeforeOwnedToken<TNode>(TNode node,
                                                        SyntaxToken token,
                                                        Func<TNode, SyntaxToken, TNode> withToken,
                                                        int blankLineCount)
        where TNode : SyntaxNode
    {
        token = GetCurrentToken(node, token);

        if (token.IsMissing || ContainsToken(node, token) == false)
        {
            return node;
        }

        var hasPreviousToken = TryGetPreviousToken(node, token, out var previousToken);

        if (hasPreviousToken && ContainsToken(node, previousToken))
        {
            previousToken = GetCurrentToken(node, previousToken);
        }

        var hasLineBreak = hasPreviousToken && TokenGapUtilities.HasLineBreakBetween(previousToken, token);
        var currentBlankLineCount = hasPreviousToken
                                        ? TokenGapUtilities.CountBlankLinesBetween(previousToken,
                                                                                   token)
                                        : 0;

        if (hasLineBreak && currentBlankLineCount == blankLineCount)
        {
            return node;
        }

        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (hasPreviousToken == false || ContainsToken(node, previousToken) == false)
        {
            return withToken(node, newToken);
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));

        return node.ReplaceTokens(new[] { previousToken, token },
                                  (originalToken, _) => originalToken == previousToken
                                                            ? newPreviousToken
                                                            : newToken);
    }

    /// <summary>
    /// Normalizes the gap before a token to the requested number of blank lines
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token whose preceding gap should be normalized</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <returns>The updated node</returns>
    protected TNode NormalizeGapBeforeToken<TNode>(TNode node,
                                                   SyntaxToken token,
                                                   int blankLineCount)
        where TNode : SyntaxNode
    {
        token = GetCurrentToken(node, token);

        if (TryGetPreviousToken(node, token, out var previousToken) == false)
        {
            return node;
        }

        if (ContainsToken(node, token) == false)
        {
            return node;
        }

        if (ContainsToken(node, previousToken))
        {
            previousToken = GetCurrentToken(node, previousToken);
        }

        var hasLineBreak = TokenGapUtilities.HasLineBreakBetween(previousToken, token);
        var currentBlankLineCount = TokenGapUtilities.CountBlankLinesBetween(previousToken, token);

        if (hasLineBreak && currentBlankLineCount == blankLineCount)
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (ContainsToken(node, previousToken) == false)
        {
            return node.ReplaceToken(token, newToken);
        }

        return node.ReplaceTokens(new[] { previousToken, token },
                                  (originalToken, _) =>
                                  {
                                      if (originalToken == previousToken)
                                      {
                                          return newPreviousToken;
                                      }

                                      return newToken;
                                  });
    }

    /// <summary>
    /// Ensures an opening brace is on its own line by prepending an end-of-line trivia if missing.
    /// Also ensures the closing brace is on its own line
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the braces</typeparam>
    /// <param name="node">The node containing the braces</param>
    /// <param name="openBrace">The open brace token</param>
    /// <param name="withOpenBrace">Function to replace the open brace on the node</param>
    /// <param name="closeBrace">The close brace token</param>
    /// <param name="withCloseBrace">Function to replace the close brace on the node</param>
    /// <returns>The node with braces placed on their own lines</returns>
    protected TNode EnsureBraceOnOwnLine<TNode>(TNode node,
                                                SyntaxToken openBrace,
                                                Func<TNode, SyntaxToken, TNode> withOpenBrace,
                                                SyntaxToken closeBrace,
                                                Func<TNode, SyntaxToken, TNode> withCloseBrace)
        where TNode : SyntaxNode
    {
        if (closeBrace.IsMissing == false)
        {
            node = NormalizeGapBeforeOwnedToken(node, closeBrace, withCloseBrace, blankLineCount: 0);
        }

        if (openBrace.IsMissing == false)
        {
            node = NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, openBrace, withOpenBrace, blankLineCount: 0);
        }

        return node;
    }

    /// <summary>
    /// Ensures the first token after an opening brace is on a new line.
    /// Also strips trailing whitespace from the open brace token
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The node containing the opening brace</param>
    /// <param name="openBrace">The opening brace token</param>
    /// <returns>The node with the first content token on a new line</returns>
    protected TNode EnsureFirstContentOnNewLine<TNode>(TNode node,
                                                       SyntaxToken openBrace)
        where TNode : SyntaxNode
    {
        if (openBrace.IsMissing)
        {
            return node;
        }

        var nextToken = openBrace.GetNextToken();

        if (nextToken == default || nextToken.IsMissing)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(nextToken))
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasTrailingEndOfLine(openBrace))
        {
            return node;
        }

        return MoveTokenToNewLine(node, nextToken);
    }

    /// <summary>
    /// Ensures a line break after a closing brace unless the next token is <c>;</c>, <c>,</c>, or <c>)</c>
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The node containing the closing brace</param>
    /// <param name="closeBrace">The closing brace token</param>
    /// <returns>The node with correct close-brace continuation</returns>
    protected TNode EnsureCloseBraceContinuation<TNode>(TNode node,
                                                        SyntaxToken closeBrace)
        where TNode : SyntaxNode
    {
        if (closeBrace.IsMissing)
        {
            return node;
        }

        var nextToken = closeBrace.GetNextToken();

        if (nextToken == default || nextToken.IsMissing)
        {
            return node;
        }

        if (nextToken.IsKind(SyntaxKind.SemicolonToken)
            || nextToken.IsKind(SyntaxKind.CommaToken)
            || nextToken.IsKind(SyntaxKind.CloseParenToken))
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(nextToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(closeBrace))
        {
            return NormalizeGapBeforeToken(node, nextToken, blankLineCount: 0);
        }

        var newNextToken = PrependEndOfLine(nextToken);

        return node.ReplaceToken(nextToken, newNextToken);
    }

    /// <summary>
    /// Prepends an end-of-line trivia to a token's leading trivia
    /// </summary>
    /// <param name="token">The token to modify</param>
    /// <returns>The token with an end-of-line trivia prepended to its leading trivia</returns>
    protected SyntaxToken PrependEndOfLine(SyntaxToken token)
    {
        var endOfLine = SyntaxFactory.EndOfLine(Context.EndOfLine);
        var newLeading = token.LeadingTrivia.Insert(0, endOfLine);

        return token.WithLeadingTrivia(newLeading);
    }

    /// <summary>
    /// Appends an end-of-line trivia to a trivia list
    /// </summary>
    /// <param name="triviaList">The trivia list to extend</param>
    /// <returns>The trivia list with an end-of-line trivia appended</returns>
    protected SyntaxTriviaList AppendEndOfLine(SyntaxTriviaList triviaList)
    {
        return triviaList.Add(SyntaxFactory.EndOfLine(Context.EndOfLine));
    }

    #endregion // Methods
}