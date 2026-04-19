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
/// Providing fixes for <see cref="RH0333MultiLineArgumentsShouldBeAlignedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0333MultiLineArgumentsShouldBeAlignedCodeFixProvider))]
public class RH0333MultiLineArgumentsShouldBeAlignedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="argument">Argument with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ArgumentSyntax argument, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var argumentList = argument.FirstAncestorOrSelf<ArgumentListSyntax>();

        if (argumentList == null)
        {
            return document;
        }

        var openParenColumn = argumentList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Character;
        var alignColumn = openParenColumn + 1;
        var alignWhitespace = new string(' ', alignColumn);

        var firstToken = argument.GetFirstToken();
        var newLeadingTrivia = default(SyntaxTriviaList);

        foreach (var trivia in firstToken.LeadingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false))
        {
            newLeadingTrivia = newLeadingTrivia.Add(trivia);
        }

        newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.Whitespace(alignWhitespace));

        root = root.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(newLeadingTrivia));

        return document.WithSyntaxRoot(root);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0333MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId];

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
                var argument = node as ArgumentSyntax ?? node.FirstAncestorOrSelf<ArgumentSyntax>();

                if (argument != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0333Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, argument, cancellationToken),
                                                              nameof(RH0333MultiLineArgumentsShouldBeAlignedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}