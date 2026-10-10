using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helper for code fixes that lay out a fluent chain
/// </summary>
internal static class FluentChainCodeFixHelper
{
    #region Methods

    /// <summary>
    /// Gets the outermost node of the chain whose link operator a diagnostic reports
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="diagnostic">The diagnostic</param>
    /// <returns>The outermost chain node, or <see langword="null"/></returns>
    public static SyntaxNode FindChainNode(SyntaxNode root, Diagnostic diagnostic)
    {
        var operatorToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

        if (operatorToken.Parent == null
            || FluentChain.IsChainNode(operatorToken.Parent) == false)
        {
            return null;
        }

        return FluentChain.GetOutermostChainNode(operatorToken.Parent);
    }

    /// <summary>
    /// Formats a chain the way formatting the document lays it out. A chain usually starts in the middle of a line, and
    /// its columns depend on the tokens in front of it, so the statement or member that starts the chain's line is
    /// formatted as positional context and only the chain is written back. When that context is not formatted itself —
    /// spacing in front of the chain, a conditional expression the formatter wraps, an enclosing chain it joins, or a raw
    /// string literal it re-indents — the chain written back alone would not get the columns formatting the document gives
    /// it, and the fix would leave its own diagnostic; the whole context is formatted instead
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="chainNode">The outermost node of the chain</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    public static async Task<Document> FormatChainAsync(Document document, SyntaxNode chainNode, CancellationToken cancellationToken)
    {
        var contextNode = chainNode.Ancestors().FirstOrDefault(static node => node is StatementSyntax or MemberDeclarationSyntax
                                                                              && SyntaxTokenPositionUtilities.IsFirstOnLine(node.GetFirstToken()));

        if (contextNode == null)
        {
            return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, chainNode, cancellationToken).ConfigureAwait(false);
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var chainAnnotation = new SyntaxAnnotation();
        var contextAnnotation = new SyntaxAnnotation();
        var annotatedRoot = root.ReplaceNodes([chainNode, contextNode],
                                              (originalNode, rewrittenNode) => rewrittenNode.WithAdditionalAnnotations(originalNode == chainNode ? chainAnnotation : contextAnnotation));
        var annotatedDocument = document.WithSyntaxRoot(annotatedRoot);

        annotatedRoot = await annotatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        var annotatedChain = annotatedRoot.GetAnnotatedNodes(chainAnnotation).Single();
        var annotatedContext = annotatedRoot.GetAnnotatedNodes(contextAnnotation).Single();
        var chainOnlyDocument = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(annotatedDocument, annotatedChain, annotatedContext, cancellationToken).ConfigureAwait(false);
        var contextDocument = await ReihitsuFormatter.FormatNodeInDocumentAsync(annotatedDocument, annotatedContext, cancellationToken).ConfigureAwait(false);
        var resultDocument = await HasSameChainLayoutAsync(chainOnlyDocument, contextDocument, chainAnnotation, cancellationToken).ConfigureAwait(false)
                                 ? chainOnlyDocument
                                 : contextDocument;

        return await RemoveAnnotationsAsync(resultDocument, chainAnnotation, contextAnnotation, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the annotated chain has the same text and starts in the same column in both documents, so every
    /// line of it has the same layout
    /// </summary>
    /// <param name="first">The first document</param>
    /// <param name="second">The second document</param>
    /// <param name="chainAnnotation">The annotation of the chain</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the chain is found in both documents and has the same layout in both</returns>
    private static async Task<bool> HasSameChainLayoutAsync(Document first, Document second, SyntaxAnnotation chainAnnotation, CancellationToken cancellationToken)
    {
        var firstRoot = await first.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var secondRoot = await second.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var firstChain = firstRoot?.GetAnnotatedNodes(chainAnnotation).FirstOrDefault();
        var secondChain = secondRoot?.GetAnnotatedNodes(chainAnnotation).FirstOrDefault();

        return firstChain != null
               && secondChain != null
               && firstChain.ToString() == secondChain.ToString()
               && SyntaxTokenPositionUtilities.GetColumn(firstChain.GetFirstToken()) == SyntaxTokenPositionUtilities.GetColumn(secondChain.GetFirstToken());
    }

    /// <summary>
    /// Removes the annotations the fix added to find the chain and its context
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="chainAnnotation">The annotation of the chain</param>
    /// <param name="contextAnnotation">The annotation of the context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document without the annotations</returns>
    private static async Task<Document> RemoveAnnotationsAsync(Document document, SyntaxAnnotation chainAnnotation, SyntaxAnnotation contextAnnotation, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var annotatedNodes = root.GetAnnotatedNodes(chainAnnotation).Concat(root.GetAnnotatedNodes(contextAnnotation)).ToList();

        return annotatedNodes.Count == 0
                   ? document
                   : document.WithSyntaxRoot(root.ReplaceNodes(annotatedNodes, (_, rewrittenNode) => rewrittenNode.WithoutAnnotations(chainAnnotation, contextAnnotation)));
    }

    #endregion // Methods
}