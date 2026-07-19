using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider))]
public class RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="attributeSyntax">Attribute syntax</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, AttributeSyntax attributeSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || attributeSyntax.ArgumentList == null)
        {
            return document;
        }

        if (attributeSyntax.RemoveNode(attributeSyntax.ArgumentList, SyntaxRemoveOptions.KeepExteriorTrivia) is not AttributeSyntax updatedAttributeSyntax)
        {
            return document;
        }

        var updatedRoot = root.ReplaceNode(attributeSyntax, updatedAttributeSyntax);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer.DiagnosticId];

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
                var attributeSyntax = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent
                                          ?.AncestorsAndSelf()
                                          .OfType<AttributeSyntax>()
                                          .FirstOrDefault();

                if (attributeSyntax != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3107Title,
                                                              token => ApplyCodeFixAsync(context.Document, attributeSyntax, token),
                                                              nameof(RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}