using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.BlankLines;
using Reihitsu.Formatter.Rules.Cleanup;
using Reihitsu.Formatter.Rules.Indentation;
using Reihitsu.Formatter.Rules.Regions;
using Reihitsu.Formatter.Rules.Spacing;
using Reihitsu.Formatter.Rules.Structural;

namespace Reihitsu.Formatter;

/// <summary>
/// Executes the formatting pipeline by running all rules in phase order.
/// </summary>
internal static class FormattingPipeline
{
    #region Methods

    /// <summary>
    /// Applies the full formatting pipeline to a syntax node.
    /// </summary>
    /// <param name="node">The syntax node to format.</param>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The formatted syntax node.</returns>
    public static SyntaxNode Execute(SyntaxNode node, FormattingContext context, CancellationToken cancellationToken)
    {
        var current = node;

        foreach (var rule in CreateRules(context, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            current = rule.Apply(current);
        }

        return current;
    }

    /// <summary>
    /// Creates all formatting rules for a single pipeline execution, sorted by phase.
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ordered list of formatting rules.</returns>
    private static IReadOnlyList<IFormattingRule> CreateRules(FormattingContext context, CancellationToken cancellationToken)
    {
        return new IFormattingRule[]
               {
                   // Phase 0: Structural transforms
                   new ExpressionBodiedMethodRule(context, cancellationToken),
                   new ExpressionBodiedConstructorRule(context, cancellationToken),
                   new BracePlacementRule(context, cancellationToken),

                   // Phase 1: Spacing
                   new HorizontalSpacingRule(context, cancellationToken),

                   // Phase 2: Indentation and continuation-line alignment
                   new IndentationAndAlignmentRule(context, cancellationToken),

                   // Phase 3: Blank line management
                   new BlankLineBeforeStatementRule(context, cancellationToken),
                   new BlankLineAfterStatementRule(context, cancellationToken),

                   // Phase 4: Region formatting
                   new RegionFormattingRule(context, cancellationToken),

                   // Phase 5: Cleanup
                   new TrailingTriviaCleanupRule(context, cancellationToken),
               };
    }

    #endregion // Methods
}