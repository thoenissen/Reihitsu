using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    /// region boundary. A conditional or region pair that sits complete inside either half travels intact and is
    /// therefore safe (SHARED-ORDER-006). Every other directive refuses the move, unless it is a <c>#pragma warning</c>
    /// or <c>#nullable</c> pair that ends its own effect inside one node of the list
    /// (<see cref="ContainsPositionSensitiveDirectivesThatBlockRelocation"/>, SHARED-ORDER-011).
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

        return RelocationChangesDirectiveScope(root, nodes, crossedSpan, crossedSpan.Start)
               || RelocationChangesDirectiveScope(root, nodes, movedSpan, crossedSpan.Start);
    }

    /// <summary>
    /// Determines whether a span that a reorder relocates holds a directive that is neither a conditional nor a region
    /// directive and that the relocation may not carry (SHARED-ORDER-011). A <c>#pragma warning</c> or <c>#nullable</c>
    /// directive is carried when it stands behind the first token of one of the declarations that lie wholly in the
    /// span, the directives of its kind in that declaration form closed pairs (<see cref="FormClosedPairs"/>), and the
    /// directives of its kind in the document in front of the reordered spans form closed pairs as well, so that the
    /// project's setting is in effect where the spans begin. Every other such directive is not carried. Each condition
    /// is read from where a directive stands and what it names; no condition is evaluated, and a directive in an
    /// excluded branch counts like an active one
    /// </summary>
    /// <param name="root">Syntax node containing the span</param>
    /// <param name="span">Span that the reorder relocates</param>
    /// <param name="declarations">Declarations of the list the reorder changes</param>
    /// <param name="spansStart">Start of the reordered spans; the directives in front of it are read in the whole document</param>
    /// <returns><see langword="true"/> if the span holds a directive the relocation may not carry, or when <paramref name="root"/> is <see langword="null"/></returns>
    public static bool ContainsPositionSensitiveDirectivesThatBlockRelocation(SyntaxNode root, TextSpan span, IEnumerable<SyntaxNode> declarations, int spansStart)
    {
        if (root == null)
        {
            return true;
        }

        var declarationList = declarations.ToList();

        return SyntaxTriviaUtilities.GetPositionSensitiveDirectives(root, span)
                                    .Any(trivia => IsCarriedByItsDeclaration(trivia, span, declarationList, spansStart) == false);
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
        var previousToken = SyntaxTokenNavigationUtilities.GetPreviousToken(firstToken);

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
        var nextToken = SyntaxTokenNavigationUtilities.GetNextToken(lastToken);

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
    /// <typeparam name="TNode">List node type</typeparam>
    /// <param name="root">Syntax node containing the span</param>
    /// <param name="nodes">List the move operates on</param>
    /// <param name="span">Span to inspect</param>
    /// <param name="spansStart">Start of the crossed span, at the target node's first trivia</param>
    /// <returns><see langword="true"/> if relocating the span would change a directive scope</returns>
    private static bool RelocationChangesDirectiveScope<TNode>(SyntaxNode root, SyntaxList<TNode> nodes, TextSpan span, int spansStart)
        where TNode : SyntaxNode
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(root, span)
               || SyntaxTriviaUtilities.ContainsUnbalancedRegionDirectives(root, span)
               || ContainsPositionSensitiveDirectivesThatBlockRelocation(root, span, nodes, spansStart);
    }

    /// <summary>
    /// Determines whether a <c>#pragma warning</c> or <c>#nullable</c> directive in a relocated span is carried with its
    /// declaration (SHARED-ORDER-011): it stands in the leading trivia of a token of a declaration that lies wholly in the
    /// span, behind the declaration's first token, the directives of its kind in that declaration form closed pairs, and
    /// so do the directives of its kind in the document in front of the reordered spans
    /// </summary>
    /// <param name="trivia">Directive trivia</param>
    /// <param name="span">Relocated span</param>
    /// <param name="declarations">Declarations of the list the reorder changes</param>
    /// <param name="spansStart">Start of the reordered spans</param>
    /// <returns><see langword="true"/> if the directive is carried with its declaration</returns>
    private static bool IsCarriedByItsDeclaration(SyntaxTrivia trivia, TextSpan span, IReadOnlyList<SyntaxNode> declarations, int spansStart)
    {
        if (trivia.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia) == false
            && trivia.IsKind(SyntaxKind.NullableDirectiveTrivia) == false)
        {
            return false;
        }

        var tokenStart = trivia.Token.SpanStart;
        var declaration = declarations.FirstOrDefault(node => node.Span.Contains(tokenStart));

        if (declaration == null
            || span.Contains(declaration.Span) == false
            || tokenStart == declaration.SpanStart)
        {
            return false;
        }

        var documentRoot = trivia.SyntaxTree.GetRoot();

        return FormClosedPairs(GetDirectivesOfKind(declaration, declaration.Span, trivia.Kind()))
               && FormClosedPairs(GetDirectivesOfKind(documentRoot, TextSpan.FromBounds(0, spansStart), trivia.Kind()));
    }

    /// <summary>
    /// Gets the directives of one kind that start inside a span, active ones and those in an excluded branch alike, in
    /// the order of the text
    /// </summary>
    /// <param name="node">Node containing the span</param>
    /// <param name="span">Span</param>
    /// <param name="kind">Directive kind</param>
    /// <returns>The directives</returns>
    private static List<DirectiveTriviaSyntax> GetDirectivesOfKind(SyntaxNode node, TextSpan span, SyntaxKind kind)
    {
        return node.DescendantTrivia(span)
                   .Where(trivia => span.Contains(trivia.SpanStart) && trivia.IsKind(kind))
                   .Select(trivia => trivia.GetStructure())
                   .OfType<DirectiveTriviaSyntax>()
                   .ToList();
    }

    /// <summary>
    /// Determines whether directives of one kind, in the order of the text, form consecutive closed pairs
    /// (SHARED-ORDER-011): each opening directive is followed directly by the directive that closes it, and every
    /// conditional directive between the two belongs to a group that lies wholly between them
    /// </summary>
    /// <param name="directives">Directives of one kind, in the order of the text</param>
    /// <returns><see langword="true"/> if the directives form closed pairs, also when there is none</returns>
    private static bool FormClosedPairs(IReadOnlyList<DirectiveTriviaSyntax> directives)
    {
        if (directives.Count % 2 != 0)
        {
            return false;
        }

        for (var index = 0; index < directives.Count; index += 2)
        {
            if (Closes(directives[index], directives[index + 1]) == false
                || SplitsConditionalGroup(directives[index], directives[index + 1]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether a directive closes an opening directive (SHARED-ORDER-011): a <c>#pragma warning restore</c>
    /// that names the diagnostic IDs of a <c>#pragma warning disable</c> in the same order, each written as there, or
    /// none where it names none, or a <c>#nullable restore</c> with the target of a <c>#nullable enable</c> or
    /// <c>#nullable disable</c>
    /// </summary>
    /// <param name="opening">Opening directive</param>
    /// <param name="closing">Closing directive</param>
    /// <returns><see langword="true"/> if the second directive closes the first</returns>
    private static bool Closes(DirectiveTriviaSyntax opening, DirectiveTriviaSyntax closing)
    {
        return (opening, closing) switch
               {
                   (PragmaWarningDirectiveTriviaSyntax disable, PragmaWarningDirectiveTriviaSyntax restore) => disable.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword)
                                                                                                               && restore.DisableOrRestoreKeyword.IsKind(SyntaxKind.RestoreKeyword)
                                                                                                               && disable.ErrorCodes.Select(code => code.ToString()).SequenceEqual(restore.ErrorCodes.Select(code => code.ToString())),
                   (NullableDirectiveTriviaSyntax change, NullableDirectiveTriviaSyntax restore) => (change.SettingToken.IsKind(SyntaxKind.EnableKeyword) || change.SettingToken.IsKind(SyntaxKind.DisableKeyword))
                                                                                                    && restore.SettingToken.IsKind(SyntaxKind.RestoreKeyword)
                                                                                                    && restore.TargetToken.IsKind(change.TargetToken.Kind()),
                   _ => false,
               };
    }

    /// <summary>
    /// Determines whether a conditional directive between two directives belongs to a group that does not lie wholly
    /// between them, so that a build can compile one of the two without the other
    /// </summary>
    /// <param name="opening">Opening directive</param>
    /// <param name="closing">Closing directive</param>
    /// <returns><see langword="true"/> if a conditional group is split by the pair</returns>
    private static bool SplitsConditionalGroup(DirectiveTriviaSyntax opening, DirectiveTriviaSyntax closing)
    {
        var between = TextSpan.FromBounds(opening.Span.End, closing.SpanStart);

        return ConditionalGroupUnitUtilities.GetConditionalDirectives(opening.SyntaxTree.GetRoot(), between)
                                            .Any(directive => directive.GetRelatedDirectives()
                                                                       .Any(related => between.Contains(related.SpanStart) == false));
    }

    #endregion // Methods
}