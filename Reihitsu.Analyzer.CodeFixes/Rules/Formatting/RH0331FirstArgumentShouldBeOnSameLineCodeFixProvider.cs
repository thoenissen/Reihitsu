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
/// Providing fixes for <see cref="RH0331FirstArgumentShouldBeOnSameLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0331FirstArgumentShouldBeOnSameLineCodeFixProvider))]
public class RH0331FirstArgumentShouldBeOnSameLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="argumentList">Argument list with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ArgumentListSyntax argumentList, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var firstArgument = argumentList.Arguments[0];
        var firstToken = firstArgument.GetFirstToken();
        var openParen = argumentList.OpenParenToken;

        // Remove leading EndOfLine and whitespace from first token, then prepend a single space
        var newFirstTokenLeading = default(SyntaxTriviaList);
        var skipping = true;

        foreach (var trivia in firstToken.LeadingTrivia)
        {
            if (skipping
                && (trivia.IsKind(SyntaxKind.EndOfLineTrivia) || trivia.IsKind(SyntaxKind.WhitespaceTrivia)))
            {
                continue;
            }

            skipping = false;

            newFirstTokenLeading = newFirstTokenLeading.Add(trivia);
        }

        var newFirstToken = firstToken.WithLeadingTrivia(newFirstTokenLeading);

        // Check if the open paren has trailing EndOfLine trivia and remove it
        var hasTrailingEndOfLine = openParen.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

        if (hasTrailingEndOfLine)
        {
            var newOpenParenTrailing = default(SyntaxTriviaList);

            foreach (var trivia in openParen.TrailingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    continue;
                }

                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    continue;
                }

                newOpenParenTrailing = newOpenParenTrailing.Add(trivia);
            }

            var newOpenParen = openParen.WithTrailingTrivia(newOpenParenTrailing);

            root = root.ReplaceTokens([openParen, firstToken],
                                      (original, _) =>
                                      {
                                          if (original == openParen)
                                          {
                                              return newOpenParen;
                                          }

                                          return newFirstToken;
                                      });
        }
        else
        {
            root = root.ReplaceToken(firstToken, newFirstToken);
        }

        return document.WithSyntaxRoot(root);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0331FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId];

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
                var node = root.FindNode(diagnostic.Location.SourceSpan);
                var argumentList = node.FirstAncestorOrSelf<ArgumentListSyntax>();

                if (argumentList != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0331Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, argumentList, cancellationToken),
                                                              nameof(RH0331FirstArgumentShouldBeOnSameLineCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}