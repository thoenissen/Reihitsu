using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline.BlankLines;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5104CommentsMustBeOnTheirOwnLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider))]
public class RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var affectedLine = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var commentText = sourceText.ToString(diagnosticSpan);
        var indentation = GetIndentation(FormattingTextAnalysisUtilities.GetLineText(sourceText, affectedLine));
        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var (spanToRemove, removalReplacement) = GetCommentRemoval(sourceText, diagnosticSpan, indentation, endOfLine);

        var updatedText = sourceText.Replace(spanToRemove, removalReplacement);
        var blankLinePrefix = RequiresBlankLineBeforeRelocatedComment(root, sourceText, affectedLine) ? endOfLine : string.Empty;
        var insertionText = $"{blankLinePrefix}{indentation}{commentText}{endOfLine}";

        return document.WithText(updatedText.Replace(new TextSpan(affectedLine.Start, 0), insertionText));
    }

    /// <summary>
    /// Gets the span to remove when moving the comment, and the text that replaces it. A multi-line
    /// comment that carries the only line break between surrounding code is replaced with a line break
    /// instead of nothing, so removing it does not join the two sides onto one line (RH5103, issue #412)
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="diagnosticSpan">Comment span</param>
    /// <param name="indentation">Indentation to restore if the removal would otherwise join two lines</param>
    /// <param name="endOfLine">The document's detected end-of-line sequence</param>
    /// <returns>The adjusted span to replace and its replacement text</returns>
    private static (TextSpan SpanToRemove, string Replacement) GetCommentRemoval(SourceText sourceText, TextSpan diagnosticSpan, string indentation, string endOfLine)
    {
        var startLine = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var endPosition = diagnosticSpan.Length == 0 ? diagnosticSpan.End : diagnosticSpan.End - 1;
        var endLine = sourceText.Lines.GetLineFromPosition(endPosition);
        var hasCodeBeforeComment = string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(startLine.Start, diagnosticSpan.Start))) == false;
        var hasCodeAfterComment = string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(diagnosticSpan.End, endLine.End))) == false;
        var replacementStart = diagnosticSpan.Start;
        var replacementEnd = diagnosticSpan.End;

        if (hasCodeBeforeComment == false && hasCodeAfterComment)
        {
            replacementEnd = SkipTrailingHorizontalWhitespace(sourceText, replacementEnd, endLine.End);

            return (TextSpan.FromBounds(replacementStart, replacementEnd), string.Empty);
        }

        if (hasCodeBeforeComment && hasCodeAfterComment == false)
        {
            replacementStart = SkipLeadingHorizontalWhitespace(sourceText, replacementStart, startLine.Start);

            return (TextSpan.FromBounds(replacementStart, replacementEnd), string.Empty);
        }

        replacementEnd = SkipTrailingHorizontalWhitespace(sourceText, replacementEnd, endLine.End);

        if (hasCodeBeforeComment && hasCodeAfterComment && startLine.LineNumber != endLine.LineNumber)
        {
            replacementStart = SkipLeadingHorizontalWhitespace(sourceText, replacementStart, startLine.Start);

            return (TextSpan.FromBounds(replacementStart, replacementEnd), endOfLine + indentation);
        }

        return (TextSpan.FromBounds(replacementStart, replacementEnd), string.Empty);
    }

    /// <summary>
    /// Determines whether the relocated comment needs a preceding blank line to satisfy the RH5020
    /// policy (issue #412)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="insertionLine">The line the comment is inserted above</param>
    /// <returns><see langword="true"/> if a blank line must be inserted before the relocated comment</returns>
    private static bool RequiresBlankLineBeforeRelocatedComment(SyntaxNode root, SourceText sourceText, TextLine insertionLine)
    {
        var lineIndex = insertionLine.LineNumber;

        if (lineIndex == 0)
        {
            return false;
        }

        var previousLineIndex = FormattingTextAnalysisUtilities.FindPreviousNonBlankLineIndex(sourceText, lineIndex);

        if (previousLineIndex < 0 || lineIndex - previousLineIndex > 1)
        {
            return false;
        }

        var previousLineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[previousLineIndex]).TrimStart();

        if (previousLineText.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        var anchorToken = root.FindToken(insertionLine.Start);

        return BlankLineEditor.IsFirstInBlock(anchorToken.GetPreviousToken()) == false
               && SyntaxTriviaUtilities.IsPrecededByDirective(anchorToken.LeadingTrivia) == false;
    }

    /// <summary>
    /// Gets the leading whitespace for the specified line
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns>The leading whitespace</returns>
    private static string GetIndentation(string lineText)
    {
        var length = 0;

        while (length < lineText.Length
               && char.IsWhiteSpace(lineText[length]))
        {
            length++;
        }

        return lineText.Substring(0, length);
    }

    /// <summary>
    /// Determines whether the specified character is horizontal whitespace
    /// </summary>
    /// <param name="value">Character to inspect</param>
    /// <returns><see langword="true"/> if the character is a space or tab</returns>
    private static bool IsHorizontalWhitespace(char value)
    {
        return value == ' ' || value == '\t';
    }

    /// <summary>
    /// Skips horizontal whitespace to the left
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="start">Start position</param>
    /// <param name="minimum">Minimum allowed position</param>
    /// <returns>The adjusted position</returns>
    private static int SkipLeadingHorizontalWhitespace(SourceText sourceText, int start, int minimum)
    {
        var position = start;

        while (position > minimum
               && IsHorizontalWhitespace(sourceText[position - 1]))
        {
            position--;
        }

        return position;
    }

    /// <summary>
    /// Skips horizontal whitespace to the right
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="start">Start position</param>
    /// <param name="maximum">Maximum allowed position</param>
    /// <returns>The adjusted position</returns>
    private static int SkipTrailingHorizontalWhitespace(SourceText sourceText, int start, int maximum)
    {
        var position = start;

        while (position < maximum
               && IsHorizontalWhitespace(sourceText[position]))
        {
            position++;
        }

        return position;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5104Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}