using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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