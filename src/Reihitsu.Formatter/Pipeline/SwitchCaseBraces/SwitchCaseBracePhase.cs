using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.SwitchCaseBraces.Rewriter;

namespace Reihitsu.Formatter.Pipeline.SwitchCaseBraces;

/// <summary>
/// Adds or removes braces from switch case sections based on multi-line detection.
/// If any section in a switch statement is multi-line, all sections get braces.
/// If all sections are single-line, braces are removed from all sections
/// </summary>
internal sealed class SwitchCaseBracePhase : IFormattingPhase
{
    #region IFormattingPhase

    /// <inheritdoc/>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return new SwitchCaseBraceRewriter(context, cancellationToken).Visit(root);
    }

    #endregion // IFormattingPhase
}