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

using Reihitsu.Analyzer.CodeFixes.Core;
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
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (TryCreateReorder(typeDeclaration, semanticModel, sourceText, cancellationToken, out var blocks, out var order) == false
            || order.SequenceEqual(Enumerable.Range(0, order.Count)))
        {
            return document;
        }

        var changes = blocks.Select((block, slot) => new TextChange(block.Span, blocks[order[slot]].Text));

        return document.WithText(sourceText.WithChanges(changes));
    }

    /// <summary>
    /// Determines the region blocks of the type and the order the fix puts them in, and whether that reorder is safe
    /// to offer: it must neither move a directive across (RH7309-005) nor change the program or separate a comment
    /// from its block (RH7309-006), nor move a block that holds another line ending (RH7309-007)
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose regions should be reordered</param>
    /// <param name="semanticModel">Semantic model of the document</param>
    /// <param name="sourceText">Source text of the document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="blocks">Region blocks in written order</param>
    /// <param name="order">For each block position, the index of the block that the fix puts there</param>
    /// <returns><see langword="true"/> if the reorder can be offered</returns>
    private static bool TryCreateReorder(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, SourceText sourceText, CancellationToken cancellationToken, out List<(TextSpan Span, string Text)> blocks, out IReadOnlyList<int> order)
    {
        blocks = [];
        order = [];

        if (semanticModel?.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not INamedTypeSymbol typeSymbol)
        {
            return false;
        }

        var regions = RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration);

        if (CanReorderSafely(typeDeclaration, regions, sourceText) == false)
        {
            return false;
        }

        var categories = new List<RegionCategory>();

        foreach (var region in regions)
        {
            if (region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
            {
                return false;
            }

            var span = GetRegionBlockSpan(sourceText, region);

            blocks.Add((span, sourceText.ToString(span)));
            categories.Add(RegionCategoryUtilities.GetRegionCategory(typeSymbol, RegionDirectiveUtilities.GetRegionDescription(regionDirective)));
        }

        order = Enumerable.Range(0, blocks.Count)
                          .OrderBy(index => categories[index])
                          .ToList();

        if (MovesOtherLineBreak(typeDeclaration, sourceText, blocks, order, cancellationToken))
        {
            return false;
        }

        var layout = new RegionBlockLayout(blocks.Select(block => block.Span).ToList(), order);

        return layout.ChangesMemberOrder(typeDeclaration, typeSymbol) == false
               && SeparatesCommentFromBlock(regions, order) == false
               && ChangesDocumentedDeclaration(typeDeclaration, layout) == false;
    }

    /// <summary>
    /// Determines whether a block whose place among the blocks changes holds a line break other than the document's line
    /// ending, from the start of its <c>#region</c> line through the line break that ends its <c>#endregion</c> line. The
    /// fix moves the blocks as written, so it would keep that line break (RH7309-007)
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose regions should be reordered</param>
    /// <param name="sourceText">Source text of the document</param>
    /// <param name="blocks">Region blocks in written order</param>
    /// <param name="order">For each block position, the index of the block that the fix puts there</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if a moved block holds another line break</returns>
    private static bool MovesOtherLineBreak(TypeDeclarationSyntax typeDeclaration, SourceText sourceText, IReadOnlyList<(TextSpan Span, string Text)> blocks, IReadOnlyList<int> order, CancellationToken cancellationToken)
    {
        var endOfLine = LineEndingUtilities.DetectEndOfLine(typeDeclaration.SyntaxTree.GetRoot(cancellationToken));

        for (var slot = 0; slot < order.Count; slot++)
        {
            if (order[slot] == slot)
            {
                continue;
            }

            var span = blocks[order[slot]].Span;
            var movedSpan = TextSpan.FromBounds(span.Start, sourceText.Lines.GetLineFromPosition(span.End).EndIncludingLineBreak);

            if (LineEndingUtilities.HoldsOtherLineBreak(sourceText, movedSpan, endOfLine))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a block that the fix moves has a comment above its <c>#region</c> line, with nothing but
    /// blank lines between the comment and that line. The text between blocks stays in place, so the comment would
    /// head another block
    /// </summary>
    /// <param name="regions">Top-level region pairs of the type</param>
    /// <param name="order">For each block position, the index of the block that the fix puts there</param>
    /// <returns><see langword="true"/> if a moved block would lose its comment</returns>
    private static bool SeparatesCommentFromBlock(IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions, IReadOnlyList<int> order)
    {
        for (var index = 0; index < regions.Count; index++)
        {
            if (order[index] != index
                && HasCommentAbove(regions[index].Region))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a comment stands above the directive's line, with nothing but blank lines between them.
    /// A comment behind code belongs to the trailing trivia of that code's last token, so only comments that start a
    /// line of the directive's leading trivia are found
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if a comment stands above the directive</returns>
    private static bool HasCommentAbove(SyntaxTrivia directiveTrivia)
    {
        var leadingTrivia = directiveTrivia.Token.LeadingTrivia;
        var index = leadingTrivia.IndexOf(directiveTrivia) - 1;

        while (index >= 0
               && (leadingTrivia[index].IsKind(SyntaxKind.WhitespaceTrivia) || leadingTrivia[index].IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            index--;
        }

        return index >= 0
               && SyntaxTriviaUtilities.IsCommentTrivia(leadingTrivia[index]);
    }

    /// <summary>
    /// Determines whether the reorder would change which declaration a documentation comment documents. A
    /// documentation comment in the leading trivia of a member that stands in another block, or between blocks,
    /// documents that member only as long as the text between the two stays the same. A documentation comment in the
    /// leading trivia of the type's closing brace documents nothing, and keeps documenting nothing only as long as the
    /// text between it and the closing brace stays the same; otherwise it would stand in front of the member that
    /// follows it after the reorder. In the documentation mode <c>None</c>, the parser reads no documentation
    /// comments, so there are none to check
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose regions should be reordered</param>
    /// <param name="layout">Layout of the region blocks before and after the reorder</param>
    /// <returns><see langword="true"/> if a documentation comment would document another declaration</returns>
    private static bool ChangesDocumentedDeclaration(TypeDeclarationSyntax typeDeclaration, RegionBlockLayout layout)
    {
        return typeDeclaration.Members
                              .Select(member => (LeadingTrivia: member.GetLeadingTrivia(), Start: member.SpanStart))
                              .Append((LeadingTrivia: typeDeclaration.CloseBraceToken.LeadingTrivia, Start: typeDeclaration.CloseBraceToken.SpanStart))
                              .Any(owner => owner.LeadingTrivia.Any(trivia => SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia)
                                                                              && layout.KeepsSequenceBetween(trivia.SpanStart, owner.Start) == false));
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
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        var reorderableTypes = new Dictionary<TypeDeclarationSyntax, bool>();

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindTrivia(diagnostic.Location.SourceSpan.Start).Token.Parent?.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } typeDeclaration)
            {
                continue;
            }

            if (reorderableTypes.TryGetValue(typeDeclaration, out var canReorder) == false)
            {
                canReorder = TryCreateReorder(typeDeclaration, semanticModel, sourceText, context.CancellationToken, out _, out _);
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