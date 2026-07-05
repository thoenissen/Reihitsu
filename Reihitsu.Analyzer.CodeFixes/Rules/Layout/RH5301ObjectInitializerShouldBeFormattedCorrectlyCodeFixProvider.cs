using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5301ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH5301ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="objectCreationExpression">Object creation expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ExpressionSyntax objectCreationExpression, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, objectCreationExpression, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Try to get the object creation expression that should be formatted
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>The object creation expression</returns>
    private static ExpressionSyntax TryGetObjectCreationExpression(SyntaxNode root, Diagnostic diagnostic)
    {
        return root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<ExpressionSyntax>().FirstOrDefault(node => node is ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

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
                var objectCreationExpression = TryGetObjectCreationExpression(root, diagnostic);

                if (objectCreationExpression is ObjectCreationExpressionSyntax { Initializer: not null }
                                             or ImplicitObjectCreationExpressionSyntax { Initializer: not null })
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5301Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, objectCreationExpression, cancellationToken),
                                                              nameof(RH5301ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}