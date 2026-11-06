using System.Collections.Generic;
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

using Reihitsu.Analyzer.CodeFixes.Core;
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
    /// its first to its last statement, with their comments, directives, and the blank lines between them, keeps its own
    /// layout, and every one of its lines moves by the same column delta, so that the block's first statement line
    /// stands one level inside the braces the formatter laid out. A directive line in column 0 other than
    /// <c>#region</c> and <c>#endregion</c>, and a line whose comment starts in column 0, stay in column 0, and a line
    /// that continues a verbatim string literal keeps its columns. When the first statement shares the opening brace's
    /// line, the lines after it move by their own delta that puts the leftmost of them in the statement column, where one
    /// of them starts left of the first statement. Otherwise, where the delta would put a moved line left of column 0,
    /// every moved line moves by the delta that puts the leftmost of them in the statement column. Statements that shared
    /// the block's line stay on one line. A comment behind the opening brace on its line stays behind the new opening brace
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

        if (originalRoot == null)
        {
            return formattedDocument;
        }

        var firstLine = originalText.Lines.GetLineFromPosition(contentStart);
        var startsOnOpenBraceLine = originalText.Lines.GetLineFromPosition(originalBlock.OpenBraceToken.SpanStart).LineNumber == firstLine.LineNumber;
        var measuredPosition = GetFirstStatementLinePosition(originalRoot, originalText, originalBlock, contentStart, contentEnd);
        var measuredColumn = GetColumn(originalText, measuredPosition);
        var columnDelta = contentIndentation.Length - measuredColumn;
        var laterLineNumber = startsOnOpenBraceLine ? firstLine.LineNumber + 1 : firstLine.LineNumber;
        var laterColumns = GetMovedLineColumns(originalRoot, originalText, laterLineNumber, contentEnd);
        var laterColumnDelta = columnDelta;

        if (startsOnOpenBraceLine
            && originalText.Lines.GetLineFromPosition(measuredPosition).LineNumber == firstLine.LineNumber)
        {
            // The first statement shares the opening brace's line: the lines after it keep their offset to it only when
            // none of them starts left of it, otherwise the leftmost of them moves into the statement column
            if (laterColumns.Count > 0
                && laterColumns.Min() < measuredColumn)
            {
                laterColumnDelta = contentIndentation.Length - laterColumns.Min();
            }
        }
        else
        {
            // The whole block moves by one number of columns; where the measured one would put a line left of column 0,
            // the leftmost line moves into the statement column instead
            var movedColumns = startsOnOpenBraceLine
                                   ? laterColumns.Append(GetColumn(originalText, contentStart)).ToList()
                                   : laterColumns;

            if (movedColumns.Count > 0
                && movedColumns.Min() + columnDelta < 0)
            {
                columnDelta = contentIndentation.Length - movedColumns.Min();
                laterColumnDelta = columnDelta;
            }
        }

        var content = new StringBuilder();

        content.Append(openBraceTail)
               .Append(endOfLine);

        if (startsOnOpenBraceLine)
        {
            content.Append(new string(' ', GetColumn(originalText, contentStart) + columnDelta))
                   .Append(originalText.ToString(TextSpan.FromBounds(contentStart, Math.Min(firstLine.EndIncludingLineBreak, contentEnd))));
        }

        for (var lineIndex = laterLineNumber; lineIndex < originalText.Lines.Count && originalText.Lines[lineIndex].Start < contentEnd; lineIndex++)
        {
            var line = originalText.Lines[lineIndex];
            var lineEnd = Math.Min(line.EndIncludingLineBreak, contentEnd);
            var textStart = SkipIndentation(originalText, line.Start, Math.Min(line.End, contentEnd));
            var copyStart = line.Start;

            if (StaysInColumnZero(originalRoot, originalText, line) == false
                && KeepsColumns(originalRoot, line.Start) == false)
            {
                if (textStart < Math.Min(line.End, contentEnd))
                {
                    content.Append(ShiftIndentation(originalText.ToString(TextSpan.FromBounds(line.Start, textStart)), laterColumnDelta));
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
    /// Gets the position the column delta of the moved code is measured on: the first character of the moved code on the
    /// block's first statement line. That is the first line, in source order, on which a statement starts, whether the
    /// statement is active code or disabled text of an inactive branch, so the result does not depend on which
    /// preprocessor symbols are defined. A line of disabled text holds a statement when it holds anything other than
    /// whitespace and comments. A block in which no line holds a statement is measured on its first line that does not
    /// stay in column 0
    /// </summary>
    /// <param name="root">Root of the original document</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="block">Block of the anonymous method</param>
    /// <param name="contentStart">Start of the moved code</param>
    /// <param name="contentEnd">End of the moved code</param>
    /// <returns>The position the column delta is measured on</returns>
    private static int GetFirstStatementLinePosition(SyntaxNode root, SourceText text, BlockSyntax block, int contentStart, int contentEnd)
    {
        var firstLineNumber = text.Lines.GetLineFromPosition(contentStart).LineNumber;
        var lastLineNumber = block.Statements.Count > 0
                                 ? text.Lines.GetLineFromPosition(block.Statements[0].SpanStart).LineNumber
                                 : text.Lines.GetLineFromPosition(contentEnd).LineNumber;

        for (var lineNumber = firstLineNumber; lineNumber <= lastLineNumber; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var position = SkipIndentation(text, Math.Max(line.Start, contentStart), line.End);

            if (lineNumber == lastLineNumber
                && block.Statements.Count > 0)
            {
                return position;
            }

            if (position < line.End
                && IsDisabledStatementLine(root, text, line, position))
            {
                return position;
            }
        }

        for (var lineNumber = firstLineNumber; lineNumber <= lastLineNumber; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var position = SkipIndentation(text, Math.Max(line.Start, contentStart), line.End);

            if (position < line.End
                && StaysInColumnZero(root, text, line) == false)
            {
                return position;
            }
        }

        return contentStart;
    }

    /// <summary>
    /// Determines whether the line is disabled text of an inactive branch that holds anything other than whitespace and
    /// comments
    /// </summary>
    /// <param name="root">Root of the original document</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="line">Line</param>
    /// <param name="position">First position on the line that is neither a space nor a tab</param>
    /// <returns><see langword="true"/> if the line holds a statement in disabled text</returns>
    private static bool IsDisabledStatementLine(SyntaxNode root, SourceText text, TextLine line, int position)
    {
        if (root.FindTrivia(position).IsKind(SyntaxKind.DisabledTextTrivia) == false)
        {
            return false;
        }

        var lineText = text.ToString(TextSpan.FromBounds(position, line.End));
        var trivia = SyntaxFactory.ParseLeadingTrivia(lineText);

        return trivia.FullSpan.Length < lineText.Length
               || trivia.Any(static triviaItem => triviaItem.IsKind(SyntaxKind.WhitespaceTrivia) == false
                                                  && triviaItem.IsKind(SyntaxKind.EndOfLineTrivia) == false
                                                  && triviaItem.IsKind(SyntaxKind.SingleLineCommentTrivia) == false
                                                  && triviaItem.IsKind(SyntaxKind.MultiLineCommentTrivia) == false);
    }

    /// <summary>
    /// Determines whether the line stays in column 0: a preprocessor directive line that starts in column 0 and is
    /// neither <c>#region</c> nor <c>#endregion</c>, or a line whose comment starts in column 0, also in disabled text.
    /// Such a line belongs to no nesting depth and keeps its column. The continuation line of a multi-line comment is
    /// not such a line, and it moves with its comment
    /// </summary>
    /// <param name="root">Root of the original document</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="line">Line</param>
    /// <returns><see langword="true"/> if the line keeps column 0</returns>
    private static bool StaysInColumnZero(SyntaxNode root, SourceText text, TextLine line)
    {
        if (line.Span.IsEmpty)
        {
            return false;
        }

        var trivia = root.FindTrivia(line.Start);

        if (trivia.IsKind(SyntaxKind.DisabledTextTrivia))
        {
            return line.Span.Length >= 2
                   && text[line.Start] == '/'
                   && (text[line.Start + 1] == '/' || text[line.Start + 1] == '*');
        }

        if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
        {
            // One documentation comment spans every one of its lines, each of which starts with its own marker
            return text.ToString(TextSpan.FromBounds(line.Start, line.End)).StartsWith("///", StringComparison.Ordinal);
        }

        if (trivia.SpanStart != line.Start)
        {
            return false;
        }

        if (trivia.IsDirective)
        {
            return trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) == false
                   && trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false;
        }

        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Gets the columns of the lines of the moved code from a line on that move with the code: every line that holds
    /// text, except the lines that stay in column 0 and the lines that keep their columns
    /// </summary>
    /// <param name="root">Root of the original document</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="firstLineNumber">Number of the first line</param>
    /// <param name="contentEnd">End of the moved code</param>
    /// <returns>The columns</returns>
    private static List<int> GetMovedLineColumns(SyntaxNode root, SourceText text, int firstLineNumber, int contentEnd)
    {
        var columns = new List<int>();

        for (var lineNumber = firstLineNumber; lineNumber < text.Lines.Count && text.Lines[lineNumber].Start < contentEnd; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var textStart = SkipIndentation(text, line.Start, Math.Min(line.End, contentEnd));

            if (textStart < Math.Min(line.End, contentEnd)
                && StaysInColumnZero(root, text, line) == false
                && KeepsColumns(root, line.Start) == false)
            {
                columns.Add(GetColumn(text, textStart));
            }
        }

        return columns;
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
    /// Determines whether a line of the moved code keeps its columns: a line that starts inside the text of a string
    /// literal continues the literal, and its whitespace is part of the literal's value. A multi-line raw string literal
    /// withholds the fix (RH3004-007), so only verbatim string literals reach this
    /// </summary>
    /// <param name="root">Root of the original document</param>
    /// <param name="lineStart">Start of the line</param>
    /// <returns><see langword="true"/> if the line keeps its columns</returns>
    private static bool KeepsColumns(SyntaxNode root, int lineStart)
    {
        var token = root.FindToken(lineStart, findInsideTrivia: true);

        return token.Span.Start < lineStart
               && token.Span.End > lineStart;
    }

    /// <summary>
    /// Determines whether the block holds a multi-line raw string literal, also an interpolated or a UTF-8 one, in its
    /// code or in the disabled text of an inactive branch, so that the fix does not depend on the defined preprocessor
    /// symbols
    /// </summary>
    /// <param name="block">Block of the anonymous method</param>
    /// <returns><see langword="true"/> if the block holds a multi-line raw string literal</returns>
    private static bool ContainsMultiLineRawStringLiteral(BlockSyntax block)
    {
        return block.DescendantTokens().Any(IsMultiLineRawStringLiteralToken)
               || block.DescendantTrivia()
                       .Where(static trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia))
                       .Any(static trivia => SyntaxFactory.ParseTokens(trivia.ToFullString()).Any(IsMultiLineRawStringLiteralToken));
    }

    /// <summary>
    /// Determines whether the token is a multi-line raw string literal or the start of a multi-line interpolated raw
    /// string literal
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> for such a token</returns>
    private static bool IsMultiLineRawStringLiteralToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
               || token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken)
               || token.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken);
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
    /// <remarks>
    /// The fixes of a document are applied in document order, so that an anonymous method inside the block of another
    /// is fixed where the outer fix moved it (SHARED-FIX-002)
    /// </remarks>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return DocumentOrderFixAllProvider.Create(new RH3004UseLambdaSyntaxAnalyzer());
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
                    && IsOperandOfCastOrBinaryOperator(anonymousMethodExpression) == false
                    && ContainsMultiLineRawStringLiteral(anonymousMethodExpression.Block) == false)
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