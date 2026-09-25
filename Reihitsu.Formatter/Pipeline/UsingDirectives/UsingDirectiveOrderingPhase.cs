using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.UsingDirectives.Rewriter;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// Reorders using directives into canonical groups before whitespace phases run
/// </summary>
internal sealed class UsingDirectiveOrderingPhase : IFormattingPhase
{
    #region IFormattingPhase

    /// <inheritdoc/>
    /// <remarks>
    /// Reused internally by the RH7207 code fix through <c>InternalsVisibleTo</c>
    /// </remarks>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var rewriter = new UsingDirectiveOrderingRewriter(context.EndOfLine, cancellationToken);

        return rewriter.Visit(root);
    }

    #endregion // IFormattingPhase
}