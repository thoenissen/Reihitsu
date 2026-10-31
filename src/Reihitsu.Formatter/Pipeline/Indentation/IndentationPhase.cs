using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

        // The node-level entry points pass a root without a parent (FMT-PIPE-023). A root that is still part of a tree is
        // laid out as a tree of its own as well: its first token starts the tree's text, its lines are numbered from its own
        // start, which the layout and the move of raw string lines share, and no token outside it decides a line
        if (root.Parent != null)
        {
            root = CSharpSyntaxTree.Create((CSharpSyntaxNode)root, root.SyntaxTree.Options as CSharpParseOptions).GetRoot(cancellationToken);
        }

        var layoutModel = LayoutComputer.Compute(root, context, cancellationToken);

        return IndentationRewriter.Apply(root, layoutModel, cancellationToken);
    }

    #endregion // IFormattingPhase
}