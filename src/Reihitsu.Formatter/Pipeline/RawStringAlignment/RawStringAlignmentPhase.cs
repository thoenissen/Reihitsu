using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

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
        var lineShifts = ComputeLineShifts(root, text, cancellationToken);

        if (lineShifts.Count == 0)
        {
            return replacements;
        }

        foreach (var node in root.DescendantNodesAndSelf())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsMultiLineRawStringLiteral(node) == false
                || IsNestedInRawStringHole(node))
            {
                continue;
            }

            var replacement = ComputeReplacement(node, text, lineShifts, options);

            if (replacement != null)
            {
                replacements[node] = replacement;
            }
        }

        return replacements;
    }

    /// <summary>
    /// Computes how many columns each line of every multi-line raw string literal moves. The literals are visited
    /// outermost first, so a literal nested in an interpolation hole measures its opening quotes and its closing
    /// marker after the literals around it moved them. A line that starts inside the text of a nested literal other
    /// than a raw string, or inside a nested comment, never moves, because its start is part of that text
    /// </summary>
    /// <param name="root">The syntax root to scan</param>
    /// <param name="text">The text of the syntax tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A mapping from line numbers to the number of columns the line moves; lines that do not move are absent</returns>
    private static Dictionary<int, int> ComputeLineShifts(SyntaxNode root, SourceText text, CancellationToken cancellationToken)
    {
        var lineShifts = new Dictionary<int, int>();

        foreach (var node in root.DescendantNodesAndSelf())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsMultiLineRawStringLiteral(node) == false)
            {
                continue;
            }

            var openingLine = text.Lines.GetLineFromPosition(node.SpanStart);
            var closingLine = text.Lines.GetLineFromPosition(node.Span.End);

            if (openingLine.LineNumber == closingLine.LineNumber)
            {
                continue;
            }

            var openingColumn = GetOpeningQuoteColumn(node, openingLine) + GetShift(lineShifts, openingLine.LineNumber);
            var closingColumn = GetLeadingSpaceCount(text, closingLine) + GetShift(lineShifts, closingLine.LineNumber);
            var delta = openingColumn - closingColumn;

            if (delta == 0)
            {
                continue;
            }

            for (var lineNumber = openingLine.LineNumber + 1; lineNumber <= closingLine.LineNumber; lineNumber++)
            {
                if (StartsInsideNestedText(root, text.Lines[lineNumber].Start))
                {
                    continue;
                }

                lineShifts[lineNumber] = GetShift(lineShifts, lineNumber) + delta;
            }
        }

        return lineShifts;
    }

    /// <summary>
    /// Computes the replacement for a multi-line raw string literal by moving each of its lines after the first by
    /// the number of columns <see cref="ComputeLineShifts"/> determined for it
    /// </summary>
    /// <param name="node">The literal expression or interpolated string expression</param>
    /// <param name="text">The text of the syntax tree</param>
    /// <param name="lineShifts">The number of columns each line moves</param>
    /// <param name="options">Parse options used to reparse the adjusted raw string</param>
    /// <returns>A replacement node if any line of the literal moves; otherwise <see langword="null"/></returns>
    private static SyntaxNode ComputeReplacement(SyntaxNode node, SourceText text, Dictionary<int, int> lineShifts, CSharpParseOptions options)
    {
        var openingLineNumber = text.Lines.GetLineFromPosition(node.SpanStart).LineNumber;
        var closingLineNumber = text.Lines.GetLineFromPosition(node.Span.End).LineNumber;
        var builder = new StringBuilder();
        var position = node.SpanStart;
        var changed = false;

        for (var lineNumber = openingLineNumber + 1; lineNumber <= closingLineNumber; lineNumber++)
        {
            var shift = GetShift(lineShifts, lineNumber);
            var line = text.Lines[lineNumber];

            if (shift == 0
                || line.Span.Length == 0)
            {
                continue;
            }

            var currentSpaces = GetLeadingSpaceCount(text, line);
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
    /// Determines whether the node is a multi-line raw string literal: plain, UTF-8, or interpolated
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node is a multi-line raw string literal</returns>
    private static bool IsMultiLineRawStringLiteral(SyntaxNode node)
    {
        return node switch
               {
                   LiteralExpressionSyntax literal => literal.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
                                                      || literal.Token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken),
                   InterpolatedStringExpressionSyntax interpolated => interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether the node lies inside an interpolation hole of a multi-line interpolated raw string
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if a multi-line interpolated raw string encloses the node</returns>
    private static bool IsNestedInRawStringHole(SyntaxNode node)
    {
        return node.Ancestors()
                   .Any(IsMultiLineRawStringLiteral);
    }

    /// <summary>
    /// Gets the column of the first quote of the literal's opening marker, so an interpolated literal is measured
    /// under its quotes rather than under its <c>$</c> characters
    /// </summary>
    /// <param name="node">The literal expression or interpolated string expression</param>
    /// <param name="openingLine">The line on which the literal starts</param>
    /// <returns>The column of the first opening quote</returns>
    private static int GetOpeningQuoteColumn(SyntaxNode node, TextLine openingLine)
    {
        var column = node.SpanStart - openingLine.Start;

        if (node is InterpolatedStringExpressionSyntax interpolated)
        {
            column += RawStringLiteralUtilities.GetQuoteOffset(interpolated.StringStartToken.Text);
        }

        return column;
    }

    /// <summary>
    /// Determines whether a line that starts at the given position starts inside the text of a literal other than a
    /// raw string, or inside a comment, that began on an earlier line. Moving the start of such a line would change
    /// the literal's value or the comment's text
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="lineStart">The position at which the line starts</param>
    /// <returns><see langword="true"/> if the line starts inside such text</returns>
    private static bool StartsInsideNestedText(SyntaxNode root, int lineStart)
    {
        var trivia = root.FindTrivia(lineStart);

        if (trivia.Span.Start < lineStart
            && trivia.Span.End > lineStart
            && SyntaxTriviaUtilities.IsCommentTrivia(trivia))
        {
            return true;
        }

        var token = root.FindToken(lineStart);

        if (token.SpanStart > lineStart)
        {
            return false;
        }

        if (token.SpanStart < lineStart)
        {
            return IsNonRawStringText(token);
        }

        var previousToken = token.GetPreviousToken();

        return token.LeadingTrivia.Count == 0
               && previousToken.Span.End == lineStart
               && IsNonRawStringText(previousToken);
    }

    /// <summary>
    /// Determines whether the token holds the text of a string literal other than a raw string: a regular or verbatim
    /// string, or the text of an interpolated string that is not raw
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token holds such text</returns>
    private static bool IsNonRawStringText(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.StringLiteralToken)
            || token.IsKind(SyntaxKind.Utf8StringLiteralToken))
        {
            return true;
        }

        return token.IsKind(SyntaxKind.InterpolatedStringTextToken)
               && token.Parent?.Parent is InterpolatedStringExpressionSyntax interpolated
               && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken) == false
               && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedSingleLineRawStringStartToken) == false;
    }

    /// <summary>
    /// Gets the number of columns the given line moves
    /// </summary>
    /// <param name="lineShifts">The number of columns each line moves</param>
    /// <param name="lineNumber">The line number</param>
    /// <returns>The number of columns, or zero for a line that does not move</returns>
    private static int GetShift(Dictionary<int, int> lineShifts, int lineNumber)
    {
        return lineShifts.TryGetValue(lineNumber, out var shift) ? shift : 0;
    }

    /// <summary>
    /// Counts the number of leading space characters of a line
    /// </summary>
    /// <param name="text">The text of the syntax tree</param>
    /// <param name="line">The line to examine</param>
    /// <returns>The number of leading space characters</returns>
    private static int GetLeadingSpaceCount(SourceText text, TextLine line)
    {
        var count = 0;

        while (line.Start + count < line.End
               && text[line.Start + count] == ' ')
        {
            count++;
        }

        return count;
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