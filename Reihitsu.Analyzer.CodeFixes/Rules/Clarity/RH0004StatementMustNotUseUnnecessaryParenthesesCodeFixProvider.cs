using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0004StatementMustNotUseUnnecessaryParenthesesCodeFixProvider))]
public class RH0004StatementMustNotUseUnnecessaryParenthesesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ParenthesizedExpressionSyntax parenthesizedExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var updatedRoot = root.ReplaceNode(parenthesizedExpression, parenthesizedExpression.Expression.WithTriviaFrom(parenthesizedExpression));

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId];

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
                if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is ParenthesizedExpressionSyntax parenthesizedExpression)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0004Title,
                                                              token => ApplyCodeFixAsync(context.Document, parenthesizedExpression, token),
                                                              nameof(RH0004StatementMustNotUseUnnecessaryParenthesesCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}