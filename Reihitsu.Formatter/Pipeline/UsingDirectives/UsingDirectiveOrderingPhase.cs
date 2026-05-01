using System.Threading;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// Reorders using directives into canonical groups before whitespace phases run
/// </summary>
internal static class UsingDirectiveOrderingPhase
{
    #region Methods

    /// <summary>
    /// Reorganizes using directives for compilation units and namespace scopes
    /// </summary>
    /// <param name="root">Root syntax node</param>
    /// <param name="context">Formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated root node</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var rewriter = new UsingDirectiveOrderingRewriter(context.EndOfLine, cancellationToken);

        return rewriter.Visit(root);
    }

    #endregion // Methods
}