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
/// Code fix provider for <see cref="RH0010UseReadableConditionsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0010UseReadableConditionsCodeFixProvider))]
public class RH0010UseReadableConditionsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Get the replacement operator token kind
    /// </summary>
    /// <param name="kind">Binary expression kind</param>
    /// <returns>Operator token kind</returns>
    private static SyntaxKind GetReplacementOperatorTokenKind(SyntaxKind kind)
    {
        return kind switch
               {
                   SyntaxKind.LessThanExpression => SyntaxKind.GreaterThanToken,
                   SyntaxKind.GreaterThanExpression => SyntaxKind.LessThanToken,
                   SyntaxKind.LessThanOrEqualExpression => SyntaxKind.GreaterThanEqualsToken,
                   SyntaxKind.GreaterThanOrEqualExpression => SyntaxKind.LessThanEqualsToken,
                   SyntaxKind.EqualsExpression => SyntaxKind.EqualsEqualsToken,
                   SyntaxKind.NotEqualsExpression => SyntaxKind.ExclamationEqualsToken,
                   _ => SyntaxKind.None
               };
    }

    /// <summary>
    /// Get the replacement operator text
    /// </summary>
    /// <param name="kind">Binary expression kind</param>
    /// <returns>Operator text</returns>
    private static string GetReplacementOperatorText(SyntaxKind kind)
    {
        return GetReplacementOperatorTokenKind(kind) switch
               {
                   SyntaxKind.GreaterThanToken => ">",
                   SyntaxKind.LessThanToken => "<",
                   SyntaxKind.GreaterThanEqualsToken => ">=",
                   SyntaxKind.LessThanEqualsToken => "<=",
                   SyntaxKind.EqualsEqualsToken => "==",
                   SyntaxKind.ExclamationEqualsToken => "!=",
                   _ => string.Empty
               };
    }

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="binaryExpression">Binary expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, BinaryExpressionSyntax binaryExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var replacementExpression = SyntaxFactory.ParseExpression($"{binaryExpression.Right.WithoutTrivia()} {GetReplacementOperatorText(binaryExpression.Kind())} {binaryExpression.Left.WithoutTrivia()}").WithTriviaFrom(binaryExpression);
        var updatedRoot = root.ReplaceNode(binaryExpression, replacementExpression);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0010UseReadableConditionsAnalyzer.DiagnosticId];

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
                var binaryExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<BinaryExpressionSyntax>().FirstOrDefault();

                if (binaryExpression != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0010Title,
                                                              token => ApplyCodeFixAsync(context.Document, binaryExpression, token),
                                                              nameof(RH0010UseReadableConditionsCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}