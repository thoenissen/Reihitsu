using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider))]
public class RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider : CodeFixProvider
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
        return await UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync(document, scope, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId];

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
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7207Title,
                                                          token => ApplyCodeFixAsync(context.Document, scope, token),
                                                          nameof(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}