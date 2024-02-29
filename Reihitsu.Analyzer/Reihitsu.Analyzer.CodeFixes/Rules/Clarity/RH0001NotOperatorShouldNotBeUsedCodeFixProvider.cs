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

namespace Reihitsu.Analyzer.Rules.Clarity;

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
        if (syntaxRoot != null)
        {
            var newSyntaxRoot = syntaxRoot.ReplaceNode(node, replacementNode);

            document = document.WithSyntaxRoot(newSyntaxRoot);
        }

        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId);

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
                if (root.FindNode(diagnostic.Location.SourceSpan) is PrefixUnaryExpressionSyntax node)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0001Title,
                                                              c => ApplyCodeFixAsync(context.Document, node, c),
                                                              nameof(RH0001NotOperatorShouldNotBeUsedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}