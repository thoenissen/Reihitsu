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
    /// marker starts in the column of its opening quotes. Every line the move writes shifts by the same number of
    /// columns; which lines it writes is decided by <see cref="RawStringLineUtilities.MovesLine"/>, the decision the
    /// formatter follows, so an empty line stays empty and a line that starts inside a nested literal other than a raw
    /// string, or inside a nested comment, stays as written. A nested raw string literal is not aligned here: its lines
    /// shift with the outer literal, and it keeps its own diagnostic
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
        var currentClosingColumn = GetLeadingWhitespaceCount(sourceText, closingLine);
        var delta = targetColumn - currentClosingColumn;

        if (delta == 0)
        {
            return document;
        }

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
                var charactersToRemove = Math.Min(-delta, GetLeadingWhitespaceCount(sourceText, line));

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
    /// Gets the number of leading whitespace characters in a line
    /// </summary>
    /// <param name="text">The text that holds the line</param>
    /// <param name="line">The line to inspect</param>
    /// <returns>The count of leading whitespace characters</returns>
    private static int GetLeadingWhitespaceCount(SourceText text, TextLine line)
    {
        var count = 0;

        // Both spaces and tabs are counted so that outdenting a tab-indented raw string actually removes the
        // leading whitespace instead of silently doing nothing
        while (line.Start + count < line.End
               && (text[line.Start + count] == ' ' || text[line.Start + count] == '\t'))
        {
            count++;
        }

        return count;
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