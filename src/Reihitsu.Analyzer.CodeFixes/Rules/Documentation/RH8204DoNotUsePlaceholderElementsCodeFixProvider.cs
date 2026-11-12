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
using Reihitsu.Analyzer.Rules.Documentation;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8204DoNotUsePlaceholderElementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8204DoNotUsePlaceholderElementsCodeFixProvider))]
public class RH8204DoNotUsePlaceholderElementsCodeFixProvider : CodeFixProvider
{
    #region Methods

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
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var replacementText = ExtractInnerContent(root, text, diagnosticSpan);

        return document.WithText(text.Replace(diagnosticSpan, replacementText));
    }

    /// <summary>
    /// Extracts the inner content of a placeholder element as it is written, unwrapping the placeholder tags
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="text">Source text</param>
    /// <param name="diagnosticSpan">Span of the placeholder element</param>
    /// <returns>The inner content with the placeholder tags removed</returns>
    /// <remarks>
    /// The analyzer reports the span of an <see cref="XmlElementSyntax"/> or an <see cref="XmlEmptyElementSyntax"/>, so
    /// the node found for that span is the element itself. The content of an element is taken from the source text
    /// between the end of the start tag and the start of the end tag, so its references, markup, white space and line
    /// endings stay exactly as the author wrote them; an empty element has no content
    /// </remarks>
    private static string ExtractInnerContent(SyntaxNode root, SourceText text, TextSpan diagnosticSpan)
    {
        var node = root?.FindNode(diagnosticSpan, findInsideTrivia: true, getInnermostNodeForTie: true);

        return node is XmlElementSyntax element
                   ? text.ToString(TextSpan.FromBounds(element.StartTag.Span.End, element.EndTag.Span.Start))
                   : string.Empty;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId];

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

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8204Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH8204DoNotUsePlaceholderElementsCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}