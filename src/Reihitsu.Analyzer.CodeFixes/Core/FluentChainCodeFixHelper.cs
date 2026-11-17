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
    /// Determines whether the formatter refuses as unsupported what <see cref="FormatChainAsync"/> formats for a chain: the
    /// context, or the chain when it has none (SHARED-FIX-010)
    /// </summary>
    /// <param name="chainNode">The outermost node of the chain</param>
    /// <returns><see langword="true"/> if the formatter refuses it; otherwise, <see langword="false"/></returns>
    public static bool IsFormattingRefused(SyntaxNode chainNode)
    {
        return ReihitsuFormatter.FindUnsupportedSyntax(GetContextNode(chainNode) ?? chainNode) != null;
    }

    /// <summary>
    /// Formats a chain the way formatting the document lays it out, and changes nothing but the chain (SHARED-CHAIN-009). A
    /// chain usually starts in the middle of a line, so the statement or member that starts the chain's line is passed to
    /// the formatter as context and only the chain is written back: the chain's first token keeps its line and its column in
    /// the document, which the formatter takes as a fact and lays the chain's other lines out from (FMT-PIPE-017), so the
    /// code around the chain stays as written (SHARED-FIX-006). The same context decides which code the refusal check of
    /// <see cref="IsFormattingRefused"/> inspects
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="chainNode">The outermost node of the chain</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    public static async Task<Document> FormatChainAsync(Document document, SyntaxNode chainNode, CancellationToken cancellationToken)
    {
        var contextNode = GetContextNode(chainNode);
        var result = contextNode == null
                         ? await ReihitsuFormatter.FormatNodeInDocumentAsync(document, chainNode, cancellationToken).ConfigureAwait(false)
                         : await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, chainNode, contextNode, cancellationToken).ConfigureAwait(false);

        return result.Output;
    }

    /// <summary>
    /// Gets the statement or member that starts the chain's line, which <see cref="FormatChainAsync"/> passes to the
    /// formatter as context and which <see cref="IsFormattingRefused"/> checks for unsupported syntax
    /// </summary>
    /// <param name="chainNode">The outermost node of the chain</param>
    /// <returns>The context, or <see langword="null"/> when no statement or member starts the chain's line</returns>
    private static SyntaxNode GetContextNode(SyntaxNode chainNode)
    {
        return chainNode.Ancestors().FirstOrDefault(static node => node is StatementSyntax or MemberDeclarationSyntax
                                                                   && SyntaxTokenPositionUtilities.IsFirstOnLine(node.GetFirstToken()));
    }

    #endregion // Methods
}