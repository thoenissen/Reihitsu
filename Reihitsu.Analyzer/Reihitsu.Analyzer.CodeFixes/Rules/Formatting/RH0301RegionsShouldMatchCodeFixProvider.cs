using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0301RegionsShouldMatchAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0301RegionsShouldMatchCodeFixProvider))]
public class RH0301RegionsShouldMatchCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, SyntaxTrivia node, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken);
        if (syntaxRoot != null)
        {
            var searcher = new SyntaxTreeRegionSearcher();

            if (searcher.SearchRegionPair(node.Token, node, out var regionTrivia))
            {
                var startText = regionTrivia.ToString();

                if (startText.Length >= 8)
                {
                    startText = startText.Substring(8);

                    var replacementTrivia = SyntaxFactory.Trivia(SyntaxFactory.EndRegionDirectiveTrivia(true)
                                                                             .WithEndRegionKeyword(SyntaxFactory.Token(SyntaxFactory.TriviaList(),
                                                                                                                       SyntaxKind.EndRegionKeyword,
                                                                                                                       SyntaxFactory.TriviaList(SyntaxFactory.Comment($" // {startText}{Environment.NewLine}")))));

                    syntaxRoot = syntaxRoot.ReplaceTrivia(node, replacementTrivia);
                    document = document.WithSyntaxRoot(syntaxRoot);
                }
            }
        }

        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0301RegionsShouldMatchAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (root.FindTrivia(diagnostic.Location.SourceSpan.Start) is { RawKind: (int)SyntaxKind.EndRegionDirectiveTrivia } syntaxTrivia)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0301Title,
                                                              c => ApplyCodeFixAsync(context.Document, syntaxTrivia, c),
                                                              nameof(CodeFixResources.RH0301Title)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}