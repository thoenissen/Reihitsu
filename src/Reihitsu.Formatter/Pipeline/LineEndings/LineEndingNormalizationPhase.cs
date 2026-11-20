using Microsoft.CodeAnalysis;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.LineEndings;

/// <summary>
/// Second sub-phase of the preparation phase, run before every phase that decides layout. Normalizes end-of-line
/// trivia, the line breaks that end directives, and documentation XML newline tokens to the formatter context value
/// through <see cref="LineEndingUtilities.NormalizeLineEndings"/>, the rewrite the mixed-line-ending code fix shares,
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

        return LineEndingUtilities.NormalizeLineEndings(root, context.EndOfLine);
    }

    #endregion // IFormattingPhase
}