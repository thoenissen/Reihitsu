using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for generic syntax-node inspection
/// </summary>
public static class SyntaxNodeUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a node contains a preprocessor directive or a comment that is not a documentation
    /// comment. The name states the omission because it is a trap: <see cref="ContainsCommentOrDirective"/> is
    /// the ordinary predicate and the one any guard protecting a reshape that could discard trivia must use,
    /// since the formatter's own join guard <see cref="SyntaxTriviaUtilities.ContainsUnjoinableTrivia"/> counts
    /// documentation comments as comments. Use this one only where the formatter demonstrably reshapes across a
    /// documentation comment without losing it, so that a guard blocking on one would withhold a fix that works
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if a non-documentation comment or a directive is present; otherwise <see langword="false"/></returns>
    public static bool ContainsNonDocumentationCommentOrDirective(SyntaxNode node)
    {
        foreach (var trivia in node.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsDirective
                || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given trivia is a comment or a preprocessor directive
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is a comment or a directive; otherwise <see langword="false"/></returns>
    public static bool IsCommentOrDirective(SyntaxTrivia trivia)
    {
        return SyntaxTriviaUtilities.IsCommentTrivia(trivia) || trivia.IsDirective;
    }

    /// <summary>
    /// Determines whether the trivia between two tokens contains a comment
    /// </summary>
    /// <param name="firstToken">First token</param>
    /// <param name="secondToken">Second token (expected to follow <paramref name="firstToken"/>)</param>
    /// <returns><see langword="true"/> if a comment is present in the gap; otherwise <see langword="false"/></returns>
    public static bool GapContainsComment(SyntaxToken firstToken, SyntaxToken secondToken)
    {
        return firstToken.TrailingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia)
               || secondToken.LeadingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia);
    }

    /// <summary>
    /// Determines whether any comment trivia intersects the given span
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="span">Span</param>
    /// <returns><see langword="true"/> if a comment is present in the span; otherwise <see langword="false"/></returns>
    public static bool SpanContainsComment(SyntaxNode root, TextSpan span)
    {
        return root.DescendantTrivia(span, descendIntoTrivia: true)
                   .Any(SyntaxTriviaUtilities.IsCommentTrivia);
    }

    /// <summary>
    /// Determines whether a node contains a comment or a preprocessor directive. This is the node-scoped form
    /// of <see cref="IsCommentOrDirective"/>, next to the span-scoped <see cref="SpanContainsCommentOrDirective"/>.
    /// The comment set includes documentation comments, which is what every guard protecting a reshape needs: the
    /// formatter's own join guard <see cref="SyntaxTriviaUtilities.ContainsUnjoinableTrivia"/> counts them too, so a
    /// guard that omitted them would let a reshape delete a documentation comment or register a fix the formatter
    /// then refuses. The two sets are not identical — the formatter's guard additionally blocks on disabled text
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if a comment or directive is present; otherwise <see langword="false"/></returns>
    public static bool ContainsCommentOrDirective(SyntaxNode node)
    {
        return node.DescendantTrivia(descendIntoTrivia: true)
                   .Any(IsCommentOrDirective);
    }

    /// <summary>
    /// Determines whether any comment or preprocessor directive trivia intersects the given span
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="span">Span</param>
    /// <returns><see langword="true"/> if a comment or directive is present in the span; otherwise <see langword="false"/></returns>
    public static bool SpanContainsCommentOrDirective(SyntaxNode root, TextSpan span)
    {
        return root.DescendantTrivia(span, descendIntoTrivia: true)
                   .Any(IsCommentOrDirective);
    }

    /// <summary>
    /// Determines whether a node's own span contains a comment or a preprocessor directive, ignoring the
    /// leading and trailing trivia attached to it. <see cref="ContainsCommentOrDirective"/> walks the node's
    /// full trivia, so for a node that begins a member declaration — an attribute list, most commonly — it also
    /// counts that member's documentation comment, which sits in the leading trivia of the node's first token.
    /// A guard for a reshape that only rearranges the node's interior must use this predicate, or a documented
    /// member loses a fix that an undocumented one still gets
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node's own span contains a comment or directive; otherwise <see langword="false"/></returns>
    public static bool InteriorContainsCommentOrDirective(SyntaxNode node)
    {
        return SpanContainsCommentOrDirective(node, node.Span);
    }

    /// <summary>
    /// Determines whether the region covered by a group of sibling nodes — including the gaps between them —
    /// contains a comment or a preprocessor directive. A rewrite that folds the siblings into one must refuse
    /// when trivia sits between them, because the fold would consume it, while the leading documentation
    /// comment of the member the first sibling begins lies before the group and must not block the rewrite.
    /// <see cref="InteriorContainsCommentOrDirective"/> is the single-node form, for a rewrite that only
    /// rearranges one node
    /// </summary>
    /// <typeparam name="TNode">The node type</typeparam>
    /// <param name="nodes">The sibling nodes, in source order</param>
    /// <returns><see langword="true"/> if the group's region contains a comment or directive; otherwise <see langword="false"/></returns>
    public static bool GroupInteriorContainsCommentOrDirective<TNode>(IReadOnlyList<TNode> nodes)
        where TNode : SyntaxNode
    {
        if (nodes.Count == 0)
        {
            return false;
        }

        var root = nodes[0].Parent;

        if (root == null)
        {
            return true;
        }

        return SpanContainsCommentOrDirective(root, TextSpan.FromBounds(nodes[0].Span.Start, nodes[nodes.Count - 1].Span.End));
    }

    /// <summary>
    /// Determines whether a node is single line
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node is single line; otherwise <see langword="false"/></returns>
    public static bool IsSingleLine(SyntaxNode node)
    {
        if (node?.SyntaxTree == null)
        {
            return false;
        }

        return IsSingleLineSpan(node.SyntaxTree, node.Span);
    }

    /// <summary>
    /// Determines whether a span occupies a single source line
    /// </summary>
    /// <param name="syntaxTree">The syntax tree the span belongs to</param>
    /// <param name="span">The span to inspect</param>
    /// <returns><see langword="true"/> if the span is on a single line; otherwise <see langword="false"/></returns>
    public static bool IsSingleLineSpan(SyntaxTree syntaxTree, TextSpan span)
    {
        return CoversSingleLine(syntaxTree.GetLineSpan(span));
    }

    /// <summary>
    /// Determines whether every node in the sequence occupies a single source line
    /// </summary>
    /// <typeparam name="TNode">The node type</typeparam>
    /// <param name="nodes">The nodes to inspect</param>
    /// <returns><see langword="true"/> if every node is on a single line; otherwise <see langword="false"/></returns>
    public static bool AreAllSingleLine<TNode>(IEnumerable<TNode> nodes)
        where TNode : SyntaxNode
    {
        foreach (var node in nodes)
        {
            if (CoversSingleLine(node.GetLocation().GetLineSpan()) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether a line span starts and ends on the same source line
    /// </summary>
    /// <param name="lineSpan">Line span</param>
    /// <returns><see langword="true"/> if the line span covers a single line; otherwise <see langword="false"/></returns>
    private static bool CoversSingleLine(FileLinePositionSpan lineSpan)
    {
        return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line;
    }

    #endregion // Methods
}