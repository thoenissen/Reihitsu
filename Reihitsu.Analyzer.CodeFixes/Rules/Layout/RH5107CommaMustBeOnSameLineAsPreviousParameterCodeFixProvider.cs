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

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var token = root.FindToken(diagnosticSpan.Start);
        var previousToken = token.GetPreviousToken();
        var removalEnd = token.Span.End;

        if (removalEnd < sourceText.Length
            && sourceText[removalEnd] == ' ')
        {
            removalEnd++;
        }

        var updatedText = sourceText.Replace(TextSpan.FromBounds(token.SpanStart, removalEnd), string.Empty);
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