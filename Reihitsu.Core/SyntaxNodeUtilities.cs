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
    /// Determines whether comments or directives are present in a node
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
    /// Determines whether the given trivia is a comment
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is a comment; otherwise <see langword="false"/></returns>
    public static bool IsComment(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Determines whether the trivia between two tokens contains a comment
    /// </summary>
    /// <param name="firstToken">First token</param>
    /// <param name="secondToken">Second token (expected to follow <paramref name="firstToken"/>)</param>
    /// <returns><see langword="true"/> if a comment is present in the gap; otherwise <see langword="false"/></returns>
    public static bool GapContainsComment(SyntaxToken firstToken, SyntaxToken secondToken)
    {
        foreach (var trivia in firstToken.TrailingTrivia)
        {
            if (IsComment(trivia))
            {
                return true;
            }
        }

        foreach (var trivia in secondToken.LeadingTrivia)
        {
            if (IsComment(trivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether any comment trivia intersects the given span
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="span">Span</param>
    /// <returns><see langword="true"/> if a comment is present in the span; otherwise <see langword="false"/></returns>
    public static bool SpanContainsComment(SyntaxNode root, TextSpan span)
    {
        foreach (var trivia in root.DescendantTrivia(span, descendIntoTrivia: true))
        {
            if (IsComment(trivia))
            {
                return true;
            }
        }

        return false;
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

        var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);

        return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line;
    }

    #endregion // Methods
}