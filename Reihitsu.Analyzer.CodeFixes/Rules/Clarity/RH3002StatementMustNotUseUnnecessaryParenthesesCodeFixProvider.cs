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

        var replacement = parenthesizedExpression.Expression.WithTriviaFrom(parenthesizedExpression);

        if (RequiresSeparatingSpace(parenthesizedExpression, replacement))
        {
            replacement = replacement.WithLeadingTrivia(SyntaxFactory.Space);
        }

        var updatedRoot = root.ReplaceNode(parenthesizedExpression, replacement);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Determine whether removing the opening parenthesis would merge the preceding token and the first token of the
    /// inner expression into one token, as in <c>throw(value)</c> or <c>return(value)</c>. Only a gap without any
    /// trivia qualifies, so user-authored trivia between the two tokens is never touched.
    /// </summary>
    /// <param name="parenthesizedExpression">Parenthesized expression</param>
    /// <param name="replacement">Expression replacing the parenthesized expression</param>
    /// <returns><see langword="true"/> if a space has to separate the two tokens</returns>
    private static bool RequiresSeparatingSpace(ParenthesizedExpressionSyntax parenthesizedExpression, ExpressionSyntax replacement)
    {
        if (parenthesizedExpression.OpenParenToken.LeadingTrivia.Count > 0)
        {
            return false;
        }

        var previousToken = parenthesizedExpression.OpenParenToken.GetPreviousToken();

        if (previousToken.RawKind == 0
            || previousToken.TrailingTrivia.Count > 0)
        {
            return false;
        }

        var firstToken = replacement.GetFirstToken();
        var mergedToken = SyntaxFactory.ParseToken(previousToken.Text + firstToken.Text);

        return mergedToken.Text.Length > previousToken.Text.Length;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId];

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
                if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is ParenthesizedExpressionSyntax parenthesizedExpression
                    && SyntaxNodeUtilities.InteriorContainsCommentOrDirective(parenthesizedExpression) == false)
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