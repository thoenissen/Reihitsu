using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Line breaks — determines where line breaks are placed.
/// Handles Allman brace placement, argument wrapping, chain link collapsing,
/// operator position, ternary placement, constructor initializer placement,
/// generic constraint placement, and expression-bodied property collapse
/// </summary>
internal static class LineBreakPhase
{
    #region Methods

    /// <summary>
    /// Applies line break formatting rules to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node with corrected line breaks</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var rewriter = new LineBreakRewriter(context, cancellationToken);

        return rewriter.Visit(root);
    }

    #endregion // Methods
}