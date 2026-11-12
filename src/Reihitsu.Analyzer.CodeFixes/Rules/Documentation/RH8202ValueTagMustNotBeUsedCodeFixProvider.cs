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

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Rules.Documentation;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8202ValueTagMustNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8202ValueTagMustNotBeUsedCodeFixProvider))]
public class RH8202ValueTagMustNotBeUsedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    /// <remarks>
    /// The rule reports a documentation comment once, on its first <c>&lt;value&gt;</c> element, so the fix removes
    /// every <c>&lt;value&gt;</c> element of that comment the rule reports, and not only the reported one
    /// </remarks>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var elementSpans = GetRemovableElementSpans(root, diagnosticSpan);
        var changes = new List<TextChange>();
        var previousEnd = -1;

        foreach (var elementSpan in elementSpans)
        {
            var removalSpan = DocumentationCommentCodeFixUtilities.GetElementRemovalSpan(text, elementSpan);

            if (removalSpan.Start < previousEnd)
            {
                continue;
            }

            changes.Add(new TextChange(removalSpan, string.Empty));

            previousEnd = removalSpan.End;
        }

        return document.WithText(text.WithChanges(changes));
    }

    /// <summary>
    /// Gets the spans of the <c>&lt;value&gt;</c> elements the fix removes: the reported element, and every other
    /// <c>&lt;value&gt;</c> element of its documentation comment outside <c>&lt;code&gt;</c> and <c>&lt;example&gt;</c>
    /// samples which is closed by an end tag of its own or empty, in source order
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <returns>The element spans</returns>
    private static List<TextSpan> GetRemovableElementSpans(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var reportedNode = root?.FindNode(diagnosticSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
        var documentationComment = reportedNode?.FirstAncestorOrSelf<DocumentationCommentTriviaSyntax>();

        if (documentationComment == null)
        {
            return [diagnosticSpan];
        }

        var elements = DirectDocumentationSyntaxChecker.GetDocumentingTags(documentationComment, "value")
                                                       .Where(obj => DocumentationCommentCodeFixUtilities.CanRewriteElement(root, obj.Span))
                                                       .ToList();

        return elements.Where(obj => obj.Ancestors().OfType<XmlNodeSyntax>().Any(elements.Contains) == false)
                       .Select(obj => obj.Span)
                       .Append(diagnosticSpan)
                       .Distinct()
                       .OrderBy(obj => obj.Start)
                       .ToList();
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8202ValueTagMustNotBeUsedAnalyzer.DiagnosticId];

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
            if (DocumentationCommentCodeFixUtilities.CanRewriteElement(root, diagnostic.Location.SourceSpan) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8202Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH8202ValueTagMustNotBeUsedCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}