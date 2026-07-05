using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;
using Reihitsu.Formatter;

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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, SyntaxTrivia node, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot != null)
        {
            if (RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, node, out var regionTrivia))
            {
                var startText = regionTrivia.ToString();

                if (startText.Length >= 8)
                {
                    startText = startText.Substring(8);

                    var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(syntaxRoot);
                    var replacementTrivia = SyntaxFactory.Trivia(SyntaxFactory.EndRegionDirectiveTrivia(true)
                                                                              .WithEndRegionKeyword(SyntaxFactory.Token(SyntaxFactory.TriviaList(),
                                                                                                                        SyntaxKind.EndRegionKeyword,
                                                                                                                        SyntaxFactory.TriviaList(SyntaxFactory.Comment($" // {startText}{endOfLine}")))));

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
                if (root.FindTrivia(diagnostic.Location.SourceSpan.Start) is { RawKind: (int)SyntaxKind.EndRegionDirectiveTrivia } syntaxTrivia)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7301Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, syntaxTrivia, cancellationToken),
                                                              nameof(RH7301RegionsShouldMatchCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}