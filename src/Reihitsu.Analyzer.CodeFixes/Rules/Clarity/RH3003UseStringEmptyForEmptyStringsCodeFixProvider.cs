using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3003UseStringEmptyForEmptyStringsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3003UseStringEmptyForEmptyStringsCodeFixProvider))]
public class RH3003UseStringEmptyForEmptyStringsCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="literalExpression">Literal expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, LiteralExpressionSyntax literalExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        // Every token of the replacement carries explicit trivia: elastic trivia would let the code-action clean-up re-format
        // the gap in front of the literal and rewrite its line breaks in the host's line ending (SHARED-FIX-006, RH3003-004).
        // A literal can touch a keyword or identifier without trivia in between, but `string` and `Empty` cannot, so the
        // replacement gets one space on each side where it would otherwise be glued to such a word token
        var leadingTrivia = literalExpression.GetLeadingTrivia();
        var trailingTrivia = literalExpression.GetTrailingTrivia();
        var literalToken = literalExpression.Token;

        if (TouchesWordToken(literalToken.GetPreviousToken(), literalToken))
        {
            leadingTrivia = SyntaxFactory.TriviaList(SyntaxFactory.Space);
        }

        if (TouchesWordToken(literalToken, literalToken.GetNextToken()))
        {
            trailingTrivia = SyntaxFactory.TriviaList(SyntaxFactory.Space);
        }

        var stringKeyword = SyntaxFactory.Token(leadingTrivia, SyntaxKind.StringKeyword, SyntaxFactory.TriviaList());
        var dotToken = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.DotToken, SyntaxFactory.TriviaList());
        var emptyIdentifier = SyntaxFactory.Identifier(SyntaxFactory.TriviaList(), nameof(string.Empty), trailingTrivia);
        var replacementExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                         SyntaxFactory.PredefinedType(stringKeyword),
                                                                         dotToken,
                                                                         SyntaxFactory.IdentifierName(emptyIdentifier));
        var updatedRoot = root.ReplaceNode(literalExpression, replacementExpression);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Determines whether two neighbouring tokens touch without trivia in between and the one that is not the literal is a
    /// word token, i.e. its text adjoins the literal with an identifier character
    /// </summary>
    /// <param name="first">The token in front</param>
    /// <param name="second">The token behind</param>
    /// <returns><see langword="true"/> if the replacement of the literal would be glued to a keyword or identifier</returns>
    private static bool TouchesWordToken(SyntaxToken first, SyntaxToken second)
    {
        if (first.RawKind == 0
            || second.RawKind == 0
            || first.Span.End != second.SpanStart
            || first.Span.IsEmpty
            || second.Span.IsEmpty)
        {
            return false;
        }

        return SyntaxFacts.IsIdentifierPartCharacter(first.Text[first.Text.Length - 1])
               || SyntaxFacts.IsIdentifierPartCharacter(second.Text[0]);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3003UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var literalExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<LiteralExpressionSyntax>().FirstOrDefault();

                if (literalExpression != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3003Title,
                                                              token => ApplyCodeFixAsync(context.Document, literalExpression, token),
                                                              nameof(RH3003UseStringEmptyForEmptyStringsCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}