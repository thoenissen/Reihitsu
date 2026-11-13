using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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

    /// <summary>
    /// Determines whether a line of the comment has a byte order mark (U+FEFF) or a substitute character (U+001A) in
    /// front of its <c>///</c>. The lexer reads both as white space, so the line belongs to the comment and is reported,
    /// but the conversion does not, so it would leave that line a <c>///</c> line
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if such a character stands in front of the <c>///</c> of a line</returns>
    private static bool HasStrayCharacterInFrontOfExterior(DocumentationCommentTriviaSyntax documentationComment, SourceText sourceText)
    {
        var commentStart = documentationComment.ParentTrivia.FullSpan.Start;
        var firstLine = sourceText.Lines.GetLineFromPosition(commentStart);

        return ContainsStrayCharacter(sourceText.ToString(TextSpan.FromBounds(firstLine.Start, commentStart)))
               || documentationComment.DescendantTrivia()
                                      .Any(static obj => obj.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)
                                                         && ContainsStrayCharacter(obj.ToString()));
    }

    /// <summary>
    /// Determines whether the text holds a byte order mark (U+FEFF) or a substitute character (U+001A)
    /// </summary>
    /// <param name="text">Text</param>
    /// <returns><see langword="true"/> if the text holds one of the characters</returns>
    private static bool ContainsStrayCharacter(string text)
    {
        return text.IndexOf('\uFEFF') >= 0
               || text.IndexOf('\u001A') >= 0;
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

        var sourceText = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            var trivia = root.FindTrivia(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);
            var documentationComment = trivia.Token.Parent?.AncestorsAndSelf().OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();

            if (documentationComment == null
                || HasStrayCharacterInFrontOfExterior(documentationComment, sourceText))
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