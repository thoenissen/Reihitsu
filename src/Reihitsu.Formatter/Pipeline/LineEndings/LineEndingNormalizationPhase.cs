using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.LineEndings;

/// <summary>
/// Second sub-phase of the preparation phase, run before every phase that decides layout. Normalizes end-of-line
/// trivia, the line breaks that end directives, and documentation XML newline tokens to the formatter context value,
/// so no later phase receives another line ending (FMT-PIPE-019). Line breaks inside literals, other comments, and
/// disabled text are not trivia or tokens of these kinds and keep their characters. Without a line break to rewrite,
/// the input node itself is returned. A node-level root already has no parent (FMT-PIPE-023), so whether the phase returns
/// its input or a copy is not visible to the phases after it
/// </summary>
internal sealed class LineEndingNormalizationPhase : IFormattingPhase
{
    #region IFormattingPhase

    /// <inheritdoc/>
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