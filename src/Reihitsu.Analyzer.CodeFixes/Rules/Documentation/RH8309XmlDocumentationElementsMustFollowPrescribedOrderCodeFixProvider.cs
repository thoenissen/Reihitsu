using System.Collections.Generic;
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

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Core;
using Reihitsu.Formatter;

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
    /// Gets the declaration that owns the reported documentation comment, and that comment
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="declaration">The declaration the comment documents</param>
    /// <returns>The documentation comment of the declaration, or <see langword="null"/> when there is none</returns>
    private static DocumentationCommentTriviaSyntax GetDocumentationComment(SyntaxNode root, TextSpan diagnosticSpan, out SyntaxNode declaration)
    {
        var diagnosticNode = root.FindNode(diagnosticSpan, findInsideTrivia: true, getInnermostNodeForTie: true);

        declaration = diagnosticNode.AncestorsAndSelf().FirstOrDefault(static node => node is MemberDeclarationSyntax or EnumMemberDeclarationSyntax);

        return declaration == null
                   ? null
                   : DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);
    }

    /// <summary>
    /// Reorders the top-level XML documentation elements into their canonical order
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <returns>The documentation comment with reordered elements</returns>
    private static DocumentationCommentTriviaSyntax ReorderElements(DocumentationCommentTriviaSyntax documentationComment)
    {
        var content = documentationComment.Content;

        // Only canonical elements participate in the reordering. Unknown or custom elements (such as
        // <inheritdoc>, <seealso> or <include>) are tolerated by the analyzer and must keep their original
        // positions, so they are pinned in place while the canonical elements are sorted around them
        var canonicalElements = content.Where(IsCanonicalElement)
                                       .ToList();

        // OrderBy is stable, so canonical elements sharing a rank (such as several <param> tags) keep their
        // original relative order
        var orderedCanonicalElements = canonicalElements.OrderBy(static node => XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank(XmlDocumentationElementOrderingUtilities.GetTagName(node)))
                                                        .ToList();

        var reorderedContent = new List<XmlNodeSyntax>(content.Count);
        var canonicalIndex = 0;

        foreach (var node in content)
        {
            if (IsCanonicalElement(node))
            {
                reorderedContent.Add(orderedCanonicalElements[canonicalIndex]);

                canonicalIndex++;
            }
            else
            {
                reorderedContent.Add(node);
            }
        }

        return documentationComment.WithContent(SyntaxFactory.List(reorderedContent));
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

        var documentationComment = GetDocumentationComment(root, diagnosticSpan, out var declaration);

        if (documentationComment == null)
        {
            return document;
        }

        var formattingAnnotation = new SyntaxAnnotation();
        var updatedDeclaration = declaration.ReplaceTrivia(documentationComment.ParentTrivia, SyntaxFactory.Trivia(ReorderElements(documentationComment)))
                                            .WithAdditionalAnnotations(formattingAnnotation);
        var updatedRoot = root.ReplaceNode(declaration, updatedDeclaration);
        var updatedDocument = document.WithSyntaxRoot(updatedRoot);
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedDeclaration = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).FirstOrDefault();

        return formattedDeclaration == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedDeclaration, cancellationToken).ConfigureAwait(false);
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

        foreach (var diagnostic in context.Diagnostics)
        {
            var documentationComment = GetDocumentationComment(root, diagnostic.Location.SourceSpan, out _);

            if (documentationComment == null
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