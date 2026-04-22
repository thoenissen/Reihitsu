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
/// Code fix provider for <see cref="RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider))]
public class RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ExpressionSyntax node, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var updatedRoot = node switch
                          {
                              MemberAccessExpressionSyntax memberAccessExpression => root.ReplaceNode(node, memberAccessExpression.Name.WithTriviaFrom(memberAccessExpression)),
                              ElementAccessExpressionSyntax elementAccessExpression => root.ReplaceNode(node, elementAccessExpression.WithExpression(SyntaxFactory.ThisExpression().WithTriviaFrom(elementAccessExpression.Expression))),
                              _ => root
                          };

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Try to get the node from the diagnostic.
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>Matching node</returns>
    private static ExpressionSyntax TryGetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        return root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<ExpressionSyntax>().FirstOrDefault(node => node is MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax }
                                                                                                                                                      or ElementAccessExpressionSyntax { Expression: BaseExpressionSyntax });
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId];

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
                var node = TryGetNode(root, diagnostic);

                if (node != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0002Title,
                                                              token => ApplyCodeFixAsync(context.Document, node, token),
                                                              nameof(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}