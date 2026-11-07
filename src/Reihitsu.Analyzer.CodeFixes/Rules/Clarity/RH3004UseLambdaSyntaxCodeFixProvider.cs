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
    /// Gets the expression the lambda's expression body is written from: the expression of the block's only statement,
    /// a <see langword="return"/> statement, when the block holds no comment or directive. Otherwise, the block stays the
    /// lambda's block body
    /// </summary>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <returns>The expression, or <see langword="null"/> when the block stays a block body</returns>
    private static ExpressionSyntax GetExpressionBody(AnonymousMethodExpressionSyntax anonymousMethodExpression)
    {
        return anonymousMethodExpression.Block is { Statements.Count: 1 } block
               && block.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement
               && SyntaxNodeUtilities.ContainsCommentOrDirective(block) == false
                   ? returnStatement.Expression
                   : null;
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
        var expressionBody = GetExpressionBody(anonymousMethodExpression);
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
    /// its first to its last statement, with their comments and the blank lines between them, keeps its own layout. Every
    /// line either moves or stays (<see cref="IsMovingLine"/>), and every moving line moves by the same column delta,
    /// measured on the block's first statement line, which always moves, so that this line stands one level inside the
    /// braces the formatter laid out. When the first statement shares the opening brace's line, the moving lines after it
    /// move by their own delta that puts the leftmost of them in the statement column, where one of them starts left of
    /// the first statement. Otherwise, where the delta would put a moving line left of column 0, every moving line moves
    /// by the delta that puts the leftmost of them in the statement column. Statements that shared the block's line stay
    /// on one line. A comment behind the opening brace on its line stays behind the new opening brace
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

        var elements = GetLineElements(originalBlock);
        var firstLine = originalText.Lines.GetLineFromPosition(contentStart);
        var startsOnOpenBraceLine = originalText.Lines.GetLineFromPosition(originalBlock.OpenBraceToken.SpanStart).LineNumber == firstLine.LineNumber;
        var measuredPosition = GetFirstStatementLinePosition(elements, originalText, originalBlock, contentStart, contentEnd);
        var measuredColumn = GetColumn(originalText, measuredPosition);
        var columnDelta = contentIndentation.Length - measuredColumn;
        var laterLineNumber = startsOnOpenBraceLine ? firstLine.LineNumber + 1 : firstLine.LineNumber;
        var laterColumns = GetMovingLineColumns(elements, originalText, laterLineNumber, contentEnd);
        var sharesOpenBraceLine = startsOnOpenBraceLine
                                  && originalText.Lines.GetLineFromPosition(measuredPosition).LineNumber == firstLine.LineNumber;
        var movedColumns = startsOnOpenBraceLine && sharesOpenBraceLine == false
                               ? laterColumns.Append(GetColumn(originalText, contentStart)).ToList()
                               : laterColumns;
        var laterColumnDelta = GetColumnDeltas(sharesOpenBraceLine, movedColumns, measuredColumn, contentIndentation.Length, ref columnDelta);

        var content = new StringBuilder();

        content.Append(openBraceTail)
               .Append(endOfLine);

        if (startsOnOpenBraceLine)
        {
            content.Append(new string(' ', GetColumn(originalText, contentStart) + columnDelta))
                   .Append(originalText.ToString(TextSpan.FromBounds(contentStart, Math.Min(firstLine.EndIncludingLineBreak, contentEnd))));
        }

        AppendLaterLines(content, originalText, elements, laterLineNumber, contentEnd, laterColumnDelta);

        content.Append(endOfLine)
               .Append(closeBraceIndentation);

        var change = new TextChange(TextSpan.FromBounds(formattedBlock.OpenBraceToken.Span.End, formattedBlock.CloseBraceToken.SpanStart), content.ToString());

        return formattedDocument.WithText(formattedText.WithChanges(change));
    }

    /// <summary>
    /// Appends the lines of the moved code after the first statement line: a moving line keeps its text and shifts its
    /// indentation by the column delta, a line that stays is copied as written
    /// </summary>
    /// <param name="content">Content built so far</param>
    /// <param name="originalText">Text of the original document</param>
    /// <param name="elements">Elements of the block's lines</param>
    /// <param name="firstLineNumber">Number of the first line to append</param>
    /// <param name="contentEnd">End of the moved code</param>
    /// <param name="columnDelta">Delta of the moving lines</param>
    private static void AppendLaterLines(StringBuilder content, SourceText originalText, List<(TextSpan Span, SyntaxKind Kind)> elements, int firstLineNumber, int contentEnd, int columnDelta)
    {
        for (var lineIndex = firstLineNumber; lineIndex < originalText.Lines.Count && originalText.Lines[lineIndex].Start < contentEnd; lineIndex++)
        {
            var line = originalText.Lines[lineIndex];
            var lineEnd = Math.Min(line.EndIncludingLineBreak, contentEnd);
            var textStart = SkipIndentation(originalText, line.Start, Math.Min(line.End, contentEnd));
            var copyStart = line.Start;

            if (IsMovingLine(elements, originalText, lineIndex, contentEnd))
            {
                if (textStart < Math.Min(line.End, contentEnd))
                {
                    content.Append(ShiftIndentation(originalText.ToString(TextSpan.FromBounds(line.Start, textStart)), columnDelta));
                }

                copyStart = textStart;
            }

            content.Append(originalText.ToString(TextSpan.FromBounds(copyStart, lineEnd)));
        }
    }

    /// <summary>
    /// Gets the column delta of the moving lines after the first statement line and, where the measured delta would put
    /// a moving line left of column 0, corrects the delta of the first statement line. When the first statement shares
    /// the opening brace's line, the lines after it keep their offset to it only when none of them starts left of it,
    /// otherwise the leftmost of them moves into the statement column. Otherwise every moving line moves by one number of
    /// columns, and where the measured one would put a moving line left of column 0, the leftmost moving line moves into
    /// the statement column instead
    /// </summary>
    /// <param name="sharesOpenBraceLine">Whether the first statement shares the opening brace's line</param>
    /// <param name="movedColumns">Columns of the moving lines the delta applies to</param>
    /// <param name="measuredColumn">Column the delta is measured on</param>
    /// <param name="statementColumn">Column of the statements in the formatted block</param>
    /// <param name="columnDelta">Delta of the first statement line; corrected when it would put a moving line left of column 0</param>
    /// <returns>The delta of the moving lines after the first statement line</returns>
    private static int GetColumnDeltas(bool sharesOpenBraceLine, List<int> movedColumns, int measuredColumn, int statementColumn, ref int columnDelta)
    {
        if (sharesOpenBraceLine)
        {
            return movedColumns.Count > 0 && movedColumns.Min() < measuredColumn
                       ? statementColumn - movedColumns.Min()
                       : columnDelta;
        }

        if (movedColumns.Count > 0
            && movedColumns.Min() + columnDelta < 0)
        {
            columnDelta = statementColumn - movedColumns.Min();
        }

        return columnDelta;
    }

    /// <summary>
    /// Gets the position the column delta of the moved code is measured on: the first character of the block's first
    /// statement line, also where it starts a comment in front of the statement. That is the first line, in source order,
    /// on which code starts, so it always moves. A block in which no line holds code is measured on its first moving line
    /// </summary>
    /// <param name="elements">Elements of the block's lines</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="block">Block of the anonymous method</param>
    /// <param name="contentStart">Start of the moved code</param>
    /// <param name="contentEnd">End of the moved code</param>
    /// <returns>The position the column delta is measured on</returns>
    private static int GetFirstStatementLinePosition(List<(TextSpan Span, SyntaxKind Kind)> elements, SourceText text, BlockSyntax block, int contentStart, int contentEnd)
    {
        var firstLineNumber = text.Lines.GetLineFromPosition(contentStart).LineNumber;
        var lastLineNumber = block.Statements.Count > 0
                                 ? text.Lines.GetLineFromPosition(block.Statements[0].SpanStart).LineNumber
                                 : text.Lines.GetLineFromPosition(contentEnd).LineNumber;

        for (var lineNumber = firstLineNumber; lineNumber <= lastLineNumber; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var lineStart = Math.Max(line.Start, contentStart);
            var lineEnd = Math.Min(line.End, contentEnd);

            if (elements.Exists(element => IsCode(element) && element.Span.Start >= lineStart && element.Span.Start < lineEnd))
            {
                return SkipIndentation(text, lineStart, line.End);
            }
        }

        for (var lineNumber = firstLineNumber; lineNumber <= lastLineNumber; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var position = SkipIndentation(text, Math.Max(line.Start, contentStart), line.End);

            if (position < Math.Min(line.End, contentEnd)
                && IsMovingLine(elements, text, lineNumber, contentEnd))
            {
                return position;
            }
        }

        return contentStart;
    }

    /// <summary>
    /// Gets the elements the outcome of the block's lines is decided on, in source order: every token and every comment.
    /// The fix is offered only for a block without directives and disabled text, and without strings and delimited
    /// comments that span lines, so every element but a documentation comment written with <c>///</c> lies on one line
    /// </summary>
    /// <param name="block">Block of the anonymous method</param>
    /// <returns>The elements, with their spans and kinds</returns>
    private static List<(TextSpan Span, SyntaxKind Kind)> GetLineElements(BlockSyntax block)
    {
        var elements = new List<(TextSpan Span, SyntaxKind Kind)>();

        foreach (var token in block.DescendantTokens())
        {
            AddTriviaElements(token.LeadingTrivia, elements);
            AddTriviaElements(token.TrailingTrivia, elements);

            if (token.Span.IsEmpty == false)
            {
                elements.Add((token.Span, token.Kind()));
            }
        }

        elements.Sort(static (left, right) => left.Span.Start.CompareTo(right.Span.Start));

        return elements;
    }

    /// <summary>
    /// Adds the comments and skipped tokens of a trivia list to the elements of the block's lines
    /// </summary>
    /// <param name="triviaList">Trivia</param>
    /// <param name="elements">Elements</param>
    private static void AddTriviaElements(SyntaxTriviaList triviaList, List<(TextSpan Span, SyntaxKind Kind)> elements)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false
                && trivia.FullSpan.IsEmpty == false)
            {
                // The full span of a documentation comment starts at its first marker, which its span leaves out
                elements.Add((trivia.FullSpan, trivia.Kind()));
            }
        }
    }

    /// <summary>
    /// Determines whether an element of the block's lines is code: a token or skipped tokens
    /// </summary>
    /// <param name="element">Element</param>
    /// <returns><see langword="true"/> for code</returns>
    private static bool IsCode((TextSpan Span, SyntaxKind Kind) element)
    {
        return SyntaxFacts.IsTrivia(element.Kind) == false
               || element.Kind == SyntaxKind.SkippedTokensTrivia;
    }

    /// <summary>
    /// Determines whether a line of the moved code moves with the statements or stays in its columns. A line stays when
    /// it holds no code and its first comment starts in column 0; a line of a documentation comment written with
    /// <c>///</c> is decided on its own marker. Every other line moves, above all every line that holds code
    /// </summary>
    /// <param name="elements">Elements of the block's lines</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="lineNumber">Number of the line</param>
    /// <param name="contentEnd">End of the moved code</param>
    /// <returns><see langword="true"/> if the line moves; <see langword="false"/> if it stays</returns>
    private static bool IsMovingLine(List<(TextSpan Span, SyntaxKind Kind)> elements, SourceText text, int lineNumber, int contentEnd)
    {
        var line = text.Lines[lineNumber];
        var lineEnd = Math.Min(line.End, contentEnd);
        (TextSpan Span, SyntaxKind Kind)? firstElement = null;

        foreach (var element in elements)
        {
            if (element.Span.Start >= lineEnd)
            {
                break;
            }

            if (element.Span.End <= line.Start)
            {
                continue;
            }

            if (IsCode(element))
            {
                return true;
            }

            firstElement ??= element;
        }

        if (firstElement is not { } first)
        {
            return true;
        }

        if (first.Span.Start < line.Start)
        {
            // Only a documentation comment written with /// spans lines here, each of which starts with its own marker
            return SkipIndentation(text, line.Start, lineEnd) != line.Start;
        }

        return first.Span.Start != line.Start;
    }

    /// <summary>
    /// Gets the columns of the moving lines of the moved code from a line on: every moving line that holds text
    /// </summary>
    /// <param name="elements">Elements of the block's lines</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="firstLineNumber">Number of the first line</param>
    /// <param name="contentEnd">End of the moved code</param>
    /// <returns>The columns</returns>
    private static List<int> GetMovingLineColumns(List<(TextSpan Span, SyntaxKind Kind)> elements, SourceText text, int firstLineNumber, int contentEnd)
    {
        var columns = new List<int>();

        for (var lineNumber = firstLineNumber; lineNumber < text.Lines.Count && text.Lines[lineNumber].Start < contentEnd; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var textStart = SkipIndentation(text, line.Start, Math.Min(line.End, contentEnd));

            if (textStart < Math.Min(line.End, contentEnd)
                && IsMovingLine(elements, text, lineNumber, contentEnd))
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
            || trailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia) == false)
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
    /// Determines whether the anonymous method, from its first token through the block's closing brace, holds a
    /// preprocessor directive or disabled text, on either path of the fix. The lambda is reassembled from the text of the
    /// parameter list and of the body, so a directive structure inside the anonymous method cannot be kept; directives
    /// wholly outside it are left alone. Disabled text needs no check of its own: it ends at a directive, and that
    /// directive lies inside the anonymous method too, because a token of the anonymous method follows the disabled text
    /// </summary>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <returns><see langword="true"/> if the anonymous method holds a directive or disabled text</returns>
    private static bool ContainsDirectiveOrDisabledText(AnonymousMethodExpressionSyntax anonymousMethodExpression)
    {
        return SyntaxTriviaUtilities.ContainsDirectives(anonymousMethodExpression, anonymousMethodExpression.Span);
    }

    /// <summary>
    /// Determines whether the anonymous method, from its first token through the block's closing brace, its parameter
    /// list included, holds a string literal or interpolated string of any kind, or a delimited comment, that starts on
    /// one line and ends on a later one and so withholds the fix. On both paths, such a string or comment withholds the fix
    /// when a later line of it starts with <c>#</c> behind lexer whitespace: that line can be a directive in another
    /// build, where every edit the fix makes behind it can change a string's value. Where the fix keeps the block as a
    /// block body, every such string or comment withholds the fix, because the fix moves the block's lines and no reading
    /// of the text alone tells which of them are safe to move in every build
    /// </summary>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <param name="text">Text of the document</param>
    /// <param name="keepsBlockBody">Whether the fix keeps the block as the lambda's block body</param>
    /// <returns><see langword="true"/> if a string or a delimited comment that spans lines withholds the fix, <see langword="false"/> otherwise</returns>
    private static bool WithholdsFixForSpanningStringOrComment(AnonymousMethodExpressionSyntax anonymousMethodExpression, SourceText text, bool keepsBlockBody)
    {
        var span = anonymousMethodExpression.Span;
        var syntaxTree = anonymousMethodExpression.SyntaxTree;

        return anonymousMethodExpression.DescendantNodes()
                                        .Where(IsString)
                                        .Select(stringExpression => stringExpression.Span)
                                        .Concat(anonymousMethodExpression.DescendantTrivia()
                                                                         .Where(trivia => (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                                                                                          && span.Contains(trivia.Span))
                                                                         .Select(trivia => trivia.Span))
                                        .Any(carrierSpan => SyntaxNodeUtilities.IsSingleLineSpan(syntaxTree, carrierSpan) == false
                                                            && (keepsBlockBody || ContainsLineStartingWithHash(text, carrierSpan)));
    }

    /// <summary>
    /// Determines whether a later line of a span that starts on one line and ends on a later one has <c>#</c> as its first
    /// character inside the span that is not whitespace as the C# lexer counts it
    /// </summary>
    /// <param name="text">Text of the document</param>
    /// <param name="span">Span that starts on one line and ends on a later one</param>
    /// <returns><see langword="true"/> if a later line of the span starts with <c>#</c></returns>
    private static bool ContainsLineStartingWithHash(SourceText text, TextSpan span)
    {
        var firstLineNumber = text.Lines.GetLineFromPosition(span.Start).LineNumber;
        var lastLineNumber = text.Lines.GetLineFromPosition(span.End).LineNumber;

        for (var lineNumber = firstLineNumber + 1; lineNumber <= lastLineNumber; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var position = line.Start;

            while (position < line.End
                   && SyntaxFacts.IsWhitespace(text[position]))
            {
                position++;
            }

            if (position < span.End
                && position < line.End
                && text[position] == '#')
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a node is a string: a string literal, also a UTF-8 or raw one, or an interpolated string
    /// expression
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> for a string</returns>
    private static bool IsString(SyntaxNode node)
    {
        return node is InterpolatedStringExpressionSyntax
               || node.IsKind(SyntaxKind.StringLiteralExpression)
               || node.IsKind(SyntaxKind.Utf8StringLiteralExpression);
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
            var text = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var anonymousMethodExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<AnonymousMethodExpressionSyntax>().FirstOrDefault();

                if (anonymousMethodExpression?.ParameterList != null
                    && ContainsDiscardedTrivia(anonymousMethodExpression) == false
                    && IsOperandOfCastOrBinaryOperator(anonymousMethodExpression) == false
                    && ContainsDirectiveOrDisabledText(anonymousMethodExpression) == false
                    && WithholdsFixForSpanningStringOrComment(anonymousMethodExpression, text, GetExpressionBody(anonymousMethodExpression) == null) == false)
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