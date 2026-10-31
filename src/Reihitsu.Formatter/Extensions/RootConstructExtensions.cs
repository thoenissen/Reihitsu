using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data.Enumerations;

namespace Reihitsu.Formatter.Extensions;

/// <summary>
/// Reads the <see cref="RootConstruct"/> facts at the formatting root
/// </summary>
internal static class RootConstructExtensions
{
    #region Methods

    /// <summary>
    /// Determines whether the specified node is the formatting root and the facts say that the root stands in the given
    /// construct in its document. A decision that reads a node's ancestors reads this at the root, which has no parent
    /// (FMT-PIPE-023); for every other node, the ancestors are part of the root and are read directly
    /// </summary>
    /// <param name="facts">The facts the entry point supplied</param>
    /// <param name="node">The node, as it entered the current rewriter</param>
    /// <param name="construct">The construct flag</param>
    /// <returns><see langword="true"/> if the node has no parent and the facts hold the flag</returns>
    public static bool IsRootIn(this RootConstruct facts, SyntaxNode node, RootConstruct construct)
    {
        return node.Parent == null
               && (facts & construct) != RootConstruct.None;
    }

    #endregion // Methods
}