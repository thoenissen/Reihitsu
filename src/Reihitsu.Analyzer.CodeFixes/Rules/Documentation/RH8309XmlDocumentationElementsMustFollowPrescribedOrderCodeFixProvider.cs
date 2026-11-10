using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider))]
public class RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Determines whether the provided node is a canonical XML documentation element that participates in the ordering
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><c>true</c> if the node is a canonical element; otherwise, <c>false</c></returns>
    private static bool IsCanonicalElement(XmlNodeSyntax node)
    {
        return node is XmlElementSyntax or XmlEmptyElementSyntax
               && XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank(XmlDocumentationElementOrderingUtilities.GetTagName(node)) != XmlDocumentationElementOrderingUtilities.UnknownElementRank;
    }

    /// <summary>
    /// Determines whether the top-level elements of a documentation comment can be reordered without changing what the
    /// comment says. Reordering moves the elements and leaves every other top-level node in its slot, so text with a
    /// character that is not white space, an XML comment, a CDATA section, or a processing instruction would end up
    /// next to another element. An element without an end tag of its own runs to where the parser ends it, so moving it
    /// would carry an end tag of another name or the declaration that follows the comment, and even in place it marks
    /// incomplete documentation that is corrected by hand
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <returns><see langword="true"/> if the comment holds nothing at the top level besides elements closed by an end tag of their own, empty elements, and white space</returns>
    private static bool CanReorderElements(DocumentationCommentTriviaSyntax documentationComment)
    {
        foreach (var node in documentationComment.Content)
        {
            switch (node)
            {
                case XmlElementSyntax element when DocumentationCommentUtilities.HasOwnEndTag(element) == false:
                    return false;

                case XmlElementSyntax or XmlEmptyElementSyntax:
                    continue;

                case XmlTextSyntax text when text.TextTokens.All(static token => string.IsNullOrWhiteSpace(token.ValueText)):
                    continue;

                default:
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the reported documentation comment, when it is the first documentation comment of the declaration it
    /// documents. The fix sorts that comment only: sorting a later comment would have to decide what becomes of the
    /// blank lines, comments, and directives that separate it from the comments before it
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <returns>The documentation comment, or <see langword="null"/> when the reported comment is not the first documentation comment of a declaration</returns>
    private static DocumentationCommentTriviaSyntax GetDocumentationComment(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var diagnosticNode = root.FindNode(diagnosticSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
        var reportedComment = diagnosticNode.FirstAncestorOrSelf<DocumentationCommentTriviaSyntax>();
        var declaration = diagnosticNode.AncestorsAndSelf().FirstOrDefault(static node => node is MemberDeclarationSyntax or EnumMemberDeclarationSyntax);

        if (reportedComment == null
            || declaration == null)
        {
            return null;
        }

        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        return documentationComment != null
               && documentationComment.ParentTrivia == reportedComment.ParentTrivia
                   ? documentationComment
                   : null;
    }

    /// <summary>
    /// Builds the text changes which sort the top-level XML documentation elements into their canonical order. Each
    /// element's text moves as written into the slot of the element it replaces, so every element keeps its own lines,
    /// and the text, line breaks, and <c>///</c> exteriors between the elements stay where they are
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>The text changes</returns>
    private static List<TextChange> CreateReorderChanges(DocumentationCommentTriviaSyntax documentationComment, SourceText sourceText)
    {
        // Only canonical elements participate in the reordering. Unknown or custom elements (such as
        // <inheritdoc>, <seealso> or <include>) are tolerated by the analyzer and must keep their original
        // positions, so they are pinned in place while the canonical elements are sorted around them
        var canonicalElements = documentationComment.Content
                                                    .Where(IsCanonicalElement)
                                                    .ToList();

        // OrderBy is stable, so canonical elements sharing a rank (such as several <param> tags) keep their
        // original relative order
        var orderedCanonicalElements = canonicalElements.OrderBy(static node => XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank(XmlDocumentationElementOrderingUtilities.GetTagName(node)))
                                                        .ToList();

        var changes = new List<TextChange>(canonicalElements.Count);

        for (var index = 0; index < canonicalElements.Count; index++)
        {
            if (canonicalElements[index] != orderedCanonicalElements[index])
            {
                changes.Add(new TextChange(canonicalElements[index].Span, sourceText.ToString(orderedCanonicalElements[index].Span)));
            }
        }

        return changes;
    }

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var documentationComment = GetDocumentationComment(root, diagnosticSpan);

        if (documentationComment == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return document.WithText(sourceText.WithChanges(CreateReorderChanges(documentationComment, sourceText)));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId];

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
            var documentationComment = GetDocumentationComment(root, diagnostic.Location.SourceSpan);

            if (documentationComment == null
                || DocumentationCommentUtilities.IsWrittenBehindSource(documentationComment.ParentTrivia, sourceText)
                || CanReorderElements(documentationComment) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8309Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}