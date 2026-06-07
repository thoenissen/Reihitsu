using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline;

/// <summary>
/// Uniform contract for a single formatting pipeline phase.
/// Phases are executed in order by <see cref="FormattingPipeline"/>
/// </summary>
internal interface IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies the phase to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken);

    #endregion // Methods
}