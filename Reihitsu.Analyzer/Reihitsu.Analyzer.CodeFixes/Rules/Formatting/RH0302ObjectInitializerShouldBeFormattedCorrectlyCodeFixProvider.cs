using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
    /// Rebuild initializer expression
    /// </summary>
    /// <param name="objectCreationExpression">Object creation expression</param>
    /// <param name="newKeywordPosition">New keyword position</param>
    /// <returns>Fixed initializer expression</returns>
    private static InitializerExpressionSyntax RebuildInitializerExpression(ObjectCreationExpressionSyntax objectCreationExpression, LinePosition newKeywordPosition)
    {
        return SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                                                   SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                                                .WithLeadingTrivia(SyntaxFactory.Whitespace(new string(' ', newKeywordPosition.Character)))
                                                                .WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine)),
                                                   RebuildInitializerList(objectCreationExpression, newKeywordPosition),
                                                   SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                                                                .WithLeadingTrivia(SyntaxFactory.Whitespace(new string(' ', newKeywordPosition.Character))));
    }

    /// <summary>
    /// Rebuild initializer list
    /// </summary>
    /// <param name="objectCreationExpression">Object creation expression</param>
    /// <param name="newKeywordPosition">New keyword position</param>
    /// <returns>Fixed initializer list</returns>
    private static SeparatedSyntaxList<ExpressionSyntax> RebuildInitializerList(ObjectCreationExpressionSyntax objectCreationExpression, LinePosition newKeywordPosition)
    {
        if (objectCreationExpression.Initializer?.Expressions.Count > 0)
        {
            return SyntaxFactory.SeparatedList(objectCreationExpression.Initializer.Expressions.Select(obj => RebuildAssignmentExpression(obj, newKeywordPosition)));
        }

        return SyntaxFactory.SeparatedList<ExpressionSyntax>();
    }

    /// <summary>
    /// Rebuild assignment
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <param name="newKeywordPosition">New keyword position</param>
    /// <returns>Fixed assignment expression</returns>
    private static ExpressionSyntax RebuildAssignmentExpression(ExpressionSyntax expression, LinePosition newKeywordPosition)
    {
        if (expression is AssignmentExpressionSyntax)
        {
            return expression.WithLeadingTrivia(SyntaxFactory.Whitespace(new string(' ', newKeywordPosition.Character + 4)));
        }

        return expression;
    }

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="objectCreationExpression">Node with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, ObjectCreationExpressionSyntax objectCreationExpression, CancellationToken cancellationToken)
    {
        if (objectCreationExpression != null)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                                     .ConfigureAwait(false);

            if (root != null)
            {
                var newKeyword = objectCreationExpression.NewKeyword;

                var newKeywordPosition = newKeyword.GetLocation()
                                                   .GetLineSpan()
                                                   .StartLinePosition;

                var correctedInitializer = SyntaxFactory.ObjectCreationExpression(objectCreationExpression.NewKeyword,
                                                                                  objectCreationExpression.Type,
                                                                                  objectCreationExpression.ArgumentList,
                                                                                  RebuildInitializerExpression(objectCreationExpression, newKeywordPosition))
                                                        .WithLeadingTrivia(objectCreationExpression.GetLeadingTrivia())
                                                        .WithTrailingTrivia(objectCreationExpression.GetTrailingTrivia());
                root = root.ReplaceNode(objectCreationExpression, correctedInitializer);

                return document.WithSyntaxRoot(root);
            }
        }

        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId);

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
                if (root.FindNode(diagnostic.Location.SourceSpan) is ObjectCreationExpressionSyntax objectCreationExpression)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0302Title,
                                                              c => ApplyCodeFixAsync(context.Document, objectCreationExpression, c),
                                                              nameof(CodeFixResources.RH0301Title)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}