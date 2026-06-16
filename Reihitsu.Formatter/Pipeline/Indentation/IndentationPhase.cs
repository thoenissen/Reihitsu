using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Indentation and alignment phase. Computes the layout model and applies it to the syntax tree
/// </summary>
internal sealed class IndentationPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies indentation and alignment to the given syntax node by computing the layout model
    /// and rewriting first-on-line tokens to their desired indentation
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var layoutModel = LayoutComputer.Compute(root, context);

        return IndentationRewriter.Apply(root, layoutModel);
    }

    #endregion // Methods
}