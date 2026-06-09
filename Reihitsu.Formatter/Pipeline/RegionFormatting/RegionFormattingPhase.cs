using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting;

/// <summary>
/// Region formatting — capitalizes region descriptions and synchronizes endregion comments
/// (<see cref="RegionNamingRewriter"/>), then removes regions nested inside element bodies
/// (<see cref="NestedRegionRemovalStep"/>)
/// </summary>
internal sealed class RegionFormattingPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies region formatting rules to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        root = RegionNamingRewriter.Rewrite(root, cancellationToken);

        return NestedRegionRemovalStep.Remove(root, cancellationToken);
    }

    #endregion // Methods
}