using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.SwitchCaseBraces;

/// <summary>
/// Adds or removes braces from switch case sections based on multi-line detection.
/// If any section in a switch statement is multi-line, all sections get braces.
/// If all sections are single-line, braces are removed from all sections.
/// </summary>
internal static class SwitchCaseBracePhase
{
    #region Methods

    /// <summary>
    /// Applies switch case brace formatting to the given syntax node.
    /// </summary>
    /// <param name="root">The syntax node to format.</param>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The formatted syntax node.</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return new SwitchCaseBraceRewriter(context, cancellationToken).Visit(root);
    }

    #endregion // Methods
}