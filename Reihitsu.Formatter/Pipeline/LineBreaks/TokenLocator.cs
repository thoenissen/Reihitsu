using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Stateless helpers for resolving tokens relative to a syntax node during line-break normalization
/// </summary>
/// <remarks>
/// Invariant for line-break phases: never carry a raw <see cref="SyntaxToken"/> across a tree mutation.
/// Every Roslyn edit (<c>ReplaceToken</c>/<c>ReplaceTokens</c>/<c>ReplaceNode</c>) returns a detached node
/// whose token positions are reset into the node's own coordinate space, so a token captured before an edit
/// holds a stale span. Re-resolve the token from the current node before each edit, either by a structural
/// selector (a getter such as <c>n =&gt; n.OpenBraceToken</c>) or by a <see cref="SyntaxAnnotation"/> via
/// <see cref="GetAnnotatedNode{TNode}"/> — never by trusting a previously captured span start. Resolving by
/// position alone can match a different token of the same kind whose span start coincides after the edit
/// (issue #306, generalized in #329); <see cref="GetCurrentToken"/> therefore refuses such positional guesses
/// </remarks>
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
    /// Resolves the token in <paramref name="node"/> that is identical to <paramref name="token"/>.
    /// Resolution is identity based: a token is only returned when it is the very token passed in, so a
    /// stale token whose span start coincides with an unrelated token of the same kind is never substituted
    /// (see the class remarks and issue #306 / #329). When the token cannot be confirmed as current, the
    /// original token is returned unchanged rather than guessing by position
    /// </summary>
    /// <param name="node">The syntax node containing the token</param>
    /// <param name="token">The token to refresh</param>
    /// <returns>The token from the node when it is confirmed current; otherwise, the original token</returns>
    public static SyntaxToken GetCurrentToken(SyntaxNode node,
                                              SyntaxToken token)
    {
        if (ContainsToken(node, token) == false)
        {
            return token;
        }

        var currentToken = node.FindToken(token.SpanStart, findInsideTrivia: true);

        // Only an identity match is trusted. The previous (RawKind, SpanStart) comparison accepted a
        // different same-kind token whenever an earlier edit reflowed the tree and pushed another token of
        // that kind onto the stale span start, which is the latent corruption #329 retires. Anything other
        // than the original token is rejected; callers must re-resolve through a selector or annotation
        // before editing instead of carrying a raw token across a mutation.
        if (currentToken == token)
        {
            return currentToken;
        }

        return token;
    }

    /// <summary>
    /// Resolves the current annotated node of the requested type from the given tree.
    /// Annotations survive tree edits, so this returns the up-to-date node even after earlier edits
    /// shifted token positions
    /// </summary>
    /// <typeparam name="TNode">The expected syntax node type</typeparam>
    /// <param name="root">The syntax node to search</param>
    /// <param name="annotation">The annotation identifying the node</param>
    /// <returns>The current annotated node if found; otherwise, <see langword="null"/></returns>
    public static TNode GetAnnotatedNode<TNode>(SyntaxNode root,
                                                SyntaxAnnotation annotation)
        where TNode : SyntaxNode
    {
        foreach (var annotatedNode in root.GetAnnotatedNodes(annotation))
        {
            if (annotatedNode is TNode typedNode)
            {
                return typedNode;
            }
        }

        return null;
    }

    #endregion // Methods
}