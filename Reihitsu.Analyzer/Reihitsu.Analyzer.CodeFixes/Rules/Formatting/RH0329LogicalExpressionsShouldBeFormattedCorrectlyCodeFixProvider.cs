using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0329LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH0329LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="operatorToken">Operator token with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxToken operatorToken, CancellationToken cancellationToken)
    {
        if (operatorToken.Parent is not BinaryExpressionSyntax binaryExpression)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var leftLineSpan = binaryExpression.Left.SyntaxTree.GetLineSpan(binaryExpression.Left.Span);
        var targetColumn = leftLineSpan.StartLinePosition.Character;
        var newLeadingTrivia = default(SyntaxTriviaList);

        foreach (var trivia in operatorToken.LeadingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false))
        {
            newLeadingTrivia = newLeadingTrivia.Add(trivia);
        }

        newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.Whitespace(new string(' ', targetColumn)));
        root = root.ReplaceToken(operatorToken, operatorToken.WithLeadingTrivia(newLeadingTrivia));

        return document.WithSyntaxRoot(root);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

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
                var operatorToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

                if (operatorToken.Parent is BinaryExpressionSyntax)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0329Title,
                                                              c => ApplyCodeFixAsync(context.Document, operatorToken, c),
                                                              nameof(RH0329LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}