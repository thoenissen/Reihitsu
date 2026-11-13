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
    /// Creates the lambda expression the fix replaces the anonymous method with, before the formatter lays it out
    /// </summary>
    /// <param name="anonymousMethodExpression">Anonymous method expression with a parameter list</param>
    /// <returns>The lambda expression, with the anonymous method's leading and trailing trivia</returns>
    private static ExpressionSyntax CreateLambdaExpression(AnonymousMethodExpressionSyntax anonymousMethodExpression)
    {
        var asyncPrefix = anonymousMethodExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword)
                              ? "async "
                              : string.Empty;
        var bodyText = GetExpressionBody(anonymousMethodExpression)?.ToString() ?? anonymousMethodExpression.Block.ToString();

        return SyntaxFactory.ParseExpression($"{asyncPrefix}{anonymousMethodExpression.ParameterList} => {bodyText}")
                            .WithTriviaFrom(anonymousMethodExpression);
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

        var expressionBody = GetExpressionBody(anonymousMethodExpression);
        var formattingAnnotation = new SyntaxAnnotation();
        var replacementExpression = CreateLambdaExpression(anonymousMethodExpression).WithAdditionalAnnotations(formattingAnnotation);
        var updatedDocument = document.WithSyntaxRoot(root.ReplaceNode(anonymousMethodExpression, replacementExpression));
        var updatedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (updatedRoot?.GetAnnotatedNodes(formattingAnnotation).FirstOrDefault() is not { } lambdaExpression)
        {
            return updatedDocument;
        }

        // The code the fix creates, the parameter list, the arrow, and the braces, is laid out as the formatter lays it
        // out: a block body on lines of its own, aligned under the lambda's anchor
        var formattedDocument = (await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, lambdaExpression, cancellationToken).ConfigureAwait(false)).Output;

        if (expressionBody != null)
        {
            return formattedDocument;
        }

        return await MoveStatementsAsync(document, formattedDocument, anonymousMethodExpression.Block, formattingAnnotation, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes the content of the anonymous method's block into the formatted lambda's block as moved code: the text from
    /// its first to its last statement, with their comments and the blank lines between them, keeps its own layout. Every
    /// line either moves or stays (<see cref="SyntaxIndentationUtilities.KeepsItsColumn"/>), and every moving line moves
    /// by the same column delta (<see cref="SyntaxIndentationUtilities.TryShiftMovedLines"/>), measured on the block's
    /// first statement line, which always moves, so that this line stands one level inside the braces the formatter laid
    /// out. When the first statement shares the opening brace's line, the moving lines after it
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

        var originalRoot = await originalDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var firstLine = originalText.Lines.GetLineFromPosition(contentStart);
        var startsOnOpenBraceLine = originalText.Lines.GetLineFromPosition(originalBlock.OpenBraceToken.SpanStart).LineNumber == firstLine.LineNumber;
        var measuredPosition = GetFirstStatementLinePosition(originalRoot, originalText, originalBlock, contentStart, contentEnd);
        var measuredColumn = GetColumn(originalText, measuredPosition);
        var columnDelta = contentIndentation.Length - measuredColumn;
        var laterLineNumber = startsOnOpenBraceLine ? firstLine.LineNumber + 1 : firstLine.LineNumber;
        var laterColumns = GetMovingLineColumns(originalRoot, originalText, laterLineNumber, contentEnd);
        var sharesOpenBraceLine = startsOnOpenBraceLine
                                  && originalText.Lines.GetLineFromPosition(measuredPosition).LineNumber == firstLine.LineNumber;
        var movedColumns = startsOnOpenBraceLine && sharesOpenBraceLine == false
                               ? laterColumns.Append(GetColumn(originalText, contentStart)).ToList()
                               : laterColumns;
        var laterColumnDelta = GetColumnDeltas(sharesOpenBraceLine, movedColumns, measuredColumn, contentIndentation.Length, ref columnDelta);

        var laterLinesText = string.Empty;

        if (laterLineNumber < originalText.Lines.Count
            && originalText.Lines[laterLineNumber].Start < contentEnd
            && SyntaxIndentationUtilities.TryShiftMovedLines(originalRoot, originalText, TextSpan.FromBounds(originalText.Lines[laterLineNumber].Start, contentEnd), laterColumnDelta, out laterLinesText) == false)
        {
            return formattedDocument;
        }

        var content = new StringBuilder();

        content.Append(openBraceTail)
               .Append(endOfLine);

        if (startsOnOpenBraceLine)
        {
            content.Append(new string(' ', GetColumn(originalText, contentStart) + columnDelta))
                   .Append(originalText.ToString(TextSpan.FromBounds(contentStart, Math.Min(firstLine.EndIncludingLineBreak, contentEnd))));
        }

        content.Append(laterLinesText)
               .Append(endOfLine)
               .Append(closeBraceIndentation);

        var change = new TextChange(TextSpan.FromBounds(formattedBlock.OpenBraceToken.Span.End, formattedBlock.CloseBraceToken.SpanStart), content.ToString());

        return formattedDocument.WithText(formattedText.WithChanges(change));
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
    /// on which a statement starts, so it always moves. A block without a statement is measured on its first moving line
    /// </summary>
    /// <param name="root">Root of the original document</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="block">Block of the anonymous method</param>
    /// <param name="contentStart">Start of the moved code</param>
    /// <param name="contentEnd">End of the moved code</param>
    /// <returns>The position the column delta is measured on</returns>
    private static int GetFirstStatementLinePosition(SyntaxNode root, SourceText text, BlockSyntax block, int contentStart, int contentEnd)
    {
        if (block.Statements.Count > 0)
        {
            var statementLine = text.Lines.GetLineFromPosition(block.Statements[0].SpanStart);

            return SkipIndentation(text, Math.Max(statementLine.Start, contentStart), statementLine.End);
        }

        for (var lineNumber = text.Lines.GetLineFromPosition(contentStart).LineNumber; lineNumber <= text.Lines.GetLineFromPosition(contentEnd).LineNumber; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var position = SkipIndentation(text, Math.Max(line.Start, contentStart), line.End);

            if (position < Math.Min(line.End, contentEnd)
                && SyntaxIndentationUtilities.KeepsItsColumn(root, text, line, contentEnd) == false)
            {
                return position;
            }
        }

        return contentStart;
    }

    /// <summary>
    /// Gets the columns of the moving lines of the moved code from a line on: every moving line that holds text
    /// </summary>
    /// <param name="root">Root of the original document</param>
    /// <param name="text">Text of the original document</param>
    /// <param name="firstLineNumber">Number of the first line</param>
    /// <param name="contentEnd">End of the moved code</param>
    /// <returns>The columns</returns>
    private static List<int> GetMovingLineColumns(SyntaxNode root, SourceText text, int firstLineNumber, int contentEnd)
    {
        var columns = new List<int>();

        for (var lineNumber = firstLineNumber; lineNumber < text.Lines.Count && text.Lines[lineNumber].Start < contentEnd; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var textStart = SkipIndentation(text, line.Start, Math.Min(line.End, contentEnd));

            if (textStart < Math.Min(line.End, contentEnd)
                && SyntaxIndentationUtilities.KeepsItsColumn(root, text, line, contentEnd) == false)
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
    /// Determines whether the whitespace in front of the first character of a line the block-body path moves holds a tab
    /// (RH3004-006): every line of the block after its opening brace's line that is not blank, except a line that starts
    /// with the closing brace, which the formatter lays out. A tab there has no single column count, so the fix is
    /// withheld rather than measured
    /// </summary>
    /// <param name="block">Block of the anonymous method</param>
    /// <param name="text">Text of the document</param>
    /// <returns><see langword="true"/> if a moving line has a tab in front of its first character</returns>
    private static bool HoldsTabInFrontOfMovingLine(BlockSyntax block, SourceText text)
    {
        var firstLineNumber = text.Lines.GetLineFromPosition(block.OpenBraceToken.SpanStart).LineNumber + 1;
        var lastLineNumber = text.Lines.GetLineFromPosition(block.CloseBraceToken.SpanStart).LineNumber;

        for (var lineNumber = firstLineNumber; lineNumber <= lastLineNumber; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var indentationEnd = SkipIndentation(text, line.Start, line.End);

            if (indentationEnd < line.End
                && indentationEnd != block.CloseBraceToken.SpanStart
                && text.ToString(TextSpan.FromBounds(line.Start, indentationEnd)).IndexOf('\t') >= 0)
            {
                return true;
            }
        }

        return false;
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
    /// Gets the column of a position on its line, each character counted as one column, a tab included, as the moving
    /// lines are shifted (<see cref="SyntaxIndentationUtilities.TryShiftMovedLines"/>)
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="position">Position</param>
    /// <returns>The column</returns>
    private static int GetColumn(SourceText text, int position)
    {
        return position - text.Lines.GetLineFromPosition(position).Start;
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
                    && WithholdsFixForSpanningStringOrComment(anonymousMethodExpression, text, GetExpressionBody(anonymousMethodExpression) == null) == false
                    && (GetExpressionBody(anonymousMethodExpression) != null
                        || HoldsTabInFrontOfMovingLine(anonymousMethodExpression.Block, text) == false)
                    && ReihitsuFormatter.FindUnsupportedSyntax(CreateLambdaExpression(anonymousMethodExpression)) == null)
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