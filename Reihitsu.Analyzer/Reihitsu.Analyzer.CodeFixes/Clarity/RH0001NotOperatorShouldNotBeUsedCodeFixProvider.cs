using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Reihitsu.Analyzer.Clarity
{
    /// <summary>
    /// Providing fixes for <see cref="RH0001NotOperatorShouldNotBeUsedAnalyzer"/>
    /// </summary>
    [Shared]
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0001NotOperatorShouldNotBeUsedCodeFixProvider))]
    public class RH0001NotOperatorShouldNotBeUsedCodeFixProvider : CodeFixProvider
    {
        #region Methods

        /// <summary>
        /// Applying code fix
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="node">Node with diagnostics</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        private async Task<Document> ApplyCodeFixAsync(Document document, PrefixUnaryExpressionSyntax node, CancellationToken cancellationToken)
        {
            var replacementNode = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, node.Operand, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)))
                                               .WithAdditionalAnnotations(Simplifier.Annotation);

            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken);

            var newSyntaxRoot = syntaxRoot.ReplaceNode(node, replacementNode);

            return document.WithSyntaxRoot(newSyntaxRoot);
        }

        #endregion // Methods

        #region CodeFixProvider

        /// <summary>
        /// A list of diagnostic IDs that this provider can provide fixes for.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId);

        /// <summary>
        /// Gets an optional <see cref="T:Microsoft.CodeAnalysis.CodeFixes.FixAllProvider" /> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
        /// Return null if the provider doesn't support fix all/multiple occurrences.
        /// Otherwise, you can return any of the well known fix all providers from <see cref="T:Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders" /> or implement your own fix all provider.
        /// </summary>
        /// <returns>Provider</returns>
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <summary>
        /// Computes one or more fixes for the specified <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" />.
        /// </summary>
        /// <param name="context">
        /// A <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" /> containing context information about the diagnostics to fix.
        /// The context must only contain diagnostics with a <see cref="P:Microsoft.CodeAnalysis.Diagnostic.Id" /> included in the <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider.FixableDiagnosticIds" /> for the current provider.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (root.FindNode(diagnostic.Location.SourceSpan) is PrefixUnaryExpressionSyntax node)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0001_Title,
                                                              c => ApplyCodeFixAsync(context.Document, node, c),
                                                              nameof(CodeFixResources.RH0001_Title)),
                                            diagnostic);
                }
            }
        }

        #endregion // CodeFixProvider
    }
}
