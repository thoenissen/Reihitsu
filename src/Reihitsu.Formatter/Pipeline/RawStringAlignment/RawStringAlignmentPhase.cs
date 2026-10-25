using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.RawStringAlignment.Utilities;

namespace Reihitsu.Formatter.Pipeline.RawStringAlignment;

/// <summary>
/// Aligns raw string literal content and closing markers after indentation changes.
/// This phase runs after the indentation phase to correct raw string content that
/// becomes misaligned when the containing statement's indentation changes. The lines of
/// an interpolation hole move with the literal, and a raw string nested in a hole is
/// aligned in the same pass as the literal around it
/// </summary>
internal sealed class RawStringAlignmentPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Executes the raw string alignment phase on the given syntax tree
    /// </summary>
    /// <param name="root">The root syntax node to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax node with aligned raw string literals</returns>
    public static SyntaxNode Execute(SyntaxNode root, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = root.SyntaxTree.Options as CSharpParseOptions
                          ?? CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
        var replacements = CollectReplacements(root, options, cancellationToken);

        return replacements.Count == 0
                   ? root
                   : root.ReplaceNodes(replacements.Keys, (originalNode, _) => replacements[originalNode]);
    }

    #endregion // Methods

    #region Private methods

    /// <summary>
    /// Collects replacement nodes for misaligned raw string literals. Only literals that are not nested in an
    /// interpolation hole of another multi-line interpolated raw string are replaced; a nested literal is aligned
    /// within the replacement of the outermost literal around it, so both move in the same pass
    /// </summary>
    /// <param name="root">The syntax root to scan</param>
    /// <param name="options">Parse options used to reparse adjusted raw strings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A mapping from original nodes to adjusted replacements</returns>
    private static Dictionary<SyntaxNode, SyntaxNode> CollectReplacements(SyntaxNode root, CSharpParseOptions options, CancellationToken cancellationToken)
    {
        var replacements = new Dictionary<SyntaxNode, SyntaxNode>();
        var text = root.SyntaxTree.GetText(cancellationToken);
        var lineShifts = new RawStringLineShifts(root, text, cancellationToken);

        if (lineShifts.IsEmpty)
        {
            return replacements;
        }

        // Nothing has moved yet, so a token's column before the shift is the column it has in the text
        Func<SyntaxToken, int> getColumnBeforeShift = token => token.SpanStart - text.Lines.GetLineFromPosition(token.SpanStart).Start;

        foreach (var node in root.DescendantNodesAndSelf())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (RawStringLineShifts.IsMultiLineRawStringLiteral(node) == false
                || IsNestedInRawStringHole(node))
            {
                continue;
            }

            var replacement = ComputeReplacement(node, text, lineNumber => lineShifts.GetShift(lineNumber, getColumnBeforeShift), options);

            if (replacement != null)
            {
                replacements[node] = replacement;
            }
        }

        return replacements;
    }

    /// <summary>
    /// Computes the replacement for a multi-line raw string literal by moving each of its lines after the first by
    /// the number of columns <see cref="RawStringLineShifts"/> determines for it
    /// </summary>
    /// <param name="node">The literal expression or interpolated string expression</param>
    /// <param name="text">The text of the syntax tree</param>
    /// <param name="getShift">Gets the number of columns a line moves</param>
    /// <param name="options">Parse options used to reparse the adjusted raw string</param>
    /// <returns>A replacement node if any line of the literal moves; otherwise <see langword="null"/></returns>
    private static SyntaxNode ComputeReplacement(SyntaxNode node, SourceText text, Func<int, int> getShift, CSharpParseOptions options)
    {
        var openingLineNumber = text.Lines.GetLineFromPosition(node.SpanStart).LineNumber;
        var closingLineNumber = text.Lines.GetLineFromPosition(node.Span.End).LineNumber;
        var builder = new StringBuilder();
        var position = node.SpanStart;
        var changed = false;

        for (var lineNumber = openingLineNumber + 1; lineNumber <= closingLineNumber; lineNumber++)
        {
            var shift = getShift(lineNumber);
            var line = text.Lines[lineNumber];

            if (shift == 0
                || line.Span.Length == 0)
            {
                continue;
            }

            var currentSpaces = RawStringLineShifts.GetLeadingSpaceCount(text, line);
            var newSpaces = Math.Max(0, currentSpaces + shift);

            if (newSpaces == currentSpaces)
            {
                continue;
            }

            builder.Append(text.ToString(TextSpan.FromBounds(position, line.Start)));
            builder.Append(' ', newSpaces);
            position = line.Start + currentSpaces;
            changed = true;
        }

        if (changed == false)
        {
            return null;
        }

        builder.Append(text.ToString(TextSpan.FromBounds(position, node.Span.End)));

        var replacement = SyntaxFactory.ParseExpression(builder.ToString(), 0, options, true);

        return replacement.RawKind == node.RawKind
                   ? replacement.WithTriviaFrom(node)
                   : null;
    }

    /// <summary>
    /// Determines whether the node lies inside an interpolation hole of a multi-line interpolated raw string
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if a multi-line interpolated raw string encloses the node</returns>
    private static bool IsNestedInRawStringHole(SyntaxNode node)
    {
        return node.Ancestors()
                   .Any(RawStringLineShifts.IsMultiLineRawStringLiteral);
    }

    #endregion // Private methods

    #region IFormattingPhase

    /// <inheritdoc/>
    /// <remarks>
    /// The <paramref name="context"/> is part of the uniform phase contract and is not used by this phase
    /// </remarks>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return Execute(root, cancellationToken);
    }

    #endregion // IFormattingPhase
}