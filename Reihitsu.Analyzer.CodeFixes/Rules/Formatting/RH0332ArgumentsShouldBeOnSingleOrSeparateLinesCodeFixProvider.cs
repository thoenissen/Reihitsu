using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0332ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider))]
public class RH0332ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider : CodeFixProvider
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

        var openParenColumn = argumentList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Character;
        var alignColumn = openParenColumn + 1;
        var alignWhitespace = new string(' ', alignColumn);
        var newArguments = default(SeparatedSyntaxList<ArgumentSyntax>);

        for (var argumentIndex = 0; argumentIndex < argumentList.Arguments.Count; argumentIndex++)
        {
            var argument = argumentList.Arguments[argumentIndex];

            if (argumentIndex == 0)
            {
                newArguments = newArguments.Add(argument);
            }
            else
            {
                var firstToken = argument.GetFirstToken();
                var newLeadingTrivia = default(SyntaxTriviaList);

                foreach (var trivia in firstToken.LeadingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                                                                                && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false))
                {
                    newLeadingTrivia = newLeadingTrivia.Add(trivia);
                }

                newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.Whitespace(alignWhitespace));

                var newFirstToken = firstToken.WithLeadingTrivia(newLeadingTrivia);
                var newArgument = argument.ReplaceToken(firstToken, newFirstToken);

                newArguments = newArguments.Add(newArgument);
            }
        }

        var separators = new SyntaxToken[argumentList.Arguments.SeparatorCount];

        for (var separatorIndex = 0; separatorIndex < argumentList.Arguments.SeparatorCount; separatorIndex++)
        {
            var separator = argumentList.Arguments.GetSeparator(separatorIndex);
            var newTrailingTrivia = default(SyntaxTriviaList);

            foreach (var trivia in separator.TrailingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false
                                                                            && trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false))
            {
                newTrailingTrivia = newTrailingTrivia.Add(trivia);
            }

            newTrailingTrivia = newTrailingTrivia.Add(SyntaxFactory.EndOfLine(Environment.NewLine));

            separators[separatorIndex] = separator.WithTrailingTrivia(newTrailingTrivia);
        }

        var newArgumentList = argumentList.WithArguments(SyntaxFactory.SeparatedList(newArguments, separators));

        root = root.ReplaceNode(argumentList, newArgumentList);

        return document.WithSyntaxRoot(root);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId];

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
                var argumentList = node as ArgumentListSyntax ?? node.FirstAncestorOrSelf<ArgumentListSyntax>();

                if (argumentList != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0332Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, argumentList, cancellationToken),
                                                              nameof(RH0332ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}