using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.RegionFormatting.Utilities;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting;

/// <summary>
/// Region formatting — capitalizes region descriptions and synchronizes endregion comments through
/// <see cref="RegionNamingRewriter"/> without changing directive placement
/// </summary>
internal sealed class RegionFormattingPhase : IFormattingPhase
{
    #region IFormattingPhase

    /// <inheritdoc/>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return RegionNamingRewriter.Rewrite(root, cancellationToken);
    }

    #endregion // IFormattingPhase
}