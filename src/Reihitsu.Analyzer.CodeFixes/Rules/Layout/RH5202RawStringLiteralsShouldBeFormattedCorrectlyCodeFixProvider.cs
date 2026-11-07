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
    /// Applies the code fix by shifting the lines of the raw string literal after its first line, so that its closing
    /// marker starts in the column of its opening quotes. The width of the whitespace in front of the closing marker is
    /// measured by <see cref="RawStringLiteralUtilities.GetLeadingWhitespaceLength"/>, as the analyzer measures it, and
    /// every shifted line shifts by the same number of columns: spaces are added in front of it, or up to that many
    /// leading whitespace characters are removed. A line that starts inside a nested literal other than a raw string,
    /// or inside a nested comment (<see cref="RawStringLineUtilities.StartsInsideNestedText"/>, the decision the
    /// formatter follows), stays as written, and so does an empty line of literal text (<see cref="IsEmptyTextLine"/>);
    /// an empty line of the code in an interpolation hole shifts. A nested raw string literal is not aligned here: its
    /// lines shift with the outer literal, and it keeps its own diagnostic
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">The syntax node representing the raw string expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var span = node.Span;
        var openingLine = sourceText.Lines.GetLineFromPosition(span.Start);
        var closingLine = sourceText.Lines.GetLineFromPosition(span.End);

        if (root == null
            || openingLine.LineNumber == closingLine.LineNumber)
        {
            return document;
        }

        var quoteOffset = 0;

        if (node is InterpolatedStringExpressionSyntax interpolated)
        {
            quoteOffset = RawStringLiteralUtilities.GetQuoteOffset(interpolated.StringStartToken.Text);
        }

        var targetColumn = span.Start - openingLine.Start + quoteOffset;
        var currentClosingColumn = RawStringLiteralUtilities.GetLeadingWhitespaceLength(sourceText.ToString(closingLine.Span));
        var delta = targetColumn - currentClosingColumn;

        if (delta == 0)
        {
            return document;
        }

        var changes = new List<TextChange>();

        for (var lineNumber = openingLine.LineNumber + 1; lineNumber <= closingLine.LineNumber; lineNumber++)
        {
            var line = sourceText.Lines[lineNumber];

            if (RawStringLineUtilities.StartsInsideNestedText(root, line.Start)
                || IsEmptyTextLine(root, line))
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

    /// <summary>
    /// Determines whether a line is an empty line of literal text: a line with no character between its line breaks
    /// that lies within a token of the literal's text, not within the trivia of the code in an interpolation hole.
    /// Such a line contributes an empty line to the value whatever the closing marker strips, so it stays empty
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="line">The line, from the text of the root's tree</param>
    /// <returns><see langword="true"/> if the line is an empty line of literal text</returns>
    private static bool IsEmptyTextLine(SyntaxNode root, TextLine line)
    {
        if (line.Span.Length > 0)
        {
            return false;
        }

        var token = root.FindToken(line.Start);

        return token.SpanStart <= line.Start
               && line.Start < token.Span.End;
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
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5202Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, interpolatedString, cancellationToken),
                                                          nameof(RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}