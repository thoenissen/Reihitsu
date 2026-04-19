using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Formatter.Pipeline.RawStringAlignment;

/// <summary>
/// Aligns raw string literal content and closing markers after indentation changes.
/// This phase runs after the indentation phase to correct raw string content that
/// becomes misaligned when the containing statement's indentation changes.
/// </summary>
internal static class RawStringAlignmentPhase
{
    #region Methods

    /// <summary>
    /// Executes the raw string alignment phase on the given syntax tree.
    /// </summary>
    /// <param name="root">The root syntax node to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The syntax node with aligned raw string literals.</returns>
    public static SyntaxNode Execute(SyntaxNode root, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var adjustments = CollectAdjustments(root, cancellationToken);

        if (adjustments.Count == 0)
        {
            return root;
        }

        var sourceText = SourceText.From(root.ToFullString());
        var newSourceText = sourceText.WithChanges(adjustments);

        var options = root.SyntaxTree.Options as CSharpParseOptions
                          ?? CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

        return CSharpSyntaxTree.ParseText(newSourceText, options, cancellationToken: cancellationToken)
                               .GetRoot(cancellationToken);
    }

    #endregion // Methods

    #region Private methods

    /// <summary>
    /// Collects all necessary text adjustments for misaligned raw string literals.
    /// Skips raw strings whose spans overlap with previously collected adjustments
    /// to handle nested raw string cases.
    /// </summary>
    /// <param name="root">The syntax root to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of non-overlapping text changes.</returns>
    private static List<TextChange> CollectAdjustments(SyntaxNode root, CancellationToken cancellationToken)
    {
        var adjustments = new List<TextChange>();

        foreach (var node in root.DescendantNodes())
        {
            cancellationToken.ThrowIfCancellationRequested();

            TextChange? change = null;

            if (node is LiteralExpressionSyntax literal
                && literal.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
            {
                change = ComputeNonInterpolatedChange(literal);
            }
            else if (node is InterpolatedStringExpressionSyntax interpolated
                     && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken))
            {
                change = ComputeInterpolatedChange(interpolated);
            }

            if (change != null)
            {
                adjustments.Add(change.Value);
            }
        }

        return adjustments;
    }

    /// <summary>
    /// Computes the text change for a non-interpolated multiline raw string literal.
    /// </summary>
    /// <param name="literal">The literal expression containing the raw string token.</param>
    /// <returns>A text change if the raw string is misaligned; otherwise <see langword="null"/>.</returns>
    private static TextChange? ComputeNonInterpolatedChange(LiteralExpressionSyntax literal)
    {
        var token = literal.Token;
        var tokenText = token.Text;

        if (tokenText.IndexOf('\n') < 0)
        {
            return null;
        }

        var openingColumn = token.GetLocation().GetLineSpan().StartLinePosition.Character;
        var closingColumn = GetClosingColumnFromLastLine(tokenText);

        if (openingColumn == closingColumn)
        {
            return null;
        }

        var delta = openingColumn - closingColumn;
        var adjustedText = AdjustContentLines(tokenText, delta);

        return new TextChange(token.Span, adjustedText);
    }

    /// <summary>
    /// Computes the text change for an interpolated multiline raw string literal.
    /// </summary>
    /// <param name="interpolated">The interpolated string expression.</param>
    /// <returns>A text change if the raw string is misaligned; otherwise <see langword="null"/>.</returns>
    private static TextChange? ComputeInterpolatedChange(InterpolatedStringExpressionSyntax interpolated)
    {
        var startToken = interpolated.StringStartToken;
        var endToken = interpolated.StringEndToken;

        var quoteOffset = GetQuoteOffset(startToken.Text);
        var openingColumn = startToken.GetLocation().GetLineSpan().StartLinePosition.Character + quoteOffset;
        var closingColumn = GetClosingColumnFromLastLine(endToken.Text);

        if (openingColumn == closingColumn)
        {
            return null;
        }

        var delta = openingColumn - closingColumn;
        var expressionText = interpolated.ToString();
        var adjustedText = AdjustContentLines(expressionText, delta);

        return new TextChange(interpolated.Span, adjustedText);
    }

    /// <summary>
    /// Adjusts the indentation of all content lines (all lines after the first) by the given delta.
    /// </summary>
    /// <param name="text">The text to adjust.</param>
    /// <param name="delta">The number of spaces to add (positive) or remove (negative).</param>
    /// <returns>The text with adjusted content line indentation.</returns>
    private static string AdjustContentLines(string text, int delta)
    {
        var lines = text.Split('\n');

        for (var lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            lines[lineIndex] = AdjustLineIndentation(lines[lineIndex], delta);
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Adjusts the indentation of a single line by the given delta.
    /// </summary>
    /// <param name="line">The line to adjust.</param>
    /// <param name="delta">The number of spaces to add (positive) or remove (negative).</param>
    /// <returns>The line with adjusted indentation.</returns>
    private static string AdjustLineIndentation(string line, int delta)
    {
        var trimmed = line.TrimEnd('\r');

        if (trimmed.Length == 0)
        {
            return line;
        }

        var currentSpaces = GetLeadingSpaceCount(line);
        var newSpaces = Math.Max(0, currentSpaces + delta);

        if (newSpaces == currentSpaces)
        {
            return line;
        }

        return new string(' ', newSpaces) + line.Substring(currentSpaces);
    }

    /// <summary>
    /// Counts the number of leading space characters in a string.
    /// </summary>
    /// <param name="line">The string to examine.</param>
    /// <returns>The number of leading space characters.</returns>
    private static int GetLeadingSpaceCount(string line)
    {
        var count = 0;

        while (count < line.Length && line[count] == ' ')
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Extracts the closing column from the last line of a token's text.
    /// </summary>
    /// <param name="tokenText">The token text to examine.</param>
    /// <returns>The number of leading spaces on the last line, representing the closing marker column.</returns>
    private static int GetClosingColumnFromLastLine(string tokenText)
    {
        var lastNewlineIndex = tokenText.LastIndexOf('\n');

        if (lastNewlineIndex < 0)
        {
            return GetLeadingSpaceCount(tokenText);
        }

        return GetLeadingSpaceCount(tokenText.Substring(lastNewlineIndex + 1));
    }

    /// <summary>
    /// Finds the column offset of the first quote character in a raw string start token text.
    /// For example, returns 1 for <c>$"""</c> and 2 for <c>$$"""</c>.
    /// </summary>
    /// <param name="startTokenText">The start token text.</param>
    /// <returns>The index of the first quote character.</returns>
    private static int GetQuoteOffset(string startTokenText)
    {
        for (var charIndex = 0; charIndex < startTokenText.Length; charIndex++)
        {
            if (startTokenText[charIndex] == '"')
            {
                return charIndex;
            }
        }

        return 0;
    }

    #endregion // Private methods
}