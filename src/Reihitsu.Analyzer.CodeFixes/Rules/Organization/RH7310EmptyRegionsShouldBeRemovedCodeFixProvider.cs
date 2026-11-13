using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7310EmptyRegionsShouldBeRemovedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7310EmptyRegionsShouldBeRemovedCodeFixProvider))]
public class RH7310EmptyRegionsShouldBeRemovedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix: the lines from the <c>#region</c> through its <c>#endregion</c> are removed, together with
    /// the blank lines the removal leaves at the edge of a body or doubled (SHARED-FIX-007)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="regionTrivia">Region directive trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxTrivia regionTrivia, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || RegionDirectiveUtilities.TryFindMatchingDirective(root, regionTrivia, out var endRegionTrivia) == false)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var regionLineNumber = sourceText.Lines.GetLineFromPosition(regionTrivia.SpanStart).LineNumber;
        var endRegionLineNumber = sourceText.Lines.GetLineFromPosition(endRegionTrivia.SpanStart).LineNumber;
        var removedLines = new HashSet<int>();

        for (var lineNumber = regionLineNumber; lineNumber <= endRegionLineNumber; lineNumber++)
        {
            removedLines.Add(lineNumber);
        }

        var updatedText = RegionDirectiveBlankLineUtilities.RemoveDirectiveLines(root, sourceText, removedLines);

        return document.WithText(updatedText);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId];

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
            var regionTrivia = root.FindTrivia(diagnostic.Location.SourceSpan.Start);

            if (regionTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                && RegionDirectiveUtilities.TryFindMatchingDirective(root, regionTrivia, out _))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7310Title,
                                                          token => ApplyCodeFixAsync(context.Document, regionTrivia, token),
                                                          nameof(RH7310EmptyRegionsShouldBeRemovedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}