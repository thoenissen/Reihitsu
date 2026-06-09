using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Normalizes horizontal spacing between tokens on the same line.
/// Handles operator spacing, comma spacing, semicolons in for-loops,
/// keyword spacing, parenthesis spacing, and multiple consecutive spaces.
/// The spacing decision lives in <see cref="SpacingPolicy"/> and the trivia edit in
/// <see cref="TrailingWhitespaceWriter"/>; this phase only drives the rewriter
/// </summary>
internal sealed class HorizontalSpacingPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies horizontal spacing rules to the given syntax tree
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax tree with normalized horizontal spacing</returns>
    public static SyntaxNode Execute(SyntaxNode root, CancellationToken cancellationToken)
    {
        var rewriter = new HorizontalSpacingRewriter(cancellationToken);

        return rewriter.Visit(root);
    }

    /// <summary>
    /// Applies horizontal spacing rules to the given syntax tree as part of the formatting pipeline.
    /// The <paramref name="context"/> is part of the uniform phase contract and is not used by this phase
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="context">The formatting context (unused)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax tree with normalized horizontal spacing</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return Execute(root, cancellationToken);
    }

    #endregion // Methods
}