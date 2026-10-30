using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Neutral questions about where a token lies relative to a node, which several phases ask
/// </summary>
internal static class TokenContainmentUtilities
{
    #region Methods

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

    #endregion // Methods
}