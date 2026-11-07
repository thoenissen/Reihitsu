using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for RH5202 - aligns the closing quote markers and content of raw string literals with the opening quote markers
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Gets the number of columns by which the code fix shifts the lines of a multi-line raw string literal after its
    /// first line: the column of its opening quotes minus the width of the whitespace in front of its closing marker,
    /// measured by <see cref="RawStringLiteralUtilities.GetLeadingWhitespaceLength"/>, as the analyzer measures it. A
    /// positive shift goes to the right, a negative one to the left
    /// </summary>
    /// <param name="sourceText">Text of the document</param>
    /// <param name="node">The syntax node representing the raw string expression</param>
    /// <returns>The shift in columns, or 0 when the literal stands on one line</returns>
    private static int GetShift(SourceText sourceText, SyntaxNode node)
    {
        var span = node.Span;
        var openingLine = sourceText.Lines.GetLineFromPosition(span.Start);
        var closingLine = sourceText.Lines.GetLineFromPosition(span.End);

        if (openingLine.LineNumber == closingLine.LineNumber)
        {
            return 0;
        }

        var quoteOffset = 0;

        if (node is InterpolatedStringExpressionSyntax interpolated)
        {
            quoteOffset = RawStringLiteralUtilities.GetQuoteOffset(interpolated.StringStartToken.Text);
        }

        var targetColumn = span.Start - openingLine.Start + quoteOffset;
        var currentClosingColumn = RawStringLiteralUtilities.GetLeadingWhitespaceLength(sourceText.ToString(closingLine.Span));

        return targetColumn - currentClosingColumn;
    }

    /// <summary>
    /// Determines whether shifting the lines of an interpolated raw string literal by the given shift would change the
    /// value of a multi-line raw string literal nested in one of its interpolation holes, at any depth. Only a shift to
    /// the left can: a line loses up to that many leading whitespace characters, so a nested closing line whose
    /// whitespace is narrower than the shift loses all of it, while a content line with more whitespace than that
    /// closing line loses more, and the whitespace the nested literal strips from it no longer matches
    /// </summary>
    /// <param name="sourceText">Text of the document</param>
    /// <param name="interpolatedString">The interpolated raw string literal the fix shifts</param>
    /// <param name="shift">The shift in columns (<see cref="GetShift"/>)</param>
    /// <returns><see langword="true"/> if the shift would change a nested literal's value</returns>
    private static bool ChangesValueOfNestedRawString(SourceText sourceText, InterpolatedStringExpressionSyntax interpolatedString, int shift)
    {
        if (shift >= 0)
        {
            return false;
        }

        foreach (var nested in interpolatedString.DescendantNodes())
        {
            if (RawStringLineUtilities.IsMultiLineRawStringLiteral(nested) == false)
            {
                continue;
            }

            var openingLine = sourceText.Lines.GetLineFromPosition(nested.Span.Start);
            var closingLine = sourceText.Lines.GetLineFromPosition(nested.Span.End);
            var closingWhitespace = RawStringLiteralUtilities.GetLeadingWhitespaceLength(sourceText.ToString(closingLine.Span));

            if (closingWhitespace >= -shift)
            {
                continue;
            }

            for (var lineNumber = openingLine.LineNumber + 1; lineNumber < closingLine.LineNumber; lineNumber++)
            {
                var line = sourceText.Lines[lineNumber];

                if (StartsInOwnHole(nested, line.Start) == false
                    && RawStringLiteralUtilities.GetLeadingWhitespaceLength(sourceText.ToString(line.Span)) > closingWhitespace)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a line of a raw string literal starts in one of the literal's own interpolation holes rather
    /// than in its text
    /// </summary>
    /// <param name="literal">The raw string literal</param>
    /// <param name="lineStart">The position at which the line starts</param>
    /// <returns><see langword="true"/> if the line starts in one of the literal's own holes</returns>
    private static bool StartsInOwnHole(SyntaxNode literal, int lineStart)
    {
        if (literal is not InterpolatedStringExpressionSyntax interpolatedString)
        {
            return false;
        }

        foreach (var content in interpolatedString.Contents)
        {
            if (content is InterpolationSyntax
                && content.Span.Start <= lineStart
                && lineStart < content.Span.End)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Applies the code fix by shifting the lines of the raw string literal after its first line, so that its closing
    /// marker starts in the column of its opening quotes. Every shifted line shifts by the same number of columns
    /// (<see cref="GetShift"/>): spaces are added in front of it, or up to that many leading whitespace characters are
    /// removed. Which lines shift is the formatter's decision (<see cref="RawStringLineUtilities.MovesLine"/>): an
    /// empty line stays empty, whether it is literal text, text of a nested raw string, or code in an interpolation
    /// hole, and a line that starts inside a nested literal other than a raw string, or inside a nested comment, stays
    /// as written. A nested raw string literal is not aligned here: its lines shift with the outer literal, and it
    /// keeps its own diagnostic. The fix is not registered where a shift to the left would change a nested raw string
    /// literal's value (<see cref="ChangesValueOfNestedRawString"/>)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">The syntax node representing the raw string expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var delta = GetShift(sourceText, node);

        if (root == null
            || delta == 0)
        {
            return document;
        }

        var openingLine = sourceText.Lines.GetLineFromPosition(node.Span.Start);
        var closingLine = sourceText.Lines.GetLineFromPosition(node.Span.End);
        var changes = new List<TextChange>();

        for (var lineNumber = openingLine.LineNumber + 1; lineNumber <= closingLine.LineNumber; lineNumber++)
        {
            var line = sourceText.Lines[lineNumber];

            if (RawStringLineUtilities.MovesLine(root, line) == false)
            {
                continue;
            }

            if (delta > 0)
            {
                changes.Add(new TextChange(new TextSpan(line.Start, 0), new string(' ', delta)));
            }
            else
            {
                var charactersToRemove = Math.Min(-delta, RawStringLiteralUtilities.GetLeadingWhitespaceLength(sourceText.ToString(line.Span)));

                if (charactersToRemove > 0)
                {
                    changes.Add(new TextChange(new TextSpan(line.Start, charactersToRemove), string.Empty));
                }
            }
        }

        if (changes.Count == 0)
        {
            return document;
        }

        return document.WithText(sourceText.WithChanges(changes));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        SourceText sourceText = null;

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            if (node is LiteralExpressionSyntax literalExpression
                && (literalExpression.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
                    || literalExpression.Token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken)))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5202Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, literalExpression, cancellationToken),
                                                          nameof(RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider)),
                                        diagnostic);
            }
            else if (node is InterpolatedStringExpressionSyntax interpolatedString
                     && interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken))
            {
                sourceText ??= await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);

                if (ChangesValueOfNestedRawString(sourceText, interpolatedString, GetShift(sourceText, interpolatedString)))
                {
                    continue;
                }

                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5202Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, interpolatedString, cancellationToken),
                                                          nameof(RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}