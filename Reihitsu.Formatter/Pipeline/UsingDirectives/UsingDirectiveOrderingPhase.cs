using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// Reorders using directives into canonical groups before whitespace phases run
/// </summary>
public sealed class UsingDirectiveOrderingPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Reorganizes using directives as part of the formatting pipeline.
    /// Implemented explicitly so the public static overloads remain the supported reuse points
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