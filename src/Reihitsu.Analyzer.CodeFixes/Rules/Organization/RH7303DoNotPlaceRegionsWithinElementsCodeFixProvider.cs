using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7303DoNotPlaceRegionsWithinElementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider))]
public class RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider : CodeFixProvider
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
            || TryFindRemovablePartner(syntaxRoot, directiveTrivia, out var matchingDirectiveTrivia) == false)
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

    /// <summary>
    /// Tries to find the partner of the reported directive that the fix removes with it. The partner must lie inside
    /// executable code as well, so it is reported too: a partner between declarations stands where regions belong,
    /// and removing it would drop a directive the diagnostic is not about
    /// </summary>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <param name="directiveTrivia">Reported directive trivia</param>
    /// <param name="matchingDirectiveTrivia">Partner directive trivia</param>
    /// <returns><see langword="true"/> if the partner exists and lies inside executable code</returns>
    private static bool TryFindRemovablePartner(SyntaxNode syntaxRoot, SyntaxTrivia directiveTrivia, out SyntaxTrivia matchingDirectiveTrivia)
    {
        return RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, directiveTrivia, out matchingDirectiveTrivia)
               && RegionDirectiveUtilities.IsWithinElementBody(matchingDirectiveTrivia);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7303DoNotPlaceRegionsWithinElementsAnalyzer.DiagnosticId];

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

            if (TryFindRemovablePartner(syntaxRoot, directiveTrivia, out _))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7303Title,
                                                          token => ApplyCodeFixAsync(context.Document, directiveTrivia, token),
                                                          nameof(RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}