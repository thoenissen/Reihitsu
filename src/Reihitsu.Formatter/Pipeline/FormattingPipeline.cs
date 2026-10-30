using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Pipeline.Cleanup;
using Reihitsu.Formatter.Pipeline.DocumentationComments;
using Reihitsu.Formatter.Pipeline.HorizontalSpacing;
using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Pipeline.Preparation;
using Reihitsu.Formatter.Pipeline.RegionFormatting;
using Reihitsu.Formatter.Pipeline.StructuralTransforms;
using Reihitsu.Formatter.Pipeline.SwitchCaseBraces;
using Reihitsu.Formatter.Pipeline.UsingDirectives;

namespace Reihitsu.Formatter.Pipeline;

/// <summary>
/// Executes the formatting pipeline by running all phases in order
/// </summary>
internal static class FormattingPipeline
{
    #region Methods

    /// <summary>
    /// Applies the full formatting pipeline to a syntax node
    /// </summary>
    /// <param name="node">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    public static SyntaxNode Execute(SyntaxNode node, FormattingContext context, CancellationToken cancellationToken)
    {
        return Execute(node, context, phaseObserver: null, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Applies the full formatting pipeline to a syntax node and observes every completed phase
    /// </summary>
    /// <param name="node">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="phaseObserver">Observer that receives the phase name and its input and output nodes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    internal static SyntaxNode Execute(SyntaxNode node,
                                       FormattingContext context,
                                       Action<string, SyntaxNode, SyntaxNode> phaseObserver,
                                       CancellationToken cancellationToken)
    {
        return ExecutePhases(node, context, CreatePhases(), phaseObserver, cancellationToken);
    }

    /// <summary>
    /// Executes an explicit phase sequence through the same observer and exception boundary as the production pipeline.
    /// The preparation phase runs as its sub-phases, each observed under its sub-phase name; a sub-phase in the sequence is
    /// observed under the same name
    /// </summary>
    /// <param name="node">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="phases">Phases to execute in the supplied order</param>
    /// <param name="phaseObserver">Observer that receives the phase name and its input and output nodes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    internal static SyntaxNode ExecutePhases(SyntaxNode node,
                                             FormattingContext context,
                                             IReadOnlyList<IFormattingPhase> phases,
                                             Action<string, SyntaxNode, SyntaxNode> phaseObserver,
                                             CancellationToken cancellationToken)
    {
        var current = node;

        foreach (var phase in phases)
        {
            if (phase is PreparationPhase preparationPhase)
            {
                foreach (var subPhase in preparationPhase.SubPhases)
                {
                    current = ExecutePhase(current, context, subPhase, phaseObserver, cancellationToken);
                }
            }
            else
            {
                current = ExecutePhase(current, context, phase, phaseObserver, cancellationToken);
            }
        }

        return current;
    }

    /// <summary>
    /// Executes one reported phase and passes its input and output to the observer
    /// </summary>
    /// <param name="node">The syntax node the phase receives</param>
    /// <param name="context">The formatting context</param>
    /// <param name="phase">The phase to execute</param>
    /// <param name="phaseObserver">Observer that receives the phase name and its input and output nodes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax node the phase returns</returns>
    private static SyntaxNode ExecutePhase(SyntaxNode node,
                                           FormattingContext context,
                                           IFormattingPhase phase,
                                           Action<string, SyntaxNode, SyntaxNode> phaseObserver,
                                           CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var phaseName = PreparationPhase.GetSubPhaseName(phase) ?? phase.GetType().Name;
        SyntaxNode output;

        try
        {
            output = phase.Execute(node, context, cancellationToken);
        }
        catch (Exception exception) when (phaseObserver != null && exception is not OperationCanceledException)
        {
            throw new InvalidOperationException($"Formatting phase '{phaseName}' failed.", exception);
        }

        phaseObserver?.Invoke(phaseName, node, output);

        return output;
    }

    /// <summary>
    /// Creates the ordered list of formatting phases.
    /// Order: preparation (tab replacement, then line endings) → structural transforms → region formatting →
    /// documentation comments → using directives → blank lines → line breaks → switch-case braces → horizontal spacing →
    /// indentation (including the move of raw string literal lines) → cleanup.
    /// No phase computes a column from a move or replacement that a later phase applies
    /// </summary>
    /// <returns>The ordered list of phases to execute</returns>
    private static IReadOnlyList<IFormattingPhase> CreatePhases()
    {
        return [
                   new PreparationPhase(),
                   new StructuralTransformPhase(),
                   new RegionFormattingPhase(),
                   new DocumentationCommentFormattingPhase(),
                   new UsingDirectiveOrderingPhase(),
                   new BlankLinePhase(),
                   new LineBreakPhase(),
                   new SwitchCaseBracePhase(),
                   new HorizontalSpacingPhase(),
                   new IndentationPhase(),
                   new CleanupPhase(),
               ];
    }

    #endregion // Methods
}