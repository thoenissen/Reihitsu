using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Providing fixes for <see cref="RH7309RegionsShouldFollowCategoryOrderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7309RegionsShouldFollowCategoryOrderCodeFixProvider))]
public class RH7309RegionsShouldFollowCategoryOrderCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Reorders the top-level regions of the type into the canonical category order
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeDeclaration">Type declaration whose regions should be reordered</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ReorderRegionsAsync(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (semanticModel?.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not INamedTypeSymbol typeSymbol)
        {
            return document;
        }

        var regions = RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration);

        if (regions.Count < 2)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var blocks = new List<(TextSpan Span, string Text, RegionCategory Category, int Index)>();

        for (var index = 0; index < regions.Count; index++)
        {
            var region = regions[index];

            if (region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
            {
                return document;
            }

            var startLine = sourceText.Lines.GetLineFromPosition(region.Region.SpanStart);
            var endLine = sourceText.Lines.GetLineFromPosition(region.EndRegion.SpanStart);
            var span = TextSpan.FromBounds(startLine.Start, endLine.End);
            var description = RegionDirectiveUtilities.GetRegionDescription(regionDirective);

            blocks.Add((span, sourceText.ToString(span), RegionCategoryUtilities.GetRegionCategory(typeSymbol, description), index));
        }

        var orderedBlocks = blocks.OrderBy(block => block.Category).ToList();

        if (orderedBlocks.Select(block => block.Index).SequenceEqual(blocks.Select(block => block.Index)))
        {
            return document;
        }

        var changes = blocks.Select((block, index) => new TextChange(block.Span, orderedBlocks[index].Text));

        return document.WithText(sourceText.WithChanges(changes));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId];

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
            if (root.FindTrivia(diagnostic.Location.SourceSpan.Start).Token.Parent?.FirstAncestorOrSelf<TypeDeclarationSyntax>() is { } typeDeclaration)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7309Title,
                                                          cancellationToken => ReorderRegionsAsync(context.Document, typeDeclaration, cancellationToken),
                                                          nameof(RH7309RegionsShouldFollowCategoryOrderCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}