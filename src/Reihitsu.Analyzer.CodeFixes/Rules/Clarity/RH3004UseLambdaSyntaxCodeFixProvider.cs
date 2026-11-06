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
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3004UseLambdaSyntaxAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3004UseLambdaSyntaxCodeFixProvider))]
public class RH3004UseLambdaSyntaxCodeFixProvider : CodeFixProvider
{
    #region Constants

    /// <summary>
    /// Number of columns of one indentation level
    /// </summary>
    private const int IndentationWidth = 4;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Determines whether the anonymous method carries trivia that the rewrite would discard. The replacement is
    /// reassembled from the text of the parameter list and of the block, so only those two spans survive. Trivia
    /// between the leading keywords and the parameter list, or between the parameter list and the block, belongs
    /// to neither of them and would be dropped, which is why the fix is not offered for those shapes
    /// </summary>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <returns><see langword="true"/> if the rewrite would discard trivia; otherwise, <see langword="false"/></returns>
    private static bool ContainsDiscardedTrivia(AnonymousMethodExpressionSyntax anonymousMethodExpression)
    {
        var parameterList = anonymousMethodExpression.ParameterList;

        if (parameterList == null
            || anonymousMethodExpression.Body is not BlockSyntax block)
        {
            return true;
        }

        return SyntaxNodeUtilities.SpanContainsCommentOrDirective(anonymousMethodExpression, TextSpan.FromBounds(anonymousMethodExpression.Span.Start, parameterList.Span.Start))
               || SyntaxNodeUtilities.SpanContainsCommentOrDirective(anonymousMethodExpression, TextSpan.FromBounds(parameterList.Span.End, block.Span.Start));
    }

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, AnonymousMethodExpressionSyntax anonymousMethodExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || anonymousMethodExpression.ParameterList == null)
        {
            return document;
        }

        var asyncPrefix = anonymousMethodExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword)
                              ? "async "
                              : string.Empty;
        var expressionBody = anonymousMethodExpression.Block is { Statements.Count: 1 } blockSyntax
                             && blockSyntax.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement
                             && SyntaxNodeUtilities.ContainsCommentOrDirective(anonymousMethodExpression.Block) == false
                                 ? returnStatement.Expression
                                 : null;
        var bodyText = expressionBody?.ToString() ?? anonymousMethodExpression.Block.ToString();
        var formattingAnnotation = new SyntaxAnnotation();
        var replacementExpression = SyntaxFactory.ParseExpression($"{asyncPrefix}{anonymousMethodExpression.ParameterList} => {bodyText}")
                                                 .WithTriviaFrom(anonymousMethodExpression)
                                                 .WithAdditionalAnnotations(formattingAnnotation);
        var updatedDocument = document.WithSyntaxRoot(root.ReplaceNode(anonymousMethodExpression, replacementExpression));
        var updatedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (updatedRoot?.GetAnnotatedNodes(formattingAnnotation).FirstOrDefault() is not { } lambdaExpression)
        {
            return updatedDocument;
        }

        // The code the fix creates, the parameter list, the arrow, and the braces, is laid out as the formatter lays it
        // out: a block body on lines of its own, aligned under the lambda's anchor
        var formattedDocument = await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, lambdaExpression, cancellationToken).ConfigureAwait(false);

        if (expressionBody != null)
        {
            return formattedDocument;
        }

        return await MoveStatementsAsync(document, formattedDocument, anonymousMethodExpression.Block, formattingAnnotation, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes the content of the anonymous method's block into the formatted lambda's block as moved code: the text from
    /// its first to its last statement, with their comments and the blank lines between them, keeps its own layout, and
    /// every one of its lines moves by the same column delta, so that its first line stands one level inside the braces
    /// the formatter laid out. Statements that shared the block's line stay on one line. A comment behind the opening
    /// brace on its line stays behind the new opening brace
    /// </summary>
    /// <param name="originalDocument">Document before the fix</param>
    /// <param name="formattedDocument">Document with the formatted lambda</param>
    /// <param name="originalBlock">Block of the anonymous method</param>
    /// <param name="formattingAnnotation">Annotation of the lambda</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the statements moved into the lambda's block</returns>
    private static async Task<Document> MoveStatementsAsync(Document originalDocument, Document formattedDocument, BlockSyntax originalBlock, SyntaxAnnotation formattingAnnotation, CancellationToken cancellationToken)
    {
        var formattedRoot = await formattedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (formattedRoot?.GetAnnotatedNodes(formattingAnnotation).FirstOrDefault() is not ParenthesizedLambdaExpressionSyntax { Block: { } formattedBlock })
        {
            return formattedDocument;
        }

        var originalText = await originalDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var formattedText = await formattedDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var closeBraceLine = formattedText.Lines.GetLineFromPosition(formattedBlock.CloseBraceToken.SpanStart);
        var closeBraceIndentation = formattedText.ToString(TextSpan.FromBounds(closeBraceLine.Start, formattedBlock.CloseBraceToken.SpanStart));

        if (string.IsNullOrWhiteSpace(closeBraceIndentation) == false)
        {
            return formattedDocument;
        }

        var endOfLine = LineEndingUtilities.DetectEndOfLine(formattedRoot);
        var contentIndentation = closeBraceIndentation + new string(' ', IndentationWidth);
        var openBraceTail = GetOpenBraceTail(originalBlock.OpenBraceToken, out var contentSearchStart);
        var contentStart = SkipWhitespace(originalText, contentSearchStart, originalBlock.CloseBraceToken.SpanStart);
        var contentEnd = SkipWhitespaceBackwards(originalText, contentStart, originalBlock.CloseBraceToken.SpanStart);

        if (contentStart == contentEnd)
        {
            return formattedDocument;
        }

        var originalRoot = await originalDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        var columnDelta = contentIndentation.Length - GetColumn(originalText, contentStart);
        var content = new StringBuilder();

        content.Append(openBraceTail)
               .Append(endOfLine)
               .Append(contentIndentation);

        var firstLine = originalText.Lines.GetLineFromPosition(contentStart);

        content.Append(originalText.ToString(TextSpan.FromBounds(contentStart, Math.Min(firstLine.EndIncludingLineBreak, contentEnd))));

        for (var lineIndex = firstLine.LineNumber + 1; lineIndex < originalText.Lines.Count && originalText.Lines[lineIndex].Start < contentEnd; lineIndex++)
        {
            var line = originalText.Lines[lineIndex];
            var lineEnd = Math.Min(line.EndIncludingLineBreak, contentEnd);
            var textStart = SkipIndentation(originalText, line.Start, Math.Min(line.End, contentEnd));
            var copyStart = line.Start;

            if (originalRoot != null
                && CanMoveLine(originalRoot, originalText, line.Start, columnDelta))
            {
                if (textStart < Math.Min(line.End, contentEnd))
                {
                    content.Append(ShiftIndentation(originalText.ToString(TextSpan.FromBounds(line.Start, textStart)), columnDelta));
                }

                copyStart = textStart;
            }

            content.Append(originalText.ToString(TextSpan.FromBounds(copyStart, lineEnd)));
        }

        content.Append(endOfLine)
               .Append(closeBraceIndentation);

        var change = new TextChange(TextSpan.FromBounds(formattedBlock.OpenBraceToken.Span.End, formattedBlock.CloseBraceToken.SpanStart), content.ToString());

        return formattedDocument.WithText(formattedText.WithChanges(change));
    }

    /// <summary>
    /// Gets the text that stays behind the opening brace on its line: the comments behind the brace when nothing else
    /// follows it on its line
    /// </summary>
    /// <param name="openBraceToken">Opening brace of the anonymous method's block</param>
    /// <param name="contentSearchStart">Position at which the moved content is searched</param>
    /// <returns>The text behind the new opening brace, starting with a space, or an empty string</returns>
    private static string GetOpenBraceTail(SyntaxToken openBraceToken, out int contentSearchStart)
    {
        var trailingTrivia = openBraceToken.TrailingTrivia;

        contentSearchStart = openBraceToken.Span.End;

        if (trailingTrivia.Count == 0
            || trailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia) == false
            || trailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) && trivia.ToFullString().Contains('\n')))
        {
            return string.Empty;
        }

        contentSearchStart = trailingTrivia.Span.End;

        var comments = trailingTrivia.Where(static trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                                     .Select(static trivia => trivia.ToFullString())
                                     .ToList();

        return comments.Count == 0
                   ? string.Empty
                   : " " + string.Join(" ", comments);
    }

    /// <summary>
    /// Determines whether a line of the moved code can move by the column delta. A line that starts inside the text of
    /// a string literal other than a raw string literal keeps its columns, because its whitespace is part of the
    /// literal's value. A line inside a multi-line raw string literal moves together with the literal's closing marker,
    /// which keeps the literal's value, unless the closing marker's indentation is narrower than a move to the left
    /// </summary>
    /// <param name="root">Root of the original document</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="lineStart">Start of the line</param>
    /// <param name="columnDelta">Number of columns the moved code moves</param>
    /// <returns><see langword="true"/> if the line moves</returns>
    private static bool CanMoveLine(SyntaxNode root, SourceText text, int lineStart, int columnDelta)
    {
        var token = root.FindToken(lineStart, findInsideTrivia: true);

        if (token.Span.Start >= lineStart
            || token.Span.End <= lineStart)
        {
            return true;
        }

        var literal = token.Parent?.AncestorsAndSelf().FirstOrDefault(static node => node is LiteralExpressionSyntax or InterpolatedStringExpressionSyntax);

        if (literal == null)
        {
            return true;
        }

        var isRawStringLiteral = literal switch
                                 {
                                     LiteralExpressionSyntax literalExpression => literalExpression.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
                                                                                  || literalExpression.Token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken),
                                     InterpolatedStringExpressionSyntax interpolatedString => interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken),
                                     _ => false,
                                 };

        if (isRawStringLiteral == false)
        {
            return false;
        }

        var closingLine = text.Lines.GetLineFromPosition(literal.Span.End);

        return columnDelta >= 0
               || GetColumn(text, SkipIndentation(text, closingLine.Start, closingLine.End)) + columnDelta >= 0;
    }

    /// <summary>
    /// Moves an indentation by a number of columns. A move to the right puts spaces in front of it; a move to the left
    /// removes its leading characters, a tab counted as four columns, so that the characters behind the removed ones
    /// stay as written
    /// </summary>
    /// <param name="indentation">Indentation, spaces and tabs</param>
    /// <param name="columnDelta">Number of columns</param>
    /// <returns>The moved indentation</returns>
    private static string ShiftIndentation(string indentation, int columnDelta)
    {
        if (columnDelta >= 0)
        {
            return new string(' ', columnDelta) + indentation;
        }

        var remainingColumns = -columnDelta;
        var index = 0;

        while (remainingColumns > 0
               && index < indentation.Length)
        {
            var width = indentation[index] == '\t' ? IndentationWidth : 1;

            if (width > remainingColumns)
            {
                return new string(' ', width - remainingColumns) + indentation.Substring(index + 1);
            }

            remainingColumns -= width;
            index++;
        }

        return indentation.Substring(index);
    }

    /// <summary>
    /// Gets the column of a position on its line, a tab counted as four columns
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="position">Position</param>
    /// <returns>The column</returns>
    private static int GetColumn(SourceText text, int position)
    {
        var column = 0;

        for (var index = text.Lines.GetLineFromPosition(position).Start; index < position; index++)
        {
            column += text[index] == '\t' ? IndentationWidth : 1;
        }

        return column;
    }

    /// <summary>
    /// Skips spaces and tabs
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="position">Start position</param>
    /// <param name="end">End of the search</param>
    /// <returns>The first position that is neither a space nor a tab, or <paramref name="end"/></returns>
    private static int SkipIndentation(SourceText text, int position, int end)
    {
        while (position < end
               && (text[position] == ' ' || text[position] == '\t'))
        {
            position++;
        }

        return position;
    }

    /// <summary>
    /// Skips whitespace and line breaks
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="position">Start position</param>
    /// <param name="end">End of the search</param>
    /// <returns>The first position that is not whitespace, or <paramref name="end"/></returns>
    private static int SkipWhitespace(SourceText text, int position, int end)
    {
        while (position < end
               && char.IsWhiteSpace(text[position]))
        {
            position++;
        }

        return position;
    }

    /// <summary>
    /// Skips whitespace and line breaks backwards
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="start">Start of the search</param>
    /// <param name="position">End position</param>
    /// <returns>The position behind the last character that is not whitespace, or <paramref name="start"/></returns>
    private static int SkipWhitespaceBackwards(SourceText text, int start, int position)
    {
        while (position > start
               && char.IsWhiteSpace(text[position - 1]))
        {
            position--;
        }

        return position;
    }

    /// <summary>
    /// Determines whether the anonymous method is directly the operand of a cast or an operand of a binary operator.
    /// A lambda is not a valid operand there without parentheses: in front of an operator, its body takes in the rest
    /// of the expression, and behind a cast or an operator, it does not parse as the operand
    /// </summary>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <returns><see langword="true"/> if the anonymous method is the operand of a cast or a binary operator</returns>
    private static bool IsOperandOfCastOrBinaryOperator(AnonymousMethodExpressionSyntax anonymousMethodExpression)
    {
        return anonymousMethodExpression.Parent is CastExpressionSyntax or BinaryExpressionSyntax;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3004UseLambdaSyntaxAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var anonymousMethodExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<AnonymousMethodExpressionSyntax>().FirstOrDefault();

                if (anonymousMethodExpression?.ParameterList != null
                    && ContainsDiscardedTrivia(anonymousMethodExpression) == false
                    && IsOperandOfCastOrBinaryOperator(anonymousMethodExpression) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3004Title,
                                                              token => ApplyCodeFixAsync(context.Document, anonymousMethodExpression, token),
                                                              nameof(RH3004UseLambdaSyntaxCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}