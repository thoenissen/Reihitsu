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
    /// Builds the comparison with <see langword="false"/> that replaces the logical-not expression, with or without
    /// parentheses around it. The comment between <c>!</c> and the operand stands in front of the operand, without the
    /// whitespace in front of it (RH3001-005)
    /// </summary>
    /// <param name="node">Logical-not expression</param>
    /// <param name="isParenthesized">Whether the comparison is put in parentheses</param>
    /// <returns>The replacement expression</returns>
    private static ExpressionSyntax BuildReplacement(PrefixUnaryExpressionSyntax node, bool isParenthesized)
    {
        var operandLeadingTrivia = node.OperatorToken.TrailingTrivia.AddRange(node.Operand.GetLeadingTrivia());

        while (operandLeadingTrivia.Count > 0
               && operandLeadingTrivia[0].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            operandLeadingTrivia = operandLeadingTrivia.RemoveAt(0);
        }

        // Every token is written with its whitespace, so no elastic trivia is left that the code action would format
        var operand = node.Operand.WithoutTrivia();
        var equalsToken = SyntaxFactory.Token(SyntaxFactory.TriviaList(SyntaxFactory.Space), SyntaxKind.EqualsEqualsToken, SyntaxFactory.TriviaList(SyntaxFactory.Space));
        var falseLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression, SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.FalseKeyword, SyntaxFactory.TriviaList()));
        var comparison = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, operand, equalsToken, falseLiteral);

        if (isParenthesized == false)
        {
            return comparison.WithLeadingTrivia(node.GetLeadingTrivia().AddRange(operandLeadingTrivia))
                             .WithTrailingTrivia(node.GetTrailingTrivia());
        }

        var openParenthesis = SyntaxFactory.Token(node.GetLeadingTrivia(), SyntaxKind.OpenParenToken, operandLeadingTrivia);
        var closeParenthesis = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.CloseParenToken, node.GetTrailingTrivia());

        return SyntaxFactory.ParenthesizedExpression(openParenthesis, comparison, closeParenthesis);
    }

    /// <summary>
    /// Finds the replacement the fix writes: the comparison without parentheses where it parses as the negation's place in
    /// its parent, so it binds as the negation did, and in parentheses everywhere else (RH3001-004). The parser gives the
    /// answer, so no table of operator precedences is kept here. A replacement that does not parse as written in either
    /// form, such as one that would read as a generic name, is not offered (RH3001-007)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="node">Logical-not expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The replacement, or <see langword="null"/> when no replacement parses as written</returns>
    private static ExpressionSyntax FindReplacement(SyntaxNode root, PrefixUnaryExpressionSyntax node, CancellationToken cancellationToken)
    {
        var replacement = BuildReplacement(node, isParenthesized: false);

        if (RewriteParseUtilities.ParsesAsWritten(root, root.ReplaceNode(node, replacement), cancellationToken))
        {
            return replacement;
        }

        replacement = BuildReplacement(node, isParenthesized: true);

        return RewriteParseUtilities.ParsesAsWritten(root, root.ReplaceNode(node, replacement), cancellationToken)
                   ? replacement
                   : null;
    }

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node with diagnostics</param>
    /// <param name="replacement">The replacement of <paramref name="node"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, PrefixUnaryExpressionSyntax node, ExpressionSyntax replacement, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot != null)
        {
            var newSyntaxRoot = syntaxRoot.ReplaceNode(node, replacement);

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
                // The innermost node is the negation itself, also where it is the whole of an argument, an element, or
                // another node around it that adds no token of its own (RH3001-004). A negation that is the whole
                // expression of a constant pattern gets no fix (RH3001-008)
                if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is PrefixUnaryExpressionSyntax node
                    && node.Parent is not ConstantPatternSyntax
                    && HasSafeOperandTrivia(node)
                    && FindReplacement(root, node, context.CancellationToken) is { } replacement)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3001Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, node, replacement, cancellationToken),
                                                              nameof(RH3001NotOperatorShouldNotBeUsedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}