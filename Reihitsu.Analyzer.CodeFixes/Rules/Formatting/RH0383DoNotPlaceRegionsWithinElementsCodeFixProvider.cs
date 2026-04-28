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
/// Code fix provider for <see cref="RH0383DoNotPlaceRegionsWithinElementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0383DoNotPlaceRegionsWithinElementsCodeFixProvider))]
public class RH0383DoNotPlaceRegionsWithinElementsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxTrivia directiveTrivia, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot == null
            || RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, directiveTrivia, out var matchingDirectiveTrivia) == false)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var regionDirectiveTrivia = directiveTrivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RegionDirectiveTrivia)
                                        ? directiveTrivia
                                        : matchingDirectiveTrivia;
        var endRegionDirectiveTrivia = directiveTrivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EndRegionDirectiveTrivia)
                                           ? directiveTrivia
                                           : matchingDirectiveTrivia;
        var regionDirectiveLine = sourceText.Lines.GetLineFromPosition(regionDirectiveTrivia.Span.Start);
        var endRegionDirectiveLine = sourceText.Lines.GetLineFromPosition(endRegionDirectiveTrivia.Span.Start);
        var endRegionRemovalEnd = endRegionDirectiveLine.EndIncludingLineBreak > endRegionDirectiveLine.End
                                      ? endRegionDirectiveLine.EndIncludingLineBreak
                                      : endRegionDirectiveLine.End;
        var updatedText = sourceText.Replace(TextSpan.FromBounds(endRegionDirectiveLine.Start, endRegionRemovalEnd), string.Empty);
        var regionRemovalEnd = regionDirectiveLine.EndIncludingLineBreak > regionDirectiveLine.End
                                   ? regionDirectiveLine.EndIncludingLineBreak
                                   : regionDirectiveLine.End;

        updatedText = updatedText.Replace(TextSpan.FromBounds(regionDirectiveLine.Start, regionRemovalEnd), string.Empty);

        return document.WithText(updatedText);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0383DoNotPlaceRegionsWithinElementsAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (syntaxRoot == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var directiveTrivia = syntaxRoot.FindTrivia(diagnostic.Location.SourceSpan.Start);

            if (RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, directiveTrivia, out _))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0383Title,
                                                          token => ApplyCodeFixAsync(context.Document, directiveTrivia, token),
                                                          nameof(RH0383DoNotPlaceRegionsWithinElementsCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}