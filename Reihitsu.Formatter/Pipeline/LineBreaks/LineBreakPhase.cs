using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Line breaks — determines where line breaks are placed.
/// Handles Allman brace placement, argument wrapping, chain link collapsing,
/// operator position, ternary placement, constructor initializer placement,
/// generic constraint placement, and expression-bodied property collapse
/// </summary>
internal sealed class LineBreakPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Creates the ordered line-break subphase rewriters
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ordered list of rewriters to execute</returns>
    private static IReadOnlyList<CSharpSyntaxRewriter> CreateRewriters(FormattingContext context,
                                                                       CancellationToken cancellationToken)
    {
        var gapNormalizer = new TokenGapNormalizer(context.EndOfLine);
        var bracePlacer = new BracePlacer(gapNormalizer, context.EndOfLine);

        return [
                   new LineBreakBlockRewriter(gapNormalizer, bracePlacer, cancellationToken),
                   new LineBreakInitializerRewriter(context, gapNormalizer, bracePlacer, cancellationToken),
                   new LineBreakContainedBlockRewriter(context, gapNormalizer, bracePlacer, cancellationToken),
                   new LineBreakAssignmentRewriter(cancellationToken),
                   new DeclarationSemicolonLineBreakRewriter(cancellationToken),
                   new LineBreakListRewriter(context, cancellationToken),
                   new PropertyLayoutLineBreakRewriter(gapNormalizer, bracePlacer, cancellationToken),
                   new GenericConstraintLineBreakRewriter(context, cancellationToken),
                   new DeclarationBraceLineBreakRewriter(context, gapNormalizer, bracePlacer, cancellationToken),
                   new AttributeTargetFormattingRewriter(context, cancellationToken),
                   new BinaryOperatorLineBreakRewriter(context, cancellationToken),
                   new ChainLineBreakRewriter(context, cancellationToken),
                   new TernaryLineBreakRewriter(context, cancellationToken),
                   new SwitchCaseWhenLineBreakRewriter(context, cancellationToken)
               ];
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <summary>
    /// Applies line break formatting rules to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node with corrected line breaks</returns>
    public SyntaxNode Execute(SyntaxNode root,
                              FormattingContext context,
                              CancellationToken cancellationToken)
    {
        var current = root;

        foreach (var rewriter in CreateRewriters(context, cancellationToken))
        {
            current = rewriter.Visit(current);

            cancellationToken.ThrowIfCancellationRequested();
        }

        return current;
    }

    #endregion // IFormattingPhase
}