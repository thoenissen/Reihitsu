using System.Collections.Generic;
using System.Text;

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
    /// Creates the text change that moves a node in front of an earlier node of the same list as a move of whole lines
    /// (SHARED-ORDER-009). The move is refused when the moved node shares a line with a token outside it, or a token
    /// stands in front of the target node on the line of its first token. Every blank-line separator stays at the
    /// position it occupied (SHARED-ORDER-002): the whole blank lines in front of each node's first comment,
    /// documentation comment, directive, or token belong to the position, and the lines from there to the end of the
    /// node move with the node. Each line keeps its column, except that every line of the moved node moves by the
    /// shift of its first token to the column of the target node's first token (SHARED-FIX-007); a line
    /// that holds nothing but comments, the first of them in column 0, and the line of a directive other than
    /// <c>#region</c> and <c>#endregion</c>, keep their column. A shift is refused when the moved node holds a line that
    /// starts inside a string of any kind or a comment that spans lines, or a line of disabled text, or when a left shift
    /// would cut a line off at column 0 (SHARED-ORDER-010). Crossed nodes that share a line move together as one block. The
    /// move is also refused when the leading trivia of the moved node or of the target starts behind code, such as a
    /// documentation comment written behind the code in front of it (SHARED-ORDER-009, SHARED-DOC-004), and when the
    /// lines it moves hold a line break other than the document's line ending, which the move would keep as written
    /// (SHARED-FIX-008)
    /// </summary>
    /// <typeparam name="TNode">List node type</typeparam>
    /// <param name="nodes">List the move operates on</param>
    /// <param name="nodeToMove">Node to move</param>
    /// <param name="targetNode">Node the moved node should precede</param>
    /// <param name="change">The text change that performs the move</param>
    /// <returns><see langword="true"/> if the move is a move of whole lines that can be performed</returns>
    public static bool TryCreateWholeLineMove<TNode>(SyntaxList<TNode> nodes, TNode nodeToMove, TNode targetNode, out TextChange change)
        where TNode : SyntaxNode
    {
        change = default;

        var nodeToMoveIndex = nodes.IndexOf(nodeToMove);
        var targetNodeIndex = nodes.IndexOf(targetNode);

        if (nodeToMoveIndex < 0
            || targetNodeIndex < 0
            || nodeToMoveIndex <= targetNodeIndex)
        {
            return false;
        }

        var root = nodeToMove.SyntaxTree.GetRoot();
        var text = nodeToMove.SyntaxTree.GetText();
        var lines = text.Lines;

        if (StartsItsLine(targetNode, lines) == false
            || IsLineStart(targetNode.FullSpan.Start, lines) == false
            || StartsItsLine(nodeToMove, lines) == false
            || IsLineStart(nodeToMove.FullSpan.Start, lines) == false
            || EndsItsLine(nodeToMove, lines) == false
            || LineEndingUtilities.HoldsOtherLineBreak(text, TextSpan.FromBounds(targetNode.FullSpan.Start, nodeToMove.FullSpan.End), LineEndingUtilities.DetectEndOfLine(root)))
        {
            return false;
        }

        var shift = GetColumn(targetNode.GetFirstToken(), lines) - GetColumn(nodeToMove.GetFirstToken(), lines);
        var movedBodyStart = GetBodyStart(nodeToMove, lines);

        if (SyntaxIndentationUtilities.TryShiftMovedLines(root, text, TextSpan.FromBounds(movedBodyStart, nodeToMove.FullSpan.End), shift, out var shiftedText) == false)
        {
            return false;
        }

        var gaps = new List<TextSpan>();
        var bodies = new List<string>();

        for (var index = targetNodeIndex; index < nodeToMoveIndex; index++)
        {
            var node = nodes[index];

            if (index > targetNodeIndex
                && IsLineStart(node.FullSpan.Start, lines) == false)
            {
                bodies[bodies.Count - 1] += node.ToFullString();

                continue;
            }

            var bodyStart = GetBodyStart(node, lines);

            gaps.Add(TextSpan.FromBounds(node.FullSpan.Start, bodyStart));
            bodies.Add(text.ToString(TextSpan.FromBounds(bodyStart, node.FullSpan.End)));
        }

        gaps.Add(TextSpan.FromBounds(nodeToMove.FullSpan.Start, movedBodyStart));

        var builder = new StringBuilder(text.ToString(gaps[0]));

        builder.Append(shiftedText);

        for (var index = 1; index < gaps.Count; index++)
        {
            builder.Append(text.ToString(gaps[index]));
            builder.Append(bodies[index - 1]);
        }

        change = new TextChange(TextSpan.FromBounds(targetNode.FullSpan.Start, nodeToMove.FullSpan.End), builder.ToString());

        return true;
    }

    /// <summary>
    /// Creates the text change that moves a node in front of an earlier node of a list that stands on one line, as a swap
    /// within that line (SHARED-ORDER-009): the nodes take each other's places, and the text between them stays at its
    /// position, so no line breaks, joins, or moves
    /// </summary>
    /// <typeparam name="TNode">List node type</typeparam>
    /// <param name="nodes">List the move operates on</param>
    /// <param name="nodeToMove">Node to move</param>
    /// <param name="targetNode">Node the moved node should precede</param>
    /// <param name="change">The text change that performs the swap</param>
    /// <returns><see langword="true"/> if the swap can be performed</returns>
    public static bool TryCreateSwapWithinLine<TNode>(SyntaxList<TNode> nodes, TNode nodeToMove, TNode targetNode, out TextChange change)
        where TNode : SyntaxNode
    {
        change = default;

        var nodeToMoveIndex = nodes.IndexOf(nodeToMove);
        var targetNodeIndex = nodes.IndexOf(targetNode);

        if (nodeToMoveIndex < 0
            || targetNodeIndex < 0
            || nodeToMoveIndex <= targetNodeIndex)
        {
            return false;
        }

        var text = nodeToMove.SyntaxTree.GetText();
        var builder = new StringBuilder(text.ToString(nodeToMove.Span));

        for (var index = targetNodeIndex; index < nodeToMoveIndex; index++)
        {
            builder.Append(text.ToString(TextSpan.FromBounds(nodes[index].Span.End, nodes[index + 1].Span.Start)));
            builder.Append(text.ToString(nodes[index].Span));
        }

        change = new TextChange(TextSpan.FromBounds(targetNode.Span.Start, nodeToMove.Span.End), builder.ToString());

        return true;
    }

    /// <summary>
    /// Determines whether a node starts its line: no token stands in front of its first token on that token's line
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="lines">Lines of the document</param>
    /// <returns><see langword="true"/> if the node starts its line</returns>
    private static bool StartsItsLine(SyntaxNode node, TextLineCollection lines)
    {
        var firstToken = node.GetFirstToken();
        var previousToken = firstToken.GetPreviousToken();

        return previousToken.IsKind(SyntaxKind.None)
               || lines.GetLineFromPosition(previousToken.Span.End).LineNumber < lines.GetLineFromPosition(firstToken.SpanStart).LineNumber;
    }

    /// <summary>
    /// Determines whether a position starts a line
    /// </summary>
    /// <param name="position">Position</param>
    /// <param name="lines">Lines of the document</param>
    /// <returns><see langword="true"/> if the position starts a line</returns>
    private static bool IsLineStart(int position, TextLineCollection lines)
    {
        return lines.GetLineFromPosition(position).Start == position;
    }

    /// <summary>
    /// Determines whether a node ends its line: its full span ends with a line break, and no token stands behind its last
    /// token on that token's line
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="lines">Lines of the document</param>
    /// <returns><see langword="true"/> if the node ends its line</returns>
    private static bool EndsItsLine(SyntaxNode node, TextLineCollection lines)
    {
        var lastToken = node.GetLastToken();
        var nextToken = lastToken.GetNextToken();

        return IsLineStart(node.FullSpan.End, lines)
               && (nextToken.IsKind(SyntaxKind.None)
                   || lines.GetLineFromPosition(nextToken.SpanStart).LineNumber > lines.GetLineFromPosition(lastToken.Span.End).LineNumber);
    }

    /// <summary>
    /// Gets the start of the line that holds a node's first comment, documentation comment, directive, or disabled text in
    /// its leading trivia, or its first token when it has none; the whole lines in front of it are the node's separator
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="lines">Lines of the document</param>
    /// <returns>The start of the node's first line</returns>
    private static int GetBodyStart(SyntaxNode node, TextLineCollection lines)
    {
        var leadingTrivia = node.GetLeadingTrivia();
        var firstSignificantIndex = SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(leadingTrivia);
        var position = firstSignificantIndex < 0
                           ? node.SpanStart
                           : leadingTrivia[firstSignificantIndex].SpanStart;

        return lines.GetLineFromPosition(position).Start;
    }

    /// <summary>
    /// Gets the column of a token in its line, each character counted as one column
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="lines">Lines of the document</param>
    /// <returns>The column of the token</returns>
    private static int GetColumn(SyntaxToken token, TextLineCollection lines)
    {
        return token.SpanStart - lines.GetLineFromPosition(token.SpanStart).Start;
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