using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH0008DoNotUseDefaultValueTypeConstructorAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0008DoNotUseDefaultValueTypeConstructorCodeFixProvider))]
public class RH0008DoNotUseDefaultValueTypeConstructorCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ExpressionSyntax expressionSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || semanticModel?.GetTypeInfo(expressionSyntax, cancellationToken).Type is not { } typeSymbol)
        {
            return document;
        }

        var typeDisplayName = typeSymbol.ToMinimalDisplayString(semanticModel, expressionSyntax.SpanStart);
        var replacementExpression = SyntaxFactory.ParseExpression($"default({typeDisplayName})").WithTriviaFrom(expressionSyntax);
        var updatedRoot = root.ReplaceNode(expressionSyntax, replacementExpression);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Try to get the expression syntax that should be replaced.
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>Expression syntax</returns>
    private static ExpressionSyntax TryGetExpressionSyntax(SyntaxNode root, Diagnostic diagnostic)
    {
        return root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<ExpressionSyntax>().FirstOrDefault(node => node is ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId];

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
                var expressionSyntax = TryGetExpressionSyntax(root, diagnostic);

                if (expressionSyntax != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0008Title,
                                                              token => ApplyCodeFixAsync(context.Document, expressionSyntax, token),
                                                              nameof(RH0008DoNotUseDefaultValueTypeConstructorCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}