using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Core;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline.UsingDirectives;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider))]
public class RH0390UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix by reorganizing using directives into groups
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode scope, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, scope, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId];

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
            if (UsingDirectiveOrderingUtilities.TryGetUsingDirectiveScope(root, diagnostic, out var scope)
                && UsingDirectiveOrderingSafety.CanSafelyReorder(UsingDirectiveOrderingUtilities.GetUsings(scope)))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0390Title,
                                                          token => ApplyCodeFixAsync(context.Document, scope, token),
                                                          nameof(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}