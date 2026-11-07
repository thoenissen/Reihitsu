using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.RegionFormatting.Utilities;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting;

/// <summary>
/// Region formatting — writes the text of paired region directives through <see cref="RegionNamingRewriter"/> without
/// changing directive placement. It leaves every region directive as written when the context does not format region
/// directives, as for the node-level entry points (FMT-PIPE-028)
/// </summary>
internal sealed class RegionFormattingPhase : IFormattingPhase
{
    #region IFormattingPhase

    /// <inheritdoc/>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        if (context.FormatsRegionDirectives == false)
        {
            return root;
        }

        return RegionNamingRewriter.Rewrite(root, cancellationToken);
    }

    #endregion // IFormattingPhase
}