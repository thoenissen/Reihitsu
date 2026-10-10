using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider))]
public class RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, DocumentationCommentTriviaSyntax documentationComment, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var triviaSpan = documentationComment.ParentTrivia.FullSpan;
        var currentCommentText = text.ToString(triviaSpan);

        // A deterministic single-pass scan over the C# line-terminator set. The pattern it replaced anchored on
        // (?m)^, which .NET resolves against the line feed alone, so a comment separated by a carriage return,
        // U+0085, U+2028 or U+2029 kept its remaining exteriors and one application left the comment half // and
        // half ///. It also carried a wall-clock match timeout, which made success depend on process scheduling
        var updatedCommentText = DocumentationCommentUtilities.ConvertDocumentationExteriorsToComments(currentCommentText, cancellationToken);

        return document.WithText(text.Replace(triviaSpan, updatedCommentText));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var trivia = root.FindTrivia(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);
            var documentationComment = trivia.Token.Parent?.AncestorsAndSelf().OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();

            if (documentationComment == null)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8401Title,
                                                      token => ApplyCodeFixAsync(context.Document, documentationComment, token),
                                                      nameof(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}