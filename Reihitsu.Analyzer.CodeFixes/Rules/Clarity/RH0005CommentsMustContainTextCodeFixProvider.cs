using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH0005CommentsMustContainTextAnalyzer"/>
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0005CommentsMustContainTextCodeFixProvider))]
public class RH0005CommentsMustContainTextCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxTrivia commentTrivia, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var line = text.Lines.GetLineFromPosition(commentTrivia.FullSpan.Start);
        var lineText = text.ToString(line.Span);
        var trimmedLineText = lineText.Trim();
        var trimmedCommentText = commentTrivia.ToString().Trim();
        TextSpan removalSpan;

        if (trimmedLineText == trimmedCommentText)
        {
            removalSpan = TextSpan.FromBounds(line.Start, line.EndIncludingLineBreak);
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

            removalSpan = TextSpan.FromBounds(removalStart, commentTrivia.FullSpan.End);
        }

        return document.WithText(text.Replace(removalSpan, string.Empty));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0005CommentsMustContainTextAnalyzer.DiagnosticId];

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

                if (commentTrivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineCommentTrivia)
                    || commentTrivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineCommentTrivia))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0005Title,
                                                              token => ApplyCodeFixAsync(context.Document, commentTrivia, token),
                                                              nameof(RH0005CommentsMustContainTextCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}