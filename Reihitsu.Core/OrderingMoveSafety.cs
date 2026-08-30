using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Safety checks and relocation mechanics shared by the declaration and accessor reordering guards and code fixes
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

        // The span analysis below walks the trivia of both halves. The green-node flag answers containers that hold
        // no directive at all without walking anything; note that a single #region anywhere in the container already
        // defeats it, so this only spares directive-free code. A null root falls through and is refused by the
        // predicates, which cannot inspect the spans without it
        if (root != null
            && root.ContainsDirectives == false)
        {
            return false;
        }

        var movedSpan = nodes[nodeToMoveIndex].FullSpan;
        var crossedSpan = TextSpan.FromBounds(nodes[targetNodeIndex].FullSpan.Start, movedSpan.Start);

        return RelocationChangesDirectiveScope(root, crossedSpan)
               || RelocationChangesDirectiveScope(root, movedSpan);
    }

    /// <summary>
    /// Moves a node before an earlier node of the same list while keeping every blank-line separator at the
    /// position it already occupied. A separator between two siblings is stored in the leading trivia of the
    /// sibling that follows it, so a plain remove-and-insert carries that trivia away with whichever node happens
    /// to own it, relocating or deleting the separator instead of leaving it where the author put it. Comments,
    /// documentation and directives in a node's leading trivia stay with that node; the whitespace and end-of-line
    /// run before them — which is a node's own indentation when it carries none of that content — is repositioned
    /// to whichever position it already belonged to, so a caller without a later formatting pass may need to
    /// re-indent the result
    /// </summary>
    /// <typeparam name="TNode">List node type</typeparam>
    /// <param name="nodes">List the move operates on</param>
    /// <param name="nodeToMove">Node to move</param>
    /// <param name="targetNode">Node the moved node should precede</param>
    /// <returns>The updated list, or the original list when the move is not forward-only or either node is missing</returns>
    public static SyntaxList<TNode> MoveNodeBeforePreservingSeparators<TNode>(SyntaxList<TNode> nodes, TNode nodeToMove, TNode targetNode)
        where TNode : SyntaxNode
    {
        var nodeToMoveIndex = nodes.IndexOf(nodeToMove);
        var targetNodeIndex = nodes.IndexOf(targetNode);

        if (nodeToMoveIndex < 0
            || targetNodeIndex < 0
            || nodeToMoveIndex <= targetNodeIndex)
        {
            return nodes;
        }

        var originalGaps = new SyntaxTriviaList[nodeToMoveIndex - targetNodeIndex + 1];

        for (var index = targetNodeIndex; index <= nodeToMoveIndex; index++)
        {
            originalGaps[index - targetNodeIndex] = GetLeadingGap(nodes[index]);
        }

        var reorderedNodes = new List<TNode>(nodes);
        var movedNode = reorderedNodes[nodeToMoveIndex];

        reorderedNodes.RemoveAt(nodeToMoveIndex);
        reorderedNodes.Insert(targetNodeIndex, movedNode);

        for (var index = targetNodeIndex; index <= nodeToMoveIndex; index++)
        {
            var node = reorderedNodes[index];
            var payload = GetLeadingPayload(node);

            reorderedNodes[index] = node.WithLeadingTrivia(originalGaps[index - targetNodeIndex].AddRange(payload));
        }

        return SyntaxFactory.List(reorderedNodes);
    }

    /// <summary>
    /// Gets the positional whitespace-and-end-of-line run at the very start of a node's leading trivia — the run
    /// up to its first comment, documentation comment, or directive, or the entire leading trivia list when none
    /// of those is present. This is the part of a separator — and, when nothing else is present, the node's own
    /// indentation — that belongs to the boundary between two siblings rather than to either sibling
    /// </summary>
    /// <param name="node">Node whose leading trivia to inspect</param>
    /// <returns>The positional leading trivia run</returns>
    private static SyntaxTriviaList GetLeadingGap(SyntaxNode node)
    {
        var leadingTrivia = node.GetLeadingTrivia();
        var firstSignificantIndex = SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(leadingTrivia);

        return firstSignificantIndex < 0
                   ? leadingTrivia
                   : SyntaxFactory.TriviaList(leadingTrivia.Take(firstSignificantIndex));
    }

    /// <summary>
    /// Gets the leading trivia that travels with a node when it is relocated — its comments, documentation
    /// comments, directives and disabled text, together with the indentation immediately preceding its first
    /// token. Empty when the node carries none of that content, since its whole leading trivia is then positional
    /// </summary>
    /// <param name="node">Node whose leading trivia to inspect</param>
    /// <returns>The trivia that must stay attached to the node</returns>
    private static SyntaxTriviaList GetLeadingPayload(SyntaxNode node)
    {
        var leadingTrivia = node.GetLeadingTrivia();
        var firstSignificantIndex = SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(leadingTrivia);

        return firstSignificantIndex < 0
                   ? SyntaxFactory.TriviaList()
                   : SyntaxFactory.TriviaList(leadingTrivia.Skip(firstSignificantIndex));
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