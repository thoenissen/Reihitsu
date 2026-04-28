using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// Code fix provider for <see cref="RH0110UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0110UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider))]
public class RH0110UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, AnonymousMethodExpressionSyntax anonymousMethodExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || anonymousMethodExpression.ParameterList == null)
        {
            return document;
        }

        if (anonymousMethodExpression.RemoveNode(anonymousMethodExpression.ParameterList, SyntaxRemoveOptions.KeepExteriorTrivia) is not AnonymousMethodExpressionSyntax updatedAnonymousMethodExpression)
        {
            return document;
        }

        var updatedRoot = root.ReplaceNode(anonymousMethodExpression, updatedAnonymousMethodExpression);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0110UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.DiagnosticId];

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
                var anonymousMethodExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf()
                                                    .OfType<AnonymousMethodExpressionSyntax>()
                                                    .FirstOrDefault();

                if (anonymousMethodExpression != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0110Title,
                                                              token => ApplyCodeFixAsync(context.Document, anonymousMethodExpression, token),
                                                              nameof(RH0110UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}