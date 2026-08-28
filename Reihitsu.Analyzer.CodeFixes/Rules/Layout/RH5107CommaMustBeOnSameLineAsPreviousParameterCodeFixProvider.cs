using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer"/>. The fix is withheld
/// when the gap between the previous parameter and the comma contains a comment or a preprocessor directive, so
/// hoisting the comma can never move it across a directive boundary and corrupt an undefined-symbol configuration
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider))]
public class RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider : CodeFixProvider
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

        var token = root.FindToken(diagnosticSpan.Start);
        var parameterList = token.Parent?.FirstAncestorOrSelf<ParameterListSyntax>();

        if (parameterList == null)
        {
            return document;
        }

        var previousToken = token.GetPreviousToken();
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        // The comma is being moved onto the previous line, so its own line's leading whitespace no longer has a
        // reason to be preserved. The analyzer only reports when this gap would join the previous parameter and the
        // comma across a comment, a directive, or disabled text
        // (RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer's WouldJoinAcrossUnjoinableTrivia check), so
        // nothing but whitespace can ever precede the comma on this line for a reported diagnostic.
        var commaLine = sourceText.Lines.GetLineFromPosition(token.SpanStart);
        var removalEnd = SkipHorizontalWhitespace(sourceText, token.Span.End);
        SourceText updatedText;

        if (removalEnd >= commaLine.End)
        {
            // Nothing but whitespace remains on the comma's line once the comma is gone: removing the whole line,
            // line break included, avoids leaving a whitespace-only line behind. The element this diagnostic is
            // about then sits on a further line whose own indentation is not this rule's concern — RH5107 only
            // requires the comma to move, and no other rule ties a parameter's column to its predecessor's.
            updatedText = sourceText.Replace(TextSpan.FromBounds(commaLine.Start, commaLine.EndIncludingLineBreak), string.Empty);
        }
        else
        {
            // Content remains on the comma's line after it (the next element, or a comment): replace the leading
            // whitespace — together with the comma and any whitespace run following it — with the exact column
            // the parameter list's first parameter starts at, not one column past the opening parenthesis (the
            // formula RH5109's fix uses). The two anchors coincide whenever the first parameter immediately follows
            // the opening parenthesis on the same line — the shape every reported example has — and diverge only
            // when it does not; that remaining shape is already reported by RH5108 (parameter list must follow
            // declaration) or normalized by the formatter's horizontal-spacing phase, so both anchors converge on
            // the same formatted output for every shape this branch reaches.
            var anchorToken = parameterList.Parameters[0].GetFirstToken();
            var anchorLine = sourceText.Lines.GetLineFromPosition(anchorToken.SpanStart);
            var anchorColumn = anchorToken.SpanStart - anchorLine.Start;

            updatedText = sourceText.Replace(TextSpan.FromBounds(commaLine.Start, removalEnd), new string(' ', anchorColumn));
        }

        updatedText = updatedText.Replace(TextSpan.FromBounds(previousToken.Span.End, previousToken.Span.End), ",");

        return document.WithText(updatedText);
    }

    /// <summary>
    /// Advances past a run of spaces and tabs, stopping at the first character that is neither (including a line
    /// break, which ends the current line)
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="start">Position to start scanning from</param>
    /// <returns>The position of the first character at or after <paramref name="start"/> that is not a space or a tab</returns>
    private static int SkipHorizontalWhitespace(SourceText sourceText, int start)
    {
        var position = start;

        while (position < sourceText.Length
               && (sourceText[position] == ' ' || sourceText[position] == '\t'))
        {
            position++;
        }

        return position;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId];

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
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var previousToken = token.GetPreviousToken();
            var guardSpan = TextSpan.FromBounds(previousToken.Span.End, token.SpanStart);

            if (SyntaxNodeUtilities.SpanContainsCommentOrDirective(root, guardSpan))
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5107Title,
                                                      cancellationToken => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, cancellationToken),
                                                      nameof(RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}