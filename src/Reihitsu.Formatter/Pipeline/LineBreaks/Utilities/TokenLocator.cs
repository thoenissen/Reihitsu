using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

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
/// position alone can match a different token of the same kind whose span start coincides after the edit;
/// <see cref="GetCurrentToken"/> therefore refuses such positional guesses
/// </remarks>
internal static class TokenLocator
{
    #region Methods

    /// <summary>
    /// Attempts to resolve the token that immediately precedes the specified token within the given syntax node,
    /// including a token embedded in structured trivia (a directive or documentation comment) carried by the
    /// token's own leading trivia — the same predecessor a full <c>DescendantTokens(descendIntoTrivia: true)</c>
    /// walk of <paramref name="node"/> would yield, without scanning the whole node to find it
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
        var structuredTriviaToken = GetLastTokenInLeadingStructuredTrivia(token);

        if (structuredTriviaToken != default
            && structuredTriviaToken.IsKind(SyntaxKind.None) == false
            && ContainsToken(node, structuredTriviaToken))
        {
            previousToken = structuredTriviaToken;

            return true;
        }

        // includeZeroWidth: true, because a legitimate zero-width token (an omitted type argument in an
        // unbound generic such as "Dictionary<,>") is a real, non-missing token the original
        // DescendantTokens(descendIntoTrivia: true) walk always yielded, and Roslyn's own default skips it
        previousToken = token.GetPreviousToken(includeZeroWidth: true);

        while (previousToken != default
               && previousToken.IsKind(SyntaxKind.None) == false
               && previousToken.IsMissing)
        {
            previousToken = previousToken.GetPreviousToken(includeZeroWidth: true);
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
    /// (see the class remarks). When the token cannot be confirmed as current, the
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
        // that kind onto the stale span start, which is the latent corruption this identity check retires.
        // Anything other than the original token is rejected; callers must re-resolve through a selector or annotation
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

    /// <summary>
    /// Finds the last non-missing token carried by the closest piece of structured trivia (a directive or
    /// documentation comment) in a token's own leading trivia, scanning backward from the token
    /// </summary>
    /// <param name="token">The token whose leading trivia is inspected</param>
    /// <returns>The last non-missing token of the closest structured trivia; <see langword="default"/> when the leading trivia carries none</returns>
    private static SyntaxToken GetLastTokenInLeadingStructuredTrivia(SyntaxToken token)
    {
        var leadingTrivia = token.LeadingTrivia;

        for (var triviaIndex = leadingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (leadingTrivia[triviaIndex].HasStructure == false)
            {
                continue;
            }

            var lastToken = GetLastNonMissingToken(leadingTrivia[triviaIndex].GetStructure());

            if (lastToken != default && lastToken.IsKind(SyntaxKind.None) == false)
            {
                return lastToken;
            }
        }

        return default;
    }

    /// <summary>
    /// Finds the last non-missing token in a structured trivia node's own token sequence. The scan is bounded by
    /// the size of this one piece of trivia (a single directive line, one documentation comment) rather than by
    /// the size of the enclosing declaration
    /// </summary>
    /// <param name="structure">The structured trivia's syntax node</param>
    /// <returns>The last non-missing token; <see langword="default"/> when the structure carries none</returns>
    private static SyntaxToken GetLastNonMissingToken(SyntaxNode structure)
    {
        var lastToken = default(SyntaxToken);

        foreach (var candidate in structure.DescendantTokens(descendIntoTrivia: true))
        {
            if (candidate.IsMissing == false)
            {
                lastToken = candidate;
            }
        }

        return lastToken;
    }

    #endregion // Methods
}