using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider))]
public class RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Determine the parenthesized expression a diagnostic reports, when its parentheses can be removed without
    /// discarding a comment or a directive
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <returns><see langword="true"/> if the parentheses can be removed</returns>
    private static bool TryGetFixableExpression(SyntaxNode root, Diagnostic diagnostic, out ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        parenthesizedExpression = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) as ParenthesizedExpressionSyntax;

        return parenthesizedExpression != null
               && SyntaxNodeUtilities.InteriorContainsCommentOrDirective(parenthesizedExpression) == false;
    }

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ParenthesizedExpressionSyntax parenthesizedExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var updatedRoot = root.ReplaceNode(parenthesizedExpression, CreateReplacement(parenthesizedExpression, parenthesizedExpression.Expression));

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Applying the code fix to every fixable diagnostic of one document in a single rewrite. Each replacement is built
    /// from its already rewritten inner expression, so nested parentheses are removed together instead of merging
    /// independent text changes that overlap at adjacent parentheses.
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Document</param>
    /// <param name="diagnostics">Diagnostics of the document</param>
    /// <returns>The updated <see cref="Document"/> with every code fix applied</returns>
    private static async Task<Document> ApplyFixAllAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics)
    {
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var parenthesizedExpressions = new List<ParenthesizedExpressionSyntax>();

        foreach (var diagnostic in diagnostics)
        {
            if (TryGetFixableExpression(root, diagnostic, out var parenthesizedExpression))
            {
                parenthesizedExpressions.Add(parenthesizedExpression);
            }
        }

        if (parenthesizedExpressions.Count == 0)
        {
            return document;
        }

        var updatedRoot = root.ReplaceNodes(parenthesizedExpressions, static (original, rewritten) => CreateReplacement(original, rewritten.Expression));

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Create the expression replacing the parentheses. The outer trivia of the parentheses moves onto the expression,
    /// and a single space is added on a side whose gap carries no trivia when the neighboring tokens would otherwise be
    /// read as one token, as in <c>throw(value)</c> or <c>await (task)is string</c>.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression of the original tree, which decides the neighboring tokens</param>
    /// <param name="expression">Inner expression, which may already be rewritten</param>
    /// <returns>The replacement expression</returns>
    private static ExpressionSyntax CreateReplacement(ParenthesizedExpressionSyntax parenthesizedExpression, ExpressionSyntax expression)
    {
        var replacement = expression.WithTriviaFrom(parenthesizedExpression);
        var openParenToken = parenthesizedExpression.OpenParenToken;
        var previousToken = openParenToken.GetPreviousToken();

        if (openParenToken.LeadingTrivia.Count == 0
            && previousToken.RawKind != 0
            && previousToken.TrailingTrivia.Count == 0
            && WouldMergeIntoOneToken(previousToken.Text, replacement.GetFirstToken().Text))
        {
            replacement = replacement.WithLeadingTrivia(SyntaxFactory.Space);
        }

        var closeParenToken = parenthesizedExpression.CloseParenToken;
        var nextToken = closeParenToken.GetNextToken();

        if (closeParenToken.TrailingTrivia.Count == 0
            && nextToken.RawKind != 0
            && nextToken.LeadingTrivia.Count == 0
            && WouldMergeIntoOneToken(replacement.GetLastToken().Text, nextToken.Text))
        {
            replacement = replacement.WithTrailingTrivia(SyntaxFactory.Space);
        }

        return replacement;
    }

    /// <summary>
    /// Determine whether two token texts written without any separation would be lexed differently than as the left
    /// token followed by the right one. The C# lexer decides this, so the check is exact for every token pair: a longer
    /// first token covers <c>return</c> + <c>value</c>, and leading trivia covers a comment start such as <c>/</c> + <c>/</c>.
    /// </summary>
    /// <param name="left">Text of the left token</param>
    /// <param name="right">Text of the right token</param>
    /// <returns><see langword="true"/> if the texts need a separating space</returns>
    private static bool WouldMergeIntoOneToken(string left, string right)
    {
        var token = SyntaxFactory.ParseToken(left + right);

        return token.LeadingTrivia.Count > 0
               || token.Text != left;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return FixAllProvider.Create(ApplyFixAllAsync);
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (TryGetFixableExpression(root, diagnostic, out var parenthesizedExpression))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3002Title,
                                                              token => ApplyCodeFixAsync(context.Document, parenthesizedExpression, token),
                                                              nameof(RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}