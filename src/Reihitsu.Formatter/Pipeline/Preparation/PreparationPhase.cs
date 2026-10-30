using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineEndings;
using Reihitsu.Formatter.Pipeline.TabReplacement;

namespace Reihitsu.Formatter.Pipeline.Preparation;

/// <summary>
/// First phase of the pipeline. Its sub-phases replace the tabs in the whitespace of the code (FMT-TAB) and then
/// normalize every line ending to the detected one (FMT-EOL), so every later phase receives the input in one
/// whitespace form and one line-ending form (FMT-PIPE-019). Each sub-phase is a phase of its own: the pipeline runs and
/// reports it under its sub-phase name, and this phase itself is never reported
/// </summary>
internal sealed class PreparationPhase : IFormattingPhase
{
    #region Constants

    /// <summary>
    /// Name under which the tab replacement sub-phase is reported
    /// </summary>
    internal const string TabReplacementName = "PreparationPhase/TabReplacement";

    /// <summary>
    /// Name under which the line-ending sub-phase is reported
    /// </summary>
    internal const string LineEndingsName = "PreparationPhase/LineEndings";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// The sub-phases in the order they run: tab replacement, then line endings
    /// </summary>
    internal IReadOnlyList<IFormattingPhase> SubPhases { get; } = [
                                                                     new TabReplacementPhase(),
                                                                     new LineEndingNormalizationPhase(),
                                                                 ];

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Gets the name under which a sub-phase of the preparation phase is reported, whether it runs as part of this phase
    /// or alone
    /// </summary>
    /// <param name="phase">The phase</param>
    /// <returns>The sub-phase name, or <see langword="null"/> if the phase is no sub-phase of the preparation phase</returns>
    internal static string GetSubPhaseName(IFormattingPhase phase)
    {
        return phase switch
               {
                   TabReplacementPhase => TabReplacementName,
                   LineEndingNormalizationPhase => LineEndingsName,
                   _ => null,
               };
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <inheritdoc/>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var current = root;

        foreach (var subPhase in SubPhases)
        {
            cancellationToken.ThrowIfCancellationRequested();

            current = subPhase.Execute(current, context, cancellationToken);
        }

        return current;
    }

    #endregion // IFormattingPhase
}