using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="objectCreationExpression">Node with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ObjectCreationExpressionSyntax objectCreationExpression, CancellationToken cancellationToken)
    {
        if (objectCreationExpression.Initializer == null)
        {
            return document;
        }

        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot == null)
        {
            return document;
        }

        var formattedNode = (ObjectCreationExpressionSyntax)ReihitsuFormatter.FormatNode(objectCreationExpression, cancellationToken: cancellationToken);

        syntaxRoot = syntaxRoot.ReplaceNode(objectCreationExpression, formattedNode);

        return document.WithSyntaxRoot(syntaxRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

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
                var objectCreationExpression = root.FindNode(diagnostic.Location.SourceSpan)
                                                   .FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

                if (objectCreationExpression?.Initializer != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0302Title,
                                                              c => ApplyCodeFixAsync(context.Document, objectCreationExpression, c),
                                                              nameof(RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}