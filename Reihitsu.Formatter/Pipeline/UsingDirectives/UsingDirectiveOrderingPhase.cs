using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// Reorders using directives into canonical groups before whitespace phases run
/// </summary>
internal sealed class UsingDirectiveOrderingPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Reorganizes using directives as part of the formatting pipeline.
    /// Reused internally by the RH7207 code fix through <c>InternalsVisibleTo</c>
    /// </summary>
    /// <param name="root">Root syntax node</param>
    /// <param name="context">Formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated root node</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var rewriter = new UsingDirectiveOrderingRewriter(context.EndOfLine, cancellationToken);

        return rewriter.Visit(root);
    }

    #endregion // Methods
}