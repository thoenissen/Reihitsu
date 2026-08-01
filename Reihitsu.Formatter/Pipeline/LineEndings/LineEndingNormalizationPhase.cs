using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.LineEndings;

/// <summary>
/// Normalizes end-of-line trivia and documentation XML newline tokens to the formatter context value
/// </summary>
internal sealed class LineEndingNormalizationPhase : IFormattingPhase
{
    #region IFormattingPhase

    /// <summary>
    /// Rewrites all end-of-line trivia and documentation XML newline tokens to <see cref="FormattingContext.EndOfLine"/>
    /// </summary>
    /// <param name="root">The root syntax node to normalize</param>
    /// <param name="context">The active formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The normalized syntax node</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var documentationEndOfLinesToReplace = root.DescendantTokens(descendIntoTrivia: true)
                                                   .Where(token => token.IsKind(SyntaxKind.XmlTextLiteralNewLineToken)
                                                                   && token.Text != context.EndOfLine)
                                                   .ToArray();
        var normalizedRoot = documentationEndOfLinesToReplace.Length == 0
                                 ? root
                                 : root.ReplaceTokens(documentationEndOfLinesToReplace,
                                                      (original, _) => original.CopyAnnotationsTo(SyntaxFactory.XmlTextNewLine(original.LeadingTrivia,
                                                                                                                               context.EndOfLine,
                                                                                                                               original.ValueText,
                                                                                                                               original.TrailingTrivia)));
        var endOfLinesToReplace = normalizedRoot.DescendantTrivia(descendIntoTrivia: true)
                                                .Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.ToString() != context.EndOfLine)
                                                .ToArray();

        if (endOfLinesToReplace.Length == 0)
        {
            return normalizedRoot;
        }

        return normalizedRoot.ReplaceTrivia(endOfLinesToReplace, (_, _) => SyntaxFactory.EndOfLine(context.EndOfLine));
    }

    #endregion // IFormattingPhase
}