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
/// Providing fixes for <see cref="RH0399CollectionInitializerShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0399CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH0399CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="formatTarget">Node that should be formatted</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode formatTarget, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, formatTarget, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Try to get the formatting scope that should be formatted
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>The formatting scope</returns>
    private static SyntaxNode TryGetFormatTarget(SyntaxNode root, Diagnostic diagnostic)
    {
        var objectCreationExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf()
                                           .OfType<ExpressionSyntax>()
                                           .FirstOrDefault(node => node is ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax);

        if (objectCreationExpression == null)
        {
            return null;
        }

        return objectCreationExpression.AncestorsAndSelf()
                                       .FirstOrDefault(node => node is StatementSyntax or EqualsValueClauseSyntax or ArrowExpressionClauseSyntax)
                   ?? objectCreationExpression;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0399CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

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
                var formatTarget = TryGetFormatTarget(root, diagnostic);

                if (formatTarget != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0399Title,
                                                              c => ApplyCodeFixAsync(context.Document, formatTarget, c),
                                                              nameof(RH0399CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}