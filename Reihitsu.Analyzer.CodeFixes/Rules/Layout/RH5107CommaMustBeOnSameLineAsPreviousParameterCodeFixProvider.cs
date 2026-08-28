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
/// hoisting the comma can never move it across a directive boundary and corrupt an undefined-symbol configuration.
/// It is also withheld when the comma is the last non-whitespace content on its own line, because the continuation
/// this rule reports on then sits on a further line the fix does not locate or realign
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

        // The continuation line must align under the parameter list's first parameter's own column, not one column
        // past the opening parenthesis (the formula RH5109's fix uses). The two anchors coincide whenever the first
        // parameter immediately follows the opening parenthesis on the same line — the shape every reported example
        // has — and diverge only when it does not; that remaining shape is already reported by RH5108 (parameter
        // list must follow declaration) or normalized by the formatter's horizontal-spacing phase, so both anchors
        // converge on the same formatted output for every shape this fix reaches.
        var anchorToken = parameterList.Parameters[0].GetFirstToken();
        var anchorLine = sourceText.Lines.GetLineFromPosition(anchorToken.SpanStart);
        var anchorColumn = anchorToken.SpanStart - anchorLine.Start;

        // The comma is being moved onto the previous line, so its own line's leading whitespace no longer has a
        // reason to be preserved; replace it — together with the comma and any whitespace run following it — with
        // the exact column the continuation line needs. The analyzer only reports when this gap would join the
        // previous parameter and the comma across a comment, a directive, or disabled text
        // (RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer's WouldJoinAcrossUnjoinableTrivia check), so
        // nothing but whitespace can ever precede the comma on this line for a reported diagnostic.
        var commaLine = sourceText.Lines.GetLineFromPosition(token.SpanStart);
        var removalEnd = SkipHorizontalWhitespace(sourceText, token.Span.End);
        var updatedText = sourceText.Replace(TextSpan.FromBounds(commaLine.Start, removalEnd), new string(' ', anchorColumn));

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

        var sourceText = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var previousToken = token.GetPreviousToken();
            var guardSpan = TextSpan.FromBounds(previousToken.Span.End, token.SpanStart);

            if (SyntaxNodeUtilities.SpanContainsCommentOrDirective(root, guardSpan))
            {
                continue;
            }

            // Nothing follows the comma on its own line but whitespace: the parameter this diagnostic is really
            // about sits on a further line the fix does not locate, so applying it here would only turn the
            // comma's line into a whitespace-only one without correcting the continuation's alignment.
            var commaLine = sourceText.Lines.GetLineFromPosition(token.SpanStart);

            if (SkipHorizontalWhitespace(sourceText, token.Span.End) >= commaLine.End)
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