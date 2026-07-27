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
    /// Determines whether ordinary comments or directives are present in a node. Documentation comments are
    /// deliberately excluded, which keeps this predicate narrower than <see cref="ContainsCommentOrDirective"/>:
    /// callers here decide whether a node can be reshaped in place, where a documentation comment does not
    /// change the outcome. Callers that predict whether the formatter will refuse a line join need the wider
    /// predicate instead, because the formatter's join guard counts documentation comments as comments;
    /// confusing the two registers code fixes that the formatter then declines
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if comments or directives are present; otherwise <see langword="false"/></returns>
    public static bool HasCommentsOrDirectives(SyntaxNode node)
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
    /// Unlike <see cref="HasCommentsOrDirectives"/> the comment set includes documentation comments, which is what
    /// analyzers and code fixes need when they predict at registration time whether the formatter will refuse to
    /// join lines: the formatter's join guard <see cref="SyntaxTriviaUtilities.ContainsUnjoinableTrivia"/> counts
    /// documentation comments too, so a narrower guard registers actions the formatter then refuses. The two sets
    /// are not identical — the formatter's guard additionally blocks on disabled text
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