using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3201CommentsMustContainTextAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3201CommentsMustContainTextCodeFixProvider))]
public class RH3201CommentsMustContainTextCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix. A comment that stands alone on its lines, also one that spans several lines, is removed
    /// together with those lines, so no empty line stays behind (SHARED-FIX-007)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxTrivia commentTrivia, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var line = text.Lines.GetLineFromPosition(commentTrivia.FullSpan.Start);
        var lastLine = text.Lines.GetLineFromPosition(commentTrivia.FullSpan.End);
        TextSpan removalSpan;

        if (string.IsNullOrWhiteSpace(text.ToString(TextSpan.FromBounds(line.Start, commentTrivia.FullSpan.Start)))
            && string.IsNullOrWhiteSpace(text.ToString(TextSpan.FromBounds(commentTrivia.FullSpan.End, lastLine.End))))
        {
            removalSpan = TextSpan.FromBounds(line.Start, lastLine.EndIncludingLineBreak);
        }
        else
        {
            var removalStart = commentTrivia.FullSpan.Start;

            while (removalStart > line.Start)
            {
                var previousCharacter = text[removalStart - 1];

                if (previousCharacter is not (' ' or '\t'))
                {
                    break;
                }

                removalStart--;
            }

            // Where code precedes the comment and the next token follows it directly, the whitespace in front of the
            // comment is what keeps the two tokens apart once the comment is gone, so the comment is removed alone
            if (removalStart > line.Start
                && IsFollowedDirectlyByCode(text, commentTrivia.FullSpan.End))
            {
                removalStart = commentTrivia.FullSpan.Start;
            }

            removalSpan = TextSpan.FromBounds(removalStart, commentTrivia.FullSpan.End);
        }

        return document.WithText(text.Replace(removalSpan, string.Empty));
    }

    /// <summary>
    /// Determines whether code follows the position directly, with no whitespace or line break in between
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="position">Position behind the comment</param>
    /// <returns><see langword="true"/> if a character other than whitespace or a line break stands at the position</returns>
    private static bool IsFollowedDirectlyByCode(SourceText text, int position)
    {
        return position < text.Length
               && text[position] is not (' ' or '\t' or '\r' or '\n');
    }

    /// <summary>
    /// Determines whether the comment can be removed without joining the token in front of it and the token after it
    /// into other tokens. Every comment between the two tokens is removed for the check, so a comment that stands
    /// next to other comments is withheld as well: removing them one after another, or together with Fix All, would
    /// join the tokens just the same
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the tokens around the comment read the same without the comments between them</returns>
    private static bool KeepsTheTokensApart(SyntaxNode root, SyntaxTrivia commentTrivia, CancellationToken cancellationToken)
    {
        var token = commentTrivia.Token;

        if (token.Parent == null
            || token.Parent.IsPartOfStructuredTrivia())
        {
            return true;
        }

        var (previousToken, nextToken) = token.LeadingTrivia.Contains(commentTrivia)
                                             ? (token.GetPreviousToken(), token)
                                             : (token, token.GetNextToken());

        // A comment in front of the first token or behind the last one has no token on its other side to join
        if (previousToken.IsKind(SyntaxKind.None)
            || nextToken.IsKind(SyntaxKind.None))
        {
            return true;
        }

        var rewrittenRoot = root.ReplaceTokens([previousToken, nextToken],
                                               (originalToken, _) =>
                                               {
                                                   var rewrittenToken = originalToken;

                                                   if (originalToken == previousToken)
                                                   {
                                                       rewrittenToken = rewrittenToken.WithTrailingTrivia(RemoveComments(rewrittenToken.TrailingTrivia));
                                                   }

                                                   if (originalToken == nextToken)
                                                   {
                                                       rewrittenToken = rewrittenToken.WithLeadingTrivia(RemoveComments(rewrittenToken.LeadingTrivia));
                                                   }

                                                   return rewrittenToken;
                                               });

        return RewriteParseUtilities.ParsesAsWritten(root, rewrittenRoot, cancellationToken);
    }

    /// <summary>
    /// Removes the single-line and multi-line comments from a trivia list
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns>The trivia list without comments</returns>
    private static SyntaxTriviaList RemoveComments(SyntaxTriviaList triviaList)
    {
        return SyntaxFactory.TriviaList(triviaList.Where(static trivia => IsRemovableComment(trivia) == false));
    }

    /// <summary>
    /// Determines whether the trivia is a comment the fix removes
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> for a single-line or a multi-line comment</returns>
    private static bool IsRemovableComment(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3201CommentsMustContainTextAnalyzer.DiagnosticId];

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
                var commentTrivia = root.FindTrivia(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);

                if (IsRemovableComment(commentTrivia)
                    && KeepsTheTokensApart(root, commentTrivia, context.CancellationToken))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3201Title,
                                                              token => ApplyCodeFixAsync(context.Document, commentTrivia, token),
                                                              nameof(RH3201CommentsMustContainTextCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}