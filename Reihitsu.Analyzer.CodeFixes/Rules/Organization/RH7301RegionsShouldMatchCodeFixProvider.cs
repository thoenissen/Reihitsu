using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Providing fixes for <see cref="RH7301RegionsShouldMatchAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7301RegionsShouldMatchCodeFixProvider))]
public class RH7301RegionsShouldMatchCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node with diagnostics</param>
    /// <param name="startDescription">Normalized start-region description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxTrivia node, string startDescription, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot != null
            && node.GetStructure() is EndRegionDirectiveTriviaSyntax endRegionDirective)
        {
            var replacementDirective = endRegionDirective.WithEndRegionKeyword(endRegionDirective.EndRegionKeyword.WithTrailingTrivia(SyntaxFactory.Space,
                                                                                                                                      SyntaxFactory.Comment($"// {startDescription}")))
                                                         .WithEndOfDirectiveToken(endRegionDirective.EndOfDirectiveToken.WithLeadingTrivia());
            var replacementTrivia = SyntaxFactory.Trivia(replacementDirective);

            // The line-ending trivia follows the structured directive in the containing trivia list, so replacing only
            // the directive preserves the document's existing LF or CRLF sequence.
            syntaxRoot = syntaxRoot.ReplaceTrivia(node, replacementTrivia);
            document = document.WithSyntaxRoot(syntaxRoot);
        }

        return document;
    }

    /// <summary>
    /// Tries to get a nonempty normalized description from the matching start-region directive
    /// </summary>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <param name="endRegionTrivia">End-region trivia carrying the diagnostic</param>
    /// <param name="startDescription">Normalized start-region description</param>
    /// <returns><see langword="true"/> when a nonempty matching start description is available</returns>
    private static bool TryGetStartDescription(SyntaxNode syntaxRoot, SyntaxTrivia endRegionTrivia, out string startDescription)
    {
        startDescription = string.Empty;

        if (RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, endRegionTrivia, out var regionTrivia)
            && regionTrivia.GetStructure() is RegionDirectiveTriviaSyntax regionDirective)
        {
            startDescription = RegionDirectiveUtilities.GetRegionDescription(regionDirective);
        }

        return startDescription.Length > 0;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7301RegionsShouldMatchAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (root.FindTrivia(diagnostic.Location.SourceSpan.Start) is { RawKind: (int)SyntaxKind.EndRegionDirectiveTrivia } syntaxTrivia
                    && TryGetStartDescription(root, syntaxTrivia, out var startDescription))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7301Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, syntaxTrivia, startDescription, cancellationToken),
                                                              nameof(RH7301RegionsShouldMatchCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}