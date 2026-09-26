using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Indentation and alignment phase. Computes the layout model and applies it to the syntax tree
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