using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Shared guards that determine whether a node can be reshaped (collapsed or re-laid out)
/// without discarding comments or directives, used by both analyzers and code fixes
/// </summary>
public static class FormattingSafetyUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the node contains comment or directive trivia that a reshape would discard
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node contains comments or directives; otherwise, <see langword="false"/></returns>
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
    /// Determines whether a span occupies a single source line
    /// </summary>
    /// <param name="syntaxTree">The syntax tree the span belongs to</param>
    /// <param name="span">The span to inspect</param>
    /// <returns><see langword="true"/> if the span is on a single line; otherwise, <see langword="false"/></returns>
    public static bool IsSingleLineSpan(SyntaxTree syntaxTree, TextSpan span)
    {
        var lineSpan = syntaxTree.GetLineSpan(span);

        return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line;
    }

    /// <summary>
    /// Determines whether every node in the sequence occupies a single source line
    /// </summary>
    /// <typeparam name="TNode">The node type</typeparam>
    /// <param name="nodes">The nodes to inspect</param>
    /// <returns><see langword="true"/> if every node is on a single line; otherwise, <see langword="false"/></returns>
    public static bool AreAllSingleLine<TNode>(IEnumerable<TNode> nodes)
        where TNode : SyntaxNode
    {
        foreach (var node in nodes)
        {
            var lineSpan = node.GetLocation().GetLineSpan();

            if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
            {
                return false;
            }
        }

        return true;
    }

    #endregion // Methods
}