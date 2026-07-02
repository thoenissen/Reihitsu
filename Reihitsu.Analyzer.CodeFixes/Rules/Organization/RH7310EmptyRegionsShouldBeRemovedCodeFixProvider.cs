using System;
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
using Reihitsu.Formatter;

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
    /// Applies the code fix
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
        var regionLine = sourceText.Lines.GetLineFromPosition(regionTrivia.SpanStart);
        var endRegionLine = sourceText.Lines.GetLineFromPosition(endRegionTrivia.SpanStart);
        var removalEnd = endRegionLine.EndIncludingLineBreak;

        for (var lineIndex = endRegionLine.LineNumber + 1; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            var line = sourceText.Lines[lineIndex];

            if (line.Span.IsEmpty
                || string.IsNullOrWhiteSpace(sourceText.ToString(line.Span)))
            {
                removalEnd = line.EndIncludingLineBreak;
            }
            else
            {
                break;
            }
        }

        var updatedText = sourceText.Replace(TextSpan.FromBounds(regionLine.Start, removalEnd), string.Empty);
        var updatedDocument = document.WithText(updatedText);
        var updatedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (updatedRoot == null)
        {
            return updatedDocument;
        }

        var position = Math.Max(0, Math.Min(regionLine.Start, updatedRoot.FullSpan.End - 1));
        var container = updatedRoot.FindToken(position).Parent?.AncestorsAndSelf()
                                   .FirstOrDefault(node => node is TypeDeclarationSyntax or BaseNamespaceDeclarationSyntax or CompilationUnitSyntax);

        return container == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, container, cancellationToken).ConfigureAwait(false);
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