using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Blank line management — inserts required blank lines before and after
/// statements and comments, removes blank lines after opening braces, and
/// collapses excessive consecutive blank lines.
/// </summary>
internal static class BlankLinePhase
{
    #region Methods

    /// <summary>
    /// Applies blank line formatting rules to the given syntax node.
    /// </summary>
    /// <param name="root">The syntax node to format.</param>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The formatted syntax node.</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var current = new BlankLineRewriter(context, cancellationToken).Visit(root);

        cancellationToken.ThrowIfCancellationRequested();

        current = new BlankLineCollapser().Visit(current);

        return current;
    }

    #endregion // Methods
}