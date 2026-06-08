using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Stateless helpers for resolving tokens relative to a syntax node during line-break normalization
/// </summary>
internal static class TokenLocator
{
    #region Methods

    /// <summary>
    /// Attempts to resolve the token that immediately precedes the specified token within the given syntax node
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The syntax node that contains the tokens</param>
    /// <param name="token">The token whose predecessor should be resolved</param>
    /// <param name="previousToken">Receives the previous token when one exists</param>
    /// <returns><see langword="true"/> if a previous token was found; otherwise, <see langword="false"/></returns>
    public static bool TryGetPreviousToken<TNode>(TNode node,
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
    public static bool ContainsToken(SyntaxNode node,
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
    public static SyntaxToken GetCurrentToken(SyntaxNode node,
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

    #endregion // Methods
}
