using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Shared helpers for trailing-comma removal transforms
/// </summary>
internal static class TrailingCommaRemovalUtilities
{
    #region Methods

    /// <summary>
    /// Gets the separator trivia that should be preserved after removing the trailing comma
    /// </summary>
    /// <param name="separator">Separator token</param>
    /// <returns>The trivia that should stay attached to the preceding node</returns>
    internal static SyntaxTriviaList GetTriviaToPreserve(SyntaxToken separator)
    {
        var triviaToPreserve = SyntaxFactory.TriviaList();

        if (ContainsNonFormattingTrivia(separator.LeadingTrivia))
        {
            triviaToPreserve = triviaToPreserve.AddRange(separator.LeadingTrivia);
        }

        return triviaToPreserve.AddRange(separator.TrailingTrivia);
    }

    /// <summary>
    /// Adds preserved separator trivia to the final token of the node
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <param name="node">Node whose trailing trivia should be updated</param>
    /// <param name="triviaToPreserve">Trivia to preserve</param>
    /// <returns>The updated node</returns>
    internal static TNode PreserveTrailingTrivia<TNode>(TNode node, SyntaxTriviaList triviaToPreserve)
        where TNode : SyntaxNode
    {
        if (triviaToPreserve.Count == 0)
        {
            return node;
        }

        var lastToken = node.GetLastToken();
        var updatedLastToken = lastToken.WithTrailingTrivia(lastToken.TrailingTrivia.AddRange(triviaToPreserve));

        return (TNode)node.ReplaceToken(lastToken, updatedLastToken);
    }

    /// <summary>
    /// Determines whether the trivia list contains content that should be preserved when removing a separator
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns><see langword="true"/> if the trivia contains comments, directives, or other non-formatting content</returns>
    private static bool ContainsNonFormattingTrivia(SyntaxTriviaList triviaList)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods
}