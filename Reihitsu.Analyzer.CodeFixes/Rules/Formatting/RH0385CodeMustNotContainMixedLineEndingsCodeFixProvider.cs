using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0385CodeMustNotContainMixedLineEndingsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0385CodeMustNotContainMixedLineEndingsCodeFixProvider))]
public class RH0385CodeMustNotContainMixedLineEndingsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        return root == null
                   ? document
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(document, root, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0385CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0385Title,
                                                      token => ApplyCodeFixAsync(context.Document, token),
                                                      nameof(RH0385CodeMustNotContainMixedLineEndingsCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}