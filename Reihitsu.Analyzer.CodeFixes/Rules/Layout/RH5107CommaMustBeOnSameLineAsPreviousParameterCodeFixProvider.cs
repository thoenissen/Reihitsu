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

        // The continuation line must align under the parameter list's first parameter, not one column past the
        // opening parenthesis: unlike RH5109's fix, this one never relocates the first parameter, so when it starts
        // on its own line (a hanging indent), the parenthesis-derived column would be wrong for it.
        var anchorToken = parameterList.Parameters[0].GetFirstToken();
        var anchorLine = sourceText.Lines.GetLineFromPosition(anchorToken.SpanStart);
        var anchorColumn = anchorToken.SpanStart - anchorLine.Start;

        // The comma is being moved onto the previous line, so its own line's leading whitespace no longer has a
        // reason to be preserved; replace it — together with the comma and any whitespace run following it — with
        // the exact column the continuation line needs. The registration guard below has already established that
        // nothing but whitespace can precede the comma on this line.
        var commaLine = sourceText.Lines.GetLineFromPosition(token.SpanStart);
        var removalEnd = token.Span.End;

        while (removalEnd < sourceText.Length
               && (sourceText[removalEnd] == ' ' || sourceText[removalEnd] == '\t'))
        {
            removalEnd++;
        }

        var updatedText = sourceText.Replace(TextSpan.FromBounds(commaLine.Start, removalEnd), new string(' ', anchorColumn));

        updatedText = updatedText.Replace(TextSpan.FromBounds(previousToken.Span.End, previousToken.Span.End), ",");

        return document.WithText(updatedText);
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