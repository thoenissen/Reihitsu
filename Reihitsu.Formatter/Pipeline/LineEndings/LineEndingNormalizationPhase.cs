using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.LineEndings;

/// <summary>
/// Normalizes all end-of-line trivia in the syntax tree to the formatter context value
/// </summary>
internal static class LineEndingNormalizationPhase
{
    #region Methods

    /// <summary>
    /// Rewrites all end-of-line trivia to <see cref="FormattingContext.EndOfLine"/>
    /// </summary>
    /// <param name="root">The root syntax node to normalize</param>
    /// <param name="context">The active formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The normalized syntax node</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var endOfLinesToReplace = root.DescendantTrivia(descendIntoTrivia: true)
                                      .Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.ToString() != context.EndOfLine)
                                      .ToArray();

        if (endOfLinesToReplace.Length == 0)
        {
            return root;
        }

        return root.ReplaceTrivia(endOfLinesToReplace, (_, _) => SyntaxFactory.EndOfLine(context.EndOfLine));
    }

    #endregion // Methods
}