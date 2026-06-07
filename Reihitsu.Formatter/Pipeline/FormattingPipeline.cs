using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Pipeline.Cleanup;
using Reihitsu.Formatter.Pipeline.DocumentationComments;
using Reihitsu.Formatter.Pipeline.HorizontalSpacing;
using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Pipeline.LineEndings;
using Reihitsu.Formatter.Pipeline.RawStringAlignment;
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
        var current = node;

        foreach (var phase in CreatePhases())
        {
            cancellationToken.ThrowIfCancellationRequested();

            current = phase.Execute(current, context, cancellationToken);
        }

        return current;
    }

    /// <summary>
    /// Creates the ordered list of formatting phases.
    /// Order: structural transforms → region formatting → documentation comments → using
    /// directives → blank lines → line breaks → switch-case braces → horizontal spacing →
    /// indentation → raw-string alignment → cleanup → line-ending normalization
    /// </summary>
    /// <returns>The ordered list of phases to execute</returns>
    private static IReadOnlyList<IFormattingPhase> CreatePhases()
    {
        return [
                   new StructuralTransformPhase(),
                   new RegionFormattingPhase(),
                   new DocumentationCommentFormattingPhase(),
                   new UsingDirectiveOrderingPhase(),
                   new BlankLinePhase(),
                   new LineBreakPhase(),
                   new SwitchCaseBracePhase(),
                   new HorizontalSpacingPhase(),
                   new IndentationPhase(),
                   new RawStringAlignmentPhase(),
                   new CleanupPhase(),
                   new LineEndingNormalizationPhase(),
               ];
    }

    #endregion // Methods
}