using Microsoft.CodeAnalysis;

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

        cancellationToken.ThrowIfCancellationRequested();

        // Structural transforms (expression body → block body)
        current = StructuralTransforms.StructuralTransformPhase.Execute(current, context, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Region formatting (capitalize, sync endregion)
        current = RegionFormatting.RegionFormattingPhase.Execute(current, context, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Documentation comments (normalize summary layout)
        current = DocumentationComments.DocumentationCommentFormattingPhase.Execute(current, context, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Using directives (canonical group ordering and separation)
        current = UsingDirectives.UsingDirectiveOrderingPhase.Execute(current, context, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Blank lines (insert/remove vertical whitespace)
        current = BlankLines.BlankLinePhase.Execute(current, context, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Line breaks (brace placement, wrapping, operator position)
        current = LineBreaks.LineBreakPhase.Execute(current, context, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Switch case braces (add/remove based on multi-line detection)
        current = SwitchCaseBraces.SwitchCaseBracePhase.Execute(current, context, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Horizontal spacing (operators, commas, keywords, parentheses)
        current = HorizontalSpacing.HorizontalSpacingPhase.Execute(current, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Indentation & alignment (compute-then-apply)
        var layoutModel = Indentation.LayoutComputer.Compute(current, context);
        current = Indentation.IndentationRewriter.Apply(current, layoutModel);

        cancellationToken.ThrowIfCancellationRequested();

        // Raw string alignment (align content/closing markers after indentation changes)
        current = RawStringAlignment.RawStringAlignmentPhase.Execute(current, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Cleanup (trailing whitespace, consecutive blank lines, EOF)
        current = Cleanup.CleanupPhase.Execute(current, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Line ending normalization (rewrite all EOL trivia to the chosen style)
        current = LineEndings.LineEndingNormalizationPhase.Execute(current, context, cancellationToken);

        return current;
    }

    #endregion // Methods
}