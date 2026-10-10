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
    /// its columns depend on the tokens in front of it on that line, so the enclosing statement or member is formatted
    /// as positional context and only the chain is written back
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="chainNode">The outermost node of the chain</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    public static async Task<Document> FormatChainAsync(Document document, SyntaxNode chainNode, CancellationToken cancellationToken)
    {
        var contextNode = chainNode.Ancestors().FirstOrDefault(static node => node is StatementSyntax or MemberDeclarationSyntax);

        return contextNode == null
                   ? await ReihitsuFormatter.FormatNodeInDocumentAsync(document, chainNode, cancellationToken).ConfigureAwait(false)
                   : await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, chainNode, contextNode, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods
}