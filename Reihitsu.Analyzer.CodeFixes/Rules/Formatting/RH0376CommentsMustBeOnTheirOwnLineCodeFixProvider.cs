using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0376CommentsMustBeOnTheirOwnLineAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0376CommentsMustBeOnTheirOwnLineCodeFixProvider))]
public class RH0376CommentsMustBeOnTheirOwnLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix.
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var affectedLine = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var commentText = sourceText.ToString(diagnosticSpan);
        var indentation = GetIndentation(FormattingTextAnalysisUtilities.GetLineText(sourceText, affectedLine));
        var commentSpanToReplace = GetCommentSpanToReplace(sourceText, diagnosticSpan);
        var updatedText = sourceText.Replace(commentSpanToReplace, string.Empty);
        var insertionText = indentation + commentText + Environment.NewLine;

        return document.WithText(updatedText.Replace(new TextSpan(affectedLine.Start, 0), insertionText));
    }

    /// <summary>
    /// Gets the span to remove when moving the comment.
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="diagnosticSpan">Comment span</param>
    /// <returns>The adjusted span to replace</returns>
    private static TextSpan GetCommentSpanToReplace(SourceText sourceText, TextSpan diagnosticSpan)
    {
        var startLine = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var endPosition = diagnosticSpan.Length == 0 ? diagnosticSpan.End : diagnosticSpan.End - 1;
        var endLine = sourceText.Lines.GetLineFromPosition(endPosition);
        var hasCodeBeforeComment = FormattingTextAnalysisUtilities.ContainsNonWhitespace(sourceText.ToString(TextSpan.FromBounds(startLine.Start, diagnosticSpan.Start)));
        var hasCodeAfterComment = FormattingTextAnalysisUtilities.ContainsNonWhitespace(sourceText.ToString(TextSpan.FromBounds(diagnosticSpan.End, endLine.End)));
        var replacementStart = diagnosticSpan.Start;
        var replacementEnd = diagnosticSpan.End;

        if (hasCodeBeforeComment == false && hasCodeAfterComment)
        {
            while (replacementEnd < endLine.End
                   && IsHorizontalWhitespace(sourceText[replacementEnd]))
            {
                replacementEnd++;
            }
        }
        else if (hasCodeBeforeComment && hasCodeAfterComment == false)
        {
            while (replacementStart > startLine.Start
                   && IsHorizontalWhitespace(sourceText[replacementStart - 1]))
            {
                replacementStart--;
            }
        }
        else if (hasCodeBeforeComment && hasCodeAfterComment)
        {
            while (replacementEnd < endLine.End
                   && IsHorizontalWhitespace(sourceText[replacementEnd]))
            {
                replacementEnd++;
            }
        }

        return TextSpan.FromBounds(replacementStart, replacementEnd);
    }

    /// <summary>
    /// Gets the leading whitespace for the specified line.
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
    /// Determines whether the specified character is horizontal whitespace.
    /// </summary>
    /// <param name="value">Character to inspect</param>
    /// <returns><see langword="true"/> if the character is a space or tab</returns>
    private static bool IsHorizontalWhitespace(char value)
    {
        return value == ' ' || value == '\t';
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0376CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0376Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0376CommentsMustBeOnTheirOwnLineCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}