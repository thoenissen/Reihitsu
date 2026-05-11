using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline.UsingDirectives;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared helpers for code fixes that only reorganize using directives within a single scope
/// </summary>
internal static class UsingDirectiveCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Reorganizes the using directives of a single scope without formatting unrelated nodes
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    internal static async Task<Document> OrganizeScopeUsingsAsync(Document document, SyntaxNode scope, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var usingDirectives = UsingDirectiveOrderingUtilities.GetUsings(scope);

        if (usingDirectives.Count < 2 || UsingDirectiveOrderingSafety.CanSafelyReorder(usingDirectives) == false)
        {
            return document;
        }

        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var organizedScope = UsingDirectiveOrderingPhase.Execute(scope, endOfLine, cancellationToken);
        var organizedUsings = UsingDirectiveOrderingUtilities.GetUsings(organizedScope);

        if (organizedUsings.SequenceEqual(usingDirectives))
        {
            return document;
        }

        var updatedScope = UsingDirectiveOrderingUtilities.WithUsings(scope, organizedUsings);

        return document.WithSyntaxRoot(root.ReplaceNode(scope, updatedScope));
    }

    #endregion // Methods
}