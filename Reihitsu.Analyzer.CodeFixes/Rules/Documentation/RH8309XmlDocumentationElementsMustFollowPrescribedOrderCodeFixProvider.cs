using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
    /// Reorders the top-level XML documentation elements into their canonical order
    /// </summary>
    /// <param name="documentationComment">Documentation comment</param>
    /// <returns>The documentation comment with reordered elements</returns>
    private static DocumentationCommentTriviaSyntax ReorderElements(DocumentationCommentTriviaSyntax documentationComment)
    {
        var content = documentationComment.Content;
        var elements = content.Where(static node => node is XmlElementSyntax or XmlEmptyElementSyntax)
                              .ToList();

        // OrderBy is stable, so elements sharing a rank (such as several <param> tags or unknown elements)
        // keep their original relative order while unknown elements are pushed to the end
        var orderedElements = elements.OrderBy(static node => XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank(XmlDocumentationElementOrderingUtilities.GetTagName(node)))
                                      .ToList();

        var reorderedContent = new List<XmlNodeSyntax>(content.Count);
        var elementIndex = 0;

        foreach (var node in content)
        {
            if (node is XmlElementSyntax or XmlEmptyElementSyntax)
            {
                reorderedContent.Add(orderedElements[elementIndex]);

                elementIndex++;
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

        var diagnosticNode = root.FindNode(diagnosticSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
        var declaration = diagnosticNode.AncestorsAndSelf().FirstOrDefault(static node => node is MemberDeclarationSyntax or EnumMemberDeclarationSyntax);

        if (declaration == null)
        {
            return document;
        }

        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

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
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8309Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}