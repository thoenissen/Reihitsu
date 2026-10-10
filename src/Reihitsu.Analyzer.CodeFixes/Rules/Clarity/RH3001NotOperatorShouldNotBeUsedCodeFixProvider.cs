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
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Providing fixes for <see cref="RH3001NotOperatorShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3001NotOperatorShouldNotBeUsedCodeFixProvider))]
public class RH3001NotOperatorShouldNotBeUsedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, PrefixUnaryExpressionSyntax node, CancellationToken cancellationToken)
    {
        var operandLeadingTrivia = node.OperatorToken.TrailingTrivia.AddRange(node.Operand.GetLeadingTrivia());

        while (operandLeadingTrivia.Count > 0
               && operandLeadingTrivia[0].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            operandLeadingTrivia = operandLeadingTrivia.RemoveAt(0);
        }

        var openParenthesis = SyntaxFactory.Token(SyntaxKind.OpenParenToken)
                                           .WithLeadingTrivia(node.GetLeadingTrivia())
                                           .WithTrailingTrivia(operandLeadingTrivia);
        var operand = node.Operand.WithoutLeadingTrivia();
        var replacementNode = SyntaxFactory.ParenthesizedExpression(openParenthesis,
                                                                    SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, operand, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)),
                                                                    SyntaxFactory.Token(SyntaxKind.CloseParenToken))
                                           .WithTrailingTrivia(node.GetTrailingTrivia())
                                           .WithAdditionalAnnotations(Simplifier.Annotation);

        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot != null)
        {
            var newSyntaxRoot = syntaxRoot.ReplaceNode(node, replacementNode);

            document = document.WithSyntaxRoot(newSyntaxRoot);
        }

        return document;
    }

    /// <summary>
    /// Determines whether the operator-to-operand gap contains syntax that cannot be transferred safely
    /// </summary>
    /// <param name="node">Logical-not expression</param>
    /// <returns><see langword="true"/> if the operand trivia can be preserved by the rewrite; otherwise, <see langword="false"/></returns>
    private static bool HasSafeOperandTrivia(PrefixUnaryExpressionSyntax node)
    {
        var gap = TextSpan.FromBounds(node.OperatorToken.Span.End, node.Operand.SpanStart);
        var gapTrivia = node.OperatorToken.TrailingTrivia.AddRange(node.Operand.GetLeadingTrivia());

        return node.SyntaxTree.GetDiagnostics().Any(diagnostic => diagnostic.Location.SourceSpan.IntersectsWith(gap)) == false
               && gapTrivia.All(static trivia => SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia) == false
                                                 && trivia.IsKind(SyntaxKind.SkippedTokensTrivia) == false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId];

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
                if (root.FindNode(diagnostic.Location.SourceSpan) is PrefixUnaryExpressionSyntax node
                    && HasSafeOperandTrivia(node))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3001Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, node, cancellationToken),
                                                              nameof(RH3001NotOperatorShouldNotBeUsedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}