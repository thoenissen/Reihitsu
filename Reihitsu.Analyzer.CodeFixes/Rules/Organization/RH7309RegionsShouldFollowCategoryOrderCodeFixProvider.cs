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

        if (CanReorderSafely(typeDeclaration, regions, sourceText) == false)
        {
            return document;
        }

        var blocks = new List<(TextSpan Span, string Text, RegionCategory Category, int Index)>();

        for (var index = 0; index < regions.Count; index++)
        {
            var region = regions[index];

            if (region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
            {
                return document;
            }

            var span = GetRegionBlockSpan(sourceText, region);
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

    /// <summary>
    /// Determines whether the top-level regions can be exchanged without changing what the preprocessor
    /// directives around them mean. The fix swaps whole region texts, which is unsafe in two ways: a
    /// conditional directive that is not paired inside the block it belongs to gets separated from its partner,
    /// and a position sensitive directive such as <c>#pragma warning</c>, <c>#nullable</c> or <c>#line</c> ends
    /// up covering different code than before. Both are checked for every swapped block and for the text
    /// between two blocks
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose regions should be reordered</param>
    /// <param name="regions">Top-level region pairs of the type</param>
    /// <param name="sourceText">Source text of the document</param>
    /// <returns><see langword="true"/> if the regions can be reordered safely</returns>
    private static bool CanReorderSafely(TypeDeclarationSyntax typeDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions, SourceText sourceText)
    {
        if (regions.Count < 2)
        {
            return false;
        }

        var previousBlockEnd = -1;

        foreach (var region in regions)
        {
            var span = GetRegionBlockSpan(sourceText, region);

            if (previousBlockEnd >= 0
                && previousBlockEnd < span.Start
                && ContainsUnsafeDirectives(typeDeclaration, TextSpan.FromBounds(previousBlockEnd, span.Start)))
            {
                return false;
            }

            if (ContainsUnsafeDirectives(typeDeclaration, span))
            {
                return false;
            }

            previousBlockEnd = span.End;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the span holds a directive that must not be relocated by the region swap
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose regions should be reordered</param>
    /// <param name="span">Span to inspect</param>
    /// <returns><see langword="true"/> if the span must not be moved</returns>
    private static bool ContainsUnsafeDirectives(TypeDeclarationSyntax typeDeclaration, TextSpan span)
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(typeDeclaration, span)
               || SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives(typeDeclaration, span);
    }

    /// <summary>
    /// Gets the line-aligned text span of the region block, covering the <c>#region</c> line through the
    /// <c>#endregion</c> line
    /// </summary>
    /// <param name="sourceText">Source text of the document</param>
    /// <param name="region">Region pair</param>
    /// <returns>Text span of the region block</returns>
    private static TextSpan GetRegionBlockSpan(SourceText sourceText, (SyntaxTrivia Region, SyntaxTrivia EndRegion) region)
    {
        var startLine = sourceText.Lines.GetLineFromPosition(region.Region.SpanStart);
        var endLine = sourceText.Lines.GetLineFromPosition(region.EndRegion.SpanStart);

        return TextSpan.FromBounds(startLine.Start, endLine.End);
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

        var sourceText = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);
        var reorderableTypes = new Dictionary<TypeDeclarationSyntax, bool>();

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindTrivia(diagnostic.Location.SourceSpan.Start).Token.Parent?.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } typeDeclaration)
            {
                continue;
            }

            if (reorderableTypes.TryGetValue(typeDeclaration, out var canReorder) == false)
            {
                canReorder = CanReorderSafely(typeDeclaration, RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration), sourceText);
                reorderableTypes[typeDeclaration] = canReorder;
            }

            if (canReorder)
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