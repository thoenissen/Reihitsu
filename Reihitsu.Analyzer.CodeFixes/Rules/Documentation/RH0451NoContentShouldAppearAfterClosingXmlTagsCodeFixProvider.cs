using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0451NoContentShouldAppearAfterClosingXmlTagsCodeFixProvider))]
public class RH0451NoContentShouldAppearAfterClosingXmlTagsCodeFixProvider : CodeFixProvider
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
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var affectedLine = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var replacementStart = diagnosticSpan.Start;

        while (replacementStart > affectedLine.Start
               && (sourceText[replacementStart - 1] == ' ' || sourceText[replacementStart - 1] == '\t'))
        {
            replacementStart--;
        }

        var replacementSpan = TextSpan.FromBounds(replacementStart, diagnosticSpan.End);

        return document.WithText(sourceText.Replace(replacementSpan, string.Empty));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0451Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0451NoContentShouldAppearAfterClosingXmlTagsCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}