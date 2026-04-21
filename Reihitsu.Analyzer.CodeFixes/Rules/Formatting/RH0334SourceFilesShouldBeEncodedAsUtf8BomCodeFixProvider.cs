using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for RH0334 - rewrites the document using UTF-8 with BOM encoding.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0334SourceFilesShouldBeEncodedAsUtf8BomCodeFixProvider))]
public class RH0334SourceFilesShouldBeEncodedAsUtf8BomCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix by recreating the source text with UTF-8 BOM encoding.
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var updatedSourceText = SourceText.From(sourceText.ToString(),
                                                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
                                                sourceText.ChecksumAlgorithm);

        return document.WithText(updatedSourceText);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0334SourceFilesShouldBeEncodedAsUtf8BomAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0334Title,
                                                      cancellationToken => ApplyCodeFixAsync(context.Document, cancellationToken),
                                                      nameof(RH0334SourceFilesShouldBeEncodedAsUtf8BomCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}