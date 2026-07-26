using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Safety checks shared by the declaration and accessor reordering guards
/// </summary>
public static class OrderingMoveSafety
{
    #region Methods

    /// <summary>
    /// Determines whether moving a node in front of an earlier node of the same list would relocate a preprocessor
    /// directive away from the code it governs.
    /// The move relocates the whole span of the moved node in front of the nodes it jumps over, so both halves are
    /// inspected separately: a directive whose partner lies outside the moved span would be torn away from it, and a
    /// directive whose partner lies outside the crossed span means the moved node changes sides of a conditional or
    /// region boundary. A directive pair that sits complete inside either half travels intact and is therefore safe.
    /// The whole span of each half is inspected rather than the leading trivia of its first token, because a
    /// directive placed after an attribute list or a modifier attaches to a later token
    /// </summary>
    /// <typeparam name="TNode">List node type</typeparam>
    /// <param name="root">Syntax node containing the list</param>
    /// <param name="nodes">List the move operates on</param>
    /// <param name="nodeToMove">Node to move</param>
    /// <param name="targetNode">Node the moved node should precede</param>
    /// <returns><see langword="true"/> if the move would relocate a preprocessor directive</returns>
    public static bool MoveRangeContainsDirectives<TNode>(SyntaxNode root, SyntaxList<TNode> nodes, TNode nodeToMove, TNode targetNode)
        where TNode : SyntaxNode
    {
        var nodeToMoveIndex = nodes.IndexOf(nodeToMove);
        var targetNodeIndex = nodes.IndexOf(targetNode);

        if (nodeToMoveIndex < 0
            || targetNodeIndex < 0
            || nodeToMoveIndex <= targetNodeIndex)
        {
            return false;
        }

        var movedSpan = nodes[nodeToMoveIndex].FullSpan;
        var crossedSpan = TextSpan.FromBounds(nodes[targetNodeIndex].FullSpan.Start, movedSpan.Start);

        return RelocationChangesDirectiveScope(root, crossedSpan)
               || RelocationChangesDirectiveScope(root, movedSpan);
    }

    /// <summary>
    /// Determines whether relocating the specified span as one block would change which code a preprocessor
    /// directive governs
    /// </summary>
    /// <param name="root">Syntax node containing the span</param>
    /// <param name="span">Span to inspect</param>
    /// <returns><see langword="true"/> if relocating the span would change a directive scope</returns>
    private static bool RelocationChangesDirectiveScope(SyntaxNode root, TextSpan span)
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(root, span)
               || SyntaxTriviaUtilities.ContainsUnbalancedRegionDirectives(root, span)
               || SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives(root, span);
    }

    #endregion // Methods
}