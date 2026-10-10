using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;

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
    /// Applies the code fix by realigning raw string content using text-level replacement
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">The syntax node representing the raw string expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var span = node.Span;
        var expressionText = sourceText.ToString(span);

        var startLine = sourceText.Lines.GetLineFromPosition(span.Start);
        var startColumn = span.Start - startLine.Start;

        var quoteOffset = 0;

        if (node is InterpolatedStringExpressionSyntax interpolated)
        {
            quoteOffset = RawStringLiteralUtilities.GetQuoteOffset(interpolated.StringStartToken.Text);
        }

        var targetColumn = startColumn + quoteOffset;

        var newExpressionText = RealignContent(expressionText, targetColumn);

        if (expressionText == newExpressionText)
        {
            return document;
        }

        var newSourceText = sourceText.Replace(span, newExpressionText);

        return document.WithText(newSourceText);
    }

    /// <summary>
    /// Realigns all content lines of a raw string expression to match the target column
    /// </summary>
    /// <param name="expressionText">The full text of the raw string expression</param>
    /// <param name="targetColumn">The target column for the opening and closing quote markers</param>
    /// <returns>The realigned expression text</returns>
    private static string RealignContent(string expressionText, int targetColumn)
    {
        var lines = expressionText.Split(["\r\n", "\n"], StringSplitOptions.None);

        if (lines.Length < 2)
        {
            return expressionText;
        }

        var lastLine = lines[lines.Length - 1];
        var currentClosingColumn = lastLine.Length - lastLine.TrimStart().Length;

        if (currentClosingColumn == targetColumn)
        {
            return expressionText;
        }

        var delta = targetColumn - currentClosingColumn;
        var lineEnding = expressionText.Contains("\r\n")
                             ? "\r\n"
                             : "\n";
        var result = new StringBuilder();

        result.Append(lines[0]);

        for (var lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            result.Append(lineEnding);

            var line = lines[lineIndex];

            if (delta > 0)
            {
                result.Append(new string(' ', delta));
                result.Append(line);
            }
            else
            {
                var charactersToRemove = Math.Min(-delta, GetLeadingWhitespaceCount(line));

                result.Append(line.Substring(charactersToRemove));
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Gets the number of leading whitespace characters in a line
    /// </summary>
    /// <param name="line">The line to inspect</param>
    /// <returns>The count of leading whitespace characters</returns>
    private static int GetLeadingWhitespaceCount(string line)
    {
        var count = 0;

        foreach (var character in line)
        {
            // Both spaces and tabs are counted so that outdenting a tab-indented raw string actually removes the
            // leading whitespace instead of silently doing nothing
            if (character == ' ' || character == '\t')
            {
                count++;
            }
            else
            {
                break;
            }
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