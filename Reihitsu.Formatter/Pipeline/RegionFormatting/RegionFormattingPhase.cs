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

    /// <summary>
    /// Applies region formatting rules to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return RegionNamingRewriter.Rewrite(root, cancellationToken);
    }

    #endregion // IFormattingPhase
}