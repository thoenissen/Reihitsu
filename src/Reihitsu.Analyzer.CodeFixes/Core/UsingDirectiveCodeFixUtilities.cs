using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helpers for code fixes that only reorganize using directives within a single scope
/// </summary>
internal static class UsingDirectiveCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Reorganizes the using directives of a single scope without formatting unrelated nodes, through
    /// <see cref="ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync"/> (FMT-PIPE-030), for a block of at least two
    /// directives that may be reordered (SHARED-USING-009)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    internal static async Task<Document> OrganizeScopeUsingsAsync(Document document, SyntaxNode scope, CancellationToken cancellationToken)
    {
        var usingDirectives = UsingDirectiveOrderingUtilities.GetUsings(scope);

        if (usingDirectives.Count < 2 || UsingDirectiveOrderingSafety.CanSafelyReorder(usingDirectives) == false)
        {
            return document;
        }

        return await ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync(document, scope, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods
}