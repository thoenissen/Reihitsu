using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider))]
public class RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix.
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
        var updatedCommentText = Regex.Replace(currentCommentText, @"(?m)^(\s*)///", "$1//", RegexOptions.None, TimeSpan.FromMilliseconds(100));

        return document.WithText(text.Replace(triviaSpan, updatedCommentText));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId];

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

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0444Title,
                                                      token => ApplyCodeFixAsync(context.Document, documentationComment, token),
                                                      nameof(RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}