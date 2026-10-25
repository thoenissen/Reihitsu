using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Indentation and alignment phase. Computes the layout model and applies it to the syntax tree, including the move
/// of the lines of multi-line raw string literals under their opening quotes, so the column of every line is final
/// when the phase returns
/// </summary>
internal sealed class IndentationPhase : IFormattingPhase
{
    #region IFormattingPhase

    /// <inheritdoc/>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var layoutModel = LayoutComputer.Compute(root, context, cancellationToken);

        return IndentationRewriter.Apply(root, layoutModel, cancellationToken);
    }

    #endregion // IFormattingPhase
}