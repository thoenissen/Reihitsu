using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.UsingDirectives.Rewriter;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// Reorders using directives into canonical groups and places every directive on its own line before
/// whitespace phases run. A block that cannot be reordered safely keeps its order and only has
/// directives that share a line separated
/// </summary>
internal sealed class UsingDirectiveOrderingPhase : IFormattingPhase
{
    #region IFormattingPhase

    /// <inheritdoc/>
    /// <remarks>
    /// Also run on its own by <see cref="ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync"/>, on one scope where it
    /// stands in the document (FMT-PIPE-030)
    /// </remarks>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var rewriter = new UsingDirectiveOrderingRewriter(context.EndOfLine, cancellationToken);

        return rewriter.Visit(root);
    }

    #endregion // IFormattingPhase
}