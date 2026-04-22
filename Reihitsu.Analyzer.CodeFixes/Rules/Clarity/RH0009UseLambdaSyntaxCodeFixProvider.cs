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
/// Code fix provider for <see cref="RH0009UseLambdaSyntaxAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0009UseLambdaSyntaxCodeFixProvider))]
public class RH0009UseLambdaSyntaxCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, AnonymousMethodExpressionSyntax anonymousMethodExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || anonymousMethodExpression.ParameterList == null)
        {
            return document;
        }

        var asyncPrefix = anonymousMethodExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword)
                              ? "async "
                              : string.Empty;
        var bodyText = anonymousMethodExpression.Block is { Statements.Count: 1 } blockSyntax
                       && blockSyntax.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement
                           ? returnStatement.Expression.ToString()
                           : anonymousMethodExpression.Block.ToString();
        var replacementExpression = SyntaxFactory.ParseExpression($"{asyncPrefix}{anonymousMethodExpression.ParameterList} => {bodyText}").WithTriviaFrom(anonymousMethodExpression);
        var updatedRoot = root.ReplaceNode(anonymousMethodExpression, replacementExpression);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0009UseLambdaSyntaxAnalyzer.DiagnosticId];

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
                var anonymousMethodExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<AnonymousMethodExpressionSyntax>().FirstOrDefault();

                if (anonymousMethodExpression?.ParameterList != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0009Title,
                                                              token => ApplyCodeFixAsync(context.Document, anonymousMethodExpression, token),
                                                              nameof(RH0009UseLambdaSyntaxCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}